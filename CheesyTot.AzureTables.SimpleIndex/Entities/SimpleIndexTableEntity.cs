using Azure;
using Azure.Data.Tables;
using System;

namespace CheesyTot.AzureTables.SimpleIndex.Entities
{
    public abstract class SimpleIndexTableEntity : ITableEntity
    {
        public SimpleIndexTableEntity() { }

        public SimpleIndexTableEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
