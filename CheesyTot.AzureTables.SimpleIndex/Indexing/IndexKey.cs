using System;
using System.Text.RegularExpressions;

namespace CheesyTot.AzureTables.SimpleIndex.Indexing
{
    public class IndexKey
    {
        private static readonly Regex _rxInvalidPkRkChars = new Regex("[/\\\\#?\\u0000\\u0001\\u0002\\u0003\\u0004\\u0005\\u0006\\u0007\\u0008\\u0009\\u000a\\u000b\\u000c\\u000d\\u000e\\u000f\\u00010\\u0011\\u0012\\u0013\\u0014\\u0015\\u0016\\u0017\\u0018\\u0019\\u001a\\u001b\\u001c\\u001d\\u001e\\u001f\\u007f\\u0080\\u0081\\u0082\\u0083\\u0084\\u0085\\u0086\\u0087\\u0088\\u0089\\u008a\\u008b\\u008c\\u008d\\u008e\\u008f\\u0090\\u0091\\u0092\\u0093\\u0094\\u0095\\u0096\\u0097\\u0098\\u0099\\u009a\\u009b\\u009c\\u009d\\u009e\\u009f]", RegexOptions.Compiled);
        private string _propertyValue;

        public IndexKey(string propertyName, string propertyValue)
        {
            PropertyName = propertyName;
            _propertyValue = SanitizePropertyValue(propertyValue);
        }

        public string PropertyName { get; set; }

        public string PropertyValue
        {
            get => _propertyValue;
            set => _propertyValue = SanitizePropertyValue(value);
        }

        public override string ToString() => $"{PropertyName}{Index.SEPARATOR}{PropertyValue}";

        public static IndexKey FromString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return default;

            var parts = input.Split(new[] { Index.SEPARATOR }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return default;

            return new IndexKey(parts[0], parts[1]);
        }

        // https://docs.microsoft.com/en-us/rest/api/storageservices/Understanding-the-Table-Service-Data-Model#characters-disallowed-in-key-fields
        public static string SanitizePropertyValue(string propertyValue) => _rxInvalidPkRkChars.Replace(propertyValue, "*");

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            return string.Equals(ToString(), obj.ToString());
        }
    }
}