using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheesyTot.AzureTables.SimpleIndex.Extensions
{
    internal static class InternalExtensions
    {
        internal static async Task<IEnumerable<T>> AsEnumerableAsync<T>(this Azure.AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
        {
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

        internal static async Task<T> FirstAsync<T>(this Azure.AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
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

            throw new InvalidOperationException("The input sequence contains no elements.");
        }

        internal static async Task<T> FirstOrDefaultAsync<T>(this Azure.AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
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

            return default;
        }

        internal static async Task<T> SingleOrDefaultAsync<T>(this Azure.AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
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

            return result;
        }

        internal static async Task<T> SingleAsync<T>(this Azure.AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
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

            if (result == default)
                throw new InvalidOperationException("The input sequence contains no elements.");

            return result;
        }

        internal static async Task<int> Count<T>(this Azure.AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
        {
            var enumerator = asyncPageable.GetAsyncEnumerator();
            var count = 0;

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

            return count;
        }

        internal static async Task<long> LongCount<T>(this Azure.AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
        {
            var enumerator = asyncPageable.GetAsyncEnumerator();
            var count = 0L;

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

            return count;
        }

        internal static async Task<bool> Any<T>(this Azure.AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
        {
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

        internal static IEnumerable<IEnumerable<T>> InChunksOf<T>(this IEnumerable<T> input, int chunkSize)
        {
            return input.Select((x, i) => new { Index = i, Value = x })
                .Where(x => x.Value != null)
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value));
        }
    }
}
