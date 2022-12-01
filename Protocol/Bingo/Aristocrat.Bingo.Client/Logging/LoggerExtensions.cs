namespace Aristocrat.Bingo.Client.Logging
{
    using System;

    /// <summary>
    ///     Method extensions for logging.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        ///     Extension method for logging formatted message with the Info level.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="messageTemplate">The message object to log.</param>
        /// <param name="propertyValues">Arguments to fill placeholders.</param>
        public static void LogInfo(this ILogger logger, string messageTemplate, params object[] propertyValues)
        {
            logger.Log(LogLevel.Info, messageTemplate, propertyValues);
        }

        /// <summary>
        ///     Extension method for logging formatted message with the Info level.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="exception">The exception to log, including its stack trace.</param>
        /// <param name="messageTemplate">The message object to log.</param>
        /// <param name="propertyValues">Arguments to fill placeholders.</param>
        public static void LogInfo(this ILogger logger, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            logger.Log(exception, LogLevel.Info, messageTemplate, propertyValues);
        }

        /// <summary>
        ///     Extension method for logging formatted message with the Debug level.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="messageTemplate">The message object to log.</param>
        /// <param name="propertyValues">Arguments to fill placeholders.</param>
        public static void LogDebug(this ILogger logger, string messageTemplate, params object[] propertyValues)
        {
            logger.Log(LogLevel.Debug, messageTemplate, propertyValues);
        }

        /// <summary>
        ///     Extension method for logging formatted message with the Debug level.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="exception">The exception to log, including its stack trace.</param>
        /// <param name="messageTemplate">The message object to log.</param>
        /// <param name="propertyValues">Arguments to fill placeholders.</param>
        public static void LogDebug(this ILogger logger, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            logger.Log(exception, LogLevel.Debug, messageTemplate, propertyValues);
        }

        /// <summary>
        ///     Extension method for logging formatted message with the Warn level.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="messageTemplate">The message object to log.</param>
        /// <param name="propertyValues">Arguments to fill placeholders.</param>
        public static void LogWarn(this ILogger logger, string messageTemplate, params object[] propertyValues)
        {
            logger.Log(LogLevel.Warn, messageTemplate, propertyValues);
        }

        /// <summary>
        ///     Extension method for logging formatted message with the Warn level.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="exception">The exception to log, including its stack trace.</param>
        /// <param name="messageTemplate">The message object to log.</param>
        /// <param name="propertyValues">Arguments to fill placeholders.</param>
        public static void LogWarn(this ILogger logger, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            logger.Log(exception, LogLevel.Warn, messageTemplate, propertyValues);
        }

        /// <summary>
        ///     Extension method for logging formatted message with the Error level.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="messageTemplate">The message object to log.</param>
        /// <param name="propertyValues">Arguments to fill placeholders.</param>
        public static void LogError(this ILogger logger, string messageTemplate, params object[] propertyValues)
        {
            logger.Log(LogLevel.Error, messageTemplate, propertyValues);
        }

        /// <summary>
        ///     Extension method for logging formatted message with the Error level.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="exception">The exception to log, including its stack trace.</param>
        /// <param name="messageTemplate">The message object to log.</param>
        /// <param name="propertyValues">Arguments to fill placeholders.</param>
        public static void LogError(this ILogger logger, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            logger.Log(exception, LogLevel.Error, messageTemplate, propertyValues);
        }

        /// <summary>
        ///     Extension method for logging formatted message with the Fatal level.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="messageTemplate">The message object to log.</param>
        /// <param name="propertyValues">Arguments to fill placeholders.</param>
        public static void LogFatal(this ILogger logger, string messageTemplate, params object[] propertyValues)
        {
            logger.Log(LogLevel.Fatal, messageTemplate, propertyValues);
        }

        /// <summary>
        ///     Extension method for logging formatted message with the Fatal level.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="exception">The exception to log, including its stack trace.</param>
        /// <param name="messageTemplate">The message object to log.</param>
        /// <param name="propertyValues">Arguments to fill placeholders.</param>
        public static void LogFatal(this ILogger logger, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            logger.Log(exception, LogLevel.Fatal, messageTemplate, propertyValues);
        }
    }
}
