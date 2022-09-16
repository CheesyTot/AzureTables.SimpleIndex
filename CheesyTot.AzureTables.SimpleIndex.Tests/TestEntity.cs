using Azure;
using Azure.Data.Tables;
using CheesyTot.AzureTables.SimpleIndex.Attributes;

namespace CheesyTot.AzureTables.SimpleIndex.Tests
{
    public class TestEntity : ITableEntity
    {
        public TestEntity() { }

        public TestEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string PartitionKey { get; set;}
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        [SimpleIndex]
        public string IndexedProperty1 { get; set; }

        [SimpleIndex]
        public string IndexedProperty2 { get; set; }

        public string NormalProperty { get; set; }
    }
}
