using Azure.Data.Tables;
using CheesyTot.AzureTables.SimpleIndex.Repositories;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace CheesyTot.AzureTables.SimpleIndex.Indexing
{
    /// <summary>
    /// The <c>IIndexData</c> interface defines the data access methods for the indexing functionality
    /// </summary>
    /// <typeparam name="T">The implementation type of the <see cref="Azure.Data.Tables.ITableEntity">ITableEntity</see> that is indexed.</typeparam>
    public interface IIndexData<T> where T : class, ITableEntity, new()
    {
        /// <summary>
        /// Add an <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.Index">Index</see> record.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        Task AddAsync(T entity, PropertyInfo propertyInfo);

        /// <summary>
        /// Delete an <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.Index">Index</see> record.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        Task DeleteAsync(T entity, PropertyInfo propertyInfo);

        /// <summary>
        /// Retrieve all <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.Index">Index</see> records that match the the specified <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.IndexKey">IndexKey</see>.
        /// </summary>
        /// <param name="indexKey"></param>
        /// <returns></returns>
        Task<IEnumerable<Index>> GetAllIndexesAsync(IndexKey indexKey);

        /// <summary>
        /// Page through <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.Index">Index</see> records that match the the specified <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.IndexKey">IndexKey</see>.
        /// </summary>
        /// <param name="indexKey"></param>
        /// <param name="pageSize"></param>
        /// <param name="continuationToken"></param>
        /// <returns></returns>
        Task<PagedResult<Index>> PageIndexes(IndexKey indexKey, int? pageSize = null, string continuationToken = null);

        /// <summary>
        /// Retrieve the first <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.Index">Index</see> record that matches the the specified <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.IndexKey">IndexKey</see> or throw an <see cref="System.InvalidOperationException">InvalidOperationException</see> if none is found.
        /// </summary>
        /// <param name="indexKey"></param>
        /// <returns></returns>
        Task<Index> GetFirstIndexAsync(IndexKey indexKey);

        /// <summary>
        /// Retrieve the first <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.Index">Index</see> record that matches the the specified <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.IndexKey">IndexKey</see> or null if none is found.
        /// </summary>
        /// <param name="indexKey"></param>
        /// <returns></returns>
        Task<Index> GetFirstIndexOrDefaultAsync(IndexKey indexKey);

        /// <summary>
        /// Retrieve a single <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.Index">Index</see> record that matches the the specified <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.IndexKey">IndexKey</see> or throw and <see cref="System.InvalidOperationException">InvalidOperationException</see> if either none or more than one are found.
        /// </summary>
        /// <param name="indexKey"></param>
        /// <returns></returns>
        Task<Index> GetSingleIndexAsync(IndexKey indexKey);

        /// <summary>
        /// Retrieve a single <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.Index">Index</see> record that matches the the specified <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.IndexKey">IndexKey</see>, null if none is found, or throw an <see cref="System.InvalidOperationException">InvalidOperationException</see> if more than one is found.
        /// </summary>
        /// <param name="indexKey"></param>
        /// <returns></returns>
        Task<Index> GetSingleIndexOrDefaultAsync(IndexKey indexKey);

        /// <summary>
        /// Replace an existing <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.Index">Index</see> record
        /// </summary>
        /// <param name="oldEntity"></param>
        /// <param name="newEntity"></param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        Task ReplaceAsync(T oldEntity, T newEntity, PropertyInfo propertyInfo);
    }
}