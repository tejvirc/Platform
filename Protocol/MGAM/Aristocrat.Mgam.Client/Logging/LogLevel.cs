namespace Aristocrat.Mgam.Client.Logging
{
    /// <summary>
    ///     Log level severity.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        ///     Designates fine-grained informational events that are most
        ///     useful to debug an application.
        /// </summary>
        Debug,

        /// <summary>
        ///     Designates informational messages that highlight the progress
        ///     of the application at coarse-grained level.
        /// </summary>
        Info,

        /// <summary>
        ///     Designates potentially harmful situations.
        /// </summary>
        Warn,

        /// <summary>
        ///     Designates error events that might still allow the application
        ///     to continue running.
        /// </summary>
        Error,

        /// <summary>
        ///     Designates very severe error events that will presumably lead
        ///     the application to abort.
        /// </summary>
        Fatal
    }
}
