using Azure.Data.Tables;
using CheesyTot.AzureTables.SimpleIndex.Indexing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheesyTot.AzureTables.SimpleIndex.Repositories
{
    /// <summary>
    /// Interface that defines the base methods for the indexed repository functionality
    /// </summary>
    /// <typeparam name="T">The implementation type of the <see cref="Azure.Data.Tables.ITableEntity">ITableEntity</see> that is indexed.</typeparam>
    public interface ISimpleIndexRepository<T> where T : class, ITableEntity, new()
    {
        /// <summary>
        /// Add an entity and index records for all entity properties that are decorated with the <see cref="CheesyTot.AzureTables.SimpleIndex.Attributes.SimpleIndexAttribute">SimpleIndexAttribute</see>.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task AddAsync(T entity);

        /// <summary>
        /// Remove an entity and index records for all entity properties that are decorated with the <see cref="CheesyTot.AzureTables.SimpleIndex.Attributes.SimpleIndexAttribute">SimpleIndexAttribute</see>.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAsync();

        /// <summary>
        /// Page through all entities.
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="continuationToken"></param>
        /// <returns></returns>
        Task<PagedResult<T>> PageAsync(int? pageSize = null, string continuationToken = null);

        /// <summary>
        /// Get all entities with the specified <paramref name="partitionKey">PartitionKey</paramref>.
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAsync(string partitionKey);

        /// <summary>
        /// Page through entities with the specified <paramref name="partitionKey">PartitionKey</paramref>.
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="pageSize"></param>
        /// <param name="continuationToken"></param>
        /// <returns></returns>
        Task<PagedResult<T>> PageAsync(string partitionKey, int? pageSize = null, string continuationToken = null);

        /// <summary>
        /// Get an entity by its <paramref name="partitionKey">PartitionKey</paramref> and <paramref name="rowKey">RowKey</paramref>.
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        /// <returns></returns>
        Task<T> GetAsync(string partitionKey, string rowKey);

        /// <summary>
        /// Get all entities that match the indexed property name and value.
        /// </summary>
        /// <remarks>The property must be decorated with the <see cref="CheesyTot.AzureTables.SimpleIndex.Attributes.SimpleIndexAttribute">SimpleIndexAttribute</see>.</remarks>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetByIndexedPropertyAsync(string propertyName, object propertyValue);

        /// <summary>
        /// Page through the entities that match the indexed property name and value.
        /// </summary>
        /// <remarks>The property must be decorated with the <see cref="CheesyTot.AzureTables.SimpleIndex.Attributes.SimpleIndexAttribute">SimpleIndexAttribute</see>.</remarks>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <param name="pageSize"></param>
        /// <param name="continuationToken"></param>
        /// <returns></returns>
        Task<PagedResult<T>> PageByIndexedPropertyAsync(string propertyName, object propertyValue, int? pageSize = null, string continuationToken = null);

        /// <summary>
        /// Gets the first entity that matches the indexed property name and value, or throw an exception if there are no matches.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        Task<T> GetFirstByIndexedPropertyAsync(string propertyName, object propertyValue);

        /// <summary>
        /// Gets the first entity that matches the indexed property name and value, or null if there are no matches.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        Task<T> GetFirstOrDefaultByIndexedPropertyAsync(string propertyName, object propertyValue);

        /// <summary>
        /// Gets a single entity that matches the indexed property name and value, or throws an exception if there are no matches, or if there are more than one match.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        Task<T> GetSingleByIndexedPropertyAsync(string propertyName, object propertyValue);

        /// <summary>
        /// Gets a single entity that matches the indexed property name and value, returns null if there are no matches, or throws an exception if there are more than one match.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        Task<T> GetSingleOrDefaultByIndexedPropertyAsync(string propertyName, object propertyValue);

        /// <summary>
        /// Performs an entity query.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> QueryAsync(string filter);

        /// <summary>
        /// Pages through an entity query.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pageSize"></param>
        /// <param name="continuationToken"></param>
        /// <returns></returns>
        Task<PagedResult<T>> PagedQueryAsync(string filter, int? pageSize = null, string continuationToken = null);

        /// <summary>
        /// Update an entity and replace any indexes for any changed properties that are decorated with the <see cref="CheesyTot.AzureTables.SimpleIndex.Attributes.SimpleIndexAttribute.">SimpleIndexAttribute</see>.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Gets all entities that match any of the <paramref name="indexes">Indexes</paramref>.
        /// </summary>
        /// <param name="indexes"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetByIndexesAsync(IEnumerable<Index> indexes);
    }
}