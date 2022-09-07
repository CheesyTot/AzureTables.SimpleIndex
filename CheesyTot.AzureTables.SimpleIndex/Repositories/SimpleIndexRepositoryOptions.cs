namespace CheesyTot.AzureTables.SimpleIndex.Repositories
{
    public class SimpleIndexRepositoryOptions
    {
        public string StorageConnectionString { get; set; }
        public string TablePrefix { get; set; }
        public string IndexTableSuffix { get; set; }
        public int ChunkSize { get; set; }
    }
}
