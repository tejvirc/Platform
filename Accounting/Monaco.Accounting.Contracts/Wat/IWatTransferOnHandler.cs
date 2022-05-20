namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Kernel;

    /// <summary>
    ///     An interface that handles electronic transfer on of credits
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The service implementing this interface must be associated with a service implementing
    ///         <see cref="IWatTransferOnProvider" /> which receives funds transfer requests from the
    ///         WAT transfer hosts. When a request to transfer on credits is received from the hosts in
    ///         the provider, <c>InitiateTransferRequest()</c> with or without providing a transaction
    ///         Guid is called to initiate a request; this allows the IWatTransferOnHandler service verify that
    ///         the request is able to be handled. After the request is initiated successfully,
    ///         <c>TransferOnRequest(...)</c> is called to transfer on credits from accounts specified through
    ///         the method parameter. You can use <c>CancelRequest(…)</c> to end the transfer request
    ///         already initiated.
    ///     </para>
    ///     <para>
    ///         The client receives a WatOnStartedEvent from the IWatTransferOnHandler service informing the
    ///         client of a transfer on attempt beginning. At this point the client can perform any action
    ///         it needs or wants to such as putting up a message that a transfer on is in progress. When
    ///         the client receives a WatOnCompleteEvent, the client can perform any clean up that is
    ///         necessary such as removing the message from the screen. At this point, the transfer on
    ///         request has been completed.
    ///     </para>
    /// </remarks>
    [CLSCompliant(false)]
    public interface IWatTransferOnHandler : IService
    {
        /// <summary>
        ///     Used by the host to cancel a pending request
        /// </summary>
        /// <param name="requestId">The host assigned request Id</param>
        /// <param name="hostException">The reason for cancelling the transfer</param>
        /// <returns>True if the request can be cancelled</returns>
        bool CancelTransfer(string requestId, int hostException);

        /// <summary>
        ///     Used by the host to cancel a pending request
        /// </summary>
        /// <param name="transaction">The Wat Transaction to cancel</param>
        /// <param name="hostException">The reason for cancelling the transfer</param>
        /// <returns>True if the request can be cancelled</returns>
        bool CancelTransfer(WatOnTransaction transaction, int hostException);

        /// <summary>
        ///     Used by the host to request that the specified amounts be transferred on to the EGM
        /// </summary>
        /// <param name="transactionId">The bank transaction Id to use</param>
        /// <param name="requestId">The host transaction request identifier</param>
        /// <param name="cashable">The cashable amount</param>
        /// <param name="promo">The promotional amount</param>
        /// <param name="nonCashable">The non-cashable amount</param>
        /// <param name="reduceAmount">Indicates whether the EGM is permitted to subsequently reduce the amounts of the transfer</param>
        /// <returns>
        ///     True if the request is to be sent, false if the provider determines it is unable to
        ///     send the request, either as a result of state or if the provider does not
        ///     support this functionality
        /// </returns>
        bool RequestTransfer(Guid transactionId, string requestId, long cashable, long promo, long nonCashable, bool reduceAmount);

        /// <summary>
        ///     Used by the host to acknowledge the committed transaction
        /// </summary>
        /// <param name="transaction">The Wat On Transaction to acknowledge</param>
        void AcknowledgeTransfer(WatOnTransaction transaction);

        /// <summary>
        ///     Checks whether or not the provided transaction ID can be recovered
        /// </summary>
        /// <param name="transactionId">The transaction ID to recover</param>
        /// <returns>Whether or not the provided transaction ID can be recovered</returns>
        bool CanRecover(Guid transactionId);

        /// <summary>
        ///     Will attempt recover the provided transaction ID
        /// </summary>
        /// <param name="transactionId">The transaction ID to recover</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the recovery task</param>
        /// <returns>Whether on the recovery has finished</returns>
        Task<bool> Recover(Guid transactionId, CancellationToken cancellationToken);
    }
}