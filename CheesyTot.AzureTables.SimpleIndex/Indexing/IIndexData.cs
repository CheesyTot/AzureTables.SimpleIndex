using Azure.Data.Tables;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace CheesyTot.AzureTables.SimpleIndex.Indexing
{
    public interface IIndexData<T> where T : ITableEntity, new()
    {
        Task AddAsync(T entity, PropertyInfo propertyInfo);
        Task DeleteAsync(T entity, PropertyInfo propertyInfo);
        Task<IEnumerable<Index>> GetAllIndexesAsync(IndexKey indexKey);
        Task<Index> GetFirstIndexAsync(IndexKey indexKey);
        Task<Index> GetFirstIndexOrDefaultAsync(IndexKey indexKey);
        Task<Index> GetSingleIndexAsync(IndexKey indexKey);
        Task<Index> GetSingleIndexOrDefaultAsync(IndexKey indexKey);
        Task ReplaceAsync(T oldEntity, T newEntity, PropertyInfo propertyInfo);
    }
}