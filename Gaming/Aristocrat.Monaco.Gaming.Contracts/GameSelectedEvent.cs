namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     A GameSelectedEvent should be posted when the current game selection changes.  A game is defined by a game Id
    ///     (unique by theme and paytable) and a denom Id.  Otherwise, known as a Game Combo.
    ///     An example is an operator choosing a game from a list of playable games.
    ///     Another example of a game selection change could be a player choosing to use a different paytable available within
    ///     a game.
    /// </summary>
    [Serializable]
    public class GameSelectedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameSelectedEvent" /> class.
        /// </summary>
        /// <param name="gameId">The Id of the game.</param>
        /// <param name="isReplay">Is the game a replay.</param>
        /// <param name="denomination">The denomination of the game.</param>
        /// <param name="betOption">The bet option of the game.</param>
        /// <param name="bottomHwnd">The game bottom window handle.</param>
        /// <param name="topHwnd">The game top window handle.</param>
        /// <param name="virtualButtonDeckHwnd">The game virtual button deck window handle.</param>
        /// <param name="topperHwnd">The game topper window handle.</param>
        public GameSelectedEvent(
            int gameId,
            long denomination,
            string betOption,
            bool isReplay,
            IntPtr bottomHwnd,
            IntPtr topHwnd,
            IntPtr virtualButtonDeckHwnd,
            IntPtr topperHwnd)
        {
            GameId = gameId;
            Denomination = denomination;
            BetOption = betOption;
            IsReplay = isReplay;
            GameBottomHwnd = bottomHwnd;
            GameTopHwnd = topHwnd;
            GameVirtualButtonDeckHwnd = virtualButtonDeckHwnd;
            GameTopperHwnd = topperHwnd;
        }

        /// <summary>
        ///     Gets the unique identifier of the game.
        /// </summary>
        public int GameId { get; }

        /// <summary>
        ///     Gets the denomination of the game.
        /// </summary>
        public long Denomination { get; }

        /// <summary>
        ///     Gets the bet option of the game.
        /// </summary>
        public string BetOption { get; }

        /// <summary>
        ///     Gets a value indicating whether this is a replay.
        /// </summary>
        public bool IsReplay { get; }

        /// <summary>
        ///     Gets the window handle the bottom game is drawn in.
        /// </summary>
        public IntPtr GameBottomHwnd { get; }

        /// <summary>
        ///     Gets the window handle the top game is drawn in.
        /// </summary>
        public IntPtr GameTopHwnd { get; }

        /// <summary>
        ///     Gets the window handle the top game is drawn in.
        /// </summary>
        public IntPtr GameTopperHwnd { get; }

        /// <summary>
        ///     Gets the window handle the virtual button deck is drawn in.
        /// </summary>
        public IntPtr GameVirtualButtonDeckHwnd { get; }
    }
}