using Azure;
using Azure.Data.Tables;
using CheesyTot.AzureTables.SimpleIndex.Indexing;
using Moq;
using System.Reflection;

namespace CheesyTot.AzureTables.SimpleIndex.Tests
{
    [TestClass]
    public class IndexingTests
    {
        private static readonly string[] _partitionKeys = new[]
        {
            "18573fbc-c57f-4057-9301-d8d01779421c",
            "15712ce7-6aa9-49c8-a75e-732fc5dd8c1d",
            "cc7b4f26-e6de-4855-bb4a-702fbb41f757"
        };

        private static readonly string[] _rowKeys = new[]
        {
            "1fee6042-b6f7-4998-be05-1cd6fcafdad1",
            "c26a4eac-7815-477c-9f65-15d5c8b5f8bf",
            "6539c254-4146-4cd6-9024-494f4df5b492"
        };

        private static readonly string _invalidIndexValue = "Prop/erty\\name#forwhat?queue\tnothing\r\nflippy";
        private static readonly string _sanitizedIndexValue = "Prop*erty*name*forwhat*queue*nothing**flippy";

        private static readonly TestEntity[] _testEntities = new[]
        {
            new TestEntity(_partitionKeys[0], _rowKeys[0]) { IndexedProperty1 = _invalidIndexValue, IndexedProperty2 = "BCD234", NormalProperty = "CDE345" },
            new TestEntity(_partitionKeys[1], _rowKeys[1]) { IndexedProperty1 = _invalidIndexValue, IndexedProperty2 = "DEF456", NormalProperty = "EFG567" },
            new TestEntity(_partitionKeys[2], _rowKeys[2]) { IndexedProperty1 = "FooberDoober", IndexedProperty2 = "FGH678", NormalProperty = "GHI789" }
        };

        private static readonly IndexKey[] _indexKeys = new[]
        {
            new IndexKey(nameof(TestEntity.IndexedProperty1), _testEntities[0].IndexedProperty1),
            new IndexKey(nameof(TestEntity.IndexedProperty1), _testEntities[1].IndexedProperty1),
            new IndexKey(nameof(TestEntity.IndexedProperty1), _testEntities[2].IndexedProperty1),
            new IndexKey(nameof(TestEntity.IndexedProperty1), "NotGonnaWork")
        };

        private static readonly EntityKey[] _entityKeys = new[]
        {
            EntityKey.FromEntity(_testEntities[0]),
            EntityKey.FromEntity(_testEntities[1]),
            EntityKey.FromEntity(_testEntities[2])
        };

        private static readonly Indexing.Index[] _indexes = new[]
        {
            new Indexing.Index(_indexKeys[0], _entityKeys[0]),
            new Indexing.Index(_indexKeys[1], _entityKeys[1]),
            new Indexing.Index(_indexKeys[2], _entityKeys[2])
        };

        private static readonly PropertyInfo _propertyInfo = typeof(TestEntity).GetProperty(nameof(TestEntity.IndexedProperty1));

        private static readonly AsyncPageable<Indexing.Index>[] _asyncPageables = new[]
        {
            AsyncPageable<Indexing.Index>.FromPages(new[] { new TestPage<Indexing.Index>(_indexes.Take(2).ToArray(), null) }),
            AsyncPageable<Indexing.Index>.FromPages(new[] { new TestPage<Indexing.Index>(_indexes.Skip(2).Take(1).ToArray(), null) }),
            AsyncPageable<Indexing.Index>.FromPages(new[] { new TestPage<Indexing.Index>(new Indexing.Index[0], null)})
        };

        private Mock<TableClient> _moqTableClient;
        private IIndexData<TestEntity> _indexData;

