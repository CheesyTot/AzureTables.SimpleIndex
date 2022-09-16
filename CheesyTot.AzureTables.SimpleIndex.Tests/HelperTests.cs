using CheesyTot.AzureTables.SimpleIndex.Helpers;

namespace CheesyTot.AzureTables.SimpleIndex.Tests
{
    [TestClass]
    public class HelperTests
    {
        [TestMethod("TableNameHelper: Table is named after class with prefix and suffix")]
        public void TableNameHelper_NamesTableAfterClassNameWithPrefixAndSuffix()
        {
            var tableName = TableNameHelper.GetTableName<TestEntity>("Test", "Index");
            Assert.AreEqual("TestTestEntityIndex", tableName);
        }

        [TestMethod("TableNameHelper: Table is named using TableName attribute if present")]
        public void TableNameHelper_UsesTableNameAttributeIfPresent()
        {
            var tableName = TableNameHelper.GetTableName<TestEntity2>("Test", "Index");
            Assert.AreEqual("TestOtherTestEntityIndex", tableName);
        }

        [TestMethod("TableNameHelper: Invalid characters are removed from table name")]
        public void TableNameHelper_InvalidCharactersAreRemoved()
        {
            var tableName = TableNameHelper.GetTableName<Test_Entity_3>("Test", "Index");
            Assert.AreEqual("TestTestEntity3Index", tableName);
        }

        [TestMethod("TableNameHelper: Table name is truncated if too long.")]
        public void TableNameHelper_TableNameIsTruncatedIffTooLong()
        {
            var tableName = TableNameHelper.GetTableName<TestEntity4>("Test", "Index");
            Assert.AreEqual("TestAbcdefghijklmnopqrstuvwxyzAbcdefghijklmnopqrstuvwxyzAbIndex", tableName);
        }

        [TestMethod("TableNameHelper: Table name starting with digit is prepended with 'X' if the table name prefix is null, empty, or all white space")]
        public void TableNameHelper_TableNameStartingWIthDigitIsPrependedWithXWhenTablePrefixIsNullOrWhiteSpace()
        {
            var tableName = TableNameHelper.GetTableName<TestEntity5>(null, "Index");
            Assert.AreEqual("X5TestEntityIndex", tableName);
        }

        [TestMethod("TableNameHelper: Table name starting with digit is not prepended with 'X' if the name name prefix is not null, empty, or all white space.")]
        public void TableNameHelper_TableNameStartingWithDigitIsNotPrependedWithXWhenTablePrefixIsNotNullOrWhiteSpace()
        {
            var tableName = TableNameHelper.GetTableName<TestEntity5>("Test", "Index");
            Assert.AreEqual("Test5TestEntityIndex", tableName);
        }
    }
}
