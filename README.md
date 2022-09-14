# CheesyTot.AzureTables.SimpleIndex

[![.NET](https://github.com/CheesyTot/AzureTables.SimpleIndex/actions/workflows/dotnet.yml/badge.svg)](https://github.com/CheesyTot/AzureTables.SimpleIndex/actions/workflows/dotnet.yml)

This project provides a simple way to add indexing to your Azure Table Storage entities.

### Decorate properties to be indexed

```
public class Cat : SimpleIndexTableEntity
{
    public Cat(string partitionKey, string rowKey)
        : base(partitionKey, rowKey)
    { }
    
    [SimpleIndex]
    public string Breed { get; set; }
    
    public string Name { get; set; }
}
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
