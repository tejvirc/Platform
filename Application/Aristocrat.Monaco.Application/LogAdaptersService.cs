namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts.TiltLogger;

    /// <summary>
    ///     Implementation for LogAdaptersService
    /// </summary>
    public class LogAdaptersService : ILogAdaptersService, IDisposable
    {
        private readonly object _lockObject = new object();

        private bool _disposed;

        /// <inheritdoc />
        public string Name => typeof(ILogAdaptersService).Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ILogAdaptersService) };

        /// <inheritdoc />
        public void Initialize()
        {
            LogAdapters = new List<IEventLogAdapter>();
        }

        private IList<IEventLogAdapter> LogAdapters { get; set; }

        /// <inheritdoc />
        public void RegisterLogAdapter(IEventLogAdapter logAdapter)
        {
            if (logAdapter == null)
            {
                return;
            }

            lock (_lockObject)
            {
                if (LogAdapters.Any(l => l.LogType == logAdapter.LogType))
                {
                    throw new Exception("Attempt to add duplicate log adapter");
                }

                LogAdapters.Add(logAdapter);
            }
        }

        /// <inheritdoc />
        public void UnRegisterLogAdapter(string logAdapterType)
        {
            lock (_lockObject)
            {
                LogAdapters.Remove(LogAdapters.FirstOrDefault(la => la.LogType == logAdapterType));
            }
        }

        /// <inheritdoc />
        public IEnumerable<IEventLogAdapter> GetLogAdapters()
        {
            lock (_lockObject)
            {
                return LogAdapters.ToList();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                LogAdapters.Clear();
            }
            _disposed = true;
        }
    }
}
