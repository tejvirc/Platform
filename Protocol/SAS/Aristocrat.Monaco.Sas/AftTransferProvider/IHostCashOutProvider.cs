namespace Aristocrat.Monaco.Sas.AftTransferProvider
{
    using System.Threading.Tasks;
    using Accounting.Contracts.Wat;

    /// <summary>
    ///     The host cost cashout provider
    /// </summary>
    public interface IHostCashOutProvider
    {
        /// <summary>
        ///     Gets whether or not we can cashout to the host
        /// </summary>
        bool CanCashOut { get; }

        /// <summary>
        ///     Gets whether or not we have pending host cashout win pending
        /// </summary>
        bool CashOutWinPending { get; }

        /// <summary>
        ///     Gets whether or not there is a pending host cashout
        /// </summary>
        bool HostCashOutPending { get; }

        /// <summary>
        ///     Gets a value indicating the current host cashout settings
        /// </summary>
        HostCashOutMode CashOutMode { get; }

        /// <summary>
        ///     Gets the current host cashout transaction
        /// </summary>
        WatTransaction CashOutTransaction { get; }

        /// <summary>
        ///     Gets whether or not the host is locked up in cashout mode
        /// </summary>
        bool LockedUp { get; }

        /// <summary>
        ///     Handles the host cashout
        /// </summary>
        /// <param name="transaction">The transaction to cashout</param>
        /// <returns>Whether or not the host cashout was accepted</returns>
        Task<bool> HandleHostCashOut(WatTransaction transaction);

        /// <summary>
        ///     Used to signal when the cashout is requested was accepted
        /// </summary>
        void CashOutAccepted();

        /// <summary>
        ///     Used to signal when the cashout was denied
        /// </summary>
        void CashOutDenied();

        /// <summary>
        ///     Used to reset the exception timer
        /// </summary>
        void ResetCashOutExceptionTimer();
    }
}