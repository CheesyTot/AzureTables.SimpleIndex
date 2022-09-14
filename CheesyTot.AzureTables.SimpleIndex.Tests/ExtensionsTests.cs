using Azure;
using Azure.Data.Tables;

namespace CheesyTot.AzureTables.SimpleIndex.Tests
{
    [TestClass]
    public class ExtensionsTests
    {
        private IEnumerable<TestEntity> _originalEnumerable;
        private AsyncPageable<TestEntity> _asyncPageable;
        private AsyncPageable<TestEntity> _emptyAsyncPageable;
        private AsyncPageable<TestEntity> _singleAsyncPageable;

        [TestInitialize]
        public void TestInitialize()
        {
            _originalEnumerable = new[]
            {
                new TestEntity("661cf9a5-7172-43e1-914b-fcfc7dd7aae1", "6297ca58-02d7-40c1-a814-4f8a6722174c"),
                new TestEntity("d0ad0ce8-763d-446d-b682-684dcd60933f", "f4578a07-a23a-4a9e-a4dc-d806e1faebae"),
                new TestEntity("5a0b1571-c20d-4533-944c-b33c17862338", "f2433676-38cc-47d4-93f1-0e2b15995a7d"),
                new TestEntity("186acceb-725d-496b-8c3b-b2f50e134a53", "cca77349-7b35-421a-a341-83e43e351aba"),
                new TestEntity("11469630-2cf1-4088-aa91-9d57c317bfdc", "95c6b63c-fded-4bd2-9eaa-d04d074bbc38"),
                new TestEntity("18573fbc-c57f-4057-9301-d8d01779421c", "15712ce7-6aa9-49c8-a75e-732fc5dd8c1d"),
                new TestEntity("cc7b4f26-e6de-4855-bb4a-702fbb41f757", "1fee6042-b6f7-4998-be05-1cd6fcafdad1"),
                new TestEntity("c26a4eac-7815-477c-9f65-15d5c8b5f8bf", "6539c254-4146-4cd6-9024-494f4df5b492"),
                new TestEntity("f793bd89-b42a-459d-8cab-35a790165cab", "81595778-ed0d-40e6-be1d-d08fccd814dd"),
                new TestEntity("b83631c8-ee30-474f-9764-c1318f86815d", "6a048ae7-bb79-415f-86af-b35088156945"),
                new TestEntity("238f49fd-2016-4195-84a9-aa4b1c31813d", "3eb415c6-5183-4170-8374-33888de1b62a"),
                new TestEntity("d8f7ad66-a764-4421-b88f-acf2c526a76e", "951a9012-70ff-48e6-a26a-0d6566221925")
            };

            _asyncPageable = AsyncPageable<TestEntity>.FromPages(new[]
            {
                new TestPage<TestEntity>(_originalEnumerable.Take(5).ToArray(), Guid.NewGuid().ToString()),
                new TestPage<TestEntity>(_originalEnumerable.Skip(5).Take(5).ToArray(), Guid.NewGuid().ToString()),
                new TestPage<TestEntity>(_originalEnumerable.Skip(10).ToArray(), Guid.NewGuid().ToString()),
            });

            _emptyAsyncPageable = AsyncPageable<TestEntity>.FromPages(Enumerable.Empty<Page<TestEntity>>());

            _singleAsyncPageable = AsyncPageable<TestEntity>.FromPages(new[]
            {
                new TestPage<TestEntity>(_originalEnumerable.Take(1).ToArray(), Guid.NewGuid().ToString())
            });
        }

        [TestMethod("AsEnumerable returns IEnumerable with all items")]
        public async Task AsEnumerableReturnsIEnumerableWithAllItems()
        {
            var testVal = await _asyncPageable.AsEnumerableAsync();
            Assert.IsTrue(!_originalEnumerable.Except(testVal).Any() && !testVal.Except(_originalEnumerable).Any());
        }

        [TestMethod("FirstAsync throws if AsyncPageable is empty")]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task FirstAsyncThrowsIfEmpty()
        {
            var testVal = await _emptyAsyncPageable.FirstAsync();
        }

