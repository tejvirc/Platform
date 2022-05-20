namespace Aristocrat.Monaco.Gaming.Contracts.Models
{
    using System;

    /// <summary>
    ///     Defines the game initialization request.
    /// </summary>
    public class GameInitRequest
    {
        /// <summary>
        ///     Gets or sets the unique game identifier
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        ///     Gets or sets the game denomination
        /// </summary>
        public long Denomination { get; set; }

        /// <summary>
        ///     Gets or sets the game bet option
        /// </summary>
        public string BetOption { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this is a replay.
        /// </summary>
        public bool IsReplay { get; set; }

        /// <summary>
        ///     Gets or sets the game bottom window handle
        /// </summary>
        public IntPtr GameBottomHwnd { get; set; }

        /// <summary>
        ///     Gets or sets the game top window handle
        /// </summary>
        public IntPtr GameTopHwnd { get; set; }

        /// <summary>
        ///     Gets or sets the game virtual button deck window handle
        /// </summary>
        public IntPtr GameVirtualButtonDeckHwnd { get; set; }

        /// <summary>
        ///     Gets or sets the game topper window handle
        /// </summary>
        public IntPtr GameTopperHwnd { get; set; }
    }
}
