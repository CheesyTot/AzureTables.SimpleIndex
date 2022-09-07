using System;

namespace CheesyTot.AzureTables.SimpleIndex.Indexing
{
    public class IndexKey
    {
        public IndexKey(string propertyName, string propertyValue)
        {
            PropertyName = propertyName;
            PropertyValue = propertyValue;
        }

        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }

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
    }
}