namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    using System.ComponentModel;

    /// <summary>
    ///     Defines the bonus state
    /// </summary>
    public enum BonusState
    {
        /// <summary>
        ///     Bonus pending
        /// </summary>
        [Description("Pending")] Pending,

        /// <summary>
        ///     Bonus committed
        /// </summary>
        [Description("Committed")] Committed,

        /// <summary>
        ///     Bonus acknowledged
        /// </summary>
        [Description("Acknowledged")] Acknowledged
    }
}