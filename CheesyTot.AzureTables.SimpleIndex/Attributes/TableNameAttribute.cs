using System;

namespace CheesyTot.AzureTables.SimpleIndex.Attributes
{
    /// <summary>
    /// Class Attribute to specify the name used by the repository for entity and index storage.
    /// </summary>
    /// <remarks>
    /// Classes not decorated with this attribute will use the name of the entity class by default.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TableNameAttribute : Attribute
    {
        /// <summary>
        /// Constructor that accepts the name for the table
        /// </summary>
        /// <param name="name">Value may only contain alphanumeric characters, may not start with a numeric character, and is case-insensitive. Total length must be at least 3 characters but no more than 63 characters, inclusive of the TableNamePrefix and IndexTableNameSuffix values set in the repository options.</param>
        public TableNameAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// The name of the table specified in the constructor.
        /// </summary>
        public string Name { get; }
    }
}
