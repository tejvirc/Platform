namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using log4net;

    public class NoteAcceptorActivityTimeWatcher : IService, IDisposable
    {
        private const int DefaultTimeForTransaction = 5000;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;
        private readonly object _lock = new object();

        private CancellationTokenSource _delayTimerCancellationToken;
        private int _timeOutValue;
        private INoteAcceptor _noteAcceptor;

        private bool _disposed;

        public NoteAcceptorActivityTimeWatcher()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().TryGetService<INoteAcceptor>())
        {
        }

        public NoteAcceptorActivityTimeWatcher(
            IEventBus eventBus,
            IPropertiesManager properties,
            INoteAcceptor noteAcceptor)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _noteAcceptor = noteAcceptor;

            Initialize();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes { get; } = new[] { typeof(NoteAcceptorActivityTimeWatcher) };

        public void Initialize()
        {
            var enabled = _properties.GetValue(
                AccountingConstants.NoteAcceptorTimeLimitEnabled,
                false);

            if (!enabled)
            {
                Logger.Debug("NoteAcceptorActivityTimeWatcher not needed for this jurisdiction.");
                return;
            }

            _eventBus.Subscribe<ServiceAddedEvent>(this, HandleNoteAcceptorServiceAddedEvent);
            _eventBus.Subscribe<CurrencyEscrowedEvent>(this, HandleCurrencyEscrowed);
            _eventBus.Subscribe<VoucherEscrowedEvent>(this, HandleVoucherEscrowed);

            _timeOutValue = _properties.GetValue(
                AccountingConstants.NoteAcceptorTimeLimitValue,
                DefaultTimeForTransaction);

            Logger.Debug("Initialized and configured the NoteAcceptorActivityTimeWatcher.");
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);

                if (_delayTimerCancellationToken != null)
                {
                    _delayTimerCancellationToken.Cancel(false);
                    _delayTimerCancellationToken.Dispose();
                    _delayTimerCancellationToken = null;
                }
            }

            _disposed = true;
        }

        private void HandleNoteAcceptorServiceAddedEvent(ServiceAddedEvent evt)
        {
            if (evt.ServiceType == typeof(INoteAcceptor))
            {
                _noteAcceptor = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
            }
        }

        private void HandleVoucherEscrowed(VoucherEscrowedEvent obj)
        {
            StartTheTask();
        }

        private void StartTheTask()
        {
            lock (_lock)
            {
                _delayTimerCancellationToken?.Cancel();
                _delayTimerCancellationToken?.Dispose();

                _delayTimerCancellationToken = new CancellationTokenSource();

                Task.Run(
                    async () =>
                    {
                        await Task.Delay(_timeOutValue, _delayTimerCancellationToken.Token);
                        await OnNoteAcceptorTimerExpired();
                    });
            }
        }

        private void HandleCurrencyEscrowed(CurrencyEscrowedEvent escrowed)
        {
            StartTheTask();
        }

        private async Task OnNoteAcceptorTimerExpired()
        {
            if (_noteAcceptor?.LastDocumentResult != DocumentResult.Escrowed)
            {
                return;
            }

            Logger.Info(
                "Time spent in escrow " +
                $" exceeds the allowed time of {TimeSpan.FromMilliseconds(_timeOutValue).TotalSeconds}");

            if (_noteAcceptor != null)
            {
                if (!await _noteAcceptor.Return())
                {
                    Logger.Info("Could not return the Document");
                }
                else
                {
                    Logger.Info("Returned the Document");

                }
            }
        }
    }
}