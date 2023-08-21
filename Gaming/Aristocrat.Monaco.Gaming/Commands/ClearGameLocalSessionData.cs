namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts;

    /// <summary>
    ///     Handler data for when a local session's game data must be cleared.
    /// </summary>
    [Serializable]
    public class ClearGameLocalSessionData
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ClearGameLocalSessionData"/> class.
        /// </summary>
        /// <param name="details">Game details</param>
        /// <param name="denom">Base game denomination; use null to clear all denoms for this game</param>
        public ClearGameLocalSessionData(IGameDetail details, long? denom = null)
        {
            Details = details;
            Denom = denom;
        }

        /// <summary>
        ///     Game Details
        /// </summary>
        public IGameDetail Details { get; }

        /// <summary>
        ///     Specific denom to clear
        /// </summary>
        public long? Denom { get; }
    }
}
