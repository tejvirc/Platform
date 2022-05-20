namespace Aristocrat.Monaco.Bingo.UI.Events
{
    using Kernel;
    using Models;

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
            BingoHelpAppearance helpAppearance)
        {
            HelpAppearance = helpAppearance;
        }

        /// <summary>
        ///     Get the bingo help appearance.
        /// </summary>
        public BingoHelpAppearance HelpAppearance { get; }
    }
}
