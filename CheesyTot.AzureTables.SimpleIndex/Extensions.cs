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
        /// <summary>
        /// Extension for <see cref="Azure.AsyncPageable{T}">AsyncPageable</see> that gets all results as an IEnumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asyncPageable"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Extension for <see cref="Azure.AsyncPageable{T}">AsyncPageable</see> that returns the first item, or throws if the collection is empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asyncPageable"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown if there are no items.</exception>
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

        /// <summary>
        /// Extension for <see cref="Azure.AsyncPageable{T}">AsyncPageable</see> that returns the first item, or null if the collection is empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asyncPageable"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Extension for <see cref="Azure.AsyncPageable{T}">AsyncPageable</see> that returns a single item, returns null if the collection is empty, or throws if there are no items in the collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asyncPageable"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown if there are no items in the collection.</exception>
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

        /// <summary>
        /// Extension for <see cref="Azure.AsyncPageable{T}">AsyncPageable</see> that returns a single item or throws if there are no items or if there are more than one item in the collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asyncPageable"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
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

        /// <summary>
        /// Extension for <see cref="Azure.AsyncPageable{T}">AsyncPageable</see> that returns the count of the items in the collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asyncPageable"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Extension for <see cref="Azure.AsyncPageable{T}">AsyncPageable</see> that returns the LongCount (count as Int64) of items in the colleciton.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asyncPageable"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Extension for <see cref="Azure.AsyncPageable{T}">AsyncPageable</see> that returns true if there are items in the collection, or false if there are not.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asyncPageable"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Extension for <see cref="System.Collections.IEnumerable">IEnumerable</see> that divides the collection up into multiple collections specified by <paramref name="chunkSize">chunkSize</paramref>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
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
