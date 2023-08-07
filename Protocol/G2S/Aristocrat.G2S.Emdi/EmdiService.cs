namespace Aristocrat.G2S.Emdi
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Consumers;
    using Host;
    using log4net;
    using Monaco.Common;
    using Monaco.Kernel;

    /// <summary>
    ///     Implements the <see cref="IEmdi"/> interface
    /// </summary>
    public sealed class EmdiService : IEmdi, IDisposable
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IHostQueue _queue;

        private SharedConsumerContext _sharedConsumerContext;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EmdiService"/> class.
        /// </summary>
        /// <param name="queue"></param>
        public EmdiService(IHostQueue queue)
        {
            _queue = queue;
        }

        /// <inheritdoc />
        ~EmdiService()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IEmdi) };

        /// <inheritdoc />
        public void Initialize()
        {
            _sharedConsumerContext = new SharedConsumerContext();
            ServiceManager.GetInstance().AddService(_sharedConsumerContext);
        }

        /// <inheritdoc />
        public void Start(int port)
        {
            _queue.StartAsync(port)
                .FireAndForget(
                    ex => Logger.Error(
                        $"EMDI: Error occured starting server host on port {port}",
                        ex));
        }

        /// <inheritdoc />
        public void Stop()
        {
            _queue.StopAllAsync()
                .FireAndForget(
                    ex => Logger.Error(
                        "EMDI: Error occured stopping server hosts",
                        ex));
        }


        /// <inheritdoc />
        public void Unload()
        {
            if (_sharedConsumerContext != null)
            {
                ServiceManager.GetInstance().RemoveService(_sharedConsumerContext);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // ReSharper disable once UseNullPropagation
                if (_sharedConsumerContext != null)
                {
                    _sharedConsumerContext.Dispose();
                }
            }
        }
    }
}
