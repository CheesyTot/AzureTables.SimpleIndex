using Azure;
using Azure.Core;
using Azure.Data.Tables;
using Azure.Data.Tables.Sas;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheesyTot.AzureTables.SimpleIndex.Azure
{
    public class SimpleIndexTableClient : TableClient, ITableClient
    {
        public SimpleIndexTableClient(Uri endpoint, TableClientOptions options = null)
            : base(endpoint, options)
        { }

        public SimpleIndexTableClient(string connectionString, string tableName)
            : base(connectionString, tableName)
        { }

        public SimpleIndexTableClient(Uri endpoint, AzureSasCredential credential, TableClientOptions options = null)
            : base(endpoint, credential, options)
        { }

        public SimpleIndexTableClient(Uri endpoint, string tableName, TableSharedKeyCredential credential)
            : base(endpoint, tableName, credential)
        { }

        public SimpleIndexTableClient(string connectionString, string tableName, TableClientOptions options = null)
            : base(connectionString, tableName, options)
        { }

        public SimpleIndexTableClient(Uri endpoint, string tableName, TableSharedKeyCredential credential, TableClientOptions options = null)
            : base(endpoint, tableName, credential, options)
        { }

        public SimpleIndexTableClient(Uri endpoint, string tableName, TokenCredential tokenCredential, TableClientOptions options = null)
            : base(endpoint, tableName, tokenCredential, options)
        { }

        protected SimpleIndexTableClient()
        { }
    }
}
