namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    using System;

    /// <summary>
    ///     Interface for a data source that can get/set values of different data members, and which implements IDisposable.
    ///     Use this when your datasource subscribes to .Net or event bus events. Otherwise use IDataSource.
    /// </summary>
    public interface IDisposableDataSource : IDataSource, IDisposable { }
}