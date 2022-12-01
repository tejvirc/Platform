namespace Aristocrat.Bingo.Client.Logging
{
    using System;

    /// <summary>
    ///     Interface for logging messages.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        ///     Log a message object.
        /// </summary>
        /// <param name="level">Log severity level.</param>
        /// <param name="messageTemplate">The message object to log.</param>
        /// <param name="propertyValues"></param>
        void Log(LogLevel level, string messageTemplate, params object[] propertyValues);

        /// <summary>
        ///     Log a message object.
        /// </summary>
        /// <param name="exception">The exception to log, including its stack trace.</param>
        /// <param name="level">Log severity level.</param>
        /// <param name="messageTemplate">The message object to log.</param>
        /// <param name="propertyValues"></param>
        void Log(Exception exception, LogLevel level, string messageTemplate, params object[] propertyValues);
    }

    /// <summary>
    ///     Creates a category from a <see cref="Type"/> for logging messages.
    /// </summary>
    /// <typeparam name="TCategory">A class that is the category for the logger instance.</typeparam>
    public interface ILogger<TCategory> : ILogger
        where TCategory : class
    {
    }
}
