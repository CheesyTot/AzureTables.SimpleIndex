using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using CheesyTot.AzureTables.SimpleIndex.Attributes;
using CheesyTot.AzureTables.SimpleIndex.Indexing;
using CheesyTot.AzureTables.SimpleIndex.Extensions;
using CheesyTot.AzureTables.SimpleIndex.Azure;
using System.Threading;

namespace CheesyTot.AzureTables.SimpleIndex.Repositories
{
    public class SimpleIndexRepository<T> : ISimpleIndexRepository<T> where T : class, ITableEntity, new()
    {
        private const int DEFAULT_CHUNK_SIZE = 25;
        private int _chunkSize;

        public SimpleIndexRepository(IOptionsMonitor<SimpleIndexRepositoryOptions> options)
        {
            TableClient = new SimpleIndexTableClient(
                options.CurrentValue.StorageConnectionString,
                $"{options.CurrentValue.TablePrefix}{typeof(T).Name}");

            TableClient.CreateIfNotExists();

            IndexData = new IndexData<T>(options.CurrentValue);

            _chunkSize = options.CurrentValue.ChunkSize;
        }

        protected PropertyInfo[] IndexedPropertyInfos => typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => Attribute.IsDefined(x, typeof(SimpleIndexAttribute)))
            .ToArray();

        public SimpleIndexRepository(ITableClient tableClient, IIndexData<T> indexData, int chunkSize = DEFAULT_CHUNK_SIZE)
        {
            TableClient = tableClient;
            IndexData = indexData;
            _chunkSize = chunkSize;
        }

        protected ITableClient TableClient { get; }
        protected IIndexData<T> IndexData { get; }

        public virtual async Task AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            foreach (var propertyInfo in IndexedPropertyInfos)
                await IndexData.AddAsync(entity, propertyInfo);

            await TableClient.AddEntityAsync(entity);
        }

        public virtual async Task DeleteAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            foreach (var propertyInfo in IndexedPropertyInfos)
                await IndexData.DeleteAsync(entity, propertyInfo);

            await TableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey, ETag.All);
        }

        public virtual async Task<IEnumerable<T>> GetAsync() =>
            await TableClient.QueryAsync<T>().AsEnumerableAsync();

        public virtual async Task<IEnumerable<T>> GetAsync(string partitionKey) =>
            await TableClient.QueryAsync<T>($"PartitionKey eq '{partitionKey}'").AsEnumerableAsync();

        public virtual async Task<T> GetAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await TableClient.GetEntityAsync<T>(partitionKey, rowKey);
                if (response != null)
                    return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
            { /* This is a non-exception. It just means the record was not found. */ }

            return null;
        }

        public virtual async Task<IEnumerable<T>> GetByIndexedPropertyAsync(string propertyName, object propertyValue)
        {
            var indexKey = GetIndexKey(propertyName, propertyValue);

            var indexes = await IndexData.GetAllIndexesAsync(indexKey);
            if (indexes == null || !indexes.Any())
                return null;

            return await GetByIndexesAsync(indexes);
        }

        public virtual async Task<T> GetSingleByIndexedPropertyAsync(string propertyName, object propertyValue)
        {
            var indexKey = GetIndexKey(propertyName, propertyValue);

            var index = await IndexData.GetSingleIndexAsync(indexKey);
            if (index == null)
                return null;

            return await GetAsync(index.EntityKey.PartitionKey, index.EntityKey.RowKey);
        }

        public virtual async Task<T> GetSingleOrDefaultByIndexedPropertyAsync(string propertyName, object propertyValue)
        {
            var indexKey = GetIndexKey(propertyName, propertyValue);

            var index = await IndexData.GetSingleIndexOrDefaultAsync(indexKey);
            if (index == null)
                return null;

            return await GetAsync(index.EntityKey.PartitionKey, index.EntityKey.RowKey);
        }

        public virtual async Task<T> GetFirstByIndexedPropertyAsync(string propertyName, object propertyValue)
        {
            var indexKey = GetIndexKey(propertyName, propertyValue);

            var index = await IndexData.GetFirstIndexAsync(indexKey);
            if (index == null)
                return null;

            return await GetAsync(index.EntityKey.PartitionKey, index.EntityKey.RowKey);
        }

        public virtual async Task<T> GetFirstOrDefaultByIndexedPropertyAsync(string propertyName, object propertyValue)
        {
            var indexKey = GetIndexKey(propertyName, propertyValue);

            var index = await IndexData.GetFirstIndexOrDefaultAsync(indexKey);
            if (index == null)
                return null;

            return await GetAsync(index.EntityKey.PartitionKey, index.EntityKey.RowKey);
        }

        public virtual async Task<IEnumerable<T>> QueryAsync(string filter)
        {
            return await TableClient.QueryAsync<T>(filter).AsEnumerableAsync();
        }

        public virtual async Task UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var existing = await GetAsync(entity.PartitionKey, entity.RowKey);
            if (existing == null)
                throw new ArgumentOutOfRangeException($"Entity of Type {nameof(T)} with PartitionKey {entity.PartitionKey} and RowKey {entity.RowKey} does not exist.", nameof(entity));

            foreach (var propertyInfo in IndexedPropertyInfos)
                if (propertyInfo.GetValue(existing) != propertyInfo.GetValue(entity))
                    await IndexData.ReplaceAsync(existing, entity, propertyInfo);

            await TableClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);
        }

        public IndexKey GetIndexKey(string propertyName, object propertyValue)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            if (!IndexedPropertyInfos.Select(x => x.Name).Contains(propertyName))
                throw new ArgumentOutOfRangeException($"{propertyName} is not an indexed property of {typeof(T).Name}");

            var strPropertyValue = propertyValue == null
                ? string.Empty
                : Convert.ToString(propertyValue);

            return new IndexKey(propertyName, strPropertyValue);
        }

        public async Task<IEnumerable<T>> GetByIndexesAsync(IEnumerable<Index> indexes)
        {
            if ((indexes?.Count() ?? 0) == 0)
                return Enumerable.Empty<T>();

            var result = new List<T>();

            // Apparently performing many point queries is more efficient than
            // OR-ing a bunch of PartitionKeys and RowKeys together and querying
            // on that
            foreach(var index in indexes)
                result.Add(await GetAsync(index.EntityKey.PartitionKey, index.EntityKey.RowKey));

            return result.Where(x => x != null);
        }
    }
}
