namespace Aristocrat.Monaco.Gaming.Contracts.Tickets
{
    using Kernel;

    /// <summary>
    ///     Creates printer-friendly formatted data associated with a given transaction, intended for use with game history tickets.
    /// </summary>
    public interface IGameRoundPrintFormatter : IService
    {
        /// <summary>
        ///     Finds additional transaction data for the given log sequence number and returns
        ///     a printer-friendly format of the data.
        /// </summary>
        /// <param name="logSequenceNumber">The log sequence number of the central transaction associated with the desired data.</param>
        /// <returns>A printer-friendly format of the data associated with the given log sequence number.</returns>
        string GetFormattedData(long logSequenceNumber);
    }
}
