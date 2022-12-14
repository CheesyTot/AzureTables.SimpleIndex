using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CheesyTot.AzureTables.SimpleIndex.Indexing;
using System.Threading;
using CheesyTot.AzureTables.SimpleIndex.Attributes;
using System.Text.RegularExpressions;
using CheesyTot.AzureTables.SimpleIndex.Helpers;

namespace CheesyTot.AzureTables.SimpleIndex.Repositories
{
    /// <summary>
    /// Implementation of the base methods for the indexed repository functionality.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimpleIndexRepository<T> : ISimpleIndexRepository<T> where T : class, ITableEntity, new()
    {
        private Regex rxSingleAlphaCharacter = new Regex("[A-z]", RegexOptions.Compiled);
        private Regex rxNonAlphanumericCharacter = new Regex("[^A-z0-9]", RegexOptions.Compiled);
        private int _defaultPageSize;

        /// <summary>
        /// Constructor that accepts <paramref name="options"></paramref> and creates the entity and index tables in Azure Table Storage.
        /// </summary>
        /// <param name="options"></param>
        public SimpleIndexRepository(IOptionsMonitor<SimpleIndexRepositoryOptions> options)
        {
            TableClient = new TableClient(
                options.CurrentValue.StorageConnectionString,
                TableNameHelper.GetTableName<T>(options.CurrentValue.TablePrefix, null));

            TableClient.CreateIfNotExists();

            IndexData = new IndexData<T>(options.CurrentValue);

            _defaultPageSize = options.CurrentValue.DefaultPageSize;
        }

        /// <summary>
        /// Constructor that accepts TableClient and IndexData parameters for unit testing
        /// </summary>
        /// <param name="tableClient"></param>
        /// <param name="indexData"></param>
        public SimpleIndexRepository(TableClient tableClient, IIndexData<T> indexData)
        {
            TableClient = tableClient;
            IndexData = indexData;
        }

        /// <summary>
        /// The <see cref="Azure.Data.Tables.TableClient">TableClient</see> used by the repository
        /// </summary>
        protected TableClient TableClient { get; }

        /// <summary>
        /// The <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.IndexData{T}"/> used by the repository
        /// </summary>
        protected IIndexData<T> IndexData { get; }

        /// <summary>
        /// Add an entity and index records for all entity properties that are decorated with the <see cref="CheesyTot.AzureTables.SimpleIndex.Attributes.SimpleIndexAttribute">SimpleIndexAttribute</see>.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity">entity</paramref> is null.</exception>
        public virtual async Task AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await TableClient.AddEntityAsync(entity);

            if(IndexKey.HasIndexedProperties<T>())
                foreach (var propertyInfo in IndexKey.GetIndexedPropertyInfos<T>())
                    await IndexData.AddAsync(entity, propertyInfo);
        }

        /// <summary>
        /// Remove an entity and index records for all entity properties that are decorated with the <see cref="CheesyTot.AzureTables.SimpleIndex.Attributes.SimpleIndexAttribute">SimpleIndexAttribute</see>.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity">entity</paramref> is null.</exception>
        public virtual async Task DeleteAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if(IndexKey.HasIndexedProperties<T>())
                foreach (var propertyInfo in IndexKey.GetIndexedPropertyInfos<T>())
                    await IndexData.DeleteAsync(entity, propertyInfo);

