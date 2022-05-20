namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts.Bonus;

    /// <summary>Definition of the ISasBonusCallback interface.</summary>
    public interface ISasBonusCallback
    {
        /// <summary>
        /// Sets the legacy bonusing delay time.  The client should delay for the specified amount
        /// at the end of every game.
        /// </summary>
        /// <param name="delayTime">The amount to delay, in milliseconds.</param>
        void SetGameDelay(uint delayTime);

        /// <summary>Awards a legacy bonus.</summary>
        /// <param name="amount">The amount of the bonus, in millicents.</param>
        /// <param name="taxStatus">The tax status of the bonus.</param>
        void AwardLegacyBonus(long amount, TaxStatus taxStatus);

        /// <summary>Indicates whether an Aft bonus is able to be awarded.</summary>
        /// <param name="data">Aft data associated with the bonus.</param>
        /// <returns>True if the bonus can be awarded; false otherwise.</returns>
        bool IsAftBonusAllowed(AftData data);

        /// <summary>Awards an Aft bonus.</summary>
        /// <param name="data">Aft data associated with the bonus.</param>
        /// <returns>The transfer status.</returns>
        AftTransferStatusCode AwardAftBonus(AftData data);

        /// <summary>
        ///     Attempts to recover the provided transaction
        /// </summary>
        /// <param name="transactionId">The transaction id to recover</param>
        /// <returns>Whether or not the recovery was started</returns>
        bool Recover(string transactionId);

        /// <summary>
        ///     Gets the last paid legacy bonus transaction
        /// </summary>
        /// <returns>The transaction found or null</returns>
        BonusTransaction GetLastPaidLegacyBonus();

        /// <summary>
        ///     Acknowledge the bonus transaction
        /// </summary>
        /// <param name="bonusId">The bonus ID to acknowledge</param>
        void AcknowledgeBonus(string bonusId);
    }
}
