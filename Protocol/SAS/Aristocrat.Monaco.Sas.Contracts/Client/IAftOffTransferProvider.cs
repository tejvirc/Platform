namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    /// <summary>
    /// Definition of the IAftOffTransferProvider interface.
    /// </summary>
    public interface IAftOffTransferProvider
    {
        /// <summary>Gets a value indicating whether an Aft Initialization is pending.</summary>
        bool IsAftPending { get; }

        /// <summary> Gets a value indicating whether it's waiting for a key-off. </summary>
        bool WaitingForKeyOff { get; }

        /// <summary>Gets a value indicating whether an Aft off transaction can be performed at this time.</summary>
        bool IsAftOffAvailable { get; }

        /// <summary>Initiates an Aft off.</summary>
        /// <returns>True if the transaction is currently allowed; false otherwise.</returns>
        bool InitiateAftOff();

        /// <summary>Cancels the Aft request.</summary>
        void CancelAftOff();

        /// <summary>Performs the Aft Off request.</summary>
        /// <param name="data">Data associated with the request.</param>
        /// <param name="partialAllowed">Indicates whether partial transfers are OK.</param>
        /// <returns>True if the request is accepted, false otherwise.</returns>
        bool AftOffRequest(AftData data, bool partialAllowed);

        /// <summary>Sends an event as a notification that the Aft Off was rejected.</summary>
        void AftOffRejected();

        /// <summary>
        ///     Attempts to recover the provided transaction
        /// </summary>
        /// <param name="transactionId">The transaction id to recover</param>
        /// <returns>Whether or not the recovery was started</returns>
        bool Recover(string transactionId);

        /// <summary>Called when the lockup is keyed off. </summary>
        void OnKeyedOff();

        /// <summary>
        ///     Acknowledges the requested transaction
        /// </summary>
        /// <param name="transactionId">The transaction id to acknowledge</param>
        void AcknowledgeTransfer(string transactionId);
    }
}
