﻿namespace Aristocrat.Monaco.Gaming.Contracts.Central
{
    using System.Collections.Generic;

    /// <summary>
    ///     Provides additional game play info for multi-play games
    ///     like side bet or wonder 4
    /// </summary>
    public interface IAdditionalGamePlayInfo
    {
        /// <summary>
        ///     Gets or sets the game index associated with the additional game
        /// </summary>
        public int GameIndex { get; set; }

        /// <summary>
        ///     Gets or sets the denomination associated with the game play
        /// </summary>
        public long Denomination { get; set; }

        /// <summary>
        ///     Gets or sets the wager amount for this game
        /// </summary>
        public long WagerAmount { get; set; }
    }
}