# CheesyTot.AzureTables.SimpleIndex

[![.NET](https://github.com/CheesyTot/AzureTables.SimpleIndex/actions/workflows/dotnet.yml/badge.svg)](https://github.com/CheesyTot/AzureTables.SimpleIndex/actions/workflows/dotnet.yml)

This project provides a simple way to add simple indexing to your Azure Table Storage entities.

### Decorate properties to be indexed
```
public class Cat : ITableEntity
{
    public Cat(string partitionKey, string rowKey)
    { }

    public string PartitionKey { get; set;}
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    
    [SimpleIndex]
    public string Breed { get; set; }
    
    public string Name { get; set; }
}
```
### Add SimpleIndexRepository to your project
Program.cs
```
builder.Services.Configure<RepositoryOptions>(options =>
{
    options.StorageConnectionString = "UseDevelopmentStorage=true";
    options.TablePrefix = "Example";
    options.IndexTableSuffix = "Index";
});

builder.Services.AddScoped<ISimpleIndexRepository<Cat>, SimpleIndexRepository<Cat>>();
```
### Query by indexed properties
```
public class MyDataAccess
{
    private readonly ISimpleIndexRepository<Cat> _catRepository;

    public MyDataAccess(ISimpleIndexRepository<Cat> catRepository)
    {
        _catRepository = catRepository;
    }
    
    public async Task<IEnumerable<Cat>> GetCatsByBreed(string breed)
    {
        return await _cat.GetByIndexedPropertyAsync("Breed", breed);
    }
}
```
