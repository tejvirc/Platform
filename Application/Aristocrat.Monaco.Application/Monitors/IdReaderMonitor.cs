namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;
    using Contracts.Localization;
    using Hardware.Contracts.IdReader;
    using Kernel;
    using log4net;
    using Monaco.Localization.Properties;

    public sealed class IdReaderMonitor : IService, IDisposable
    {
        private static Guid? _disconnectedReaderGuid;

        private readonly IEventBus _eventBus;
        private readonly IIdReaderProvider _idReaderProvider;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly IMessageDisplay _messageDisplay;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly DisplayableMessage _disconnectedMessage;
        private bool _disposed;
        private bool _initialized;

        public IdReaderMonitor(IEventBus eventBus,
            IIdReaderProvider idReaderProvider,
            ISystemDisableManager systemDisableManager,
            IMessageDisplay messageDisplay)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _idReaderProvider = idReaderProvider ?? throw new ArgumentNullException(nameof(idReaderProvider));
            _systemDisableManager = systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _messageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));

            _disconnectedMessage = new DisplayableMessage(
                DisconnectedMessageCallback,
                DisplayableMessageClassification.SoftError,
                DisplayableMessagePriority.Immediate,
                ApplicationConstants.IdReaderDisconnectedGuid);
        }

        public IdReaderMonitor()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IIdReaderProvider>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<IMessageDisplay>()
            )
        {
        }

        /// <inheritdoc />
        ~IdReaderMonitor()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        public string Name => nameof(IdReaderMonitor);

        public ICollection<Type> ServiceTypes => new[] { typeof(IdReaderMonitor) };

        /// <inheritdoc />
        public void Initialize()
        {
            Logger.Info("Initializing IdReaderMonitor...");

            if (_initialized)
            {
                const string errorMessage = "Cannot initialize IdReaderMonitor more than once!";
                Logger.Fatal(errorMessage);
                throw new ServiceException(errorMessage);
            }

            Subscribe();

            _initialized = true;

            if (!CardReaderConnected())
            {
                Disable(Guid.NewGuid());
            }

            Logger.Info("Initializing IdReaderMonitor...complete!");
        }

        private bool CardReaderConnected()
        {
            bool connected = true;
            if (_idReaderProvider != null)
            {
                foreach (var cardReader in _idReaderProvider.Adapters)
                {
                    connected &= cardReader.Connected;
                }
            }

            return connected;
        }

        private void Subscribe()
        {
            _eventBus.Subscribe<DisconnectedEvent>(this, e => Disable(e.GloballyUniqueId));
            _eventBus.Subscribe<ConnectedEvent>(this, _ => RemoveDisableMessage());
            _eventBus.Subscribe<InspectedEvent>(this, _ => RemoveDisableMessage());
            _eventBus.Subscribe<InspectionFailedEvent>(this, e => Disable(e.GloballyUniqueId));
            _eventBus.Subscribe<RemoveDisabledMessageEvent>(this, _ => RemoveDisableMessage());
        }

        private void Disable(Guid disconnectedId)
        {
            // Only disable if we don't already have an IdReaderMonitor-initiated disable message
            if (_disconnectedReaderGuid.HasValue)
            {
                return;
            }

            _disconnectedReaderGuid = disconnectedId;

            _systemDisableManager.Disable(disconnectedId, SystemDisablePriority.Normal, DisconnectedMessageCallback);

            _messageDisplay.DisplayMessage(_disconnectedMessage);
        }

        private static string DisconnectedMessageCallback()
        {
            return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.IdReaderDisconnected);
        }

        private void RemoveDisableMessage()
        {
            if (CardReaderConnected())
            {
                if (_systemDisableManager.IsDisabled && _disconnectedReaderGuid.HasValue)
                {
                    _systemDisableManager.Enable(_disconnectedReaderGuid.Value);
                }

                _disconnectedReaderGuid = null;

                _messageDisplay.RemoveMessage(_disconnectedMessage);
            }
        }
    }
}