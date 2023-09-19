namespace Aristocrat.Monaco.Gaming.Runtime.Server
{
    using System;
    using System.Reflection;
    using Contracts;
    using Kernel;
    using log4net;
    using Snapp;

    public class SnappLogger : IExternalLogger
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly IEventBus _eventBus;

        public SnappLogger(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public void Log(Logger.Level level, string message)
        {
            var logMessage = $"SNAPP {level} {message}";
            switch (level)
            {
                case Snapp.Logger.Level.Debug:
                    Logger.Debug(logMessage);
                    break;
                case Snapp.Logger.Level.Info:
                    Logger.Info(logMessage);
                    break;
                case Snapp.Logger.Level.Warn:
                    Logger.Warn(logMessage);
                    break;
                case Snapp.Logger.Level.Error:
                    Logger.Error(logMessage);
                    break;
                case Snapp.Logger.Level.Fatal:
                    Logger.Error(logMessage);
                    Logger.Error($"Since SNAPP declared fatal error, shut down the runtime.");
                    _eventBus.Publish(new TerminateGameProcessEvent(false));
                    break;
            }
        }
    }
}
