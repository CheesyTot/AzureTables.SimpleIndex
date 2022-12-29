using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CheesyTot.AzureTables.SimpleIndex.Repositories;
using Azure;
using System.Threading;
using CheesyTot.AzureTables.SimpleIndex.Helpers;

namespace CheesyTot.AzureTables.SimpleIndex.Indexing
{
    /// <summary>
    /// Implementation of the <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.IIndexData{T}">IIndexData</see> interface that implements the data access methods for the indexing functionality.
    /// </summary>
    /// <typeparam name="T">The implementation type of the <see cref="Azure.Data.Tables.ITableEntity">ITableEntity</see> that is indexed.</typeparam>
    public class IndexData<T> : IIndexData<T> where T : class, ITableEntity, new()
    {
        private bool _hasIndexedProperties;
        private TableClient _tableClient;

        /// <summary>
        /// Constructor that accepts a <see cref="CheesyTot.AzureTables.SimpleIndex.Repositories.SimpleIndexRepositoryOptions>">SimpleIndexRepositoryOptions</see> parameter.
        /// </summary>
        /// <param name="options"></param>
        public IndexData(SimpleIndexRepositoryOptions options)
        {
            _hasIndexedProperties = IndexKey.HasIndexedProperties<T>();
            if(_hasIndexedProperties)
            {
                _tableClient = new TableClient(
                    options.StorageConnectionString,
                    TableNameHelper.GetTableName<T>(options.TablePrefix, options.IndexTableSuffix)
                );

                _tableClient.CreateIfNotExists();
            }
        }

        /// <summary>
        /// Constructor that accepts a <see cref="Azure.Data.Tables.TableClient">TableClient</see> parameter for unit testing
        /// </summary>
        /// <param name="tableClient"></param>
        public IndexData(TableClient tableClient)
        {
            _tableClient = tableClient;
        }

        /// <summary>
        /// Add an <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.Index">Index</see> record.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task AddAsync(T entity, PropertyInfo propertyInfo)
        {
            if(_tableClient != null)
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                if (propertyInfo == null)
                    throw new ArgumentNullException(nameof(propertyInfo));

                var propertyValue = Convert.ToString(propertyInfo.GetValue(entity));

                var indexKey = new IndexKey(propertyInfo.Name, propertyValue);
                var index = new Index(indexKey, EntityKey.FromEntity(entity));

                await _tableClient.AddEntityAsync(index);
            }
        }

