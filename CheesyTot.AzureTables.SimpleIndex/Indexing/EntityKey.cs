using Azure.Data.Tables;
using System;

namespace CheesyTot.AzureTables.SimpleIndex.Indexing
{
    public class EntityKey
    {
        public EntityKey(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public override string ToString() => $"{PartitionKey}{Index.SEPARATOR}{RowKey}";

        public static EntityKey FromEntity(ITableEntity entity) => entity == null
            ? default
            : new EntityKey(entity.PartitionKey, entity.RowKey);

        public static EntityKey FromString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return default;

            var parts = input.Split(new[] { Index.SEPARATOR }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return default;

            return new EntityKey(parts[0], parts[1]);
        }
    }
}
