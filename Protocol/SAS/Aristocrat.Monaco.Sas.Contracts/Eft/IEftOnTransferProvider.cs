namespace Aristocrat.Monaco.Sas.Contracts.Eft
{
    using Accounting.Contracts;

    /// <summary>
    /// Definition of IEftOnTransferProvider
    /// </summary>
    public interface IEftOnTransferProvider
    {
        /// <summary>
        /// Process an EFT on request
        /// </summary>
        /// <returns>True if successful</returns>
        bool EftOnRequest(string requestID, AccountType accountType, ulong amount);

        /// <summary>
        /// Gets a value indicating whether or not the provider can accept the On transfers
        /// </summary>
        bool CanTransfer { get; }

        /// <summary>
        /// Gets the transfer in amount the provider allows. 
        /// </summary>
        (ulong Amount, bool LimitExceeded) GetAcceptedTransferInAmount(ulong amount);
    }
}