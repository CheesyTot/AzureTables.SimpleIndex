# AzureTables.SimpleIndex
### A simple way to accomplish secondary indexing in Azure Table Storage.
*This README assumes a basic working knowledge of Azure Table Storage. Please familiarize yourself with the basics by browsing the documentation at https://docs.microsoft.com/en-us/azure/storage/tables/.*

Azure Table Storage is fantastic for simple applications that need to cheaply and quickly store and access a lot of data. But it's severly limited on querying that data because the only indexed properties are the Partition Key and the Row Key. Querying by other properties or even just the RowKey alone results in a full table scan, which is horribly inefficient.

Microsoft provides several design patterns for Azure Table Storage at https://docs.microsoft.com/en-us/azure/storage/tables/table-storage-design-patterns. One popular pattern found on many StackOverflow is the *Intra-partition secondary index pattern*, which involves storing multiple copies of each entity using different RowKey values in the same partition to enable fast and efficient lookups and alternate sort orders.
