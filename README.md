# CheesyTot.AzureTables.SimpleIndex

This project provides a simple way to add indexing to your Azure Table Storage entities.

### Decorate properties to be indexed

```
public class MyEntity : SimpleIndexTableEntity
{
    public MyEntity(string partitionKey, string rowKey)
        : base(partitionKey, rowKey)
    { }
    
    [SimpleIndex]
    public string IndexedProperty { get; set; }
}
```
