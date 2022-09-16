using Azure.Data.Tables;
using CheesyTot.AzureTables.SimpleIndex.Attributes;
using System.Linq;
using System.Text.RegularExpressions;

namespace CheesyTot.AzureTables.SimpleIndex.Helpers
{
    public static class TableNameHelper
    {
        public static string GetTableName<T>(string tablePrefix, string indexTableSuffix) where T : class, ITableEntity, new()
        {
            var tableNameAttribute = typeof(T).GetCustomAttributes(typeof(TableNameAttribute), true).FirstOrDefault() as TableNameAttribute;

            if (tableNameAttribute == null)
                return sanitizeTableName(typeof(T).Name, tablePrefix, indexTableSuffix);

            return sanitizeTableName(tableNameAttribute.Name, tablePrefix, indexTableSuffix);
        }

        private static string sanitizeTableName(string input, string tablePrefix, string indexTableSuffix)
        {
            var result = Regex.Replace(input, "[^A-Za-z0-9]", string.Empty);

            if (string.IsNullOrWhiteSpace(tablePrefix) && !Regex.IsMatch(result.Substring(0, 1), "[A-z]"))
                result = $"X{result}";

            var maxLength = 63 - (tablePrefix?.Length ?? 0) - (indexTableSuffix?.Length) ?? 0;

            if (result.Length > maxLength)
                result = result.Substring(0, maxLength);

            return $"{tablePrefix}{result}{indexTableSuffix}";
        }
    }
}
