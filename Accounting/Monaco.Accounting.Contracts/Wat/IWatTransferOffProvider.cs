namespace Aristocrat.Monaco.Accounting.Contracts.Wat
{
    using System;
    using System.Threading.Tasks;
    using Kernel;

    /// <summary>
    ///     An interface which interacts with a component handling transfer off requests
    /// </summary>
    [CLSCompliant(false)]
    public interface IWatTransferOffProvider: IService
    {
        /// <summary>
        ///     Gets a value indicating whether or not the provider has can accept WAT Off transfers
        /// </summary>
        bool CanTransfer { get; }

        /// <summary>
        ///     Used to initiate a transfer from the the client (EGM)
        /// </summary>
        /// <param name="transaction">A WAT transaction</param>
        /// <returns>true if the transfer was initiated</returns>
        Task<bool> InitiateTransfer(WatTransaction transaction);

        /// <summary>
        ///     Used to commit a transfer
        /// </summary>
        /// <param name="transaction">A WAT transaction</param>
        Task CommitTransfer(WatTransaction transaction);
    }
}