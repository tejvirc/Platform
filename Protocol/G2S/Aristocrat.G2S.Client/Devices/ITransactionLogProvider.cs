namespace Aristocrat.G2S.Client.Devices
{
    /// <summary>
    ///     Provides a mechanism to get the transaction log information for a device.
    /// </summary>
    public interface ITransactionLogProvider
    {
        /// <summary>
        ///     Gets the minimum number of log entries for the device.
        /// </summary>
        int MinLogEntries { get; }
    }
}