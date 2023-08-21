namespace Aristocrat.Monaco.Gaming.GameRound
{
    using System.Collections.Generic;

    /// <summary>
    ///     Provides methods to parse the data passed by
    ///     GameRoundEvent for the Triggered stage
    /// </summary>
    public interface IGameRoundInfoParser
    {
        /// <summary>
        ///     The game type for parser
        /// </summary>
        string GameType { get; }

        /// <summary>
        ///     The version for this parser
        /// </summary>
        string Version { get; }

        /// <summary>
        ///     Updates data based on the latest information
        /// </summary>
        /// <param name="gameRoundInfo">The information passed in by the GameRoundEvent</param>
        void UpdateGameRoundInfo(IList<string> gameRoundInfo);
    }
}