            await TableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey, ETag.All);
        }

        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IEnumerable<T>> GetAsync() =>
            await TableClient.QueryAsync<T>().AsEnumerableAsync();

        /// <summary>
        /// Page through all entities.
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="continuationToken"></param>
        /// <returns></returns>
        public virtual async Task<PagedResult<T>> PageAsync(int? pageSize = null, string continuationToken = null) =>
            await PagedQueryAsync(null, pageSize, continuationToken);

        /// <summary>
        /// Get all entities with the specified <paramref name="partitionKey">PartitionKey</paramref>.
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<T>> GetAsync(string partitionKey) =>
            await TableClient.QueryAsync<T>($"PartitionKey eq '{partitionKey}'").AsEnumerableAsync();

        /// <summary>
        /// Page through entities with the specified <paramref name="partitionKey">PartitionKey</paramref>.
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="pageSize"></param>
        /// <param name="continuationToken"></param>
        /// <returns></returns>
        public virtual async Task<PagedResult<T>> PageAsync(string partitionKey, int? pageSize = null, string continuationToken = null) =>
            await PagedQueryAsync($"PartitionKey eq '{partitionKey}'", pageSize, continuationToken);

        /// <summary>
        /// Get an entity by its <paramref name="partitionKey">PartitionKey</paramref> and <paramref name="rowKey">RowKey</paramref>.
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        /// <returns></returns>
        public virtual async Task<T> GetAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await TableClient.GetEntityAsync<T>(partitionKey, rowKey, default(IEnumerable<string>), default(CancellationToken));
                if (response != null)
                    return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
            { /* This is a non-exception. It just means the record was not found. */ }

            return null;
        }

        /// <summary>
        /// Get all entities that match the indexed property name and value.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Thrown if the propertyName is null, an empty string, or is all white space characters.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the property either does not exist or is not decorated with the <see cref="CheesyTot.AzureTables.SimpleIndex.Attributes.SimpleIndexAttribute">SimpleIndexAttribute</see>.</exception>
        public virtual async Task<IEnumerable<T>> GetByIndexedPropertyAsync(string propertyName, object propertyValue)
        {
            if(IndexKey.HasIndexedProperties<T>())
            {
                var indexKey = IndexKey.GetIndexKey<T>(propertyName, propertyValue);

                var indexes = await IndexData.GetAllIndexesAsync(indexKey);
                if (indexes == null || !indexes.Any())
                    return Enumerable.Empty<T>();

                return await GetByIndexesAsync(indexes);
            }

            return Enumerable.Empty<T>();
        }

        /// <summary>
        /// Page through the entities that match the indexed property name and value.
        /// </summary>
        /// <remarks>The property must be decorated with the <see cref="CheesyTot.AzureTables.SimpleIndex.Attributes.SimpleIndexAttribute">SimpleIndexAttribute</see>.</remarks>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <param name="pageSize"></param>
        /// <param name="continuationToken"></param>
        /// <returns></returns>
        public virtual async Task<PagedResult<T>> PageByIndexedPropertyAsync(string propertyName, object propertyValue, int? pageSize = null, string continuationToken = null)
        {
            if(IndexKey.HasIndexedProperties<T>())
            {
                var indexKey = IndexKey.GetIndexKey<T>(propertyName, propertyValue);

                var indexes = await IndexData.PageIndexes(indexKey, pageSize, continuationToken);
                if (indexes == null || !indexes.Results.Any())
                    return PagedResult<T>.Empty;

                return new PagedResult<T>
                {
                    ContinuationToken = indexes.ContinuationToken,
                    Results = await GetByIndexesAsync(indexes.Results)
                };
            }

            return PagedResult<T>.Empty;
        }

        /// <summary>
        /// Gets a single entity that matches the indexed property name and value, or throws an exception if there are no matches, or if there are more than one match.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Thrown if the propertyName is null, an empty string, or is all white space characters.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the property either does not exist or is not decorated with the <see cref="CheesyTot.AzureTables.SimpleIndex.Attributes.SimpleIndexAttribute">SimpleIndexAttribute</see>.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if there are no matches, or if there are more than one match.</exception>
        public virtual async Task<T> GetSingleByIndexedPropertyAsync(string propertyName, object propertyValue)
        {
            if(IndexKey.HasIndexedProperties<T>())
            {
                var indexKey = IndexKey.GetIndexKey<T>(propertyName, propertyValue);
                var index = await IndexData.GetSingleIndexAsync(indexKey);
                return await GetAsync(index.EntityKey.PartitionKey, index.EntityKey.RowKey);
            }

            return default;
        }

        /// <summary>
        /// Gets a single entity that matches the indexed property name and value, returns null if there are no matches, or throws an exception if there are more than one match.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Thrown if the propertyName is null, an empty string, or is all white space characters.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the property either does not exist or is not decorated with the <see cref="CheesyTot.AzureTables.SimpleIndex.Attributes.SimpleIndexAttribute">SimpleIndexAttribute</see>.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if there are no matches.</exception>
        public virtual async Task<T> GetSingleOrDefaultByIndexedPropertyAsync(string propertyName, object propertyValue)
        {
            if(IndexKey.HasIndexedProperties<T>())
            {
                var indexKey = IndexKey.GetIndexKey<T>(propertyName, propertyValue);

                var index = await IndexData.GetSingleIndexOrDefaultAsync(indexKey);
                if (index == null)
                    return null;

                return await GetAsync(index.EntityKey.PartitionKey, index.EntityKey.RowKey);
            }

            return default;
        }

        /// <summary>
        /// Gets the first entity that matches the indexed property name and value, or throw an exception if there are no matches.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Thrown if the propertyName is null, an empty string, or is all white space characters.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the property either does not exist or is not decorated with the <see cref="CheesyTot.AzureTables.SimpleIndex.Attributes.SimpleIndexAttribute">SimpleIndexAttribute</see>.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if there are no matches.</exception>
        public virtual async Task<T> GetFirstByIndexedPropertyAsync(string propertyName, object propertyValue)
        {
            if(IndexKey.HasIndexedProperties<T>())
            {
                var indexKey = IndexKey.GetIndexKey<T>(propertyName, propertyValue);

                var index = await IndexData.GetFirstIndexAsync(indexKey);
                if (index == null)
                    return null;

                return await GetAsync(index.EntityKey.PartitionKey, index.EntityKey.RowKey);
            }

            return default;
        }

        /// <summary>
        /// Gets the first entity that matches the indexed property name and value, or null if there are no matches.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Thrown if the propertyName is null, an empty string, or is all white space characters.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the property either does not exist or is not decorated with the <see cref="CheesyTot.AzureTables.SimpleIndex.Attributes.SimpleIndexAttribute">SimpleIndexAttribute</see>.</exception>
        public virtual async Task<T> GetFirstOrDefaultByIndexedPropertyAsync(string propertyName, object propertyValue)
        {
            var indexKey = IndexKey.GetIndexKey<T>(propertyName, propertyValue);

            var index = await IndexData.GetFirstIndexOrDefaultAsync(indexKey);
            if (index == null)
                return null;

            return await GetAsync(index.EntityKey.PartitionKey, index.EntityKey.RowKey);
        }

        /// <summary>
        /// Performs an entity query.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<T>> QueryAsync(string filter)
        {
            return await TableClient.QueryAsync<T>(filter, default, default, CancellationToken.None).AsEnumerableAsync();
        }

        /// <summary>
        /// Pages through an entity query.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pageSize"></param>
        /// <param name="continuationToken"></param>
        /// <returns></returns>
        public virtual async Task<PagedResult<T>> PagedQueryAsync(string filter, int? pageSize = null, string continuationToken = null)
        {
            var result = PagedResult<T>.Empty;
            var pSize = pageSize ?? _defaultPageSize;
            var pageable = TableClient.QueryAsync<T>(filter, pSize, null, CancellationToken.None);

            var pages = string.IsNullOrWhiteSpace(continuationToken)
                ? pageable.AsPages()
                : pageable.AsPages(continuationToken);

            var page = await pages.FirstOrDefault();

            if(page != null)
                result = new PagedResult<T>
                {
                    ContinuationToken = page.ContinuationToken,
                    Results = page.Values.AsEnumerable()
                };

            return result;
        }

        /// <summary>
        /// Update an entity and replace any indexes for any changed properties that are decorated with the <see cref="CheesyTot.AzureTables.SimpleIndex.Attributes.SimpleIndexAttribute.">SimpleIndexAttribute</see>.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="entity">entity</paramref> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="entity">entity</paramref> does not exist.</exception>
        public virtual async Task UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var existing = await GetAsync(entity.PartitionKey, entity.RowKey);
            if (existing == null)
                throw new ArgumentOutOfRangeException($"Entity of Type {nameof(T)} with PartitionKey {entity.PartitionKey} and RowKey {entity.RowKey} does not exist.", nameof(entity));

            if(IndexKey.HasIndexedProperties<T>())
            {
                foreach (var propertyInfo in IndexKey.GetIndexedPropertyInfos<T>())
                    if (propertyInfo.GetValue(existing) != propertyInfo.GetValue(entity))
                        await IndexData.ReplaceAsync(existing, entity, propertyInfo);
            }

            await TableClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace, CancellationToken.None);
        }

        /// <summary>
        /// Gets all entities that match any of the <paramref name="indexes">Indexes</paramref>.
        /// </summary>
        /// <param name="indexes"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetByIndexesAsync(IEnumerable<Index> indexes)
        {
            if (!IndexKey.HasIndexedProperties<T>() || (indexes?.Count() ?? 0) == 0)
                return Enumerable.Empty<T>();

            var result = new List<T>();

            // Apparently performing many point queries is more efficient than
            // OR-ing a bunch of PartitionKeys and RowKeys together and querying
            // on that
            foreach (var index in indexes)
                result.Add(await GetAsync(index.EntityKey.PartitionKey, index.EntityKey.RowKey));

            return result.Where(x => x != null);
        }
    }
}
