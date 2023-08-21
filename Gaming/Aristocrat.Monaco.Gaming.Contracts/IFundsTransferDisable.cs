namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Contains methods to determine when funds can be transferred on or off the machine
    /// </summary>
    public interface IFundsTransferDisable
    {
        /// <summary>
        ///     Gets a value indicating whether transfers to the machine are disabled
        ///     due to being in a game
        /// </summary>
        bool TransferOnDisabledInGame { get; }

        /// <summary>
        ///     Gets a value indicating whether transfers to the machine are disabled
        ///     due to a tilt
        /// </summary>
        bool TransferOnDisabledTilt { get; }

        /// <summary>
        ///     Gets a value indicating whether transfers from the machine are disabled
        ///     due to being in a game or a tilt that prevents cashout is present
        /// </summary>
        bool TransferOffDisabled { get; }

        /// <summary>
        /// Gets a value indicating whether the AFT transfers to the machine are disabled
        /// due to an Overlay screen
        /// </summary>
        bool TransferOnDisabledOverlay { get; }
    }
}