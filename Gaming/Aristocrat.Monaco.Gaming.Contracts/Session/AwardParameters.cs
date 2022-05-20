namespace Aristocrat.Monaco.Gaming.Contracts.Session
{
    /// <summary>
    ///     Abstract definition of the award parameters
    /// </summary>
    public abstract class AwardParameters
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AwardParameters" /> class.
        /// </summary>
        /// <param name="target">The value that a player must achieve to earn the Award</param>
        /// <param name="increment">The Countdown meter increment for the generic override</param>
        /// <param name="award">The countdown point award</param>
        protected AwardParameters(int target, long increment, int award)
        {
            Target = target;
            Increment = increment;
            Award = award;
        }

        /// <summary>
        ///     Gets the value that a player must achieve to earn the Award
        /// </summary>
        public int Target { get; }

        /// <summary>
        ///     Gets the Countdown meter increment for the generic override
        /// </summary>
        public long Increment { get; }

        /// <summary>
        ///     Gets the countdown point award
        /// </summary>
        public int Award { get; }
    }
}
