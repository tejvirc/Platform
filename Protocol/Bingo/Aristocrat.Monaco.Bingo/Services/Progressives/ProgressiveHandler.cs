namespace Aristocrat.Monaco.Bingo.Services.Progressives
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages.Progressives;
    using Gaming.Contracts.Progressives;
    using log4net;

    public class ProgressiveHandler : IProgressiveHandler, IProgressiveUpdateHandler, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private bool _disposed;

        public Task<bool> ProcessProgressiveUpdate(ProgressiveUpdateMessage update, CancellationToken token)
        {
            Logger.Debug("Received a progressive update response");

            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }

            token.ThrowIfCancellationRequested();

            return Task.FromResult(true);
        }

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
            }

            _disposed = true;
        }
    }
}