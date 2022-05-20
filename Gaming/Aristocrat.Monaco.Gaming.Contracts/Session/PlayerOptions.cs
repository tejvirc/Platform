namespace Aristocrat.Monaco.Gaming.Contracts.Session
{
    using System;
    using Hardware.Contracts.Persistence;

    /// <summary>
    ///     Player session related options
    /// </summary>
    [Entity(PersistenceLevel.Critical)]
    public class PlayerOptions
    {
        /// <summary>
        ///     Gets or sets the minimum allowable weighted theoretical hold percentage for player sessions.Used to calculate the
        ///     RPN
        /// </summary>
        [Field]
        public long MinimumTheoreticalHoldPercentage { get; set; }

        /// <summary>
        ///     Gets or sets the Number of decimal places to use when displaying points to the player
        /// </summary>
        [Field]
        public int DecimalPoints { get; set; }

        /// <summary>
        ///     Gets or sets whether the player session should be terminated if the IdValidExpired indicator is set to true in the
        ///     id reader
        /// </summary>
        [Field]
        public bool InactiveSessionEnd { get; set; } = true;

        /// <summary>
        ///     Gets or sets the Period, in milliseconds used for interval rating; 0 (zero) implies no intervals.The  minimum
        ///     acceptable interval is 1 (one) minute
        /// </summary>
        [Field]
        public TimeSpan IntervalPeriod { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        ///     Gets or sets whether interval ratings should be generated when the currently active game combo changes
        /// </summary>
        [Field]
        public bool GamePlayInterval { get; set; } = true;

        /// <summary>
        ///     Gets or sets the Meter basis for the countdown. This basis is constant for all countdowns, including generic and
        ///     player-specific
        ///     overrides.If set to empty then points are not awarded to players.
        /// </summary>
        [Field(Size = 256)]
        public string CountBasis { get; set; }

        /// <summary>
        ///     Gets or sets the Countdown direction. The count direction is constant for all countdowns, including generic and
        ///     player-specific
        ///     overrides
        /// </summary>
        [Field(StorageType = FieldType.Int32)]
        public CountDirection CountDirection { get; set; } = CountDirection.Down;

        /// <summary>
        ///     Gets or sets the Countdown target. Must be greater than 0 (zero) if points are awarded
        /// </summary>
        [Field]
        public int BaseTarget { get; set; }

        /// <summary>
        ///     Gets or sets the Countdown meter increment. Must be greater than 0 (zero) if points are awarded
        /// </summary>
        [Field]
        public long BaseIncrement { get; set; }

        /// <summary>
        ///     Gets or sets the Countdown point award. Must be greater than 0 (zero) if points are awarded
        /// </summary>
        [Field]
        public int BaseAward { get; set; }

        /// <summary>
        ///     Gets or sets the Meter basis to use for hot player detection. If this attribute is empty then hot player tracking
        ///     is not enabled
        /// </summary>
        [Field(Size = 256)]
        public string HotPlayerBasis { get; set; }

        /// <summary>
        ///     Gets or sets the Period, in milliseconds, used to detect hot players.
        /// </summary>
        [Field]
        public TimeSpan HotPlayerPeriod { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        ///     Gets or sets the Threshold 1 used for hot player detection. If this attribute is set to 0 (zero) then hot player
        ///     tracking is not
        ///     enabled for hot player level 1.
        /// </summary>
        [Field]
        public long HotPlayerLimit1 { get; set; }

        /// <summary>
        ///     Gets or sets the Threshold 2 used for hot player detection. If this attribute is set to 0 (zero) then hot player
        ///     tracking is not
        ///     enabled for hot player level 2.
        /// </summary>
        [Field]
        public long HotPlayerLimit2 { get; set; }

        /// <summary>
        ///     Gets or sets the Threshold 3 used for hot player detection. If this attribute is set to 0 (zero) then hot player
        ///     tracking is not
        ///     enabled for hot player level 3.
        /// </summary>
        [Field]
        public long HotPlayerLimit3 { get; set; }

        /// <summary>
        ///     Gets or sets the threshold 4 used for hot player detection. If this attribute is set to 0 (zero) then hot player
        ///     tracking is not
        ///     enabled for hot player level 4.
        /// </summary>
        [Field]
        public long HotPlayerLimit4 { get; set; }

        /// <summary>
        ///     Gets or sets the threshold 5 used for hot player detection. If this attribute is set to 0 (zero) then hot player
        ///     tracking is not
        ///     enabled for hot player level 5.
        /// </summary>
        [Field]
        public long HotPlayerLimit5 { get; set; }
    }
}
