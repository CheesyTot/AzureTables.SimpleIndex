using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CheesyTot.AzureTables.SimpleIndex.Repositories;
using CheesyTot.AzureTables.SimpleIndex.Extensions;

namespace CheesyTot.AzureTables.SimpleIndex.Indexing
{
    public class IndexData<T> where T : ITableEntity, new()
    {
        private TableClient _tableClient;

        public IndexData(SimpleIndexRepositoryOptions options)
        {
            _tableClient = new TableClient(
                options.StorageConnectionString,
                $"{options.TablePrefix}{typeof(T).Name}{options.IndexTableSuffix}"
            );

            _tableClient.CreateIfNotExists();
        }

        public async Task AddAsync(T entity, PropertyInfo propertyInfo)
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

        public async Task DeleteAsync(T entity, PropertyInfo propertyInfo)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            var propertyValue = Convert.ToString(propertyInfo.GetValue(entity));
            var indexKey = new IndexKey(propertyInfo.Name, propertyValue);

            await _tableClient.DeleteEntityAsync(indexKey.ToString(), EntityKey.FromEntity(entity).ToString());
        }

        public async Task ReplaceAsync(T oldEntity, T newEntity, PropertyInfo propertyInfo)
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
            await _tableClient.DeleteEntityAsync(oldIndexKey.ToString(), EntityKey.FromEntity(oldEntity).ToString());

            // ADD THE NEW INDEX
            var newPropertyValue = Convert.ToString(propertyInfo.GetValue(newEntity));
            var newIndexKey = new IndexKey(propertyInfo.Name, newPropertyValue);
            var newIndex = new Index(newIndexKey, EntityKey.FromEntity(newEntity));
            await _tableClient.AddEntityAsync(newIndex);
        }

        public async Task<IEnumerable<Index>> GetAllIndexesAsync(IndexKey indexKey)
        {
            return await _tableClient.QueryAsync<Index>($"PartitionKey eq '{indexKey}'").AsEnumerableAsync();
        }

        public async Task<Index> GetFirstIndexAsync(IndexKey indexKey)
        {
            return await _tableClient.QueryAsync<Index>($"PartitionKey eq '{indexKey}'").FirstAsync();
        }

        public async Task<Index> GetFirstIndexOrDefaultAsync(IndexKey indexKey)
        {
            return await _tableClient.QueryAsync<Index>($"PartitionKey eq '{indexKey}'").FirstOrDefaultAsync();
        }

        public async Task<Index> GetSingleIndexAsync(IndexKey indexKey)
        {
            return await _tableClient.QueryAsync<Index>($"PartitionKey eq '{indexKey}'").SingleAsync();
        }

        public async Task<Index> GetSingleIndexOrDefaultAsync(IndexKey indexKey)
        {
            return await _tableClient.QueryAsync<Index>($"PartitionKey eq '{indexKey}'").SingleOrDefaultAsync();
        }
    }
}
