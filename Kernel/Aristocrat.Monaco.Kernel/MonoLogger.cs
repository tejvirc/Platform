namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Reflection;
    using log4net;
    using Mono.Addins;

    /// <summary>An Implementation of IProgressStatus used to log Mono.Addins messages.</summary>
    public class MonoLogger : IProgressStatus
    {
        /// <summary>Create a logger for use in this class.</summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>Initializes a new instance of the <see cref="MonoLogger" /> class.</summary>
        public MonoLogger()
            : this(1)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MonoLogger" /> class.</summary>
        /// <param name="level">The logging level.</param>
        public MonoLogger(int level)
        {
            Logger.Info("MonoLogger up and running");
            LogLevel = level;
        }

        /// <summary>Gets the log level.</summary>
        public int LogLevel { get; }

        /// <summary>Gets a value indicating whether this instance is canceled.</summary>
        public bool IsCanceled => false;

        /// <summary>Sets the message.</summary>
        /// <param name="msg">The message.</param>
        public void SetMessage(string msg)
        {
            Logger.InfoFormat("Message: {0}", msg);
        }

        /// <summary>Sets the progress.</summary>
        /// <param name="progress">The progress.</param>
        public void SetProgress(double progress)
        {
            Logger.InfoFormat("Progress: {0}", progress);
        }

        /// <summary>Logs the specified message.</summary>
        /// <param name="msg">The message.</param>
        public void Log(string msg)
        {
            Logger.Info(msg);
        }

        /// <summary>Reports the warning.</summary>
        /// <param name="message">The message.</param>
        public void ReportWarning(string message)
        {
            Logger.Warn(message);
        }

        /// <summary>Reports the error.</summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public void ReportError(string message, Exception exception)
        {
            Logger.Error(message);
            if (exception != null)
            {
                Logger.Error(exception.ToString());
            }
        }

        /// <summary>Cancels this instance.</summary>
        public void Cancel()
        {
            Logger.Info("Cancel");
        }
    }
}