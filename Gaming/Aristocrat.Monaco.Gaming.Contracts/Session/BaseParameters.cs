namespace Aristocrat.Monaco.Gaming.Contracts.Session
{
    /// <summary>
    ///     Defines the base award parameters for player tracking purposes
    /// </summary>
    public class BaseParameters : AwardParameters
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseParameters" /> class.
        /// </summary>
        /// <param name="target">The value that a player must achieve to earn the Award</param>
        /// <param name="increment">The Countdown meter increment for the generic override</param>
        /// <param name="award">The countdown point award</param>
        public BaseParameters(int target, long increment, int award)
            :base (target, increment, award)
        {
        }
    }
}
