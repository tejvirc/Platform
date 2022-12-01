namespace Aristocrat.Bingo.Client.Logging
{
    using System;

    /// <summary>
    ///     Generic class for logging messages.
    /// </summary>
    /// <typeparam name="TCategory">A class that is the category for the logger instance.</typeparam>
    public class Logger<TCategory> : ILogger<TCategory>
        where TCategory : class
    {
        private readonly LoggerConfiguration _configuration;
        private readonly ILogger _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Logger{TCategory}"/> class.
        /// </summary>
        /// <param name="factory"><see cref="ILoggerFactory"/>.</param>
        /// <param name="configuration"><see cref="LoggerConfiguration"/>.</param>
        public Logger(ILoggerFactory factory, LoggerConfiguration configuration)
        {
            _configuration = configuration;
            _logger = factory.Create<TCategory>();
        }

        /// <inheritdoc />
        public void Log(LogLevel level, string messageTemplate, params object[] propertyValues)
        {
            if (level < _configuration.MinimumLevel)
            {
                return;
            }

            _logger.Log(level, messageTemplate, propertyValues);
        }

        /// <inheritdoc />
        public void Log(Exception exception, LogLevel level, string messageTemplate, params object[] propertyValues)
        {
            if (level < _configuration.MinimumLevel)
            {
                return;
            }

            _logger.Log(exception, level, messageTemplate, propertyValues);
        }
    }
}
