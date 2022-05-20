namespace Aristocrat.Monaco.Gaming.Contracts.Barkeeper
{
    /// <summary>
    ///     Provides a mechanism to retrieve and store barkeeper data.
    /// </summary>
    public interface IBarkeeperPropertyProvider
    {
        /// <summary>
        ///     Gets barkeeper data.
        /// </summary>
        /// <returns>Data return from storage.</returns>
        BarkeeperStorageData GetBarkeeperStorageData();

        /// <summary>
        ///     Stores barkeeper data.
        /// </summary>
        /// <param name="data">The data to pass in.</param>
        void StoreBarkeeperData(IBarkeeperSettings data);
    }
}
