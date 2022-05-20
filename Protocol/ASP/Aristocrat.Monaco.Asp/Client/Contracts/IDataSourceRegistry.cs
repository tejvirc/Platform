namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    /// <summary>
    ///     Interface for data source container. Provides access to a registered data source by its name.
    /// </summary>
    public interface IDataSourceRegistry
    {
        /// <summary>
        ///     Register's a data source. Overwrites the existing data source with same name if exists.
        /// </summary>
        /// <param name="dataSource">The data source object to register.</param>
        void RegisterDataSource(IDataSource dataSource);

        /// <summary>
        ///     Returns a data source by name if found otherwise null.
        /// </summary>
        /// <param name="name"> name of the data source.</param>
        /// <returns></returns>
        IDataSource GetDataSource(string name);
    }
}