using System;

namespace CheesyTot.AzureTables.SimpleIndex.Attributes
{
    /// <summary>
    /// A property attribute used to mark a property to be indexed by the SimpleIndexRepository implementation for its class.
    /// </summary>
    /// <example>
    /// <code>
    ///     public class Cat : ITableEntity
    ///     {
    ///         public Cat(string partitionKey, string rowKey)
    ///         { }
    ///     
    ///         public string PartitionKey { get; set; }
    ///         public string RowKey { get; set; }
    ///         public DateTimeOffset? Timestamp { get; set; }
    ///         public ETag ETag { get; set; }
    ///     
    ///         [SimpleIndex]
    ///         public string Breed { get; set; }
    ///     
    ///         public string Name { get; set; }
    ///     }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SimpleIndexAttribute : Attribute
    { }
}
