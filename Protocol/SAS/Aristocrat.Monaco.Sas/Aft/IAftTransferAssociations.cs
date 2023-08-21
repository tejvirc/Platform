namespace Aristocrat.Monaco.Sas.Aft
{
    using Aristocrat.Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     Interface for classes that provide AFT Transfer associations
    /// </summary>
    public interface IAftTransferAssociations
    {
        /// <summary>
        ///     Get the available transfers
        /// </summary>
        /// <returns>The available transfers</returns>
        AftAvailableTransfers GetAvailableTransfers();

        /// <summary>
        ///     Check whether the transfer conditions have been met
        /// </summary>
        /// <param name="data">The AFTGameLockAndStatusData</param>
        /// <returns>True if the transfer conditions have been met</returns>
        bool TransferConditionsMet(AftGameLockAndStatusData data);

        /// <summary>
        ///     Get the AFT Status
        /// </summary>
        /// <returns>The AFT Status</returns>
        AftStatus GetAftStatus();
    }
}