namespace Aristocrat.Bingo.Client.Logging
{
    using log4net;
    using log4net.Appender;
    using log4net.Config;
    using log4net.Core;
    using log4net.Layout;
    using log4net.Repository.Hierarchy;
    using Log4Net.Async;

    /// <summary>
    /// Utility class for creating logger instances.
    /// </summary>
    public class Log4NetLoggerFactory : ILoggerFactory
    {
        private const string RepositoryName = "bingo";
        private const string AsyncAppenderName = "AsyncForwardingAppender";
        private const string LogFilePath = @"..\logs\Log_Bingo.log";
        private const int MaxSizeRollBackups = 100;
        private const int MaxFileSize = 20 * 1024 * 1024;
        private const int CountDirection = 1;

        private static readonly object SyncLock = new object();
        private static bool _isInitialized;

        private static void ConfigureLogging()
        {
            var hierarchy = (Hierarchy)LogManager.CreateRepository(RepositoryName);

            var bufferAppender = new AsyncForwardingAppender
            {
                Name = AsyncAppenderName,
                Fix = FixFlags.Message | FixFlags.Exception
            };

            var fileAppender = new RollingFileAppender
            {
                File = LogFilePath,
                AppendToFile = true,
                RollingStyle = RollingFileAppender.RollingMode.Size,
                MaxSizeRollBackups = MaxSizeRollBackups,
                MaxFileSize = MaxFileSize,
                StaticLogFileName = true,
                CountDirection = CountDirection
            };

            var layout = new DynamicPatternLayout
            {
                Header =
                    "Trace started - Version %property{AssemblyInfo.Version}, %property{Runtime.Version} %date{yyyy-MM-dd HH:mm:ss.fff}%newline",
                Footer = "Trace closed%newline",
                ConversionPattern = "%level %date{yyyy-MM-dd HH:mm:ss.fff} %logger{1} - %message%newline%exception"
            };

            fileAppender.Layout = layout;

            layout.ActivateOptions();
            fileAppender.ActivateOptions();

            ((IAppenderAttachable)bufferAppender).AddAppender(fileAppender);

            bufferAppender.ActivateOptions();

            BasicConfigurator.Configure(hierarchy, bufferAppender);
        }

        /// <summary>
        /// Creates default logger instance.
        /// </summary>
        /// <typeparam name="TCategory">A class that is the category for the logger instance.</typeparam>
        /// <returns><see cref="ILogger{TCategory}"/></returns>
        public static ILogger CreateLogger<TCategory>()
                where TCategory : class
        {
            return CreateLogger(typeof(TCategory).Name);
        }

        /// <inheritdoc />
        public ILogger Create<TCategory>()
            where TCategory : class
        {
            return CreateLogger<TCategory>();
        }

        private static ILogger CreateLogger(string name)
        {
            Initialize();
            return new Log4NetLogger(LogManager.GetLogger(name), LogManager.GetLogger(RepositoryName, name));
        }

        private static void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            lock (SyncLock)
            {
                if (_isInitialized)
                {
                    return;
                }

                ConfigureLogging();
                _isInitialized = true;
            }
        }
    }
}
