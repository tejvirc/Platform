namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    /// <summary>
    /// Definition of the IAftOnTransferProvider interface.
    /// </summary>
    public interface IAftOnTransferProvider
    {
        /// <summary>Gets a value indicating whether an Aft on transaction can be performed at this time.</summary>
        bool IsAftOnAvailable { get; }

        /// <summary>Gets a value indicating whether an Aft Initialization is pending.</summary>
        bool IsAftPending { get; }

        /// <summary>Initiates an Aft on.</summary>
        /// <returns>True if the transaction is currently allowed; false otherwise.</returns>
        bool InitiateAftOn();

        /// <summary>Cancels the Aft request.</summary>
        void CancelAftOn();

        /// <summary>Performs the Aft On request.</summary>
        /// <param name="data">Data associated with the request.</param>
        /// <param name="partialAllowed">Indicates whether partial transfers are OK.</param>
        /// <returns>True if the request is accepted, false otherwise.</returns>
        bool AftOnRequest(AftData data, bool partialAllowed);

        /// <summary>Sends an event as a notification that the Aft On was rejected.</summary>
        void AftOnRejected();

        /// <summary>
        ///     Attempts to recover the provided transaction
        /// </summary>
        /// <param name="transactionId">The transaction id to recover</param>
        /// <returns>Whether or not the recovery was started</returns>
        bool Recover(string transactionId);

        /// <summary>
        ///     Acknowledges the requested transaction
        /// </summary>
        /// <param name="transactionId">The transaction id to acknowledge</param>
        void AcknowledgeTransfer(string transactionId);
    }
}
