namespace Aristocrat.Monaco.TestProtocol
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Kernel;

    /// <summary>
    ///     Definition of the TestWatOnProvider class.
    /// </summary>
    public class TestWatOnProvider : IWatTransferOnProvider, IDisposable, IService
    {
        public bool IsWatOnSupported { get; set; }

        protected bool Disposed { get; set; }

        public bool CanTransfer => false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => "Test Wat On Provider";

        public ICollection<Type> ServiceTypes => new[] { typeof(IWatTransferOnProvider) };

        public void Initialize()
        {
            IsWatOnSupported = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                Disposed = true;
            }
        }

        public Task<bool> InitiateTransfer(WatOnTransaction transaction)
        {
            return Task.FromResult(false);
        }

        public Task CommitTransfer(WatOnTransaction transaction)
        {
            return Task.CompletedTask;
        }
    }
}