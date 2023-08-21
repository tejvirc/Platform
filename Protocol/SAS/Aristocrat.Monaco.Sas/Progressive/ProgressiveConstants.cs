namespace Aristocrat.Monaco.Sas.Progressive
{
    /// <summary>
    ///     A progressive constants.
    /// </summary>
    public static class ProgressiveConstants
    {
        /// <summary>
        ///     Name of the progressive.
        /// </summary>
        public const string ProtocolName = "SAS";

        /// <summary>
        ///     The number of ms it takes before exception 53 should be issued
        /// </summary>
        public const int LevelTimeout = 5000;

        /// <summary>
        ///     The number of milliseconds between posting progressive hit exceptions
        /// </summary>
        public const int ProgressiveHitExceptionCycleTime = 15000;
    }
}