        /// <summary>
        /// Delete an <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.Index">Index</see> record.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity">entity</paramref> parameter or the <paramref name="propertyInfo">propertyInfo</paramref> parameter is null.</exception>
        public async Task DeleteAsync(T entity, PropertyInfo propertyInfo)
        {
            if(_tableClient != null)
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                if (propertyInfo == null)
                    throw new ArgumentNullException(nameof(propertyInfo));

                var propertyValue = Convert.ToString(propertyInfo.GetValue(entity));
                var indexKey = new IndexKey(propertyInfo.Name, propertyValue);

                await _tableClient.DeleteEntityAsync(indexKey.ToString(), EntityKey.FromEntity(entity).ToString(), ETag.All);
            }
        }

        /// <summary>
        /// Replace an existing <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.Index">Index</see> record
        /// </summary>
        /// <param name="oldEntity"></param>
        /// <param name="newEntity"></param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="newEntity">newEntity</paramref>, <paramref name="oldEntity">oldEntity</paramref>, or <paramref name="propertyInfo">propertyInfo</paramref> parameters are null.</exception>
        public async Task ReplaceAsync(T oldEntity, T newEntity, PropertyInfo propertyInfo)
        {
            if(_tableClient != null)
            {
                if (oldEntity == null)
                    throw new ArgumentNullException(nameof(oldEntity));

                if (newEntity == null)
                    throw new ArgumentNullException(nameof(newEntity));

                if (propertyInfo == null)
                    throw new ArgumentNullException(nameof(propertyInfo));

                // DELETE THE OLD INDEX
                var oldPropertyValue = Convert.ToString(propertyInfo.GetValue(oldEntity));
                var oldIndexKey = new IndexKey(propertyInfo.Name, oldPropertyValue);
                await _tableClient.DeleteEntityAsync(oldIndexKey.ToString(), EntityKey.FromEntity(oldEntity).ToString(), ETag.All);

                // ADD THE NEW INDEX
                var newPropertyValue = Convert.ToString(propertyInfo.GetValue(newEntity));
                var newIndexKey = new IndexKey(propertyInfo.Name, newPropertyValue);
                var newIndex = new Index(newIndexKey, EntityKey.FromEntity(newEntity));
                await _tableClient.AddEntityAsync(newIndex);
            }
        }

        /// <summary>
        /// Retrieve all <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.Index">Index</see> records that match the the specified <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.IndexKey">IndexKey</see>.
        /// </summary>
        /// <param name="indexKey"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Index>> GetAllIndexesAsync(IndexKey indexKey)
        {
            if(_tableClient != null)
                return await _tableClient.QueryAsync<Index>($"PartitionKey eq '{indexKey}'").AsEnumerableAsync();

            return default;
        }

        /// <summary>
        /// Retrieve the first <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.Index">Index</see> record that matches the the specified <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.IndexKey">IndexKey</see> or throw an <see cref="System.InvalidOperationException">InvalidOperationException</see> if none is found.
        /// </summary>
        /// <param name="indexKey"></param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Thrown if there are no matches.</exception>
        public async Task<Index> GetFirstIndexAsync(IndexKey indexKey)
        {
            if(_tableClient != null)
            {
                var partitionKey = $"PartitionKey eq '{indexKey}'";
                var foo = _tableClient.QueryAsync<Indexing.Index>(partitionKey, default(int?), default(IEnumerable<string>), default(CancellationToken));
                return await foo.FirstAsync();
            }

            return default;
        }

        /// <summary>
        /// Retrieve the first <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.Index">Index</see> record that matches the the specified <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.IndexKey">IndexKey</see> or null if none is found.
        /// </summary>
        /// <param name="indexKey"></param>
        /// <returns></returns>
        public async Task<Index> GetFirstIndexOrDefaultAsync(IndexKey indexKey)
        {
            if(_tableClient != null)
                return await _tableClient.QueryAsync<Index>($"PartitionKey eq '{indexKey}'", default(int?), default(IEnumerable<string>), default(CancellationToken)).FirstOrDefaultAsync();

            return default;
        }

        /// <summary>
        /// Retrieve a single <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.Index">Index</see> record that matches the the specified <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.IndexKey">IndexKey</see> or throw and <see cref="System.InvalidOperationException">InvalidOperationException</see> if either none or more than one are found.
        /// </summary>
        /// <param name="indexKey"></param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Thrown if there are no matches or if there are more than one match.</exception>
        public async Task<Index> GetSingleIndexAsync(IndexKey indexKey)
        {
            if(_tableClient != null)
            {
                var result = _tableClient.QueryAsync<Index>($"PartitionKey eq '{indexKey}'", default(int?), default(IEnumerable<string>), default(CancellationToken));
                return await result.SingleAsync();
            }

            return default;
        }

        /// <summary>
        /// Retrieve a single <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.Index">Index</see> record that matches the the specified <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.IndexKey">IndexKey</see>, null if none is found, or throw an <see cref="System.InvalidOperationException">InvalidOperationException</see> if more than one is found.
        /// </summary>
        /// <param name="indexKey"></param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Thrown if there is more than one match.</exception>
        public async Task<Index> GetSingleIndexOrDefaultAsync(IndexKey indexKey)
        {
            if(_tableClient != null)
            {
                var partitionKey = $"PartitionKey eq '{indexKey}'";
                var foo = _tableClient.QueryAsync<Index>(partitionKey, default(int?), default(IEnumerable<string>), default(CancellationToken));
                return await foo.SingleOrDefaultAsync();
            }

            return default;
        }
    }
}
