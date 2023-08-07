namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Application.Contracts;
    using Application.Contracts.TiltLogger;
    using Common.Events;
    using Common.G2SEventLogger;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    /// <summary>
    ///     G2SEventLogger records whenever a G2S event code has been sent. Information
    ///     about an associated IEvent is recorded if available.
    /// </summary>
    public class G2SEventLogger : IG2SEventLogger, IService, IDisposable
    {
        private const string BlockName = "Aristocrat.Monaco.G2S.G2SEventLogger.Log";
        private const string BlockIndexName = "Aristocrat.Monaco.G2S.G2SEventLogger.Index";
        private const string IndexField = @"Index";
        private const string TimeStampField = @"TimeStamp";
        private const string EventCodeField = @"EventCode";
        private const string InternalEventTypeField = @"InternalEventType";
        private const string TransactionIdField = @"TransactionId";
        private const string ProtocolEventType = @"Protocol";

        private readonly IPersistentStorageManager _persistentStorage;
        private readonly ITiltLogger _tiltLogger;
        private readonly IEventBus _eventBus;
        private readonly IIdProvider _idProvider;

        private int _maxStoredLogMessages = 100;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private List<G2SEventLogMessage> _g2sEventLogs;

        private IPersistentStorageAccessor _dataAccessor;
        private IPersistentStorageAccessor _indexAccessor;

        private bool _disposed;

        /// <summary>
        ///     Constructor.
        /// </summary>
        public G2SEventLogger() : this(
            ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
            ServiceManager.GetInstance().GetService<ITiltLogger>(),
            ServiceManager.GetInstance().GetService<IEventBus>(),
            ServiceManager.GetInstance().GetService<IIdProvider>(), false)
        {
        }

        /// <summary>
        ///     Creates the G2SEventLogger instance.
        /// </summary>
        /// <param name="persistentStorage">The persistent storage manager.</param>
        /// <param name="tiltLogger">The tilt logger.</param>
        /// <param name="eventBus">The event bus.</param>
        /// <param name="idProvider">The id provider.</param>
        /// <param name="initialize">Indicates whether or not to initialize.</param>
        public G2SEventLogger(IPersistentStorageManager persistentStorage, ITiltLogger tiltLogger, IEventBus eventBus, IIdProvider idProvider, bool initialize = true)
        {
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
            _tiltLogger = tiltLogger ?? throw new ArgumentNullException(nameof(tiltLogger));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));

            if (initialize)
            {
                InitializeLogger();
            }
        }

        /// <summary>Gets a value indicating whether the service is initialized.</summary>
        public bool Initialized { get; private set; }

        /// <inheritdoc />
        public IEnumerable<G2SEventLogMessage> Logs => _g2sEventLogs;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => typeof(G2SEventLogger).Name;

        public ICollection<Type> ServiceTypes => new[] { typeof(IG2SEventLogger) };

        /// <inheritdoc />
        public void Initialize()
        {
            InitializeLogger();
        }

        /// <summary>Realizing the IDispose interface to dispose of module.</summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void InitializeLogger()
        {
            if (Initialized)
            {
                const string errorMessage = "Cannot initialize G2SEventLogger more than once.";
                Logger.Fatal(errorMessage);
                throw new ServiceException(errorMessage);
            }

            var max = _tiltLogger.GetMax(ProtocolEventType);
            if (max > 0)
            {
                _maxStoredLogMessages = max;
            }

            _g2sEventLogs = new List<G2SEventLogMessage>(_maxStoredLogMessages);

            CreatePersistence();

            SubscribeToEvents();

            // Set service initialized.
            Initialized = true;

            LoadG2SEventLog();

            Logger.Debug("G2SEventLogger initialized");
        }

        private void LoadG2SEventLog()
        {
            _g2sEventLogs.Clear();

            var logs = _dataAccessor.GetAll();

            for (var index = 0; index < _maxStoredLogMessages; index++)
            {
                if (!logs.TryGetValue(index, out var values))
                {
                    continue;
                }

                var time = (DateTime)values[TimeStampField];
                if (time == default(DateTime))
                {
                    break;
                }

                var g2sEventLogMessage =
                    new G2SEventLogMessage
                    {
                        TimeStamp = time,
                        EventCode = (string)values[EventCodeField],
                        InternalEventType = (string)values[InternalEventTypeField],
                        TransactionId = (long)values[TransactionIdField]
                    };

                _g2sEventLogs.Insert(index, g2sEventLogMessage);
            }
        }

        private void LogToPersistence(G2SEventLogMessage g2sEventLog)
        {
            var index = ((int)_indexAccessor[IndexField] + 1) % _maxStoredLogMessages;

            using (var transaction = _dataAccessor.StartTransaction())
            {
                transaction.AddBlock(_indexAccessor);
                transaction[BlockIndexName, IndexField] = index;
                transaction[BlockName, index, TimeStampField] = g2sEventLog.TimeStamp;
                transaction[BlockName, index, EventCodeField] = g2sEventLog.EventCode;
                transaction[BlockName, index, InternalEventTypeField] = g2sEventLog.InternalEventType;
                transaction[BlockName, index, TransactionIdField] = g2sEventLog.TransactionId;
                transaction.Commit();
            }

            if (index >= _g2sEventLogs.Count)
            {
                _g2sEventLogs.Insert(index, g2sEventLog);
            }
            else
            {
                _g2sEventLogs[index] = g2sEventLog;
            }

            Logger.Info($"Logging events to persistence... complete! Index is {index}");
        }

        private void SubscribeToEvents()
        {
            _eventBus?.Subscribe<G2SEvent>(this, ReceiveEvent);
        }

        private void ReceiveEvent(G2SEvent g2sEvent)
        {
            G2SEventLogMessage g2sEventLogMessage = null;

            using (var scopedTransaction = _persistentStorage.ScopedTransaction())
            {
                var transactionId = long.MinValue;
                if (_idProvider != null)
                {
                    transactionId = _idProvider.GetNextTransactionId();
                }

                g2sEventLogMessage = new G2SEventLogMessage
                {
                    TimeStamp = g2sEvent.Timestamp,
                    EventCode = g2sEvent.EventCode,
                    InternalEventType = GetEventTypeString(g2sEvent),
                    TransactionId = transactionId
                };

                LogToPersistence(g2sEventLogMessage);

                scopedTransaction.Complete();
            }

            _eventBus?.Publish<G2SEventLogMessagePersistedEvent>(new G2SEventLogMessagePersistedEvent(g2sEventLogMessage));
        }

        private void CreatePersistence()
        {
            if (_persistentStorage.BlockExists(BlockIndexName))
            {
                _indexAccessor = _persistentStorage.GetBlock(BlockIndexName);
            }
            else
            {
                // Create and init to -1, since the initial log entry will increment it making the first entry 0
                _indexAccessor = _persistentStorage.CreateBlock(PersistenceLevel.Critical, BlockIndexName, 1);
                _indexAccessor[IndexField] = -1;
            }

            _dataAccessor = _persistentStorage.BlockExists(BlockName)
                ? _persistentStorage.GetBlock(BlockName)
                : _persistentStorage.CreateBlock(PersistenceLevel.Critical, BlockName, _maxStoredLogMessages);
        }

        private string GetEventTypeString(G2SEvent g2sEvent)
        {
            if (g2sEvent.InternalEvent == null)
            {
                return string.Empty;
            }

            var internalEventType = g2sEvent.InternalEvent.GetType();
            if (internalEventType == null)
            {
                return string.Empty;
            }

            return internalEventType.AssemblyQualifiedName;
        }
    }
}