        [TestMethod("FirstAsync returns first item")]
        public async Task FirstAsyncReturnsFirstItem()
        {
            var testVal = await _asyncPageable.FirstAsync();
            Assert.AreEqual(_originalEnumerable.First(), testVal);
        }

        [TestMethod("FirstOrDefaultAsync returns default if AsyncPageable is empty")]
        public async Task FirstOrDefaultAsyncReturnsDefaultIfEmpty()
        {
            var testVal = await _emptyAsyncPageable.FirstOrDefaultAsync();
            Assert.IsTrue(testVal == default);
        }

        [TestMethod("FirstOrDefaultAsync returns first item")]
        public async Task FirstOrDefaultAsyncReturnsFirstItem()
        {
            var testVal = await _asyncPageable.FirstOrDefaultAsync();
            Assert.AreEqual(_originalEnumerable.First(), testVal);
        }

        [TestMethod("SingleOrDefaultAsync throws if sequence has more than one item")]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SingleOrDefaultAsyncThrowsIfMoreThanOneElement()
        {
            var testVal = await _asyncPageable.SingleOrDefaultAsync();
        }

        [TestMethod("SingleOrDefaultAsync returns default if sequence is empty")]
        public async Task SingleOrDefaultAsyncReturnsDefaultIfEmpty()
        {
            var testVal = await _emptyAsyncPageable.SingleOrDefaultAsync();
            Assert.IsTrue(testVal == default);
        }

        [TestMethod("SingleOrDefaultAsync returns item if sequence has one item")]
        public async Task SingleOrDefautlAsyncReturnsItemIfOneItem()
        {
            var testVal = await _singleAsyncPageable.SingleOrDefaultAsync();
            Assert.IsTrue(testVal == _originalEnumerable.First());
        }

        [TestMethod("SingleAsync throws if sequence has more than one item")]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SingleAsyncThrowsIfMoreThanOneElement()
        {
            var testVal = await _asyncPageable.SingleAsync();
        }

        [TestMethod("SingleAsync throws if sequence is empty")]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SingleAsyncThrowsIfEmpty()
        {
            var testVal = await _emptyAsyncPageable.SingleAsync();
        }

        [TestMethod("SingleAsync returns item if sequence has one item")]
        public async Task SingleAsyncReturnsItemIfOneItem()
        {
            var testVal = await _singleAsyncPageable.SingleAsync();
            Assert.IsTrue(testVal == _originalEnumerable.First());
        }

        [TestMethod("CountAsync returns Int32 count")]
        public async Task CountAsyncReturnsInt32Count()
        {
            var testVal = await _asyncPageable.CountAsync();
            Assert.AreEqual(typeof(int), testVal.GetType());
            Assert.AreEqual(_originalEnumerable.Count(), testVal);
        }

        [TestMethod("LongCountAsync returns Int64 count")]
        public async Task LongCountAsyncReturnsInt64Count()
        {
            var testVal = await _asyncPageable.LongCountAsync();
            Assert.AreEqual(typeof(long), testVal.GetType());
            Assert.AreEqual(_originalEnumerable.LongCount(), testVal);
        }

        [TestMethod("AnyAsync returns true if sequence has items")]
        public async Task AnyAsyncReturnsTrueIfSequenceHasItems()
        {
            var testVal = await _asyncPageable.AnyAsync();
            Assert.IsTrue(testVal);
        }

        [TestMethod("AnyAsync returns false if sequence is empty")]
        public async Task AnyAsyncReturnsFalseIfSequenceIsEmpty()
        {
            var testVal = await _emptyAsyncPageable.AnyAsync();
            Assert.IsFalse(testVal);
        }

        [TestMethod("InChunksOf returns expected number of chunks")]
        public void InChunksOfReturnsExpectedNumberOfChunks()
        {
            Assert.AreEqual(_originalEnumerable.InChunksOf(12).Count(), 1);
            Assert.AreEqual(_originalEnumerable.InChunksOf(6).Count(), 2);
            Assert.AreEqual(_originalEnumerable.InChunksOf(5).Count(), 3);
            Assert.AreEqual(_originalEnumerable.InChunksOf(1).Count(), 12);
        }
    }
}
