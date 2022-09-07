using System;

namespace CheesyTot.AzureTables.SimpleIndex.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SimpleIndexAttribute : Attribute
    { }
}
