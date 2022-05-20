namespace Aristocrat.Monaco.Bingo.UI.Events
{
    using System.Windows;
    using Kernel;
    using Models;

    /// <summary>
    ///     Bingo display configuration changed event.
    /// </summary>
    public class BingoDisplayConfigurationChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Construct a <see cref="BingoDisplayConfigurationChangedEvent"/>.
        /// </summary>
        /// <param name="window"></param>
        /// <param name="settings"></param>
        public BingoDisplayConfigurationChangedEvent(
            Window window,
            BingoWindowSettings settings)
        {
            Window = window;
            Settings = settings;
        }

        /// <summary>
        ///     Get the bingo window.
        /// </summary>
        public Window Window { get; }

        /// <summary>
        ///     Get the bingo window settings.
        /// </summary>
        public BingoWindowSettings Settings { get; }
    }
}
