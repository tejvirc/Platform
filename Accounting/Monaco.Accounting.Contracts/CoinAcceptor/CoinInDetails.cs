namespace Aristocrat.Monaco.Accounting.Contracts.CoinAcceptor
{
    /// <summary>
    ///     Definition of the CurrencyInExceptionCode enum
    /// </summary>
    public enum CoinInDetails
    {
        /// <summary>
        ///     None
        /// </summary>
        None,

        /// <summary>
        ///     A coin inserted in cashBox
        /// </summary>
        CoinToCashBox,

        /// <summary>
        ///     A coin inserted in Hopper
        /// </summary>
        CoinToHopper,

        /// <summary>
        ///     A coin inserted in cashBox instead of Hopper
        /// </summary>
        CoinToCashBoxInsteadHopper,

        /// <summary>
        ///     A coin inserted in hopper instead cashBox
        /// </summary>
        CoinToHopperInsteadCashBox,
    }
}
