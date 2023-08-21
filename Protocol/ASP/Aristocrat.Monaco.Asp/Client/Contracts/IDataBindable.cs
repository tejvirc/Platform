namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    /// <summary>
    ///     Interface for binding a data source to an object.
    /// </summary>
    public interface IDataBindable
    {
        /// <summary>
        ///     name of the data source to be bound with.
        /// </summary>
        string DataSourceName { get; }

        /// <summary>
        ///     name of the data member to be bound with.
        /// </summary>
        string DataMemberName { get; }

        /// <summary>
        ///     Data source object that the object is currently bounded with.
        /// </summary>
        IDataSource DataSource { get; set; }
    }
}