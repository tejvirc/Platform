namespace Aristocrat.Mgam.Client.Logging
{
    using System;
    using System.Globalization;
    using log4net;

    /// <summary>
    ///     The <see cref="ILogger"/> implementation for logging messages to log4net framework.
    /// </summary>
    public class Log4NetLogger : ILogger
    {
        private readonly ILog _logger;
        private readonly ILog _protocolLogger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Log4NetLogger"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILog"/>.</param>
        /// <param name="protocolLogger">Protocol logger. <see cref="ILog"/>.</param>
        public Log4NetLogger(ILog logger, ILog protocolLogger)
        {
            _logger = logger;
            _protocolLogger = protocolLogger;
        }

        /// <inheritdoc />
        public void Log(LogLevel level, string messageTemplate, params object[] propertyValues)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    _logger.DebugFormat(messageTemplate, propertyValues);
                    _protocolLogger.DebugFormat(messageTemplate, propertyValues);
                    break;
                case LogLevel.Info:
                    _logger.InfoFormat(messageTemplate, propertyValues);
                    _protocolLogger.InfoFormat(messageTemplate, propertyValues);
                    break;
                case LogLevel.Warn:
                    _logger.WarnFormat(messageTemplate, propertyValues);
                    _protocolLogger.WarnFormat(messageTemplate, propertyValues);
                    break;
                case LogLevel.Error:
                    _logger.ErrorFormat(messageTemplate, propertyValues);
                    _protocolLogger.ErrorFormat(messageTemplate, propertyValues);
                    break;
                case LogLevel.Fatal:
                    _logger.FatalFormat(messageTemplate, propertyValues);
                    _protocolLogger.FatalFormat(messageTemplate, propertyValues);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        /// <inheritdoc />
        public void Log(Exception exception, LogLevel level, string messageTemplate, params object[] propertyValues)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    _logger.Debug(string.Format(CultureInfo.CurrentCulture, messageTemplate, propertyValues), exception);
                    _protocolLogger.Debug(string.Format(CultureInfo.CurrentCulture, messageTemplate, propertyValues), exception);
                    break;
                case LogLevel.Info:
                    _logger.Info(string.Format(CultureInfo.CurrentCulture, messageTemplate, propertyValues), exception);
                    _protocolLogger.Info(string.Format(CultureInfo.CurrentCulture, messageTemplate, propertyValues), exception);
                    break;
                case LogLevel.Warn:
                    _logger.Warn(string.Format(CultureInfo.CurrentCulture, messageTemplate, propertyValues), exception);
                    _protocolLogger.Warn(string.Format(CultureInfo.CurrentCulture, messageTemplate, propertyValues), exception);
                    break;
                case LogLevel.Error:
                    _logger.Error(string.Format(CultureInfo.CurrentCulture, messageTemplate, propertyValues), exception);
                    _protocolLogger.Error(string.Format(CultureInfo.CurrentCulture, messageTemplate, propertyValues), exception);
                    break;
                case LogLevel.Fatal:
                    _logger.Fatal(string.Format(CultureInfo.CurrentCulture, messageTemplate, propertyValues), exception);
                    _protocolLogger.Fatal(string.Format(CultureInfo.CurrentCulture, messageTemplate, propertyValues), exception);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
    }
}
