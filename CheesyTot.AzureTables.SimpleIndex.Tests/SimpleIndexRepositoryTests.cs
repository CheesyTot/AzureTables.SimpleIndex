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
        private static IDictionary<string, TestEntity> _entities = new Dictionary<string, TestEntity>
        {
            { "GetAsync1", new TestEntity("f793bd89-b42a-459d-8cab-35a790165cab", "81595778-ed0d-40e6-be1d-d08fccd814dd"){ IndexedProperty1 = "ABC123", IndexedProperty2 = "BCD234", NormalProperty = "CDE345" } },
            { "UpdateAsync1", new TestEntity("f793bd89-b42a-459d-8cab-35a790165cab", "81595778-ed0d-40e6-be1d-d08fccd814dd"){ IndexedProperty1 = "DEF456", IndexedProperty2 = "EFG567", NormalProperty = "FGH678" } },
            { "GetByIndexes1", new TestEntity("b83631c8-ee30-474f-9764-c1318f86815d", "6a048ae7-bb79-415f-86af-b35088156945"){ IndexedProperty1 = "GHI789", IndexedProperty2 = "HIJ890", NormalProperty = "IJK901" } },
            { "GetByIndexes2", new TestEntity("d8f7ad66-a764-4421-b88f-acf2c526a76e", "951a9012-70ff-48e6-a26a-0d6566221925"){ IndexedProperty1 = "GHI789", IndexedProperty2 = "NOP456", NormalProperty = "OPQ567" } },
            { "GetByIndexes3", new TestEntity("238f49fd-2016-4195-84a9-aa4b1c31813d", "3eb415c6-5183-4170-8374-33888de1b62a"){ IndexedProperty1 = "JKL012", IndexedProperty2 = "KLM123", NormalProperty = "LMN234" } }
        };

        private static IDictionary<string, Indexing.Index> _indexes = new Dictionary<string, Indexing.Index>
        {
            { "GetByIndexes1", new Indexing.Index(new IndexKey(nameof(TestEntity.IndexedProperty1), _entities["GetByIndexes1"].IndexedProperty1), EntityKey.FromEntity(_entities["GetByIndexes1"])) },
            { "GetByIndexes2", new Indexing.Index(new IndexKey(nameof(TestEntity.IndexedProperty1), _entities["GetByIndexes2"].IndexedProperty1), EntityKey.FromEntity(_entities["GetByIndexes2"])) },
            { "GetByIndexes3", new Indexing.Index(new IndexKey(nameof(TestEntity.IndexedProperty1), _entities["GetByIndexes3"].IndexedProperty1), EntityKey.FromEntity(_entities["GetByIndexes3"])) },
        };

        private ISimpleIndexRepository<TestEntity> _repository;

        private Mock<TableClient> _moqTableClient;
        private Mock<IIndexData<TestEntity>> _moqIndexData;

        private Azure.Response<TestEntity> getResponse(TestEntity entity)
        {
            return Azure.Response.FromValue(entity, default);
        }

        [TestInitialize]
        public void TestInit()
        {
            _moqTableClient = new Mock<TableClient>();
            _moqTableClient.Setup(x => x.GetEntityAsync<TestEntity>(_entities["GetAsync1"].PartitionKey, _entities["GetAsync1"].RowKey, default(IEnumerable<string>), default(CancellationToken))).Returns(Task.FromResult(getResponse(_entities["GetAsync1"])));


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
            await _repository.GetAsync(_entities["GetAsync1"].PartitionKey, _entities["GetAsync1"].RowKey);
            _moqTableClient.Verify(x => x.GetEntityAsync<TestEntity>(_entities["GetAsync1"].PartitionKey, _entities["GetAsync1"].RowKey, default(IEnumerable<string>), default(CancellationToken)), Times.Once);
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
            var indexes = new[] { _indexes["GetByIndexes1"], _indexes["GetByIndexes2"], _indexes["GetByIndexes3"] };

            await _repository.GetByIndexesAsync(indexes);

            _moqTableClient.Verify(x => x.GetEntityAsync<TestEntity>(indexes[0].EntityKey.PartitionKey, indexes[0].EntityKey.RowKey, null, CancellationToken.None), Times.Once);
            _moqTableClient.Verify(x => x.GetEntityAsync<TestEntity>(indexes[1].EntityKey.PartitionKey, indexes[1].EntityKey.RowKey, null, CancellationToken.None), Times.Once);
            _moqTableClient.Verify(x => x.GetEntityAsync<TestEntity>(indexes[2].EntityKey.PartitionKey, indexes[2].EntityKey.RowKey, null, CancellationToken.None), Times.Once);
        }

        [TestMethod("UpdateAsync: Throws if entity is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateAsync_ThrowsIfEntityIsNull()
        {
            await _repository.UpdateAsync(null);
        }

        [TestMethod("UpdateAsync: Calls TableClient.GetEntityAsync")]
        public async Task UpdateAsync_CallsTableClientGetEntityAsync()
        {
            var entity = _entities["UpdateAsync1"];
            await _repository.UpdateAsync(entity);
            _moqTableClient.Verify(x => x.GetEntityAsync<TestEntity>(entity.PartitionKey, entity.RowKey, default(IEnumerable<string>), default(CancellationToken)), Times.Once);
        }

        [TestMethod("UpdateAsync: Calls IndexData.ReplaceAsync for each indexed property")]
        public async Task UpdateAsync_CallsIndexDataReplaceAsyncForEachIndexedProperty()
        {
            var oldEntity = _entities["GetAsync1"];
            var newEntity = _entities["UpdateAsync1"];
            var pi_indexedProperty1 = typeof(TestEntity).GetProperty("IndexedProperty1");
            var pi_indexedProperty2 = typeof(TestEntity).GetProperty("IndexedProperty2");
            await _repository.UpdateAsync(newEntity);
            _moqIndexData.Verify(x => x.ReplaceAsync(oldEntity, newEntity, pi_indexedProperty1), Times.Once);
            _moqIndexData.Verify(x => x.ReplaceAsync(oldEntity, newEntity, pi_indexedProperty2), Times.Once);
        }

        [TestMethod("UpdateAsync: Calls TableClient.UpdateEntityAsync")]
        public async Task UpdateAsync_CallsTableClientUpdateEntityAsync()
        {
            var entity = _entities["UpdateAsync1"];
            await _repository.UpdateAsync(entity);
            _moqTableClient.Verify(x => x.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace, CancellationToken.None), Times.Once);
        }

        [TestMethod("QueryAsync: Calls TableClient.QueryAsync")]
        public async Task QueryAsync_CallsTableClientQueryAsync()
        {
            string filter = $"PartitionKey = '{_entities["GetAsync1"].PartitionKey}'";
            await _repository.QueryAsync(filter);
            _moqTableClient.Verify(x => x.QueryAsync<TestEntity>(filter, default(int?), default(IEnumerable<string>), CancellationToken.None), Times.Once);
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
