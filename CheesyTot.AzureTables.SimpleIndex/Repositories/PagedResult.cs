using System.Collections.Generic;
using System.Linq;
using Azure.Data.Tables;

namespace CheesyTot.AzureTables.SimpleIndex.Repositories
{
    public class PagedResult<T> where T : class, ITableEntity, new()
    {
        public string ContinuationToken { get; set; }
        public IEnumerable<T> Results { get; set; }

        public static PagedResult<T> Empty => new PagedResult<T>
        {
            ContinuationToken = null,
            Results = Enumerable.Empty<T>()
        };
    }
}