        [TestInitialize]
        public void TestInitialize()
        {
            _moqTableClient = new Mock<TableClient>();
            _indexData = new IndexData<TestEntity>(_moqTableClient.Object);
            _moqTableClient.Setup(x => x.QueryAsync<Indexing.Index>($"PartitionKey eq '{_indexKeys[0]}'", default(int?), default(IEnumerable<string>), default(CancellationToken))).Returns(_asyncPageables[0]);
            _moqTableClient.Setup(x => x.QueryAsync<Indexing.Index>($"PartitionKey eq '{_indexKeys[2]}'", default(int?), default(IEnumerable<string>), default(CancellationToken))).Returns(_asyncPageables[1]);
            _moqTableClient.Setup(x => x.QueryAsync<Indexing.Index>($"PartitionKey eq '{_indexKeys[3]}'", default(int?), default(IEnumerable<string>), default(CancellationToken))).Returns(_asyncPageables[2]);
        }

        [TestMethod("IndexKey: Invalid characters are sanitized for PropertyValue")]
        public void IndexKey_InvalidCharactersAreSanitizedForPropertyValue()
        {
            Assert.AreEqual(_indexKeys[0].PropertyValue, _sanitizedIndexValue);
        }

        [TestMethod("IndexKey: Invalid characters are sanitized for ToString()")]
        public void IndexKey_InvalidCharactersAreSanitizedForToString()
        {
            var testVal = $"{nameof(TestEntity.IndexedProperty1)}{Indexing.Index.SEPARATOR}{_sanitizedIndexValue}";
            Assert.AreEqual(_indexKeys[0].ToString(), testVal);
        }

        [TestMethod("IndexKey: Invalid characters are sanitized for FromString()")]
        public void IndexKey_InvalidCharactersAreSanitizedForFromString()
        {
            var inStr = $"{nameof(TestEntity.IndexedProperty1)}{Indexing.Index.SEPARATOR}{_invalidIndexValue}";
            var indexKey = IndexKey.FromString(inStr);
            Assert.AreEqual(_sanitizedIndexValue, indexKey.PropertyValue);
        }

        [TestMethod("EntityKey: FromEntity is correct")]
        public void EntityKey_FromEntityIsCorrect()
        {
            var testVal = EntityKey.FromEntity(_testEntities[0]);
            Assert.AreEqual(_partitionKeys[0], testVal.PartitionKey);
            Assert.AreEqual(_rowKeys[0], testVal.RowKey);
        }

        [TestMethod("EntityKey: FromString returns default if input string is null")]
        public void EntityKey_FromStringReturnsDefaultIfInputIsNull()
        {
            Assert.IsNull(EntityKey.FromString(null));
        }

        [TestMethod("EntityKey: FromString returns default if input string is empty")]
        public void EntityKey_FromStringReturnsDefaultIfInputIsEmpty()
        {
            Assert.IsNull(EntityKey.FromString(String.Empty));
        }

        [TestMethod("EntityKey: FromString returns default if input string is all white space")]
        public void EntityKey_FromStringReturnsDefaultIfInputIsAllWhiteSpace()
        {
            Assert.IsNull(EntityKey.FromString("     "));
        }

        [TestMethod("EntityKey: FromString returns default if input string not formatted correctly")]
        public void EntityKey_FromStringReturnsDefaultIfInputNotFormattedCorrectly()
        {
            Assert.IsNull(EntityKey.FromString(_partitionKeys[0]));
        }

        [TestMethod("EntityKey: ToString is correct")]
        public void EntityKey_ToStringIsCorrect()
        {
            var testVal = EntityKey.FromString($"{_testEntities[0].PartitionKey}{Indexing.Index.SEPARATOR}{_testEntities[0].RowKey}");
            Assert.AreEqual(_partitionKeys[0], testVal.PartitionKey);
            Assert.AreEqual(_rowKeys[0], testVal.RowKey);
        }

        [TestMethod("Index: PartitionKey is IndexKey.ToString")]
        public void Index_PartitionKeyIsIndexKeyToString()
        {
            Assert.AreEqual(_indexes[0].PartitionKey, _indexKeys[0].ToString());
        }

