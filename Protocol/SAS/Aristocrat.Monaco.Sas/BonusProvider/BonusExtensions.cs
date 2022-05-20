namespace Aristocrat.Monaco.Sas.BonusProvider
{
    using System;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts.Bonus;

    /// <summary>
    ///     Extensions for bonus items
    /// </summary>
    public static class BonusExtensions
    {
        /// <summary>
        ///     Gets the tax status from the provided bonus mode
        /// </summary>
        /// <param name="bonusMode">The bonus to get the tax status for</param>
        /// <returns>The tax status found</returns>
        public static TaxStatus GeTaxStatus(this BonusMode bonusMode)
        {
            switch (bonusMode)
            {
                case BonusMode.NonDeductible:
                    return TaxStatus.Nondeductible;
                case BonusMode.WagerMatchAllAtOnce:
                    return TaxStatus.WagerMatch;
                default:
                    return TaxStatus.Deductible;
            }
        }

        /// <summary>
        ///     Gets the bonus mode for the provided tax status
        /// </summary>
        /// <param name="taxStatus">the tax status to get the bonus mode for</param>
        /// <returns>The found bonus mode</returns>
        public static BonusMode GetBonusMode(this TaxStatus taxStatus)
        {
            switch (taxStatus)
            {
                case TaxStatus.Deductible:
                    return BonusMode.Standard;
                case TaxStatus.Nondeductible:
                    return BonusMode.NonDeductible;
                case TaxStatus.WagerMatch:
                    return BonusMode.WagerMatchAllAtOnce;
                default:
                    throw new ArgumentOutOfRangeException(nameof(taxStatus), taxStatus, null);
            }
        }
    }
}