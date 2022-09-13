using Azure;

namespace CheesyTot.AzureTables.SimpleIndex.Tests
{
    internal class TestPage<T> : Page<T>
    {
        private IReadOnlyList<T> _values;
        private string _continuationToken;

        public TestPage(T[] values, string continuationToken)
        {
            _values = values;
            _continuationToken = continuationToken;
        }

        public override IReadOnlyList<T> Values => _values;

        public override string ContinuationToken => _continuationToken;

        public override Response GetRawResponse()
        {
            return Response.FromValue<Page<T>>(this, default) as Response;
        }
    }
}
