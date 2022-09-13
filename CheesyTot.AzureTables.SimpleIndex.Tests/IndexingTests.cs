using Azure;
using Azure.Data.Tables;
using CheesyTot.AzureTables.SimpleIndex.Indexing;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CheesyTot.AzureTables.SimpleIndex.Tests
{
    [TestClass]
    public class IndexingTests
    {
        private string _partitionKey;
        private string _rowKey;
        private string _invalidIndexValue;
        private string _sanitizedIndexValue;
        private TestEntity _testEntity;
        private IndexKey _indexKey;
        private EntityKey _entityKey;
        private Indexing.Index _index;
        private Mock<TableClient> _moqTableClient;
        private IIndexData<TestEntity> _indexData;
        private PropertyInfo _propertyInfo;
        private TestEntity _newTestEntity;
        private IndexKey _newIndexKey;
        private Indexing.Index _newIndex;
        private AsyncPageable<Indexing.Index> _asyncPageable;
        private AsyncPageable<Indexing.Index> _singleAsyncPageable;

        [TestInitialize]
        public void TestInitialize()
        {
            _partitionKey = "18573fbc-c57f-4057-9301-d8d01779421c";
            _rowKey = "15712ce7-6aa9-49c8-a75e-732fc5dd8c1d";
            _invalidIndexValue = "Prop/erty\\name#forwhat?queue\tnothing\r\nflippy";
            _sanitizedIndexValue = "Prop*erty*name*forwhat*queue*nothing**flippy";

            _testEntity = new TestEntity(_partitionKey, _rowKey)
            {
                IndexedProperty1 = _invalidIndexValue,
                IndexedProperty2 = "BCD234",
                NormalProperty = "CDE345"
            };

            _indexKey = new IndexKey(nameof(TestEntity.IndexedProperty1), _testEntity.IndexedProperty1);
            _entityKey = EntityKey.FromEntity(_testEntity);
            _index = new Indexing.Index(_indexKey, _entityKey);

            _moqTableClient = new Mock<TableClient>();
            _indexData = new IndexData<TestEntity>(_moqTableClient.Object);

            _propertyInfo = typeof(TestEntity).GetProperty(nameof(TestEntity.IndexedProperty1));
            
            _newTestEntity = new TestEntity(_partitionKey, _rowKey)
            {
                IndexedProperty1 = "FooberDoober",
                IndexedProperty2 = "BCD234",
                NormalProperty = "CDE345"
            };

            _newIndexKey = new IndexKey(nameof(TestEntity.IndexedProperty1), _newTestEntity.IndexedProperty1);
            _newIndex = new Indexing.Index(_newIndexKey, _entityKey);

            _asyncPageable = AsyncPageable<Indexing.Index>.FromPages(new[]
{
                new TestPage<Indexing.Index>(new[] { _index, _newIndex }, Guid.NewGuid().ToString())
            });

            _singleAsyncPageable = AsyncPageable<Indexing.Index>.FromPages(new[]
            {
                new TestPage<Indexing.Index>(new[] { _newIndex }, Guid.NewGuid().ToString())
            });

            _moqTableClient.Setup(x => x.QueryAsync<Indexing.Index>($"PartitionKey eq '{_indexKey}'", default(int?), default(IEnumerable<string>), default(CancellationToken))).Returns(_asyncPageable);
            _moqTableClient.Setup(x => x.QueryAsync<Indexing.Index>($"PartitionKey eq '{_newIndexKey}'", default(int?), default(IEnumerable<string>), default(CancellationToken))).Returns(_singleAsyncPageable);
        }

        [TestMethod("IndexKey: Invalid characters are sanitized for PropertyValue")]
        public void IndexKey_InvalidCharactersAreSanitizedForPropertyValue()
        {
            Assert.AreEqual(_indexKey.PropertyValue, _sanitizedIndexValue);
        }

        [TestMethod("IndexKey: Invalid characters are sanitized for ToString()")]
        public void IndexKey_InvalidCharactersAreSanitizedForToString()
        {
            var testVal = $"{nameof(TestEntity.IndexedProperty1)}{Indexing.Index.SEPARATOR}{_sanitizedIndexValue}";
            Assert.AreEqual(_indexKey.ToString(), testVal);
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
            var testVal = EntityKey.FromEntity(_testEntity);
            Assert.AreEqual(_partitionKey, testVal.PartitionKey);
            Assert.AreEqual(_rowKey, testVal.RowKey);
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
            Assert.IsNull(EntityKey.FromString(_partitionKey));
        }

        [TestMethod("EntityKey: ToString is correct")]
        public void EntityKey_ToStringIsCorrect()
        {
            var testVal = EntityKey.FromString($"{_testEntity.PartitionKey}{Indexing.Index.SEPARATOR}{_testEntity.RowKey}");
            Assert.AreEqual(_partitionKey, testVal.PartitionKey);
            Assert.AreEqual(_rowKey, testVal.RowKey);
        }

        [TestMethod("Index: PartitionKey is IndexKey.ToString")]
        public void Index_PartitionKeyIsIndexKeyToString()
        {
            Assert.AreEqual(_index.PartitionKey, _indexKey.ToString());
        }

        [TestMethod("Index: RowKey is EntityKey.ToString")]
        public void Index_RowKeyIsEntityKeyToString()
        {
            Assert.AreEqual(_index.RowKey, _entityKey.ToString());
        }

        [TestMethod("Index: IndexKey matches")]
        public void Index_IndexKeyMatches()
        {
            Assert.AreEqual(_index.IndexKey, _indexKey);
        }

        [TestMethod("Index: EntityKey matches")]
        public void Index_EntityKeyMatches()
        {
            Assert.AreEqual(_index.EntityKey, _entityKey);
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
            await _indexData.AddAsync(_testEntity, null);
        }

        [TestMethod("IndexData: AddAsync calls TableClient.AddEntityAsync")]
        public async Task IndexData_AddAsyncCallsTableClientAddEntityAsync()
        {
            await _indexData.AddAsync(_testEntity, _propertyInfo);
            _moqTableClient.Verify(x => x.AddEntityAsync(_index, CancellationToken.None), Times.Once());
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
            await _indexData.DeleteAsync(_testEntity, null);
        }

        [TestMethod("IndexData: DeleteAsync calls TableClient.DeleteEntityAsync")]
        public async Task IndexData_DeleteAsyncCallsTableClientDeleteEntityAsync()
        {
            await _indexData.DeleteAsync(_testEntity, _propertyInfo);
            _moqTableClient.Verify(x => x.DeleteEntityAsync(_index.PartitionKey, _index.RowKey, Azure.ETag.All, CancellationToken.None), Times.Once());
        }

        [TestMethod("IndexData: ReplaceAsync throws if oldEntity is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task IndexData_ReplaceAsyncThrowsIfOldEntityIsNull()
        {
            await _indexData.ReplaceAsync(null, _testEntity, _propertyInfo);
        }

        [TestMethod("IndexData: ReplaceAsync throws if newEntity is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task IndexData_ReplaceAsyncThrowsIfNewEntityIsNull()
        {
            await _indexData.ReplaceAsync(_testEntity, null, _propertyInfo);
        }

        [TestMethod("IndexData: ReplaceAsync throws if propertyInfo is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task IndexData_ReplaceAsyncThrowsIfPropertyInfoIsNull()
        {
            await _indexData.ReplaceAsync(_testEntity, _newTestEntity, null);
        }

        [TestMethod("IndexData: ReplaceAsync calls TableClient.DeleteEntityAsync")]
        public async Task IndexData_ReplaceAsyncCallsTableClientDeleteEntityAsync()
        {
            await _indexData.ReplaceAsync(_testEntity, _newTestEntity, _propertyInfo);
            _moqTableClient.Verify(x => x.DeleteEntityAsync(_index.PartitionKey, _index.RowKey, Azure.ETag.All, CancellationToken.None), Times.Once());
        }

        [TestMethod("IndexData: ReplaceAsync calls TableClient.AddEntityAsync")]
        public async Task IndexData_ReplaceAsyncCallsTableClientAddEntityAsync()
        {
            await _indexData.ReplaceAsync(_testEntity, _newTestEntity, _propertyInfo);
            _moqTableClient.Verify(x => x.AddEntityAsync(_newIndex, CancellationToken.None), Times.Once());
        }

        [TestMethod("IndexData: GetAllIndexesAsync calls TableClient.Query<Index>")]
        public async Task IndexData_GetAllIndexesAsyncCallsTableClientQuery()
        {
            await _indexData.GetAllIndexesAsync(_indexKey);
            _moqTableClient.Verify(x => x.QueryAsync<Indexing.Index>($"PartitionKey eq '{_indexKey}'", null, null, CancellationToken.None), Times.Once);
        }

        [TestMethod("IndexData: GetFirstIndexAsync calls TableClient.Query<Index>")]
        public async Task IndexData_GetFirstIndexAsyncCallsTableClientQuery()
        {
            await _indexData.GetFirstIndexAsync(_indexKey);
            _moqTableClient.Verify(x => x.QueryAsync<Indexing.Index>($"PartitionKey eq '{_indexKey}'", null, null, CancellationToken.None), Times.Once);
        }

        [TestMethod("IndexData: GetFirstIndexOrDefaultAsync calls TableClient.Query<Index>")]
        public async Task IndexData_GetFirstIndexOrDefaultAsyncCallsTableClientQuery()
        {
            await _indexData.GetFirstIndexOrDefaultAsync(_indexKey);
            _moqTableClient.Verify(x => x.QueryAsync<Indexing.Index>($"PartitionKey eq '{_indexKey}'", null, null, CancellationToken.None), Times.Once);
        }

        [TestMethod("IndexData: GetSingleIndexAsync calls TableClient.Query<Index>")]
        public async Task IndexData_GetSingleIndexAsyncCallsTableClientQuery()
        {
            await _indexData.GetSingleIndexAsync(_newIndexKey);
            _moqTableClient.Verify(x => x.QueryAsync<Indexing.Index>($"PartitionKey eq '{_newIndexKey}'", null, null, CancellationToken.None), Times.Once);
        }

        [TestMethod("IndexData: GetSingleIndexOrDefaultAsync calls TableClient.Query<Index>")]
        public async Task IndexData_GetSingleIndexOrDefaultAsyncCallsTableClientQuery()
        {
            await _indexData.GetSingleIndexOrDefaultAsync(_newIndexKey);
            _moqTableClient.Verify(x => x.QueryAsync<Indexing.Index>($"PartitionKey eq '{_newIndexKey}'", null, null, CancellationToken.None), Times.Once);
        }
    }
}
