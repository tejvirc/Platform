namespace Aristocrat.Monaco.Gaming.Contracts.Session
{
    using System;

    /// <summary>
    ///     Describes the overridden award parameters
    /// </summary>
    public class GenericOverrideParameters : AwardParameters
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GenericOverrideParameters" /> class.
        /// </summary>
        /// <param name="target">The value that a player must achieve to earn the Award</param>
        /// <param name="increment">The Countdown meter increment for the generic override</param>
        /// <param name="award">The countdown point award</param>
        /// <param name="start">The starting date time for the award parameters</param>
        /// <param name="end">The ending date time for the award parameters</param>
        /// <param name="overrideId">The override id</param>
        public GenericOverrideParameters(
            int target,
            long increment,
            int award,
            DateTime start,
            DateTime end,
            long overrideId)
            : base(target, increment, award)
        {
            Start = start;
            End = end;
            OverrideId = overrideId;
        }

        /// <summary>
        ///     Gets the starting date time for the award parameters
        /// </summary>
        public DateTime Start { get; }

        /// <summary>
        ///     Gets the ending date time for the award parameters
        /// </summary>
        public DateTime End { get; }

        /// <summary>
        ///     Gets the override id
        /// </summary>
        public long OverrideId { get; }
    }
}
