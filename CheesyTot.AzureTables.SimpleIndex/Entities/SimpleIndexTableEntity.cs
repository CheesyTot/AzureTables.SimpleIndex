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

        public virtual string PartitionKey { get; set; }
        public virtual string RowKey { get; set; }
        public virtual DateTimeOffset? Timestamp { get; set; }
        public virtual ETag ETag { get; set; }
    }
}
