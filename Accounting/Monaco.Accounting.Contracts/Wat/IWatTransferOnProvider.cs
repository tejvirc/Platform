namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Threading.Tasks;
    using Kernel;

    /// <summary>
    ///     An interface which interacts with a component handling transfer on requests
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This interface must be implemented by a component which communicates with WAT
    ///         transfer hosts. After a transfer on request is initiated a host,
    ///         and the <see cref="IWatTransferOnHandler" /> service verifies it, the <see cref="IWatTransferOnHandler" />
    ///         service
    ///         waits for the <c>TransferRequestUpdate(…)</c> callback. This IWatTransferOnProvider
    ///         method takes a Boolean to indicate whether the transfer request is still good. If it
    ///         is false, then the IWatTransferOnProvider may try again or close out the transfer
    ///         request. If it is true, then the <see cref="IWatTransferOnHandler" /> service is ready to begin
    ///         the transfer request. The transfer is started by calling <c>RequestTransferOn(...)</c>
    ///         with a set of amounts grouped by <c>AccountType</c>s.
    ///     </para>
    ///     <para>
    ///         Once the <see cref="IWatTransferOnHandler" /> service completes the transfer, it will
    ///         call the <c>TransferOnComplete(…)</c>passing back a list of the transactions describing
    ///         the transfer. The IWatTransferOnProvider service can perform any closing work it needs
    ///         to for the transfer request and then call <c>TransferCommitted(…)</c> to close out the
    ///         transfer request to the <see cref="IWatTransferOnHandler" /> service.
    ///     </para>
    /// </remarks>
    [CLSCompliant(false)]
    public interface IWatTransferOnProvider:IService
    {
        /// <summary>
        ///     Gets a value indicating whether or not the provider has can accept WAT On transfers
        /// </summary>
        bool CanTransfer { get; }

        /// <summary>
        ///     Used to initiate a transfer from the the client (EGM)
        /// </summary>
        /// <param name="transaction">A WAT On transaction</param>
        /// <returns>true if the transfer was initiated</returns>
        Task<bool> InitiateTransfer(WatOnTransaction transaction);

        /// <summary>
        ///     Used to commit a transfer
        /// </summary>
        /// <param name="transaction">A WAT On transaction</param>
        Task CommitTransfer(WatOnTransaction transaction);
    }
}