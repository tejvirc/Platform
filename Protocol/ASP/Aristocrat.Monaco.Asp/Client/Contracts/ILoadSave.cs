namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    /// <summary>
    ///     An interface to load or save state of an object.
    /// </summary>
    public interface ILoadSave
    {
        /// <summary>
        ///     Loads field from DataSource.
        /// </summary>
        void Load();

        /// <summary>
        ///     Saves field to DataSource.
        /// </summary>
        void Save();
    }
}