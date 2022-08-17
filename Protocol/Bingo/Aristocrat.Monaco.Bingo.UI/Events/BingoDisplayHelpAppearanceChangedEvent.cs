namespace Aristocrat.Monaco.Bingo.UI.Events
{
    using Kernel;
    using OverlayServer.Data.Bingo;

    /// <summary>
    ///     Bingo display help appearance changed event.
    /// </summary>
    public class BingoDisplayHelpAppearanceChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Construct a <see cref="BingoDisplayHelpAppearanceChangedEvent"/>.
        /// </summary>
        /// <param name="helpAppearance"></param>
        public BingoDisplayHelpAppearanceChangedEvent(
            BingoDisplayConfigurationHelpAppearance helpAppearance)
        {
            HelpAppearance = helpAppearance;
        }

        /// <summary>
        ///     Get the bingo help appearance.
        /// </summary>
        public BingoDisplayConfigurationHelpAppearance HelpAppearance { get; }
    }
}
