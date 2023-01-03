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
            { "GetByIndexes3", new TestEntity("238f49fd-2016-4195-84a9-aa4b1c31813d", "3eb415c6-5183-4170-8374-33888de1b62a"){ IndexedProperty1 = "JKL012", IndexedProperty2 = "KLM123", NormalProperty = "LMN234" } },
            { "GetSingleByIndexedPropertyAsync", new TestEntity("238f49fd-2016-4195-84a9-aa4b1c31813d", "3eb415c6-5183-4170-8374-33888de1b62a"){ IndexedProperty1 = "MNO345", IndexedProperty2 = "NOP456", NormalProperty = "OPQ567" } }
        };

        private static IDictionary<string, Indexing.Index> _indexes = new Dictionary<string, Indexing.Index>
        {
            { "GetByIndexes1", new Indexing.Index(new IndexKey(nameof(TestEntity.IndexedProperty1), _entities["GetByIndexes1"].IndexedProperty1), EntityKey.FromEntity(_entities["GetByIndexes1"])) },
            { "GetByIndexes2", new Indexing.Index(new IndexKey(nameof(TestEntity.IndexedProperty1), _entities["GetByIndexes2"].IndexedProperty1), EntityKey.FromEntity(_entities["GetByIndexes2"])) },
            { "GetByIndexes3", new Indexing.Index(new IndexKey(nameof(TestEntity.IndexedProperty1), _entities["GetByIndexes3"].IndexedProperty1), EntityKey.FromEntity(_entities["GetByIndexes3"])) },
            { "GetSingleByIndexedPropertyAsync", new Indexing.Index(new IndexKey(nameof(TestEntity.IndexedProperty1), _entities["GetSingleByIndexedPropertyAsync"].IndexedProperty1), EntityKey.FromEntity(_entities["GetSingleByIndexedPropertyAsync"])) }
        };

        private ISimpleIndexRepository<TestEntity> _repository;

        private Mock<TableClient> _moqTableClient;
        private Mock<IIndexData<TestEntity>> _moqIndexData;

        private Azure.Response<TestEntity> getResponse(TestEntity entity)
        {
            return Azure.Response.FromValue(entity, default);
        }

        private AsyncPageable<TestEntity> getAsyncPageable(string key)
        {
            var page1 = Page<TestEntity>.FromValues(new[] { _entities["GetAsync1"] }, "continuationToken", Mock.Of<Response>());
            return AsyncPageable<TestEntity>.FromPages(new[] { page1 });
        }

        [TestInitialize]
        public void TestInit()
        {
            _moqTableClient = new Mock<TableClient>();
            _moqTableClient.Setup(x => x.GetEntityAsync<TestEntity>(_entities["GetAsync1"].PartitionKey, _entities["GetAsync1"].RowKey, default(IEnumerable<string>), default(CancellationToken))).Returns(Task.FromResult(getResponse(_entities["GetAsync1"])));
            _moqTableClient.Setup(x => x.QueryAsync<TestEntity>($"PartitionKey eq '{_entities["GetAsync1"].PartitionKey}'", 10, null, CancellationToken.None)).Returns(getAsyncPageable("GetAsync1"));
            _moqTableClient.Setup(x => x.QueryAsync<TestEntity>(default(string), 10, null, CancellationToken.None)).Returns(getAsyncPageable("GetAsync1"));

            _moqIndexData = new Mock<IIndexData<TestEntity>>();
            _moqIndexData.Setup(x => x.GetAllIndexesAsync(new IndexKey(nameof(TestEntity.IndexedProperty1), "GHI789"))).Returns(Task.FromResult(new[] { _indexes["GetByIndexes1"], _indexes["GetByIndexes2"] }.AsEnumerable()));
            _moqIndexData.Setup(x => x.GetSingleIndexAsync(new IndexKey(nameof(TestEntity.IndexedProperty1), _entities["GetSingleByIndexedPropertyAsync"].IndexedProperty1))).Returns(Task.FromResult(_indexes["GetSingleByIndexedPropertyAsync"]));
            _moqIndexData.Setup(x => x.GetSingleIndexAsync(new IndexKey(nameof(TestEntity.IndexedProperty1), "NotFoundValue"))).Throws<InvalidOperationException>();
            _moqIndexData.Setup(x => x.GetFirstIndexAsync(new IndexKey(nameof(TestEntity.IndexedProperty1), "NotFoundValue"))).Throws<InvalidOperationException>();
            _moqIndexData.Setup(x => x.GetSingleIndexOrDefaultAsync(new IndexKey(nameof(TestEntity.IndexedProperty1), _entities["GetSingleByIndexedPropertyAsync"].IndexedProperty1))).Returns(Task.FromResult(_indexes["GetSingleByIndexedPropertyAsync"]));
            _moqIndexData.Setup(x => x.PageIndexes(new IndexKey(nameof(TestEntity.IndexedProperty1), "GHI789"), 10, null)).Returns(Task.FromResult(new PagedResult<Indexing.Index> { ContinuationToken = null, Results = new[] { _indexes["GetByIndexes1"], _indexes["GetByIndexes2"] }.AsEnumerable() } ));

            _repository = new SimpleIndexRepository<TestEntity>(_moqTableClient.Object, _moqIndexData.Object);
        }

        [TestMethod("AddAsync: Throws ArgumentNullException if entity argument is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddAsync_ThrowsArgumentNullExceptionIfEntityIsNull()
        {
            await _repository.AddAsync(null);
        }

        [TestMethod("AddAsync: Calls IndexData.AddAsync for each indexed property")]
        public async Task AddAsync_CallsIndexDataAddAsyncForEachIndexedProperty()
        {
            var entity = getTestEntity();
            await _repository.AddAsync(entity);
            _moqIndexData.Verify(x => x.AddAsync(entity, It.IsAny<PropertyInfo>()), Times.Exactly(3));
        }

        [TestMethod("AddAsync: Does not call IndexData.AddAsync for normal properties")]
        public async Task AddAsync_DoesNotCallIndexDataAddAsyncForNormalProperties()
        {
            var entity = getTestEntity();
            await _repository.AddAsync(entity);
            _moqIndexData.Verify(x => x.AddAsync(entity, getPropertyInfo(nameof(TestEntity.NormalProperty))), Times.Never);
        }

        [TestMethod("Addsync: Calls TableClient.AddEntityAsync")]
        public async Task AddAsync_CallsTableClientAddEntityAsync()
        {
            var entity = getTestEntity();
            await _repository.AddAsync(entity);
            _moqTableClient.Verify(x => x.AddEntityAsync(entity, CancellationToken.None), Times.Once);
        }

        [TestMethod("DeleteAsync: Throws ArgumentNullException if entity argument is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteAsync_ThrowsArgumentNullExceptionIfEntityIsNull()
        {
            await _repository.DeleteAsync(null);
        }

        [TestMethod("DeleteAsync: Calls IndexData.DeleteAsync for each indexed property")]
        public async Task DeleteAsync_CallsIndexDataDeleteAsyncForEachIndexedProperty()
        {
            var entity = getTestEntity();
            await _repository.DeleteAsync(entity);
            _moqIndexData.Verify(x => x.DeleteAsync(entity, It.IsAny<PropertyInfo>()), Times.Exactly(3));
        }

        [TestMethod("DeleteAsync: Does not call IndexData.DeleteAsync for normal properties")]
        public async Task DeleteAsync_DoesNotCallIndexDataDeleteAsyncForNormalProperties()
        {
            var entity = getTestEntity();
            await _repository.DeleteAsync(entity);
            _moqIndexData.Verify(x => x.DeleteAsync(entity, getPropertyInfo(nameof(TestEntity.NormalProperty))), Times.Never);
        }

        [TestMethod("DeleteAsync: Calls TableClient.DeleteEntityAsync")]
        public async Task DeleteAsync_CallsTableClientDeleteEntityAsync()
        {
            var entity = getTestEntity();
            await _repository.DeleteAsync(entity);
            _moqTableClient.Verify(x => x.DeleteEntityAsync(entity.PartitionKey, entity.RowKey, ETag.All, CancellationToken.None), Times.Once);
        }

        [TestMethod("GetAsync(): Calls TableClient.QueryAsync()")]
        public async Task GetAsync_CallsTableClientQueryAsync()
        {
            await _repository.GetAsync();
            _moqTableClient.Verify(x => x.QueryAsync<TestEntity>(default(string), null, null, CancellationToken.None), Times.Once);
        }

        [TestMethod("PageAsync: Calls TableClient.QueryAsync")]
        public async Task PageAsync_CallsTableClientQueryAsync()
        {
            await _repository.PageAsync(10, null);
            _moqTableClient.Verify(x => x.QueryAsync<TestEntity>(default(string), 10, null, CancellationToken.None), Times.Once);
        }

        [TestMethod("GetAsync(partitionKey): Calls TableClient.QueryAsync([partitionKeyFilter])")]
        public async Task GetAsyncPartitionKey_CallsQueryAsyncWithPartitionKeyFilter()
        {
            var partitionKey = Guid.NewGuid().ToString();
            var filter = $"PartitionKey eq '{partitionKey}'";
            await _repository.GetAsync(partitionKey);
            _moqTableClient.Verify(x => x.QueryAsync<TestEntity>(filter, null, null, CancellationToken.None), Times.Once);
        }

        [TestMethod("PageAsync(partitionKey): Calls TableClient.QueryAsync")]
        public async Task PageAsyncPartitionKey_CallsQueryAsync()
        {
            var partitionKey = _entities["GetAsync1"].PartitionKey;
            await _repository.PageAsync(partitionKey, 10, null);
            _moqTableClient.Verify(x => x.QueryAsync<TestEntity>($"PartitionKey eq '{partitionKey}'", 10, null, CancellationToken.None), Times.Once);
        }

        [TestMethod("GetAsync(partitionKey, rowKey): Calls TableClient.GetEntityAsync<T>(partitionKey, rowKey)")]
        public async Task GetAsyncPartitionKeyRowKey_CallsTableClientGetEntityAsyncPartitionKeyRowKey()
        {
            await _repository.GetAsync(_entities["GetAsync1"].PartitionKey, _entities["GetAsync1"].RowKey);
            _moqTableClient.Verify(x => x.GetEntityAsync<TestEntity>(_entities["GetAsync1"].PartitionKey, _entities["GetAsync1"].RowKey, default(IEnumerable<string>), default(CancellationToken)), Times.Once);
        }

        [TestMethod("GetByIndexesAsync: Returns empty if indexes are null")]
        public async Task GetByIndexesAsync_ReturnsEmptyIfIndexesAreNull()
        {
            var result = await _repository.GetByIndexesAsync(null);
            Assert.IsTrue(!result.Any());
        }

        [TestMethod("GetByIndexesAsync: Returns empty if indexes are empty")]
        public async Task GetByIndexesAsync_ReturnsEmptyIfIndexesAreEmpty()
        {
            var result = await _repository.GetByIndexesAsync(Enumerable.Empty<Indexing.Index>());
            Assert.IsTrue(!result.Any());
        }

        [TestMethod("GetByIndexesAsync: Calls TableClient.GetEntityAsync for each index")]
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

        [TestMethod("PagedQueryAsync: Calls TableClient.QueryAsync")]
        public async Task PagedQueryAsync_CallsTableClientQueryAsync() 
        {
            string filter = $"PartitionKey eq '{_entities["GetAsync1"].PartitionKey}'";
            await _repository.PagedQueryAsync(filter, 10, null);
            _moqTableClient.Verify(x => x.QueryAsync<TestEntity>(filter, 10, null, CancellationToken.None), Times.Once);
        }

        [TestMethod("GetByIndexedPropertyAsync: Calls IndexData.GetAllIndexesAsync")]
        public async Task GetByIndexedPropertyAsyncCallsIndexDataGetAllIndexesAsync()
        {
            var propertyName = nameof(TestEntity.IndexedProperty1);
            var propertyValue = _entities["GetByIndexes1"].IndexedProperty1;
            var indexKey = new IndexKey(propertyName, propertyValue);
            
            await _repository.GetByIndexedPropertyAsync(propertyName, propertyValue);

            _moqIndexData.Verify(x => x.GetAllIndexesAsync(indexKey), Times.Once);
        }

        [TestMethod("PageByIndexedPropertyAsync: Calls IndexData.PageIndexesAsync")]
        public async Task PageByIndexedPropertyAsync_CallsIndexDataPageIndexesAsync()
        {
            var propertyName = nameof(TestEntity.IndexedProperty1);
            var propertyValue = _entities["GetByIndexes1"].IndexedProperty1;
            var indexKey = new IndexKey(propertyName, propertyValue);

            await _repository.PageByIndexedPropertyAsync(propertyName, propertyValue, 10, null);

            _moqIndexData.Verify(x => x.PageIndexes(indexKey, 10, null), Times.Once);
        }

        [TestMethod("GetByIndexedPropertiesAsync: Calls TableClient.GetEntityAsync for each index result")]
        public async Task GetByIndexedPropertiesAsync_CallsTableClientGetEntityAsyncForEachIndexResult()
        {
            var propertyName = nameof(TestEntity.IndexedProperty1);
            var propertyValue = _entities["GetByIndexes1"].IndexedProperty1;

            await _repository.GetByIndexedPropertyAsync(propertyName, propertyValue);

            _moqTableClient.Verify(x => x.GetEntityAsync<TestEntity>(_entities["GetByIndexes1"].PartitionKey, _entities["GetByIndexes1"].RowKey, default(IEnumerable<string>), default(CancellationToken)), Times.Once);
            _moqTableClient.Verify(x => x.GetEntityAsync<TestEntity>(_entities["GetByIndexes2"].PartitionKey, _entities["GetByIndexes2"].RowKey, default(IEnumerable<string>), default(CancellationToken)), Times.Once);
        }

        [TestMethod("PageByIndexedPropertiesAsync: Calls TableClient.GetEntityAsync for each index result")]
        public async Task PageByIndexedPropertiesAsync_CallsTableClientGetEntityAsyncForEachIndexResult()
        {
            var propertyName = nameof(TestEntity.IndexedProperty1);
            var propertyValue = _entities["GetByIndexes1"].IndexedProperty1;

            await _repository.PageByIndexedPropertyAsync(propertyName, propertyValue, 10, null);

            _moqTableClient.Verify(x => x.GetEntityAsync<TestEntity>(_entities["GetByIndexes1"].PartitionKey, _entities["GetByIndexes1"].RowKey, default(IEnumerable<string>), default(CancellationToken)), Times.Once);
            _moqTableClient.Verify(x => x.GetEntityAsync<TestEntity>(_entities["GetByIndexes2"].PartitionKey, _entities["GetByIndexes2"].RowKey, default(IEnumerable<string>), default(CancellationToken)), Times.Once);
        }

        [TestMethod("GetByIndexedPropertiesAsync: Returns empty if no records are found")]
        public async Task GetByIndexedPropertiesAsync_ReturnsEmptyIfNoRecordsAreFound()
        {
            var propertyName = nameof(TestEntity.IndexedProperty1);
            var propertyValue = "NotFoundValue";

            var result = await _repository.GetByIndexedPropertyAsync(propertyName, propertyValue);

            Assert.IsFalse(result.Any());
        }

        [TestMethod("PageByIndexedPropertiesAsync: Returns empty if no records are found")]
        public async Task PageByIndexedPropertiesAsync_ReturnsEmptyIfNoRecordsAreFound()
        {
            var propertyName = nameof(TestEntity.IndexedProperty1);
            var propertyValue = "NotFoundValue";

            var result = await _repository.PageByIndexedPropertyAsync(propertyName, propertyValue, 10, null);

            Assert.IsTrue(result == null || result.Results.Any() == false);
        }

        [TestMethod("GetSingleByIndexedPropertyAsync: Calls IndexData.GetSingleIndexOrDefaultAsync")]
        public async Task GetSingleByIndexedPropertyAsync_CallsIndexDataGetSingleIndexOrDefaultAsync()
        {
            var propertyName = nameof(TestEntity.IndexedProperty1);
            var propertyValue = _entities["GetSingleByIndexedPropertyAsync"].IndexedProperty1;
            var indexKey = new IndexKey(propertyName, propertyValue);

            await _repository.GetSingleByIndexedPropertyAsync(propertyName, propertyValue);

            _moqIndexData.Verify(x => x.GetSingleIndexAsync(indexKey), Times.Once);
        }

        [TestMethod("GetSingleByIndexedPropertyAsync: Calls TableClient.GetEntityAsync")]
        public async Task GetSingleByIndexedPropertyAsync_CallsTableClientGetEntityAsync()
        {
            var propertyName = nameof(TestEntity.IndexedProperty1);
            var propertyValue = _entities["GetSingleByIndexedPropertyAsync"].IndexedProperty1;

            await _repository.GetSingleByIndexedPropertyAsync(propertyName, propertyValue);

            _moqTableClient.Verify(x => x.GetEntityAsync<TestEntity>(_entities["GetSingleByIndexedPropertyAsync"].PartitionKey, _entities["GetSingleByIndexedPropertyAsync"].RowKey, default(IEnumerable<string>), default(CancellationToken)), Times.Once);
        }

        [TestMethod("GetSingleByIndexedPropertyAsync: Throws if no record is found")]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GetSingleByIndexedPropertyAsync_ThrowsIfNoRecordFound()
        {
            var propertyName = nameof(TestEntity.IndexedProperty1);
            var propertyValue = "NotFoundValue";
            await _repository.GetSingleByIndexedPropertyAsync(propertyName, propertyValue);
        }

        [TestMethod("GetSingleOrDefaultByIndexedPropertyAsync: Calls IndexData.GetSingleIndexOrDefaultAsync")]
        public async Task GetSingleOrDefaultByIndexedPropertyAsync_CallsIndexDataGetSingleIndexOrDefaultAsync()
        {
            var propertyName = nameof(TestEntity.IndexedProperty1);
            var propertyValue = _entities["GetSingleByIndexedPropertyAsync"].IndexedProperty1;
            var indexKey = new IndexKey(propertyName, propertyValue);

            await _repository.GetSingleOrDefaultByIndexedPropertyAsync(propertyName, propertyValue);

            _moqIndexData.Verify(x => x.GetSingleIndexOrDefaultAsync(indexKey), Times.Once);
        }

        [TestMethod("GetSingleOrDefaultByIndexedPropertyAsync: Calls TableClient.GetEntityAsync")]
        public async Task GetSingleOrDefaultByIndexedPropertyAsync_CallsTableClientGetEntityAsync()
        {
            var propertyName = nameof(TestEntity.IndexedProperty1);
            var propertyValue = _entities["GetSingleByIndexedPropertyAsync"].IndexedProperty1;

            await _repository.GetSingleOrDefaultByIndexedPropertyAsync(propertyName, propertyValue);

            _moqTableClient.Verify(x => x.GetEntityAsync<TestEntity>(_entities["GetSingleByIndexedPropertyAsync"].PartitionKey, _entities["GetSingleByIndexedPropertyAsync"].RowKey, default(IEnumerable<string>), default(CancellationToken)), Times.Once);
        }

        [TestMethod("GetSingleOrDefaultByIndexedPropertyAsync: Returns null if not found")]
        public async Task GetSingleOrDefaultByIndexedPropertyAsync_ReturnsNullIfNotFound()
        {
            var propertyName = nameof(TestEntity.IndexedProperty1);
            var propertyValue = "NotFoundValue";
            var result = await _repository.GetSingleOrDefaultByIndexedPropertyAsync(propertyName, propertyValue);
            Assert.IsNull(result);
        }

        [TestMethod("GetFirstByIndexedPropertyAsync: Calls IndexData.GetFirstIndexOrDefaultAsync")]
        public async Task GetFirstByIndexedPropertyAsync_CallsIndexDataGetFirstIndexOrDefaultAsync()
        {
            var propertyName = nameof(TestEntity.IndexedProperty1);
            var propertyValue = _entities["GetSingleByIndexedPropertyAsync"].IndexedProperty1;
            var indexKey = new IndexKey(propertyName, propertyValue);

            await _repository.GetFirstByIndexedPropertyAsync(propertyName, propertyValue);

            _moqIndexData.Verify(x => x.GetFirstIndexAsync(indexKey), Times.Once);
        }

        [TestMethod("GetFirstOrDefaultByIndexedPropertyAsync: Calls IndexData.GetFirstIndexOrDefaultAsync")]
        public async Task GetFirstOrDefaultByIndexedPropertyAsync_CallsIndexDataGetFirstIndexOrDefaultAsync()
        {
            var propertyName = nameof(TestEntity.IndexedProperty1);
            var propertyValue = _entities["GetSingleByIndexedPropertyAsync"].IndexedProperty1;
            var indexKey = new IndexKey(propertyName, propertyValue);

            await _repository.GetFirstOrDefaultByIndexedPropertyAsync(propertyName, propertyValue);

            _moqIndexData.Verify(x => x.GetFirstIndexOrDefaultAsync(indexKey), Times.Once);
        }

        [TestMethod("GetFirstByIndexedPropertyAsync: Throws if no record is found")]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GetFirstByIndexedPropertyAsync_ThrowsIfNoRecordFound()
        {
            var propertyName = nameof(TestEntity.IndexedProperty1);
            var propertyValue = "NotFoundValue";
            await _repository.GetFirstByIndexedPropertyAsync(propertyName, propertyValue);
        }

        [TestMethod("GetFirstOrDefaultByIndexedPropertyAsync: Returns null if not found")]
        public async Task GetFirstOrDefaultByIndexedPropertyAsync_ReturnsNullIfNotFound()
        {
            var propertyName = nameof(TestEntity.IndexedProperty1);
            var propertyValue = "NotFoundValue";
            var result = await _repository.GetFirstOrDefaultByIndexedPropertyAsync(propertyName, propertyValue);
            Assert.IsNull(result);
        }

        private TestEntity getTestEntity() => new TestEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        {
            IndexedProperty1 = "ABC123",
            NormalProperty = "DEF345",
        };

        private PropertyInfo getPropertyInfo(string propertyName) => typeof(TestEntity).GetProperty(propertyName);
    }
}
