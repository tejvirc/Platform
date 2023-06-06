namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using System.Collections.Generic;

    /// <summary>
    ///     Provides a read-only view of a <see cref="ProgressiveLevel" />
    /// </summary>
    public interface IViewableProgressiveLevel
    {
        /// <summary>
        ///     Gets the device identifier of the progressive.  This Id is guaranteed to be unique for each unique game and progressive pack
        /// </summary>
        int DeviceId { get; }

        /// <summary>
        ///     Gets the progressive pack name. This is not necessarily unique.
        /// </summary>
        string ProgressivePackName { get; }

        /// <summary>
        ///     Gets the progressive pack id. This is not necessarily unique.
        /// </summary>
        int ProgressivePackId { get; }

        /// <summary>
        ///     Gets the progressive id. This is not necessarily unique.
        /// </summary>
        int ProgressiveId { get; }

        /// <summary>
        ///     Gets the denominations associated with this progressive level.
        /// </summary>
        IEnumerable<long> Denomination { get; }

        /// <summary>
        ///     Gets or sets the bet option name (if any) associated with this progressive level
        /// </summary>
        string BetOption { get; }


        /// <summary>
        ///     Gets the variation associated with this progressive level
        /// </summary>
        string Variation { get; }

        /// <summary>
        ///     Gets the RTP of the progressive pack associated with this progressive level
        /// </summary>
        ProgressiveRtp ProgressivePackRtp { get; }

        /// <summary>
        ///     Gets the progressive level type associated with this level.
        /// </summary>
        ProgressiveLevelType LevelType { get; }

        /// <summary>
        ///     Gets the progressive funding type associated with this level.
        /// </summary>
        SapFundingType FundingType { get; }

        /// <summary>
        ///     Gets the id for the progressive level. This should be unique within a progressive pack.
        /// </summary>
        int LevelId { get; }

        /// <summary>
        ///     Gets the name of the progressive level. This should be unique within a progressive pack.
        /// </summary>
        string LevelName { get; }

        /// <summary>
        ///     Gets the increment rate for this progressive level.
        /// </summary>
        long IncrementRate { get; }

        /// <summary>
        ///     Gets the hidden increment rate for this progressive level. It is for incrementing the hidden pool
        ///     which will be added to the start-up value after JP hit and reset
        /// </summary>
        long HiddenIncrementRate { get; }

        /// <summary>
        ///     The total value of the hidden pool (in millicents) which will be added after JP hit and reset
        /// </summary>
        long HiddenValue { get; }

        /// <summary>
        ///     Gets the probability for the level to be hit. Optional for linked progressives. This is the theoretical probability
        ///     for a 1c bet to trigger a hit on the progressive level
        /// </summary>
        long Probability { get; }

        /// <summary>
        ///     Gets the maximum value (Ceiling)
        /// </summary>
        long MaximumValue { get; }

        /// <summary>
        ///     Gets the reset value. This is also the minimum value of the progressive level. It
        ///     should not be confused with the startup value which is used to transfer standalone progressive
        ///     level values after a memory clear.
        /// </summary>
        long ResetValue { get; }

        /// <summary>
        ///     Gets the return to player value for a given progressive level.
        /// </summary>
        long LevelRtp { get; }

        /// <summary>
        ///     Gets or set the Line Group ID required for Line-Based funding.
        /// </summary>
        int LineGroup { get; }

        /// <summary>
        ///     Gets the allowTruncation value indicating whether it is a platinum jackpot level (allowTruncation)
        /// </summary>
        bool AllowTruncation { get; }

        /// <summary>
        ///     Gets a turnover value. Turnover games, also called hyperlink games, can enable the game to use a random
        ///     number between 0 and the turnover value to determine if a progressive level can be hit. The platform doesn't care
        ///     about this value because the game will always perform this calculation. Turnover games can be both linked and
        ///     standalone.
        /// </summary>
        long Turnover { get; }

        /// <summary>
        ///     Gets what kind of trigger will be used for the progressive level.
        /// </summary>
        TriggerType TriggerControl { get; }

        /// <summary>
        ///     Gets the current state of the progressive level
        /// </summary>
        ProgressiveLevelState CurrentState { get; }

        /// <summary>
        ///     Gets the errors associated with this progressive level
        /// </summary>
        ProgressiveErrors Errors { get; }

        /// <summary>
        ///     Gets the current value of the progressive level in millicents
        /// </summary>
        long CurrentValue { get; }

        /// <summary>
        ///     Gets the initial value of the progressive level in millicents
        /// </summary>
        long InitialValue { get; }

        /// <summary>
        ///     Get the current value of the overflow amount in millicents
        /// </summary>
        long Overflow { get; }

        /// <summary>
        ///     The total accumulated value of the overflow amounts over the life of the machine (in millicents)
        /// </summary>
        long OverflowTotal { get; }

        /// <summary>
        ///     Get the current residual/fractional value
        /// </summary>
        long Residual { get; }

        /// <summary>
        ///     Gets the associated assigned progressive id
        /// </summary>
        AssignableProgressiveId AssignedProgressiveId { get; }

        /// <summary>
        ///     Gets the game id associated with the progressive level. This should always match 1-1 with what is defined
        ///     in <see cref="IGameDetail" />
        /// </summary>
        int GameId { get; }

        /// <summary>
        ///     Gets whether or not this levels values can be edited
        /// </summary>
        bool CanEdit { get; }

        /// <summary>
        ///     Gets or sets the Level Creation Type {Default, All, Max}
        /// </summary>
        LevelCreationType CreationType { get; }

        /// <summary>
        ///     Gets the WagerCredits in cents
        ///     associated with this progressive level
        /// </summary>
        long WagerCredits { get; }
    }
}