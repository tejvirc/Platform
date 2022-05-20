namespace Aristocrat.Monaco.G2S.Logging
{
    using System.Diagnostics;
    using System.Linq;
    using Diagnostics;
    using log4net;

    /// <summary>
    ///     log4net configuration extensions
    /// </summary>
    internal static class Log4NetConfigurationExtensions
    {
        /// <summary>
        ///     Adds as trace source.
        /// </summary>
        /// <param name="this">The log4net instance to add as a trace source.</param>
        /// <param name="defaultSwitchValue">The default switch value.</param>
        public static void AddAsTraceSource(this ILog @this, string defaultSwitchValue = @"Verbose")
        {
            const string sourceName = @"G2S";
            const string listenerName = @"Log4Net";
            const string traceLevelName = @"G2STraceLevel";

            // The G2S Lib uses TraceSource (actually it's SourceTrace, so it requires special handling)
            var source = SourceTrace.GetListeners(sourceName);
            if (source.Cast<object>().Any(listener => listener.GetType() == typeof(Log4NetTraceListener)))
            {
                return;
            }

            SourceTrace.SetSwitch(sourceName, new SourceSwitch(traceLevelName, defaultSwitchValue));
            var netListener = new Log4NetTraceListener(@this) { Name = listenerName };
            source.Add(netListener);
            Trace.Listeners.Add(netListener);
        }
    }
}