namespace Aristocrat.Monaco.Sas.BonusProvider
{
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Gaming.Contracts.Bonus;

    /// <inheritdoc />
    public class NullBonusProvider : ISasBonusCallback
    {
        /// <inheritdoc />
        public AftTransferStatusCode AwardAftBonus(AftData data)
        {
            return AftTransferStatusCode.GamingMachineUnableToPerformTransfer;
        }

        /// <inheritdoc />
        public bool Recover(string transactionId)
        {
            return false;
        }

        /// <inheritdoc />
        public BonusTransaction GetLastPaidLegacyBonus()
        {
            return null;
        }

        /// <inheritdoc />
        public void AcknowledgeBonus(string bonusId)
        {
        }

        /// <inheritdoc />
        public void AwardLegacyBonus(long amount, TaxStatus taxStatus)
        {
        }

        /// <inheritdoc />
        public bool IsAftBonusAllowed(AftData data)
        {
            return false;
        }

        /// <inheritdoc />
        public void SetGameDelay(uint delayTime)
        {
        }
    }
}