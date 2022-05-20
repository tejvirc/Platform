namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;

    /// <summary>
    ///     Definition of the ITransactionRequestor interface.
    /// </summary>
    /// <remarks>
    ///     The implementation of this interface should always be associated with
    ///     the <see cref="ITransactionCoordinator" /> interface. When designing a
    ///     component which is involved in the transaction request, if you want
    ///     the component not to be blocked while requesting a transaction process,
    ///     you should consider implementing this interface.
    /// </remarks>
    public interface ITransactionRequestor
    {
        /// <summary>
        ///     Gets the requestor's guid
        /// </summary>
        Guid RequestorGuid { get; }

        /// <summary>
        ///     Called by the TransactionCoordinator when the requestor can retrieve its transactionId
        /// </summary>
        /// <param name="requestId">The requestId used to retrieve the transactionId</param>
        /// <remarks>
        ///     The <see cref="ITransactionCoordinator" /> file already provides an example about
        ///     how it is used.
        /// </remarks>
        void NotifyTransactionReady(Guid requestId);
    }
}
