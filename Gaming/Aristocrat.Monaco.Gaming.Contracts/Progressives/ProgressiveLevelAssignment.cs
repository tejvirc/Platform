namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using System;

    /// <summary>
    ///     Simple mapping of AssignableProgressiveIds to ProgressiveLevels
    /// </summary>
    public class ProgressiveLevelAssignment
    {
        /// <summary>
        ///     Creates a new instance of a ProgressiveLevelAssignment
        /// </summary>
        /// <param name="gameDetail">The game detail associated with this assignment</param>
        /// <param name="denom">The denom associated with this assignment</param>
        /// <param name="progressiveLevel">The target progressive level for the assignment</param>
        /// <param name="assignedProgressiveIdInfo">The id of the related progressive</param>
        /// <param name="initialValue">The initial value of the UI</param>
        /// <param name="wagerCredits">The wager amount associated with this level</param>
        public ProgressiveLevelAssignment(
            IGameDetail gameDetail,
            long denom,
            IViewableProgressiveLevel progressiveLevel,
            AssignableProgressiveId assignedProgressiveIdInfo,
            long initialValue,
            long wagerCredits = 0)
        {
            ProgressiveLevel = progressiveLevel
                               ?? throw new ArgumentNullException(nameof(progressiveLevel));
            AssignedProgressiveIdInfo = assignedProgressiveIdInfo
                                        ?? throw new ArgumentNullException(nameof(assignedProgressiveIdInfo));
            GameDetail = gameDetail
                         ?? throw new ArgumentNullException(nameof(gameDetail));
            Denom = denom;
            InitialValue = initialValue;
            WagerCredits = wagerCredits;
        }

        /// <summary>
        ///     Gets the game detail associated with this assignment
        /// </summary>
        public IGameDetail GameDetail { get; }

        /// <summary>
        ///     Gets the denom associated with this assignment
        /// </summary>
        public long Denom { get; }

        /// <summary>
        ///     Gets the assigned progressive
        /// </summary>
        public AssignableProgressiveId AssignedProgressiveIdInfo { get; }

        /// <summary>
        ///     Gets the Progressive Level to be assigned
        /// </summary>
        public IViewableProgressiveLevel ProgressiveLevel { get; }

        /// <summary>
        ///     Gets the initial value from the UI
        /// </summary>
        public long InitialValue { get; }

        /// <summary>
        ///     Gets the wager credits for this progressive level
        /// </summary>
        public long WagerCredits { get; }
    }
}