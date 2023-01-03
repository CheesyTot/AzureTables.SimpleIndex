using Azure;
using Azure.Data.Tables;
using CheesyTot.AzureTables.SimpleIndex.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CheesyTot.AzureTables.SimpleIndex
{
    public static class Extensions
    {
        /// <summary>
        /// Extension for <see cref="Azure.AsyncPageable{T}">AsyncPageable</see> that gets all results as an IEnumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asyncPageable"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> AsEnumerableAsync<T>(this AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
        {
            var result = new List<T>();
            
            if(asyncPageable != null)
            {
                await foreach (var page in asyncPageable.AsPages())
                {
                    result.AddRange(page.Values);
                }
            }

            return result.AsEnumerable();
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
            if (asyncPageable == null)
                throw new InvalidOperationException("The input sequence contains no elements.");

            await foreach (var page in asyncPageable.AsPages())
            {
                return page.Values.First();
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
            if (asyncPageable == null)
                throw new InvalidOperationException("The input sequence contains no elements.");

            await foreach (var page in asyncPageable.AsPages())
            {
                return page.Values.FirstOrDefault();
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
            if (asyncPageable == null)
                throw new InvalidOperationException("The input sequence contains no elements.");

            await foreach (var page in asyncPageable.AsPages())
            {
                return page.Values.SingleOrDefault();
            }

            return default;
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
            if(asyncPageable == null)
                throw new InvalidOperationException("The input sequence contains no elements.");

            await foreach (var page in asyncPageable.AsPages())
            {
                return page.Values.Single();
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
            if (asyncPageable == null)
                throw new InvalidOperationException("The input sequence contains no elements.");

            var count = 0;

            await foreach (var page in asyncPageable.AsPages())
            {
                count += page.Values.Count;
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
            if(asyncPageable == null)
                throw new InvalidOperationException("The input sequence contains no elements.");

            var count = 0L;

            await foreach (var page in asyncPageable.AsPages())
            {
                count += page.Values.Count;
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
                throw new InvalidOperationException("The input sequence contains no elements.");

            await foreach (var page in asyncPageable.AsPages())
            {
                if (page.Values.Any())
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Extension for IAsyncEnumerable that returns the first item in the enumeration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asyncEnumerable"></param>
        /// <returns></returns>
        public static async Task<T> FirstOrDefault<T>(this IAsyncEnumerable<T> asyncEnumerable)
        {
            if (asyncEnumerable != null)
            {
                await foreach (var item in asyncEnumerable)
                {
                    return item;
                }
            }

            return default;
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
            if (input == null)
                throw new InvalidOperationException("The input sequence contains no elements.");

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
