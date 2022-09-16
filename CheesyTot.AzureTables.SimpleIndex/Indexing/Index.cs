using Azure;
using Azure.Data.Tables;
using System;
using System.Runtime.Serialization;

namespace CheesyTot.AzureTables.SimpleIndex.Indexing
{
    /// <summary>
    /// Representation of the entity index that is persisted to storage
    /// </summary>
    public class Index : ITableEntity
    {
        /// <summary>
        /// The separator used to delimit values in the <see cref="IndexKey"/> and <see cref="EntityKey"/>.
        /// </summary>
        public const string SEPARATOR = "|%|";

        /// <summary>
        /// Parameterless constructor that is called when creating the instance from storage
        /// </summary>
        public Index() { }

        /// <summary>
        /// Constructor that takes an <paramref name="indexKey">IndexKey</paramref> and an <paramref name="entityKey">EntityKey</paramref>.
        /// </summary>
        /// <param name="indexKey"></param>
        /// <param name="entityKey"></param>
        public Index(IndexKey indexKey, EntityKey entityKey)
        {
            PartitionKey = indexKey.ToString();
            RowKey = entityKey.ToString();
        }

        /// <summary>
        /// The PartitionKey is the <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.IndexKey.ToString">ToString</see> value of the <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.IndexKey">IndexKey</see>.
        /// </summary>
        public string PartitionKey { get; set; }
 
        /// <summary>
        /// The RowKey is the <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.EntityKey.ToString">ToString</see> value of the <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.RowKey">IndexKey</see>.
        /// </summary>
        public string RowKey { get; set; }

        /// <summary>
        /// The Timestamp property implementation required by <see cref="Azure.Data.Tables.ITableEntity">ITableEntity</see>.
        /// </summary>
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// The ETag property implementation required by <see cref="Azure.Data.Tables.ITableEntity">ITableEntity</see>.
        /// </summary>
        public ETag ETag { get; set; }

        /// <summary>
        /// The <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.IndexKey">IndexKey</see> deserialized from the <see cref="PartitionKey">PartitionKey</see>.
        /// </summary>
        [IgnoreDataMember]
        public IndexKey IndexKey => IndexKey.FromString(PartitionKey);


        /// <summary>
        /// The <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.EntityKey">EntityKey</see> deserialized from the <see cref="RowKey">RowKey</see>.
        /// </summary>
        [IgnoreDataMember]
        public EntityKey EntityKey => EntityKey.FromString(RowKey);

        /// <summary>
        /// Overridden Equals method compares the <see cref="PartitionKey">PartitionKey</see> and <see cref="RowKey">RowKey</see>
        /// </summary>
        /// <param name="obj">The other object being compared</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as Indexing.Index;

            if (obj == null)
                return false;

            return string.Equals(PartitionKey, other.PartitionKey) && string.Equals(RowKey, other.RowKey);
        }

        /// <summary>
        /// Overridden GetHashCode calculates hash based on PartitionKey and RowKey.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hash = 37;
            unchecked
            {
                hash = 29 * hash ^ PartitionKey.GetHashCode();
                hash = 23 * hash ^ RowKey.GetHashCode();
            }
            return hash;
        }
    }
}
