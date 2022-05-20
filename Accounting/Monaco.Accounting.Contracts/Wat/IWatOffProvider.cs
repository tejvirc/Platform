namespace Aristocrat.Monaco.Accounting.Contracts.Wat
{
    using Kernel;
    using System;
    using TransferOut;

    /// <summary>
    ///     Provides a mechanism to interact with the Wat Off Provider
    /// </summary>
    public interface IWatOffProvider : ITransferOutProvider, IService
    {
        /// <summary>
        ///     Used by the host to request that the specified amounts be transferred off of the EGM
        /// </summary>
        /// <param name="requestId">The host transaction request identifier</param>
        /// <param name="cashable">The cashable amount requiring a handpay</param>
        /// <param name="promo">The promotional amount requiring a handpay</param>
        /// <param name="nonCashable">The non-cashable amount requiring a handpay</param>
        /// <param name="reduceAmount">Indicates whether the EGM is permitted to subsequently reduce the amounts of the transfer</param>
        /// <returns>
        ///     True if the request is to be sent, false if the provider determines it is unable to
        ///     send the request, either as a result of state or if the provider does not
        ///     support this functionality
        /// </returns>
        bool RequestTransfer(string requestId, long cashable, long promo, long nonCashable, bool reduceAmount);

        /// <summary>
        ///     Used by the host to request that the specified amounts be transferred off of the EGM
        /// </summary>
        /// <param name="transactionId">The bank transaction Id to use</param>
        /// <param name="requestId">The host transaction request identifier</param>
        /// <param name="cashable">The cashable amount requiring a handpay</param>
        /// <param name="promo">The promotional amount requiring a handpay</param>
        /// <param name="nonCashable">The non-cashable amount requiring a handpay</param>
        /// <param name="reduceAmount">Indicates whether the EGM is permitted to subsequently reduce the amounts of the transfer</param>
        /// <returns>
        ///     True if the request is to be sent, false if the provider determines it is unable to
        ///     send the request, either as a result of state or if the provider does not
        ///     support this functionality
        /// </returns>
        bool RequestTransfer(Guid transactionId, string requestId, long cashable, long promo, long nonCashable, bool reduceAmount);

        /// <summary>
        ///     Used by the host to cancel a pending request
        /// </summary>
        /// <param name="requestId">The host assigned request Id</param>
        /// <param name="hostException">The reason for cancelling the transfer</param>
        /// <returns>
        ///     True if the request can be cancelled
        /// </returns>
        bool CancelTransfer(string requestId, int hostException);

        /// <summary>
        ///     Used by the host to cancel a pending request
        /// </summary>
        /// <param name="transaction">The Wat Transaction to cancel</param>
        /// <param name="hostException">The reason for cancelling the transfer</param>
        /// <returns>
        ///     True if the request can be cancelled
        /// </returns>
        bool CancelTransfer(WatTransaction transaction, int hostException);

        /// <summary>
        ///     Used by the host to acknowledge the committed transaction
        /// </summary>
        /// <param name="requestId">The host assigned request Id</param>
        void AcknowledgeTransfer(string requestId);

        /// <summary>
        ///     Used by the host to acknowledge the committed transaction
        /// </summary>
        /// <param name="transaction">The Wat Transaction to acknowledge</param>
        void AcknowledgeTransfer(WatTransaction transaction);
    }
}