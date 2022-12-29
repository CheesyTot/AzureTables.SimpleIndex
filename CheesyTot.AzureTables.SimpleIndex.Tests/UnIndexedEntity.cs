using Azure;
using Azure.Data.Tables;
using CheesyTot.AzureTables.SimpleIndex.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheesyTot.AzureTables.SimpleIndex.Tests
{
    public class UnIndexedEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string NormalProperty1 { get; set; }
        public string NormalProperty2 { get; set; }
        public string NormalProperty3 { get; set; }
    }
}
