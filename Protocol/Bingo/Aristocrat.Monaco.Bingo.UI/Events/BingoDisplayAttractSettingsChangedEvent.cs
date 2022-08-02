namespace Aristocrat.Monaco.Bingo.UI.Events
{
    using Kernel;
    using OverlayServer.Data.Bingo;

    /// <summary>
    ///     Bingo display help appearance changed event.
    /// </summary>
    public class BingoDisplayAttractSettingsChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Construct a <see cref="BingoDisplayAttractSettingsChangedEvent"/>.
        /// </summary>
        /// <param name="attractSettings"></param>
        public BingoDisplayAttractSettingsChangedEvent(
            BingoDisplayConfigurationBingoAttractSettings attractSettings)
        {
            AttractSettings = attractSettings;
        }

        /// <summary>
        ///     Get the bingo attractSettings
        /// </summary>
        public BingoDisplayConfigurationBingoAttractSettings AttractSettings { get; }
    }
}