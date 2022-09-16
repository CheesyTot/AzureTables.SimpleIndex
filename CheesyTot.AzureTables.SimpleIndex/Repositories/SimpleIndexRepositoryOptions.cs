namespace CheesyTot.AzureTables.SimpleIndex.Repositories
{
    public class SimpleIndexRepositoryOptions
    {
        /// <summary>
        /// The connection string for the Azure Storage Account to use with Table Storage
        /// </summary>
        public string StorageConnectionString { get; set; }

        /// <summary>
        /// The prefix prepended to each table name
        /// </summary>
        /// <remarks>
        /// Used in cases where a single storage account may be used for multiple applications.
        /// </remarks>
        public string TablePrefix { get; set; }

        /// <summary>
        /// The suffix appended to each index table name
        /// </summary>
        public string IndexTableSuffix { get; set; }
    }
}
