namespace Aristocrat.Monaco.Accounting.Contracts.CoinAcceptor
{
    using Application.Contracts.Localization;
    using Localization.Properties;

    /// <summary>
    ///     Coin extensions that need to be in the Accounting layer
    /// </summary>

    public static class CoinAccountingExtensions
    {
        /// <summary>Creates a message that tells the coin in details.</summary>
        /// <param name="detailsCode">The coin in details code.</param>
        /// <returns>The details of the coin.</returns>
        public static string GetDetailsMessage(int detailsCode)
        {
            switch ((CoinInDetails)detailsCode)
            {
                case CoinInDetails.CoinToCashBox:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CoinToCashBox);

                case CoinInDetails.CoinToHopper:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CoinToHopper);

                case CoinInDetails.CoinToCashBoxInsteadHopper:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CoinToCashBoxInsteadHopper);

                case CoinInDetails.CoinToHopperInsteadCashBox:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CoinToHopperInsteadCashBox);

                case CoinInDetails.None:
                default:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.None);
            }
        }
    }
}
