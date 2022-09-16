using Azure.Data.Tables;
using System;

namespace CheesyTot.AzureTables.SimpleIndex.Indexing
{
    /// <summary>
    /// The <c>EntityKey</c> represents the key information (<see cref="Azure.Data.Tables.ITableEntity.PartitionKey">PartitionKey</see> and <see cref="Azure.Data.Tables.ITableEntity.RowKey">RowKey</see>) for an Azure Table Storage entity <seealso cref="Azure.Data.Tables.ITableEntity"/> and provides serialization/deserialization functionality critical to indexing.
    /// </summary>
    public class EntityKey
    {
        /// <summary>
        /// Creates an <see cref="EntityKey">EntityKey</see> instance using the specified partitionKey and rowKey.
        /// </summary>
        /// <param name="partitionKey">The <see cref="Azure.Data.Tables.ITableEntity.PartitionKey">PartitionKey</see> of an Azure Table Storage entity <seealso cref="Azure.Data.Tables.ITableEntity"/>.</param>
        /// <param name="rowKey">The <see cref="Azure.Data.Tables.ITableEntity.RowKey">RowKey</see> of an Azure Table Storage entity <seealso cref="Azure.Data.Tables.ITableEntity"/>.</param>
        public EntityKey(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        /// <summary>
        /// The <c>PartitionKey</c> property represents the <see cref="Azure.Data.Tables.ITableEntity.PartitionKey">PartitionKey</see> of an Azure Table Storage entity <seealso cref="Azure.Data.Tables.ITableEntity"/>.
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// The <c>RowKey</c> property represents the <see cref="Azure.Data.Tables.ITableEntity.RowKey">RowKey</see> of an Azure Table Storage entity <seealso cref="Azure.Data.Tables.ITableEntity"/>.
        /// </summary>
        public string RowKey { get; set; }

        /// <summary>
        /// The <c>ToString</c> method is overridden to return a string representation of the <see cref="EntityKey">EntityKey</see> instance, used in <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.Index">Index</see> records.
        /// </summary>
        /// <returns>The values of the <see cref="PartitionKey">PartitionKey</see> and <see cref="RowKey">RowKey</see> properties, separated by the <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.Index.SEPARATOR">Index.SEPARATOR constant</see>.</returns>
        public override string ToString() => $"{PartitionKey}{Index.SEPARATOR}{RowKey}";

        /// <summary>
        /// Creates an <see cref="EntityKey">EntityKey</see> instance from an Azure Table Storage entity <seealso cref="Azure.Data.Tables.ITableEntity"/>.
        /// </summary>
        /// <param name="entity">An Azure Table Storage entity <seealso cref="Azure.Data.Tables.ITableEntity"/>.</param>
        /// <returns>An <see cref="EntityKey">EntityKey</see> instance constructed with the <see cref="Azure.Data.Tables.ITableEntity.PartitionKey">PartitionKey</see> and <see cref="Azure.Data.Tables.ITableEntity.RowKey">RowKey</see> of the <paramref name="entity">entity</paramref> parameter.</returns>
        /// <example>
        /// <code>
        /// var entityKey = EntityKey.FromEntity(entity);
        /// </code>
        /// </example>
        public static EntityKey FromEntity(ITableEntity entity) => entity == null
            ? default
            : new EntityKey(entity.PartitionKey, entity.RowKey);

        /// <summary>
        /// Creates an <see cref="EntityKey">EntityKey</see> instance from a string in the same format as those produced by the <see cref="ToString">ToString()</see> method.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>An <see cref="EntityKey">EntityKey</see> instance constructed from the <paramref name="input">input parameter</paramref>. If the input parameter is null, an empty string, all white space, or not in the correct format, then null is returned.</returns>
        /// <example>
        /// <code>
        /// var entityKey = EntityKey.FromString("PartitionKey<#>RowKey");
        /// </code>
        /// </example>
        public static EntityKey FromString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return default;

            var parts = input.Split(new[] { Index.SEPARATOR }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return default;

            return new EntityKey(parts[0], parts[1]);
        }

        /// <summary>
        /// The <c>Equals</c> method is overridden to allow comparison of <see cref="EntityKey">EntityKey</see> objects based on their serialized values.
        /// </summary>
        /// <param name="obj">The other <see cref="EntityKey">EntityKey</see> object being compared for equality.</param>
        /// <returns>True if the other object is equal to the instance, False if it is not.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is EntityKey))
                return false;

            return string.Equals(ToString(), obj.ToString());
        }

        /// <summary>
        /// The <c>GetHashCode</c> method is overridden to calculate the hashcode based on the <see cref="ToString">ToString()</see> value.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
