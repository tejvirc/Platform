
namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    using System;

    /// <summary>
    /// Attribute to identify IDataSourceRegistry properties that require dynamic registration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DatasourceRegistryAttribute : Attribute
    { 
    }
}
