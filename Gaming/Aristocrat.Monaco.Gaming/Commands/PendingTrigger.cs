namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;

    /// <summary>
    ///     Defines a pending jackpot triggers command.  This will only be used with central determinant games.  It's used to
    ///     notify the platform of the level that will be claimed in the game round.  This will likely only be used when the
    ///     central determinant host does not notify of the client of the levels that were awarded in the outcome request.
    /// </summary>
    public class PendingTrigger
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PendingTrigger" /> class.
        /// </summary>
        public PendingTrigger(IReadOnlyCollection<int> levelIds)
        {
            LevelIds = levelIds;
        }

        /// <summary>
        ///     Gets a collection of level Ids that will be triggered during a game round
        /// </summary>
        public IReadOnlyCollection<int> LevelIds { get; }
    }
}