using CheesyTot.AzureTables.SimpleIndex.Attributes;
using CheesyTot.AzureTables.SimpleIndex.Entities;

namespace CheesyTot.AzureTables.SimpleIndex.Tests
{
    public class TestEntity : SimpleIndexTableEntity
    {
        public TestEntity() { }

        public TestEntity(string partitionKey, string rowKey)
            : base(partitionKey, rowKey)
        { }

        [SimpleIndex]
        public string IndexedProperty1 { get; set; }

        [SimpleIndex]
        public string IndexedProperty2 { get; set; }

        public string NormalProperty { get; set; }
    }
}
