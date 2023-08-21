namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    /// <summary>
    ///     The Real Time Event Reporting status provider
    /// </summary>
    public interface IRteStatusProvider
    {
        /// <summary>
        ///     Gets or sets a value indicating whether
        ///     client 1 Real Time Event Reporting is enabled or not
        /// </summary>
        bool Client1RteEnabled { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether
        ///     client 2 Real Time Event Reporting is enabled or not
        /// </summary>
        bool Client2RteEnabled { get; }
    }
}