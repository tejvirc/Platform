namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;
    using Contracts.AlteredMediaLogger;
    using Contracts.TiltLogger;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts;
    using log4net;

    /// <summary>
    ///     AlteredMediaLogger record logs whenever any Alterable Media got changed and provide an interface
    ///     to retrieve recorded log to display in audit menu
    /// </summary>
    public class AlteredMediaLogger : IAlteredMediaLogger, IService, IDisposable
    {
        private const string BlockName = "Aristocrat.Monaco.Application.AlteredMediaLogger.Log";
        private const string BlockIndexName = "Aristocrat.Monaco.Application.AlteredMediaLogger.Index";
        private const string IndexField = @"Index";
        private const string TimeStampField = @"TimeStamp";
        private const string MediaTypeField = @"MediaType";
        private const string ReasonForChangeField = @"ReasonForChange";
        private const string AuthenticationField = @"Authentication";
        private const string TransactionIdField = @"TransactionId";
        private const string SoftwareChangeEventType = "SoftwareChange";

        private readonly IPersistentStorageManager _persistentStorage;
        private readonly ITiltLogger _tiltLogger;
        private readonly IEventBus _eventBus;
        private readonly IIdProvider _idProvider;

        private int _maxStoredLogMessages = 100;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private List<AlteredMediaLogMessage> _alteredMediaLogs;

        private IPersistentStorageAccessor _dataAccessor;
        private IPersistentStorageAccessor _indexAccessor;

        private bool _disposed;

        /// <summary>
        ///     Constructor.
        /// </summary>
        public AlteredMediaLogger() : this(
            ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
            ServiceManager.GetInstance().GetService<ITiltLogger>(),
            ServiceManager.GetInstance().GetService<IEventBus>(),
            ServiceManager.GetInstance().GetService<IIdProvider>(), false)
        {
        }

        /// <summary>
        ///     Creates the AlteredMediaLogger instance.
        /// </summary>
        /// <param name="persistentStorage">The persistent storage manager.</param>
        /// <param name="tiltLogger">The tilt logger.</param>
        /// <param name="eventBus">The event bus.</param>
        /// <param name="idProvider">The id provider.</param>
        /// <param name="initialize">Indicates whether or not to initialize.</param>
        public AlteredMediaLogger(IPersistentStorageManager persistentStorage, ITiltLogger tiltLogger, IEventBus eventBus, IIdProvider idProvider, bool initialize = true)
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
        public IEnumerable<AlteredMediaLogMessage> Logs => _alteredMediaLogs;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => typeof(AlteredMediaLogger).Name;

        public ICollection<Type> ServiceTypes => new[] { typeof(IAlteredMediaLogger) };

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
                const string errorMessage = "Cannot initialize AlterMediaLogger more than once.";
                Logger.Fatal(errorMessage);
                throw new ServiceException(errorMessage);
            }

            var max = _tiltLogger.GetMax(SoftwareChangeEventType);
            if (max > 0)
            {
                _maxStoredLogMessages = max;
            }

            _alteredMediaLogs = new List<AlteredMediaLogMessage>(_maxStoredLogMessages);

            CreatePersistence();

            SubscribeToEvents();

            // Set service initialized.
            Initialized = true;

            LoadAlterMediaLog();

            Logger.Debug("AlterMediaLogger initialized");
        }

        private void LoadAlterMediaLog()
        {
            _alteredMediaLogs.Clear();

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

                var alteredMediaLogMessage =
                    new AlteredMediaLogMessage
                    {
                        TimeStamp = time,
                        MediaType = (string)values[MediaTypeField],
                        ReasonForChange = (string)values[ReasonForChangeField],
                        Authentication = (string)values[AuthenticationField],
                        TransactionId = (long)values[TransactionIdField]
                    };

                _alteredMediaLogs.Insert(index, alteredMediaLogMessage);
            }
        }

        private void LogToPersistence(AlteredMediaLogMessage alteredMediaLog)
        {
            var index = ((int)_indexAccessor[IndexField] + 1) % _maxStoredLogMessages;

            using (var transaction = _dataAccessor.StartTransaction())
            {
                transaction.AddBlock(_indexAccessor);
                transaction[BlockIndexName, IndexField] = index;
                transaction[BlockName, index, TimeStampField] = alteredMediaLog.TimeStamp;
                transaction[BlockName, index, MediaTypeField] = alteredMediaLog.MediaType;
                transaction[BlockName, index, ReasonForChangeField] = alteredMediaLog.ReasonForChange;
                transaction[BlockName, index, AuthenticationField] = alteredMediaLog.Authentication;
                transaction[BlockName, index, TransactionIdField] = alteredMediaLog.TransactionId;
                transaction.Commit();
            }
 
            if (index >= _alteredMediaLogs.Count)
            {
                _alteredMediaLogs.Insert(index, alteredMediaLog);
            }
            else
            {
                _alteredMediaLogs[index] = alteredMediaLog;
            }

            Logger.Info($"Logging events to persistence... complete! Index is {index}");
        }

        private void SubscribeToEvents()
        {
            _eventBus?.Subscribe<MediaAlteredEvent>(this, ReceiveEvent);
        }

        private void ReceiveEvent(MediaAlteredEvent mediaAlteredEvent)
        {
            using (var scopedTransaction = _persistentStorage.ScopedTransaction())
            {
                var transactionId = long.MinValue;
                if (_idProvider != null)
                {
                    transactionId = _idProvider.GetNextTransactionId();
                }

                LogToPersistence(
                    new AlteredMediaLogMessage
                    {
                        MediaType = mediaAlteredEvent.MediaType,
                        ReasonForChange = mediaAlteredEvent.ReasonForChange,
                        TimeStamp = mediaAlteredEvent.Timestamp,
                        Authentication = mediaAlteredEvent.MediaDescriptor,
                        TransactionId = transactionId
                    }
                );

                scopedTransaction.Complete();
            }
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
    }
}