        [TestMethod("Index: RowKey is EntityKey.ToString")]
        public void Index_RowKeyIsEntityKeyToString()
        {
            Assert.AreEqual(_indexes[0].RowKey, _entityKeys[0].ToString());
        }

        [TestMethod("Index: IndexKey matches")]
        public void Index_IndexKeyMatches()
        {
            Assert.AreEqual(_indexes[0].IndexKey, _indexKeys[0]);
        }

        [TestMethod("Index: EntityKey matches")]
        public void Index_EntityKeyMatches()
        {
            Assert.AreEqual(_indexes[0].EntityKey, _entityKeys[0]);
        }

        [TestMethod("IndexData: AddAsync throws if entity is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task IndexData_AddAsyncThrowsIfEntityIsNull()
        {
            await _indexData.AddAsync(null, _propertyInfo);
        }

        [TestMethod("IndexData: AddAsync throws if propertyInfo is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task IndexData_AddAsyncThrowsIfPropertyInfoIsNull()
        {
            await _indexData.AddAsync(_testEntities[0], null);
        }

        [TestMethod("IndexData: AddAsync calls TableClient.AddEntityAsync")]
        public async Task IndexData_AddAsyncCallsTableClientAddEntityAsync()
        {
            await _indexData.AddAsync(_testEntities[0], _propertyInfo);
            _moqTableClient.Verify(x => x.AddEntityAsync(_indexes[0], CancellationToken.None), Times.Once());
        }

        [TestMethod("IndexData: DeleteAsync throws if entity is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task IndexData_DeleteAsyncThrowsIfEntityIsNull()
        {
            await _indexData.DeleteAsync(null, _propertyInfo);
        }

        [TestMethod("IndexData: DeleteAsync throws if propertyInfo is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task IndexData_DeleteAsyncThrowsIfPropertyInfoIsNull()
        {
            await _indexData.DeleteAsync(_testEntities[0], null);
        }

        [TestMethod("IndexData: DeleteAsync calls TableClient.DeleteEntityAsync")]
        public async Task IndexData_DeleteAsyncCallsTableClientDeleteEntityAsync()
        {
            await _indexData.DeleteAsync(_testEntities[0], _propertyInfo);
            _moqTableClient.Verify(x => x.DeleteEntityAsync(_indexes[0].PartitionKey, _indexes[0].RowKey, Azure.ETag.All, CancellationToken.None), Times.Once());
        }

        [TestMethod("IndexData: ReplaceAsync throws if oldEntity is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task IndexData_ReplaceAsyncThrowsIfOldEntityIsNull()
        {
            await _indexData.ReplaceAsync(null, _testEntities[2], _propertyInfo);
        }

        [TestMethod("IndexData: ReplaceAsync throws if newEntity is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task IndexData_ReplaceAsyncThrowsIfNewEntityIsNull()
        {
            await _indexData.ReplaceAsync(_testEntities[0], null, _propertyInfo);
        }

        [TestMethod("IndexData: ReplaceAsync throws if propertyInfo is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task IndexData_ReplaceAsyncThrowsIfPropertyInfoIsNull()
        {
            await _indexData.ReplaceAsync(_testEntities[0], _testEntities[2], null);
        }

        [TestMethod("IndexData: ReplaceAsync calls TableClient.DeleteEntityAsync")]
        public async Task IndexData_ReplaceAsyncCallsTableClientDeleteEntityAsync()
        {
            await _indexData.ReplaceAsync(_testEntities[0], _testEntities[2], _propertyInfo);
            _moqTableClient.Verify(x => x.DeleteEntityAsync(_indexes[0].PartitionKey, _indexes[0].RowKey, Azure.ETag.All, CancellationToken.None), Times.Once());
        }

        [TestMethod("IndexData: ReplaceAsync calls TableClient.AddEntityAsync")]
        public async Task IndexData_ReplaceAsyncCallsTableClientAddEntityAsync()
        {
            await _indexData.ReplaceAsync(_testEntities[0], _testEntities[2], _propertyInfo);
            _moqTableClient.Verify(x => x.AddEntityAsync(_indexes[2], CancellationToken.None), Times.Once());
        }

        [TestMethod("IndexData: GetAllIndexesAsync calls TableClient.Query<Index>")]
        public async Task IndexData_GetAllIndexesAsyncCallsTableClientQuery()
        {
            await _indexData.GetAllIndexesAsync(_indexKeys[0]);
            _moqTableClient.Verify(x => x.QueryAsync<Indexing.Index>($"PartitionKey eq '{_indexKeys[0]}'", null, null, CancellationToken.None), Times.Once);
        }

        [TestMethod("IndexData: GetFirstIndexAsync calls TableClient.Query<Index>")]
        public async Task IndexData_GetFirstIndexAsyncCallsTableClientQuery()
        {
            await _indexData.GetFirstIndexAsync(_indexKeys[0]);
            _moqTableClient.Verify(x => x.QueryAsync<Indexing.Index>($"PartitionKey eq '{_indexKeys[0]}'", null, null, CancellationToken.None), Times.Once);
        }

        [TestMethod("IndexData: GetFirstIndexAsync throws if the sequence is empty")]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task IndexData_GetFirstIndexAsyncThrowsIfSequenceIsEmpty()
        {
            await _indexData.GetFirstIndexAsync(_indexKeys[3]);
        }

        [TestMethod("IndexData: GetFirstIndexOrDefaultAsync calls TableClient.Query<Index>")]
        public async Task IndexData_GetFirstIndexOrDefaultAsyncCallsTableClientQuery()
        {
            await _indexData.GetFirstIndexOrDefaultAsync(_indexKeys[0]);
            _moqTableClient.Verify(x => x.QueryAsync<Indexing.Index>($"PartitionKey eq '{_indexKeys[0]}'", null, null, CancellationToken.None), Times.Once);
        }

        [TestMethod("IndexData: GetSingleAsync throws if more than one object in the sequence")]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task IndexData_GetSingleAsyncThrowsIfMoreThanOneObjectInSequence()
        {
            await _indexData.GetSingleIndexAsync(_indexKeys[0]);
        }

        [TestMethod("IndexData: GetSingleIndexAsync calls TableClient.Query<Index>")]
        public async Task IndexData_GetSingleIndexAsyncCallsTableClientQuery()
        {
            await _indexData.GetSingleIndexAsync(_indexKeys[2]);
            _moqTableClient.Verify(x => x.QueryAsync<Indexing.Index>($"PartitionKey eq '{_indexKeys[2]}'", null, null, CancellationToken.None), Times.Once);
        }

        [TestMethod("IndexData: GetSingleIndexAsync throws if the sequence is empty")]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task IndexData_GetSingleIndexAsyncThrowsIfSequenceIsEmpty()
        {
            await _indexData.GetSingleIndexAsync(_indexKeys[3]);
        }

        [TestMethod("IndexData: GetSingleIndexOrDefaultAsync calls TableClient.Query<Index>")]
        public async Task IndexData_GetSingleIndexOrDefaultAsyncCallsTableClientQuery()
        {
            await _indexData.GetSingleIndexOrDefaultAsync(_indexKeys[2]);
            _moqTableClient.Verify(x => x.QueryAsync<Indexing.Index>($"PartitionKey eq '{_indexKeys[2]}'", null, null, CancellationToken.None), Times.Once);
        }

        [TestMethod("IndexData: GetSingleOrDefaultAsync throws if more than one object in the sequence")]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task IndexData_GetSingleIndexOrDefaultAsyncThrowsIfMoreThanOneObjectInSequence()
        {
            await _indexData.GetSingleIndexOrDefaultAsync(_indexKeys[0]);
        }
    }
}
