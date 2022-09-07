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

namespace CheesyTot.AzureTables.SimpleIndex.Repositories
{
    public class SimpleIndexRepository<T> : ISimpleIndexRepository<T> where T : class, ITableEntity, new()
    {
        private readonly IndexData<T> _indexData;
        private int _chunkSize;
        private PropertyInfo[] _indexedPropertyInfos;

        public SimpleIndexRepository(IOptionsMonitor<SimpleIndexRepositoryOptions> options)
        {
            TableClient = new TableClient(
                options.CurrentValue.StorageConnectionString,
                $"{options.CurrentValue.TablePrefix}{typeof(T).Name}");

            TableClient.CreateIfNotExists();

            _indexData = new IndexData<T>(options.CurrentValue);

            _chunkSize = options.CurrentValue.ChunkSize;

            _indexedPropertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => Attribute.IsDefined(x, typeof(SimpleIndexAttribute)))
                .ToArray();
        }

        protected TableClient TableClient { get; }

        public virtual async Task AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            foreach (var propertyInfo in _indexedPropertyInfos)
                await _indexData.AddAsync(entity, propertyInfo);

            await TableClient.AddEntityAsync(entity);
        }

        public virtual async Task DeleteAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            foreach (var propertyInfo in _indexedPropertyInfos)
                await _indexData.DeleteAsync(entity, propertyInfo);

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
            var indexKey = getIndexKey(propertyName, propertyValue);

            var indexes = await _indexData.GetAllIndexesAsync(indexKey);
            if (indexes == null || !indexes.Any())
                return null;

            return await getByIndexesAsync(indexes);
        }

        public virtual async Task<T> GetSingleByIndexedPropertyAsync(string propertyName, object propertyValue)
        {
            var indexKey = getIndexKey(propertyName, propertyValue);

            var index = await _indexData.GetSingleIndexAsync(indexKey);
            if (index == null)
                return null;

            return await GetAsync(index.EntityKey.PartitionKey, index.EntityKey.RowKey);
        }

        public virtual async Task<T> GetSingleOrDefaultByIndexedPropertyAsync(string propertyName, object propertyValue)
        {
            var indexKey = getIndexKey(propertyName, propertyValue);

            var index = await _indexData.GetSingleIndexOrDefaultAsync(indexKey);
            if (index == null)
                return null;

            return await GetAsync(index.EntityKey.PartitionKey, index.EntityKey.RowKey);
        }

        public virtual async Task<T> GetFirstByIndexedPropertyAsync(string propertyName, object propertyValue)
        {
            var indexKey = getIndexKey(propertyName, propertyValue);

            var index = await _indexData.GetFirstIndexAsync(indexKey);
            if (index == null)
                return null;

            return await GetAsync(index.EntityKey.PartitionKey, index.EntityKey.RowKey);
        }

        public virtual async Task<T> GetFirstOrDefaultByIndexedPropertyAsync(string propertyName, object propertyValue)
        {
            var indexKey = getIndexKey(propertyName, propertyValue);

            var index = await _indexData.GetFirstIndexOrDefaultAsync(indexKey);
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

            foreach (var propertyInfo in _indexedPropertyInfos)
                if (propertyInfo.GetValue(existing) != propertyInfo.GetValue(entity))
                    await _indexData.ReplaceAsync(existing, entity, propertyInfo);

            await TableClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);
        }

        private IndexKey getIndexKey(string propertyName, object propertyValue)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            if (!_indexedPropertyInfos.Select(x => x.Name).Contains(propertyName))
                throw new ArgumentOutOfRangeException($"{propertyName} is not an indexed property of {typeof(T).Name}");

            if (propertyValue == null)
                throw new ArgumentNullException(nameof(propertyValue));

            return new IndexKey(propertyName, Convert.ToString(propertyValue));
        }

        private async Task<IEnumerable<T>> getByIndexesAsync(IEnumerable<Index> indexes)
        {
            if ((indexes?.Count() ?? 0) == 0)
                return Enumerable.Empty<T>();

            var result = new List<T>();

            foreach (var chunk in indexes.InChunksOf(_chunkSize))
            {
                var filter = string.Join(" OR ", chunk.Select(x => $"(PartitionKey eq '{x.EntityKey.PartitionKey}' AND RowKey eq '{x.EntityKey.RowKey}')"));
                result.AddRange(await TableClient.QueryAsync<T>(filter).AsEnumerableAsync());
            }

            return result;
        }
    }
}
