namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System.ComponentModel;

    /// <summary>
    ///     Enumeration containing the predefined account types for use with the Bank.
    /// </summary>
    public enum AccountType
    {
        /// <summary>
        ///     Cashable credits.
        /// </summary>
        [Description("Cashable")]
        Cashable,

        /// <summary>
        ///     Promotional credits - cashable.
        /// </summary>
        [Description("Cashable Promotional")]
        Promo,

        /// <summary>
        ///     Promotional credits - non-cashable.
        /// </summary>
        [Description("Non-Cashable Promotional")]
        NonCash
    }
}