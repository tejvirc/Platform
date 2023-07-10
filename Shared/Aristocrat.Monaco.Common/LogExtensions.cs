namespace Aristocrat.Monaco.Common
{
    using JetBrains.Annotations;
    using log4net;

    /// <summary>
    ///     Extension Methods for <see cref="ILog"/>
    /// </summary>
    public static class LogExtensions
    {
        /// <summary>
        ///     A method delegate for <see cref="MethodTraceLogger"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> that would like to format the scope method timer with</param>
        /// <param name="format">The message for the logger to use for formatting</param>
        /// <param name="caller">The caller for the function</param>
        [StringFormatMethod("format")]
        public static void DebugMethodTraceLogger(this ILog logger, string format, string caller) =>
            logger.DebugFormat(format, caller);

        /// <summary>
        ///     A method delegate for <see cref="MethodLogger"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> that would like to format the scope method timer with</param>
        /// <param name="format">The message for the logger to use for formatting</param>
        /// <param name="caller">The caller for the function</param>
        /// <param name="elapsed">The elapsed time</param>
        [StringFormatMethod("format")]
        public static void DebugMethodLogger(this ILog logger, string format, string caller, double elapsed) =>
            logger.DebugFormat(format, caller, elapsed);
    }
}