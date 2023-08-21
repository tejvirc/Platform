namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using System;

    /// <summary>Definition of the ISasTransactionCoordinator interface.</summary>
    public interface ISasTransactionCoordinator
    {
        /// <summary>
        /// Retrieves a valid transaction from the TransactionCoordinator.
        /// </summary>
        /// <param name="timeout">Amount of time in milliseconds to wait to start a transaction </param>
        /// <returns>A non-empty GUID if a transaction could be started, empty GUID otherwise.</returns>
        Guid StartTransaction(int timeout);

        /// <summary>
        /// End the transaction.
        /// </summary>
        /// <param name="transactionId">Id of the transaction to end.</param>
        void EndTransaction(Guid transactionId);
    }
}
