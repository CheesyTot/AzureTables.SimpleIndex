using Azure;
using Azure.Data.Tables;
using CheesyTot.AzureTables.SimpleIndex.Indexing;
using CheesyTot.AzureTables.SimpleIndex.Repositories;
using Moq;
using System.Reflection;

namespace CheesyTot.AzureTables.SimpleIndex.Tests
{
    [TestClass]
    public class SimpleIndexRepositoryTests
    {
        private Mock<TableClient> _moqTableClient;
        private Mock<IIndexData<TestEntity>> _moqIndexData;
        private ISimpleIndexRepository<TestEntity> _repository;

        [TestInitialize]
        public void TestInit()
        {
            _moqTableClient = new Mock<TableClient>();
            _moqIndexData = new Mock<IIndexData<TestEntity>>();
            _repository = new SimpleIndexRepository<TestEntity>(_moqTableClient.Object, _moqIndexData.Object);
        }

        [TestMethod("AddAsync throws ArgumentNullException if entity argument is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddAsync_ThrowsArgumentNullExceptionIfEntityIsNull()
        {
            await _repository.AddAsync(null);
        }

        [TestMethod("AddAsync calls IndexData.AddAsync for each indexed property")]
        public async Task AddAsync_CallsIndexDataAddAsyncForEachIndexedProperty()
        {
            var entity = getTestEntity();
            await _repository.AddAsync(entity);
            _moqIndexData.Verify(x => x.AddAsync(entity, It.IsAny<PropertyInfo>()), Times.Exactly(2));
        }

        [TestMethod("AddAsync does not call IndexData.AddAsync for normal properties")]
        public async Task AddAsync_DoesNotCallIndexDataAddAsyncForNormalProperties()
        {
            var entity = getTestEntity();
            await _repository.AddAsync(entity);
            _moqIndexData.Verify(x => x.AddAsync(entity, getPropertyInfo(nameof(TestEntity.NormalProperty))), Times.Never);
        }

        [TestMethod("Addsync calls TableClient.AddEntityAsync")]
        public async Task AddAsync_CallsTableClientAddEntityAsync()
        {
            var entity = getTestEntity();
            await _repository.AddAsync(entity);
            _moqTableClient.Verify(x => x.AddEntityAsync(entity, CancellationToken.None), Times.Once);
        }

        [TestMethod("DeleteAsync throws ArgumentNullException if entity argument is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteAsync_ThrowsArgumentNullExceptionIfEntityIsNull()
        {
            await _repository.DeleteAsync(null);
        }

        [TestMethod("DeleteAsync calls IndexData.DeleteAsync for each indexed property")]
        public async Task DeleteAsync_CallsIndexDataDeleteAsyncForEachIndexedProperty()
        {
            var entity = getTestEntity();
            await _repository.DeleteAsync(entity);
            _moqIndexData.Verify(x => x.DeleteAsync(entity, It.IsAny<PropertyInfo>()), Times.Exactly(2));
        }

        [TestMethod("DeleteAsync does not call IndexData.DeleteAsync for normal properties")]
        public async Task DeleteAsync_DoesNotCallIndexDataDeleteAsyncForNormalProperties()
        {
            var entity = getTestEntity();
            await _repository.DeleteAsync(entity);
            _moqIndexData.Verify(x => x.DeleteAsync(entity, getPropertyInfo(nameof(TestEntity.NormalProperty))), Times.Never);
        }

        [TestMethod("DeleteAsync calls TableClient.DeleteEntityAsync")]
        public async Task DeleteAsync_CallsTableClientDeleteEntityAsync()
        {
            var entity = getTestEntity();
            await _repository.DeleteAsync(entity);
            _moqTableClient.Verify(x => x.DeleteEntityAsync(entity.PartitionKey, entity.RowKey, ETag.All, CancellationToken.None), Times.Once);
        }

        [TestMethod("GetAsync() calls TableClient.QueryAsync()")]
        public async Task GetAsync_CallsTableClientQueryAsync()
        {
            await _repository.GetAsync();
            _moqTableClient.Verify(x => x.QueryAsync<TestEntity>(default(string), null, null, CancellationToken.None), Times.Once);
        }

        [TestMethod("GetAsync(partitionKey) calls TableClient.QueryAsync([partitionKeyFilter])")]
        public async Task GetAsyncPartitionKey_CallsQueryAsyncWithPartitionKeyFilter()
        {
            var partitionKey = Guid.NewGuid().ToString();
            var filter = $"PartitionKey eq '{partitionKey}'";
            await _repository.GetAsync(partitionKey);
            _moqTableClient.Verify(x => x.QueryAsync<TestEntity>(filter, null, null, CancellationToken.None), Times.Once);
        }

        [TestMethod("GetAsync(partitionKey, rowKey) calls TableClient.GetEntityAsync<T>(partitionKey, rowKey)")]
        public async Task GetAsyncPartitionKeyRowKey_CallsTableClientGetEntityAsyncPartitionKeyRowKey()
        {
            var partitionKey = Guid.NewGuid().ToString();
            var rowKey = Guid.NewGuid().ToString();
            await _repository.GetAsync(partitionKey, rowKey);
            _moqTableClient.Verify(x => x.GetEntityAsync<TestEntity>(partitionKey, rowKey, null, CancellationToken.None), Times.Once);
        }

        [TestMethod("GetIndexKey throws ArgumentNullException if propertyName is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetIndexKey_ThrowsArgumentNullExceptionIfPropertyNameIsNull()
        {
            _repository.GetIndexKey(null, It.IsAny<object>());
        }

        [TestMethod("GetIndexKey throws ArgumentNullException if propertyName is empty")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetIndexKey_ThrowsArgumentNullExceptionIfPropertyNameIsEmpty()
        {
            _repository.GetIndexKey(string.Empty, It.IsAny<object>());
        }

        [TestMethod("GetIndexKey throws ArgumentNullException if propertyName is all white space")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetIndexKey_ThrowsArgumentNullExceptionIfPropertyNameIsAllWhiteSpace()
        {
            _repository.GetIndexKey("     ", It.IsAny<object>());
        }

        [TestMethod("GetIndexKey throws ArgumentOutOfRangeException if propertyName does not correspond to an indexed property")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetIndexKey_ThrowsArgumentOutOfRangeExceptionIfPropertyNameDoesNotCorrespondToAnIndexedProperty()
        {
            _repository.GetIndexKey(nameof(TestEntity.NormalProperty), It.IsAny<object>());
        }

        [TestMethod("GetIndexKey returns excpected IndexKey for valid propertyName and not-null propertyValue")]
        public void GetIndexKey_ReturnsExpectedIndexKeyForValidPropertyNameAndNotNullPropertyValue()
        {
            var obj = new { Fizz = "Buzz" };
            var indexKey = _repository.GetIndexKey(nameof(TestEntity.IndexedProperty1), obj);
            Assert.IsTrue(indexKey.PropertyName == nameof(TestEntity.IndexedProperty1) && indexKey.PropertyValue == Convert.ToString(obj));
        }

        [TestMethod("GetIndexKey returns excpected IndexKey for valid propertyName and null propertyValue")]
        public void GetIndexKey_ReturnsExpectedIndexKeyForValidPropertyNameAndNullPropertyValue()
        {
            var indexKey = _repository.GetIndexKey(nameof(TestEntity.IndexedProperty1), null);
            Assert.IsTrue(indexKey.PropertyName == nameof(TestEntity.IndexedProperty1) && indexKey.PropertyValue == string.Empty);
        }

        [TestMethod]
        public async Task GetByIndexesAsync_ReturnsEmptyIfIndexesAreNull()
        {
            var result = await _repository.GetByIndexesAsync(null);
            Assert.IsTrue(!result.Any());
        }

        [TestMethod]
        public async Task GetByIndexesAsync_ReturnsEmptyIfIndexesAreEmpty()
        {
            var result = await _repository.GetByIndexesAsync(Enumerable.Empty<Indexing.Index>());
            Assert.IsTrue(!result.Any());
        }

        [TestMethod]
        public async Task GetByIndexesAsync_CallsTableClientGetEntityAsyncForEachIndex()
        {
            var entities = new List<TestEntity>
            {
                new TestEntity("partitionkey1", "rowkey1") { IndexedProperty1 = "indexedproperty1_1", IndexedProperty2 = "indexedproperty2_1", NormalProperty = "normalproperty1" },
                new TestEntity("partitionkey2", "rowkey2") { IndexedProperty1 = "indexedproperty1_2", IndexedProperty2 = "indexedproperty2_2", NormalProperty = "normalproperty2" },
                new TestEntity("partitionkey3", "rowkey3") { IndexedProperty1 = "indexedproperty1_3", IndexedProperty2 = "indexedproperty2_3", NormalProperty = "normalproperty3" }
            };

            var indexes = entities.Select(x => new Indexing.Index(new IndexKey(nameof(TestEntity.IndexedProperty1), x.IndexedProperty1), EntityKey.FromEntity(x))).ToArray();

            await _repository.GetByIndexesAsync(indexes);

            _moqTableClient.Verify(x => x.GetEntityAsync<TestEntity>(It.IsAny<string>(), It.IsAny<string>(), null, CancellationToken.None), Times.Exactly(3));
        }









        //[TestMethod("GetByIndexedPropertyAsync throws ArgumentNullException if propertyName is null")]
        //[ExpectedException(typeof(ArgumentNullException))]
        //public async Task GetByIndexedPropertyAsync_ThrowsArgumentNullExceptionIfPropertyNameIsNull()
        //{
        //    await _repository.GetByIndexedPropertyAsync(null, It.IsAny<object>());
        //}

        //[TestMethod("GetByIndexedPropertyAsync throws ArgumentNullException if propertyName is empty")]
        //[ExpectedException(typeof(ArgumentNullException))]
        //public async Task GetByIndexedPropertyAsync_ThrowsArgumentNullExceptionIfPropertyNameIsEmpty()
        //{
        //    await _repository.GetByIndexedPropertyAsync(string.Empty, It.IsAny<object>());
        //}

        //[TestMethod("GetByIndexedPropertyAsync throws ArgumentNullException if propertyName is all white space")]
        //[ExpectedException(typeof(ArgumentNullException))]
        //public async Task GetByIndexedPropertyAsync_ThrowsArgumentNullExceptionIfPropertyNameIsAllWhiteSpace()
        //{
        //    await _repository.GetByIndexedPropertyAsync("     ", It.IsAny<object>());
        //}

        //[TestMethod("GetByIndexedPropertyAsync throws ArgumentOutOfRangeException if property is not indexed")]
        //[ExpectedException(typeof(ArgumentOutOfRangeException))]
        //public async Task GetByIndexedPropertyAsync_ThrowsArgumentOutOfRangeExceptionIfPropertyIsNotIndexed()
        //{
        //    await _repository.GetByIndexedPropertyAsync(nameof(TestEntity.NormalProperty), It.IsAny<object>());
        //}

        //[TestMethod("GetSingleByIndexedPropertyAsync throws ArgumentNullException if propertyName is null")]
        //public async Task GetSingleByIndexedPropertyAsync_ThrowsArgumentNullExceptionIfPropertyNameIsNull()
        //{
        //    _moqIndexData.Setup(x => x.)
        //}











        private TestEntity getTestEntity() => new TestEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        {
            IndexedProperty1 = "ABC123",
            NormalProperty = "DEF345",
        };

        private PropertyInfo getPropertyInfo(string propertyName) => typeof(TestEntity).GetProperty(propertyName);
    }
}
