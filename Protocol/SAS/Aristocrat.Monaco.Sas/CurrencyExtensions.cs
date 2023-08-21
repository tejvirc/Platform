namespace Aristocrat.Monaco.Sas
{
    using System;
    using Application.Contracts.Extensions;

    /// <summary>
    ///     Extension methods for currencies
    /// </summary>
    public static class CurrencyExtensions
    {
        /// <summary>
        ///     Converts cents into SAS accounting credits
        /// </summary>
        /// <param name="cents">The cents to convert into SAS account credits</param>
        /// <param name="accountingDenom">The accounting denom to use for this conversion</param>
        /// <returns>The value in the SAS accounting credits</returns>
        public static long CentsToAccountingCredits(this long cents, long accountingDenom)
        {
            return (long)Math.Floor(new decimal(cents) / accountingDenom);
        }

        /// <summary>
        ///     Converts millicents into SAS accounting credits
        /// </summary>
        /// <param name="millicents">The millicents to convert in SAS account credits</param>
        /// <param name="accountingDenom">The accounting denom to use for this conversion</param>
        /// <returns>The value in the SAS accounting credits</returns>
        public static long MillicentsToAccountCredits(this long millicents, long accountingDenom)
        {
            return millicents.MillicentsToCents().CentsToAccountingCredits(accountingDenom);
        }

        /// <summary>
        ///     Converts accounting credits into cents
        /// </summary>
        /// <param name="credits">The credits to convert</param>
        /// <param name="accountingDenom">The accounting denom for these credits</param>
        /// <returns>The converted value into cents</returns>
        public static long AccountingCreditsToCents(this long credits, long accountingDenom)
        {
            return credits * accountingDenom;
        }

        /// <summary>
        ///     Converts accounting credits into millicents
        /// </summary>
        /// <param name="credits">The credits to convert</param>
        /// <param name="accountingDenom">The accounting denom for these credits</param>
        /// <returns>The converted value into millicents</returns>
        public static long AccountingCreditsToMillicents(this long credits, long accountingDenom)
        {
            return credits.AccountingCreditsToCents(accountingDenom).CentsToMillicents();
        }
    }
}