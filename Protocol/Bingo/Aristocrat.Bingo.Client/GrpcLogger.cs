namespace Aristocrat.Bingo.Client
{
    using System;
    using System.Reflection;
    using Grpc.Core;
    using Grpc.Core.Logging;
    using log4net;

    /// <summary>
    ///     A logger for GRPC used for debugging issues
    /// </summary>
    public class GrpcLogger : ILogger
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public GrpcLogger()
        {
            GrpcEnvironment.SetLogger(this);
        }

        public ILogger ForType<T>() => this;

        public void Debug(string message) => Logger.Debug(message);

        public void Debug(string format, params object[] formatArgs) => Logger.DebugFormat(format, formatArgs);

        public void Info(string message) => Logger.Info(message);

        public void Info(string format, params object[] formatArgs) => Logger.InfoFormat(format, formatArgs);

        public void Warning(string message) => Logger.Warn(message);

        public void Warning(string format, params object[] formatArgs) => Logger.WarnFormat(format, formatArgs);

        public void Warning(Exception exception, string message) => Logger.Warn(message, exception);

        public void Error(string message) => Logger.Error(message);

        public void Error(string format, params object[] formatArgs) => Logger.ErrorFormat(format, formatArgs);

        public void Error(Exception exception, string message) => Logger.Error(message, exception);
    }
}