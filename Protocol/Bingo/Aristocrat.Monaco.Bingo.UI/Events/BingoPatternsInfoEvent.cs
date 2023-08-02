// ReSharper disable UnusedAutoPropertyAccessor.Global Used for Automation
namespace Aristocrat.Monaco.Bingo.UI.Events
{
    using Kernel;
    using System.Collections.Generic;

    /// <summary>
    ///     Definition of the BingoPatternsInfoEvent class (used for Automation ONLY!)
    /// </summary>
    public class BingoPatternsInfoEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BingoPatternsInfoEvent" /> class.
        /// </summary>
        /// <param name="patterns">patterns info</param>
        /// <param name="gameIndex">index of the game being played</param>
        public BingoPatternsInfoEvent(IEnumerable<OverlayServer.Data.Bingo.BingoPattern> patterns, int gameIndex)
        {
            Patterns = patterns;
            GameIndex = gameIndex;
        }

        /// <summary>
        ///     Gets a values for Bingo patterns
        /// </summary>
        public IEnumerable<OverlayServer.Data.Bingo.BingoPattern> Patterns { get; }

        /// <summary>
        ///     Gets the index of the game being played
        /// </summary>
        public int GameIndex { get; }
    }
}
