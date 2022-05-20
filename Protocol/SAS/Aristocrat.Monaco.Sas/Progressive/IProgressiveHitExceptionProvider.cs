namespace Aristocrat.Monaco.Sas.Progressive
{
    /// <summary>
    ///     Provides and interface to report progressive hit exceptions.
    /// </summary>
    public interface IProgressiveHitExceptionProvider
    {
        /// <summary>
        ///     Start reporting progressive hit exceptions on a continually loop
        /// </summary>
        void StartReportingSasProgressiveHit();

        /// <summary>
        ///     Stop reporting progressive hit exceptions
        /// </summary>
        void StopSasProgressiveHitReporting();

        /// <summary>
        ///     Report a non sas progressive hit once
        /// </summary>
        void ReportNonSasProgressiveHit();
    }
}