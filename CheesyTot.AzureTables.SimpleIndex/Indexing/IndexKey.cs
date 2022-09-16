using Azure.Data.Tables;
using CheesyTot.AzureTables.SimpleIndex.Attributes;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CheesyTot.AzureTables.SimpleIndex.Indexing
{
    /// <summary>
    /// Represents the PartitionKey that is used by the <see cref="CheesyTot.AzureTables.SimpleIndex.Indexing.Index">Index</see> class to specify the property name and value being indexed.
    /// </summary>
    public class IndexKey
    {
        private static readonly Regex _rxInvalidPkRkChars = new Regex("[/\\\\#?\\u0000\\u0001\\u0002\\u0003\\u0004\\u0005\\u0006\\u0007\\u0008\\u0009\\u000a\\u000b\\u000c\\u000d\\u000e\\u000f\\u00010\\u0011\\u0012\\u0013\\u0014\\u0015\\u0016\\u0017\\u0018\\u0019\\u001a\\u001b\\u001c\\u001d\\u001e\\u001f\\u007f\\u0080\\u0081\\u0082\\u0083\\u0084\\u0085\\u0086\\u0087\\u0088\\u0089\\u008a\\u008b\\u008c\\u008d\\u008e\\u008f\\u0090\\u0091\\u0092\\u0093\\u0094\\u0095\\u0096\\u0097\\u0098\\u0099\\u009a\\u009b\\u009c\\u009d\\u009e\\u009f]", RegexOptions.Compiled);
        private string _propertyValue;

        /// <summary>
        /// Constructor that accepts the propertyName and propertyValue as strings
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        public IndexKey(string propertyName, string propertyValue)
        {
            PropertyName = propertyName;
            _propertyValue = SanitizePropertyValue(propertyValue);
        }

        /// <summary>
        /// The name of the property to be indexed
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// The value of the property to be indexed
        /// </summary>
        /// <remarks>The value is sanitized to remove any characters that are not allowed in Azure Table Storage PartitionKeys.</remarks>
        public string PropertyValue
        {
            get => _propertyValue;
            set => _propertyValue = SanitizePropertyValue(value);
        }

        /// <summary>
        /// Overridden ToString() method returns a serialized version of the IndexKey.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{PropertyName}{Index.SEPARATOR}{PropertyValue}";

        /// <summary>
        /// Creates an IndexKey record by deserializing it from its serialized form.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static IndexKey FromString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return default;

            var parts = input.Split(new[] { Index.SEPARATOR }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return default;

            return new IndexKey(parts[0], parts[1]);
        }

        /// <summary>
        /// Sanitizes the propertyValue to replace any characters that are not allowed in Azure Table Storage PartitionKeys.
        /// </summary>
        /// <remarks>See https://docs.microsoft.com/en-us/rest/api/storageservices/Understanding-the-Table-Service-Data-Model#characters-disallowed-in-key-fields</remarks>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        public static string SanitizePropertyValue(string propertyValue) => _rxInvalidPkRkChars.Replace(propertyValue, "*");

        /// <summary>
        /// Determines the equality to another IndexKey based on the equality of the ToString() value
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as IndexKey;

            if (other == null)
                return false;

            return string.Equals(ToString(), obj.ToString());
        }

        /// <summary>
        /// Overridden GetHashCode method based on the value returned from the <see cref="ToString()">ToString()</see> method.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// Gets an IndexKey based on the provided propertyName and propertyValue, and whether the propertyName exists as an indexed property in the entity specifed by the <typeparamref name="T">type parameter</typeparamref>.
        /// </summary>
        /// <typeparam name="T">The type of the entity</typeparam>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the propertyName is null, empty, or all white space characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the corresponding property either does not exist in the entity type or is not decorated with the <see cref="CheesyTot.AzureTables.SimpleIndex.Attributes.SimpleIndexAttribute">SimpleIndexAttribute</see>.</exception>
        public static IndexKey GetIndexKey<T>(string propertyName, object propertyValue) where T : class, ITableEntity, new()
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            if(!GetIndexedPropertyInfos<T>().Any(x => x.Name == propertyName))
                throw new ArgumentOutOfRangeException($"Entity of Type {nameof(T)} does not have indexed property {propertyName} with PartitionKey");

            var strPropertyValue = propertyValue == null
                ? string.Empty
                : Convert.ToString(propertyValue);

            return new IndexKey(propertyName, strPropertyValue);
        }

        /// <summary>
        /// Gets the PropertyInfos for each property in the <typeparamref name="T">entity type</typeparamref> that are decorated with the <see cref="CheesyTot.AzureTables.SimpleIndex.Attributes.SimpleIndexAttribute">SimpleIndexAttribute</see>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static PropertyInfo[] GetIndexedPropertyInfos<T>() where T: class, ITableEntity, new()
        {
            return typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => Attribute.IsDefined(x, typeof(SimpleIndexAttribute)))
                .ToArray();
        }
    }
}