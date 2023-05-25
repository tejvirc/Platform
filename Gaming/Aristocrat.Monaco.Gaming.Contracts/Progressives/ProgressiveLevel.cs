namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using ProtoBuf;
    using System.Collections.Generic;

    /// <summary>
    ///     The fundamental data structure for a progressive level defined by a game. <see cref="IProgressiveLevelProvider" />
    ///     will manage all progressive levels for all games.
    /// </summary>
    [ProtoContract]
    public class ProgressiveLevel : IViewableProgressiveLevel
    {
        /// <summary>
        ///     Gets or sets the device identifier of the progressive.  This Id is guaranteed to be unique for each unique game and progressive pack
        /// </summary>
        [ProtoMember(1)]
        public int DeviceId { get; set; }

        /// <summary>
        ///     Gets or sets the progressive pack name. This is not necessarily unique.
        /// </summary>
        [ProtoMember(2)]
        public string ProgressivePackName { get; set; }

        /// <summary>
        ///     Gets or sets the progressive pack id. This is not necessarily unique across games.
        /// </summary>
        [ProtoMember(3)]
        public int ProgressivePackId { get; set; }

        /// <summary>
        ///     Gets or sets the progressive id. This is not necessarily unique across games.
        /// </summary>
        [ProtoMember(4)]
        public int ProgressiveId { get; set; }

        /// <summary>
        ///     Gets or sets the denominations associated with this progressive level.
        ///     Should this should be a a collection of longs? Or is should progressive levels map 1-1 with denoms?
        ///     If we use a collection of longs, will an empty collection indicate "ALL" from the config?
        ///     If more than one denom is specified, what rules apply?
        ///     If more than one denom is specified what should the startup value be if "cr" is specified in the config?
        ///     Should we always assume currency values locally?
        /// </summary>
        [ProtoMember(5)]
        public IEnumerable<long> Denomination { get; set; }

        /// <summary>
        ///     Gets or sets the bet option name (if any) associated with this progressive level
        /// </summary>
        [ProtoMember(6)]
        public string BetOption { get; set; }

        /// <summary>
        ///     Gets or sets the variation associated with this progressive level
        /// </summary>
        [ProtoMember(7)]
        public string Variation { get; set; }

        /// <summary>
        ///     Gets or sets the RTP of the progressive pack associated with this progressive level
        /// </summary>
        [ProtoMember(8)]
        public ProgressiveRtp ProgressivePackRtp { get; set; }

        /// <summary>
        ///     Gets or sets the progressive level type associated with this level.
        /// </summary>
        [ProtoMember(9)]
        public ProgressiveLevelType LevelType { get; set; }

        /// <summary>
        ///     Gets or sets the progressive funding type associated with this level.
        /// </summary>
        [ProtoMember(10)]
        public SapFundingType FundingType { get; set; }

        /// <summary>
        ///     Gets or sets the id for the progressive level. This should be unique within a progressive pack.
        /// </summary>
        [ProtoMember(11)]
        public int LevelId { get; set; }

        /// <summary>
        ///     Gets or sets the name of the progressive level.
        /// </summary>
        [ProtoMember(12)]
        public string LevelName { get; set; }

        /// <summary>
        ///     Gets or sets the increment rate for this progressive level.
        /// </summary>
        [ProtoMember(13)]
        public long IncrementRate { get; set; }

        /// <summary>
        ///     Gets or sets the hidden increment rate for this progressive level. It is for incrementing the hidden pool
        ///     which will be added to the start-up value after JP hit and reset
        /// </summary>
        [ProtoMember(14)]
        public long HiddenIncrementRate { get; set; }

        /// <summary>
        ///     The total value of the hidden pool (in millicents) which will be added after JP hit and reset
        /// </summary>
        [ProtoMember(15)]
        public long HiddenValue { get; set; }

        /// <summary>
        ///     Gets or sets the probability for the level to be hit. Optional for linked progressives. This is the theoretical
        ///     probability for a 1c bet to trigger a hit on the progressive level
        /// </summary>
        [ProtoMember(16)]
        public long Probability { get; set; }

        /// <summary>
        ///     Gets or sets the maximum value (Ceiling)
        /// </summary>
        [ProtoMember(17)]
        public long MaximumValue { get; set; }

        /// <summary>
        ///     Gets or sets the reset value. This is also the minimum value of the progressive level.
        /// </summary>
        [ProtoMember(18)]
        public long ResetValue { get; set; }

        /// <summary>
        ///     Gets or sets the return to player value for a given progressive level.
        /// </summary>
        [ProtoMember(19)]
        public long LevelRtp { get; set; }

        /// <summary>
        ///     Gets or set the Line Group ID required for Line-Based funding.
        /// </summary>
        [ProtoMember(20)]
        public int LineGroup { get; set; }

        /// <summary>
        ///     Gets or sets the allowTruncation value indicating whether it is a platinum jackpot level (allowTruncation)
        /// </summary>
        [ProtoMember(21)]
        public bool AllowTruncation { get; set; }

        /// <summary>
        ///     Gets or sets a turnover value. Turnover games, also called hyperlink games, can enable the game to use a random
        ///     number between 0 and the turnover value to determine if a progressive level can be hit. The platform doesn't care
        ///     about this value because the game will always perform this calculation. Turnover games can be both linked and
        ///     standalone.
        /// </summary>
        [ProtoMember(22)]
        public long Turnover { get; set; }

        /// <summary>
        ///     Gets or sets what kind of trigger will be used for the progressive level.
        /// </summary>
        [ProtoMember(23)]
        public TriggerType TriggerControl { get; set; }

        /// <summary>
        ///     Gets or sets the current state of the progressive level
        /// </summary>
        [ProtoMember(24)]
        public ProgressiveLevelState CurrentState { get; set; }

        /// <summary>
        ///     Gets or sets the errors associated with this progressive level
        /// </summary>
        [ProtoMember(25)]
        public ProgressiveErrors Errors { get; set; }

        /// <summary>
        ///     Gets or sets the current value of the progressive level
        /// </summary>
        [ProtoMember(26)]
        public long CurrentValue { get; set; }

        /// <summary>
        ///     Gets or sets the initial value of the progressive level
        /// </summary>
        [ProtoMember(27)]
        public long InitialValue { get; set; }

        /// <summary>
        ///     Gets or sets the current value of the overflow amount
        /// </summary>
        [ProtoMember(28)]
        public long Overflow { get; set; }

        /// <summary>
        ///     The total accumulated value of the overflow amounts over the life of the machine (in millicents)
        /// </summary>
        [ProtoMember(29)]
        public long OverflowTotal { get; set; }

        /// <summary>
        ///     Gets or sets the current residual
        /// </summary>
        [ProtoMember(30)]
        public long Residual { get; set; }

        /// <summary>
        ///     Gets or sets the assigned progressive id.
        /// </summary>
        [ProtoMember(31)]
        public AssignableProgressiveId AssignedProgressiveId { get; set; }

        /// <summary>
        ///     Gets or sets the game id associated with the progressive level. This should always match 1-1 with what is defined
        ///     in <see cref="IGameDetail" />
        /// </summary>
        [ProtoMember(32)]
        public int GameId { get; set; }

        /// <inheritdoc />
        [ProtoMember(33)]
        public bool CanEdit { get; set; }

        /// <inheritdoc />
        [ProtoMember(34)]
        public LevelCreationType CreationType { get; set; }

        /// <inheritdoc />
        [ProtoMember(35)]
        public long WagerCredits { get; set; }
    }

    /// <summary>
    /// Specifies the different ways to create the level
    /// </summary>
    public enum LevelCreationType
    {
        /// <summary>
        ///     This indicates the standard way to create a level.
        /// </summary>
        Default,

        /// <summary>
        ///     Progressive levels created for each wager amount.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        All,

        /// <summary>
        ///     Progressive levels to be created for max wager amount.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        Max
    }

    /// <summary>
    /// </summary>
    public enum ProgressiveLevelType
    {
        /// <summary>
        ///     Unknown progressive types are unknown. If they were known, they wouldn't be unknown. If this value
        ///     is used then an error will be reported.
        /// </summary>
        Unknown,

        /// <summary>
        ///     Standalone progressives are defined by the game.
        /// </summary>
        Sap,

        /// <summary>
        ///     Linked progressives are progressives defined by a remote system that the game supports.
        /// </summary>
        LP,

        /// <summary>
        ///     Selectable progressives are operator defined standalone levels
        /// </summary>
        Selectable
    }

    /// <summary>
    ///     Trigger types specify who controls the triggering a claim of a progressive level
    /// </summary>
    public enum TriggerType
    {
        /// <summary>
        ///     The game controls jackpot claim triggering
        /// </summary>
        Game,

        /// <summary>
        ///     Mystery jackpots are not triggered by the game. They are triggered by the platform. This is unsupported currently
        ///     and will
        ///     result in an error if used.
        /// </summary>
        Mystery,

        /// <summary>
        ///     External jackpot triggers are initiated externally? This is unsupported currently and will result in an error if
        ///     used.
        /// </summary>
        External
    }

    /// <summary>
    ///     The SapFundingType is defined by the game. The platform should not care about what the Sap funding type is
    ///     because the game tells the platform how much is contributed to a level.
    /// </summary>
    public enum SapFundingType
    {
        /// <summary>
        ///     Standard Standalone Progressive funding type. This is the standard funding type
        ///     for standalone progressive by default.
        /// </summary>
        Standard,

        /// <summary>
        ///     Ante funding type for standalone progressives. This is where the funding for that SAP level comes from the ante bet
        ///     component played.
        ///     For example, if you are playing 150cr + 50cr Ante bet, only 50crs gets sent across to platform and then platform
        ///     will apply the increment rate only to that 50cr.
        ///     This functionality requires the configuration.xml of the game to be configured with line options. The platform
        ///     should not care about this.
        /// </summary>
        Ante,

        /// <summary>
        ///     This is a SAP funding method in which a progressive level can be associated with a specific line option(s) and the
        ///     progressive pool of the level is incremented only when the associated line option is the active play line.
        /// </summary>
        LineBased,

        /// <summary>
        ///     This is the same as Line-Based except the levels are funded by the ante bet component rather than the total bet.
        /// </summary>
        LineBasedAnte,

        /// <summary>
        ///     This is specified in the xsd, but there is no documentation for this.
        /// </summary>
        BulkOnly,

        /// <summary>
        ///     Used for progressive levels that aren't a standalone progressive and do not require a funding type.
        /// </summary>
        NotApplicable = 99
    }

    /// <summary>
    ///     Flavor attribute is used to support progressive features which are specific to a particular controller, protocol or
    ///     market.
    /// </summary>
    public enum FlavorType
    {
        /// <summary>
        ///     The default, for sane rational people who don't use Vertex, apparently.
        /// </summary>
        Standard,

        /// <summary>
        ///     This is to support Vertex's AnteBet Bulk contribution feature. Not currently supported
        /// </summary>
        BulkContribution,

        /// <summary>
        ///     This is to support Vertex's Mystery feature, which is different from mystery sap. Not currently supported.
        /// </summary>
        VertexMystery,

        /// <summary>
        ///     Vertex specific feature. Not currently supported.
        /// </summary>
        HostChoice
    }

    /// <summary>
    ///     Each progressive level has its own state that can be tracked independently.
    /// </summary>
    public enum ProgressiveLevelState
    {
        /// <summary>
        ///     Initial state of a progressive level. When a progressive level is configured it transitions
        ///     from Init to Ready.
        /// </summary>
        Init,

        /// <summary>
        ///     A ready progressive level is any level that has been configured, but is not currently active in a game.
        /// </summary>
        Ready,

        /// <summary>
        ///     An Active progressive level is any level that is currently active in a game.
        /// </summary>
        Active,

        /// <summary>
        ///     A Hit progressive level is any level that has been reported as "Hit" during a game.
        /// </summary>
        Hit,

        /// <summary>
        ///     A Pending progressive level is any level that has reported a hit and the hit has been acknowledged.
        ///     The claim remains pending until it has been awarded to the player.
        /// </summary>
        Pending,

        /// <summary>
        ///     A Committed progressive level is any level that has been awarded to the player.
        /// </summary>
        Committed,

        /// <summary>
        ///     Indicates that there is one or more active errors associated with the progressive level
        /// </summary>
        Error
    }
}