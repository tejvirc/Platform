namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;
    using V1;

    /// <summary>
    ///     Request the status of the mechanical reels
    /// </summary>
    public class GetReelState
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GetReelState" /> class.
        /// </summary>
        public GetReelState()
        {
            States = new Dictionary<int, ReelState>();
        }

        /// <summary>
        ///     Gets a map of reel Ids to reel states for the connected reels.
        /// </summary>
        public Dictionary<int, ReelState> States { get; set; }
    }
}