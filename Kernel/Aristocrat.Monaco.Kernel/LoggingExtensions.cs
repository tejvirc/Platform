namespace Aristocrat.Monaco.Kernel
{
    using log4net;

    /// <summary>
    ///     Definition of the GenericLoggingExtensions class.
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        ///     Makes logging a class-level message generic.
        /// </summary>
        /// <typeparam name="T">The generic type</typeparam>
        /// <param name="owner">The object who wants to log - never used.</param>
        /// <returns>Instance of a logger.</returns>
        public static ILog Log<T>(this T owner)
        {
            return LogManager.GetLogger(typeof(T));
        }
    }
}
