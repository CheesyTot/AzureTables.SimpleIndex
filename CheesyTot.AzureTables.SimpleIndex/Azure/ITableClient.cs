using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Azure.Data.Tables.Sas;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace CheesyTot.AzureTables.SimpleIndex.Azure
{
    public interface ITableClient
    {
        TableSasBuilder GetSasBuilder(TableSasPermissions permissions, DateTimeOffset expiresOn);
        Response<TableItem> Create(CancellationToken cancellationToken = default(CancellationToken));
        Task<Response<TableItem>> CreateAsync(CancellationToken cancellationToken = default(CancellationToken));
        Response<TableItem> CreateIfNotExists(CancellationToken cancellationToken = default(CancellationToken));
        Task<Response<TableItem>> CreateIfNotExistsAsync(CancellationToken cancellationToken = default(CancellationToken));
        Response Delete(CancellationToken cancellationToken = default(CancellationToken));
        Task<Response> DeleteAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<Response> AddEntityAsync<T>(T entity, CancellationToken cancellationToken = default(CancellationToken)) where T : ITableEntity;
        Response AddEntity<T>(T entity, CancellationToken cancellationToken = default(CancellationToken)) where T : ITableEntity;
        Response<T> GetEntity<T>(string partitionKey, string rowKey, IEnumerable<string> select = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class, ITableEntity, new();
        Task<Response<T>> GetEntityAsync<T>(string partitionKey, string rowKey, IEnumerable<string> select = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class, ITableEntity, new();
        Task<Response> UpsertEntityAsync<T>(T entity, TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default(CancellationToken)) where T : ITableEntity;
        Response UpsertEntity<T>(T entity, TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default(CancellationToken)) where T : ITableEntity;
        Task<Response> UpdateEntityAsync<T>(T entity, ETag ifMatch, TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default(CancellationToken)) where T : ITableEntity;
        Response UpdateEntity<T>(T entity, ETag ifMatch, TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default(CancellationToken)) where T : ITableEntity;
        AsyncPageable<T> QueryAsync<T>(Expression<Func<T, bool>> filter, int? maxPerPage = null, IEnumerable<string> select = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class, ITableEntity, new();
        Pageable<T> Query<T>(Expression<Func<T, bool>> filter, int? maxPerPage = null, IEnumerable<string> select = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class, ITableEntity, new();
        AsyncPageable<T> QueryAsync<T>(string filter = null, int? maxPerPage = null, IEnumerable<string> select = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class, ITableEntity, new();
        Pageable<T> Query<T>(string filter = null, int? maxPerPage = null, IEnumerable<string> select = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class, ITableEntity, new();
        Task<Response> DeleteEntityAsync(string partitionKey, string rowKey, ETag ifMatch = default(ETag), CancellationToken cancellationToken = default(CancellationToken));
        Response DeleteEntity(string partitionKey, string rowKey, ETag ifMatch = default(ETag), CancellationToken cancellationToken = default(CancellationToken));
        Task<Response<IReadOnlyList<TableSignedIdentifier>>> GetAccessPoliciesAsync(CancellationToken cancellationToken = default(CancellationToken));
        Response<IReadOnlyList<TableSignedIdentifier>> GetAccessPolicies(CancellationToken cancellationToken = default(CancellationToken));
        Task<Response> SetAccessPolicyAsync(IEnumerable<TableSignedIdentifier> tableAcl, CancellationToken cancellationToken = default(CancellationToken));
        Response SetAccessPolicy(IEnumerable<TableSignedIdentifier> tableAcl, CancellationToken cancellationToken = default(CancellationToken));
        Task<Response<IReadOnlyList<Response>>> SubmitTransactionAsync(IEnumerable<TableTransactionAction> transactionActions, CancellationToken cancellationToken = default(CancellationToken));
        Response<IReadOnlyList<Response>> SubmitTransaction(IEnumerable<TableTransactionAction> transactionActions, CancellationToken cancellationToken = default(CancellationToken));
        Uri GenerateSasUri(TableSasPermissions permissions, DateTimeOffset expiresOn);
        Uri GenerateSasUri(TableSasBuilder builder);
    }
}
