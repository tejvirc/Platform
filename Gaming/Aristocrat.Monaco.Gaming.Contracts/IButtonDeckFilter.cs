namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>Filter options.</summary>
    public enum ButtonDeckFilterMode
    {
        /// <summary>Button deck filter under normal conditions.</summary>
        Normal,

        /// <summary>Button deck filter in lock up state.</summary>
        Lockup,

        /// <summary>Button deck filter where only cashout.</summary>
        CashoutOnly
    }

    /// <summary>
    ///     Contract for high-level filtering of button deck key presses.
    /// </summary>
    public interface IButtonDeckFilter : IService
    {
        /// <summary>
        ///     Gets or sets the state of the filter.
        /// </summary>
        ButtonDeckFilterMode FilterMode { get; set; }
    }
}
