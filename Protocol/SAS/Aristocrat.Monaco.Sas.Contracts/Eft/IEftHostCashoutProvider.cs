namespace Aristocrat.Monaco.Sas.Contracts.Eft
{
    using EftTransferProvider;

    /// <summary>
    ///     The EftHostCashOutProvider which will be used by the <see cref="IEftOffTransferProvider" />.
    ///     This Provider was written to support SAS EFT v5.02 section 8.8
    /// </summary>
    public interface IEftHostCashOutProvider
    {
        /// <summary>
        ///     Handles the host cashout. Determines if cashout can occur normally, and if so it will wait for 800 ms for host to
        ///     initiate a U type LP or the cashOutAccepted is called.
        ///     <para>Blocks the thread until either CashOutAccepted is called or a 800ms timer (which can be restarted) expires.</para>
        /// </summary>
        /// <returns>Returns true if it didn't wait.</returns>
        CashOutReason HandleHostCashOut();

        /// <summary>
        ///     This method is called when host contacts EGM for transferring credits out of the machine.
        ///     IHostCashOutProvider should release the pending block of HandleHostCashOut.
        /// </summary>
        /// <returns>Returns false if the transfer is not initiated by CashoutButtonpressedConsumer.</returns>
        bool CashOutAccepted();

        /// <summary>
        ///     Restarts the timer only if it has already been started.
        /// </summary>
        void RestartTimerIfPendingCallbackFromHost();
    }
}