namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;

    /// <summary>
    ///     Request the number of connected reels for the mechanical reels
    /// </summary>
    public class ConnectedReels
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectedReels" /> class.
        /// </summary>
        public ConnectedReels()
        {
            ReelIds = new List<int>();
        }

        /// <summary>
        ///     Gets a list of reel Ids for the connected reels.
        /// </summary>
        public List<int> ReelIds { get; set; }
    }
}