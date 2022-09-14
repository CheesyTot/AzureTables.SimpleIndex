using Azure;
using Azure.Data.Tables;
using System;
using System.Runtime.Serialization;

namespace CheesyTot.AzureTables.SimpleIndex.Indexing
{
    public class Index : ITableEntity
    {
        public const string SEPARATOR = "|%|";

        public Index() { }

        public Index(IndexKey indexKey, EntityKey entityKey)
        {
            PartitionKey = indexKey.ToString();
            RowKey = entityKey.ToString();
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        [IgnoreDataMember]
        public IndexKey IndexKey => IndexKey.FromString(PartitionKey);

        [IgnoreDataMember]
        public EntityKey EntityKey => EntityKey.FromString(RowKey);

        public override bool Equals(object obj)
        {
            var other = obj as Indexing.Index;

            if (obj == null)
                return false;

            return string.Equals(PartitionKey, other.PartitionKey) && string.Equals(RowKey, other.RowKey);
        }
    }
}
