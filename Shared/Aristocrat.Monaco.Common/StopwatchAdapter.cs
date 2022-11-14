namespace Aristocrat.Monaco.Common
{
    using System.Diagnostics;

    /// <summary>
    /// Stopwatch adapter, which implements the .Net stopwatch, accompanied with an interface for testing.
    /// </summary>
    public class StopwatchAdapter : Stopwatch, IStopwatch
    {
    }
}