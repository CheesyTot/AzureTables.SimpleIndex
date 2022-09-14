using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheesyTot.AzureTables.SimpleIndex
{
    public static class InternalExtensions
    {
        public static async Task<IEnumerable<T>> AsEnumerableAsync<T>(this AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
        {
            if (asyncPageable == null)
                return Enumerable.Empty<T>();

            var enumerator = asyncPageable.GetAsyncEnumerator();
            var result = new List<T>();

            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    result.Add(enumerator.Current);
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            return result;
        }

        public static async Task<T> FirstAsync<T>(this AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
        {
            if (asyncPageable != null)
            {
                var enumerator = asyncPageable.GetAsyncEnumerator();

                try
                {
                    while (await enumerator.MoveNextAsync())
                    {
                        return enumerator.Current;
                    }
                }
                finally
                {
                    await enumerator.DisposeAsync();
                }
            }

            throw new InvalidOperationException("The input sequence contains no elements.");
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
        {
            if (asyncPageable != null)
            {
                var enumerator = asyncPageable.GetAsyncEnumerator();

                try
                {
                    while (await enumerator.MoveNextAsync())
                    {
                        return enumerator.Current;
                    }
                }
                finally
                {
                    await enumerator.DisposeAsync();
                }
            }

            return default;
        }

        public static async Task<T> SingleOrDefaultAsync<T>(this AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
        {
            T result = default;

            if (asyncPageable != null)
            {
                var enumerator = asyncPageable.GetAsyncEnumerator();
                var count = 0;

                try
                {
                    while (await enumerator.MoveNextAsync())
                    {
                        if (++count > 1)
                            throw new InvalidOperationException("The input sequence contains more than one element.");
                        result = enumerator.Current;
                    }
                }
                finally
                {
                    await enumerator.DisposeAsync();
                }
            }

            return result;
        }

        public static async Task<T> SingleAsync<T>(this AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
        {
            if (asyncPageable != null)
            {
                var enumerator = asyncPageable.GetAsyncEnumerator();
                T result = default;
                var count = 0;

                try
                {
                    while (await enumerator.MoveNextAsync())
                    {
                        if (++count > 1)
                            throw new InvalidOperationException("The input sequence contains more than one element.");
                        result = enumerator.Current;
                    }
                }
                finally
                {
                    await enumerator.DisposeAsync();
                }

                if (result != default)
                    return result;
            }

            throw new InvalidOperationException("The input sequence contains no elements.");
        }

        public static async Task<int> CountAsync<T>(this AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
        {
            var count = 0;

            if (asyncPageable != null)
            {
                var enumerator = asyncPageable.GetAsyncEnumerator();

                try
                {
                    while (await enumerator.MoveNextAsync())
                    {
                        count++;
                    }
                }
                finally
                {
                    await enumerator.DisposeAsync();
                }
            }

            return count;
        }

        public static async Task<long> LongCountAsync<T>(this AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
        {
            var count = 0L;

            if (asyncPageable != null)
            {
                var enumerator = asyncPageable.GetAsyncEnumerator();

                try
                {
                    while (await enumerator.MoveNextAsync())
                    {
                        count++;
                    }
                }
                finally
                {
                    await enumerator.DisposeAsync();
                }
            }

            return count;
        }

        public static async Task<bool> AnyAsync<T>(this AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
        {
            if (asyncPageable == null)
                return false;

            var enumerator = asyncPageable.GetAsyncEnumerator();

            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    return true;
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            return false;
        }

        public static IEnumerable<IEnumerable<T>> InChunksOf<T>(this IEnumerable<T> input, int chunkSize)
        {
            if (chunkSize == input.Count())
                return new[]
                {
                    input
                };

            return input.Select((x, i) => new { Index = i, Value = x })
                .Where(x => x.Value != null)
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value));
        }
    }
}
