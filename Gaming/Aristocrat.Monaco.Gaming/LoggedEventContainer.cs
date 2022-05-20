namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Timers;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.TiltLogger;
    using Contracts;
    using Contracts.Bonus;
    using Contracts.Models;
    using Contracts.Progressives;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Timer = System.Timers.Timer;

    public class LoggedEventContainer : ILoggedEventContainer, IDisposable
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;
        private const string EventsBlobField = @"EventsBlob";
        private const string EventsCompletedBlobField = @"EventsCompletedBlob";
        private const double EventUpdateTimerInterval = 1000;
        private const string EndEvent = "GameEndedEvent";
        private const int MaxLoggedGameEvents = 100;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ManualResetEvent EventsCompleteReady = new ManualResetEvent(false);
        private static readonly ManualResetEvent GameEndLogged = new ManualResetEvent(false);

        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IEventBus _eventBus;
        private readonly IIdProvider _idProvider;
        private readonly IPropertiesManager _props;
        private readonly IPersistentStorageAccessor _accessor;
        private readonly string _tiltLoggerConfigurationPath = "/TiltLogger/Configuration";
        private readonly ConcurrentQueue<(IEvent Event, DateTime DateTime)> _queuedEvents = new ConcurrentQueue<(IEvent Event, DateTime DateTime)>();
        private readonly Type _endEventType = typeof(GameEndedEvent);
        private readonly object _locker = new object();
        private readonly List<Type> _eventsForReceiveEvent = new List<Type>
        {
            typeof(BonusAwardedEvent),
            typeof(CurrencyInCompletedEvent),
            typeof(HandpayCompletedEvent),
            typeof(TransferOutCompletedEvent),
            typeof(VoucherRedeemedEvent),
            typeof(WatOnCompleteEvent),
            typeof(ProgressiveCommitEvent),
            typeof(PrimaryGameStartedEvent),
            typeof(PrimaryGameEndedEvent),
            typeof(SecondaryGameStartedEvent),
            typeof(SecondaryGameEndedEvent),
        };

        private IList<GameEventLogEntry> _gameEvents;
        private IList<GameEventLogEntry> _gameEventsCompleted;
        private List<EventDescription> _tiltLoggerEventDescriptions = new List<EventDescription>();
        private byte[] _eventInfoData;
        private byte[] _eventInfoDataCompleted;
        private bool _startupEventsQueued;
        private Timer _eventUpdateTimer;

        private bool _disposed;

        private bool KeepGameHistoryEvents =>
            _props.GetValue(GamingConstants.KeepGameRoundEvents, true);

        public IEnumerable<GameEventLogEntry> Events
        {
            get
            {
                lock (_locker)
                {
                    return new List<GameEventLogEntry>(_gameEvents);
                }
            }

            private set
            {
                using var transaction = _accessor.StartTransaction();

                transaction[EventsBlobField] = _eventInfoData = StorageUtilities.ToByteArray(value);

                transaction.Commit();
            }
        }

        public IEnumerable<GameEventLogEntry> EventsForCompletedGame
        {
            get => new List<GameEventLogEntry>(_gameEventsCompleted);

            private set
            {
                using var transaction = _accessor.StartTransaction();

                transaction[EventsCompletedBlobField] = _eventInfoDataCompleted = StorageUtilities.ToByteArray(value);

                transaction.Commit();
            }
        }


        public LoggedEventContainer(
            IPersistentStorageManager storageManager,
            IEventBus eventBus,
            IIdProvider idProvider,
            IPropertiesManager props)
        {
            _persistentStorage = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            _props = props ?? throw new ArgumentNullException(nameof(props));

            if (!KeepGameHistoryEvents)
            {
                return;
            }

            _accessor = _persistentStorage.GetAccessor(Level, GetType().ToString());
            _eventInfoData = (byte[])_accessor[EventsBlobField];
            _eventInfoDataCompleted = (byte[])_accessor[EventsCompletedBlobField];

            _gameEvents = StorageUtilities.GetListFromByteArray<GameEventLogEntry>(_eventInfoData).ToList();
            _gameEventsCompleted = StorageUtilities.GetListFromByteArray<GameEventLogEntry>(_eventInfoDataCompleted).ToList();

            SubscribeToTiltLoggerEvents();
            SubscribeToTransactionEvents();

            _eventUpdateTimer = new Timer(EventUpdateTimerInterval);
            _eventUpdateTimer.Elapsed += EventUpdateTimerElapsed;
            _eventUpdateTimer.Start();
        }

        public List<GameEventLogEntry> HandOffEvents()
        {
            if (!KeepGameHistoryEvents)
            {
                return new List<GameEventLogEntry>();
            }

            StopTimer();

            // Block until the events are filled
            EventsCompleteReady.WaitOne();

            var returnEvents = new List<GameEventLogEntry>(EventsForCompletedGame);

            EventsForCompletedGame = Enumerable.Empty<GameEventLogEntry>();

            _gameEventsCompleted.Clear();
            EventsCompleteReady.Reset();

            StartTimer();

            return returnEvents;
        }

        /// <summary>
        ///     Realizing the IDispose interface to dispose of module.
        /// </summary>
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
                _eventBus?.UnsubscribeAll(this);

                if (_eventUpdateTimer != null)
                {
                    lock (_locker)
                    {
                        _eventUpdateTimer.Stop();
                        _eventUpdateTimer.Dispose();
                        _eventUpdateTimer = null;
                    }
                }
            }

            _disposed = true;
        }

        private void SubscribeToTransactionEvents()
        {
            foreach (var eventType in _eventsForReceiveEvent)
            {
                _eventBus.Subscribe(this, eventType, ReceiveEvent);
            }

            _eventBus.Subscribe(this, _endEventType, ReceiveGameEndedEvent);
        }

        /// <summary>
        ///     Subscribe to the same events that TiltLogger logs
        /// </summary>
        private void SubscribeToTiltLoggerEvents()
        {
            try
            {
                var config = ConfigurationUtilities.GetConfiguration(_tiltLoggerConfigurationPath,
                    () => new TiltLoggerConfiguration
                    {
                        ArrayOfEventTypes = new EventType[0],
                        ArrayOfEventDescription = new EventDescription[0]
                    });


                _tiltLoggerEventDescriptions = config.ArrayOfEventDescription.ToList();
            }
            catch (Exception ex)
            {
                Logger.Error($"Invalid or missing configuration for TiltLogger events in {GetType()}.", ex);
            }

            foreach (var eventBeingSubscribed in _tiltLoggerEventDescriptions)
            {
                var theType = Type.GetType(eventBeingSubscribed.Name);
                if (theType != null)
                {
                    _eventBus.Subscribe(this, theType, ReceiveEvent);
                    Logger.Debug($"Subscribed to {eventBeingSubscribed.Name} {eventBeingSubscribed.Type}");
                }
                else
                {
                    Logger.Error($"{eventBeingSubscribed.Name} not found");
                }
            }
        }

        private void ReceiveGameEndedEvent(IEvent data)
        {
            StopTimer();
            ReceiveEvent(data);
            LogToPersistence();

            GameEndLogged.WaitOne();

            lock (_locker)
            {
                var gameEndedIndex = _gameEvents.ToList().FindIndex(e => e.LogEntry == EndEvent) + 1;
                _gameEventsCompleted = _gameEvents.Take(gameEndedIndex).ToList();
                _gameEvents = _gameEvents.Skip(gameEndedIndex).ToList();
            }

            using (var scopedTransaction = _persistentStorage.ScopedTransaction())
            {
                EventsForCompletedGame = _gameEventsCompleted;
                Events = _gameEvents;

                scopedTransaction.Complete();
            }

            EventsCompleteReady.Set();
            GameEndLogged.Reset();

            StartTimer();
        }

        private void ReceiveEvent(IEvent data)
        {
            QueueEvent(data);
        }

        private void EventUpdateTimerElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            StopTimer();
            LogToPersistence();
            StartTimer();
        }

        private void LogToPersistence()
        {
            lock (_locker)
            {
                var endEventFound = false;
                using (var scopedTransaction = _persistentStorage.ScopedTransaction())
                {
                    while (_queuedEvents.TryDequeue(out var localEvent))
                    {
                        if (localEvent.Event == null)
                        {
                            continue;
                        }

                        endEventFound = localEvent.Event.GetType() == _endEventType;

                        switch (localEvent.Event)
                        {
                            case BonusAwardedEvent loggedEvent:
                                LogCurrencyEvent(localEvent, ResourceKeys.BonusAward, loggedEvent.Transaction.CashableAmount);
                                break;
                            case CurrencyInCompletedEvent loggedEvent:
                                LogCurrencyEvent(localEvent, ResourceKeys.BillIn, loggedEvent.Amount);
                                break;
                            case HandpayCompletedEvent loggedEvent:
                                LogCurrencyEvent(localEvent, ResourceKeys.Handpay, loggedEvent.Transaction.TransactionAmount);
                                break;
                            case TransferOutCompletedEvent loggedEvent:
                                LogCurrencyEvent(localEvent, ResourceKeys.TransferOut, loggedEvent.Total);
                                break;
                            case VoucherRedeemedEvent loggedEvent:
                                LogCurrencyEvent(localEvent, ResourceKeys.VoucherIn, loggedEvent.Transaction.Amount);
                                break;
                            case WatOnCompleteEvent loggedEvent:
                                LogCurrencyEvent(localEvent, ResourceKeys.TransferIn, loggedEvent.Transaction.TransactionAmount);
                                break;
                            case ProgressiveCommitEvent progEvent:
                                LogProgressiveEvent(localEvent, progEvent);
                                break;
                            case GameEndedEvent _:
                                LogEndEvent(localEvent);
                                break;
                            case SecondaryGameStartedEvent _:
                            case SecondaryGameEndedEvent _:
                                LogSecondaryGameEvent(localEvent);
                                break;
                            default:
                                var subscribedEvent =
                                    (from eve in _tiltLoggerEventDescriptions
                                        where eve.Name.Split(',')[0] == localEvent.Event.GetType().ToString()
                                        select eve).FirstOrDefault();

                                var description = localEvent.Event.ToString();

                                // Some events (e.g. printer paper-in-chute) deliberately have empty descriptions,
                                // to indicate we don't want to display or log them.
                                if (string.IsNullOrEmpty(description))
                                {
                                    Logger.Debug($"Not logging {localEvent.Event.GetType()} because of empty description");
                                }
                                else
                                {
                                    AddGameEventLog(
                                        new GameEventLogEntry(
                                            localEvent.DateTime,
                                            subscribedEvent?.Type ?? localEvent.GetType().Name,
                                            description,
                                            _idProvider?.GetNextLogSequence<LoggedEventContainer>() ?? 0));
                                }

                                break;
                        }
                    }

                    Events = _gameEvents;
                    scopedTransaction.Complete();
                }

                if (endEventFound)
                {
                    GameEndLogged.Set();
                }
            }
        }

        private void LogCurrencyEvent((IEvent Event, DateTime DateTime) localEvent, string labelResource, long amount)
        {
            AddGameEventLog(
                new GameEventLogEntry(
                    localEvent.DateTime,
                    Localizer.For(CultureFor.Operator).GetString(labelResource),
                    amount.MillicentsToDollars().FormattedCurrencyString(),
                    _idProvider?.GetNextLogSequence<LoggedEventContainer>() ?? 0));
        }

        private void LogProgressiveEvent((IEvent Event, DateTime DateTime) localEvent, ProgressiveCommitEvent progEvent)
        {
            AddGameEventLog(
                new GameEventLogEntry(
                    localEvent.DateTime,
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Progressive),
                    $"{progEvent.Level.LevelName}, {Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Win)} {progEvent.Jackpot.PaidAmount.MillicentsToDollars().FormattedCurrencyString()}",
                    _idProvider?.GetNextLogSequence<LoggedEventContainer>() ?? 0));
        }

        private void LogEndEvent((IEvent Event, DateTime DateTime) localEvent)
        {
            AddGameEventLog(
                new GameEventLogEntry(
                    localEvent.DateTime,
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Gameplay),
                    localEvent.Event.ToString(),
                    _idProvider?.GetNextLogSequence<LoggedEventContainer>() ?? 0));
        }

        private void LogSecondaryGameEvent((IEvent Event, DateTime DateTime) localEvent)
        {
            AddGameEventLog(
                new GameEventLogEntry(
                    localEvent.DateTime,
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Gameplay),
                    localEvent.Event.ToString(),
                    _idProvider?.GetNextLogSequence<LoggedEventContainer>() ?? 0));
        }

        private void QueueEvent(IEvent receivedEvent)
        {
            // We queue any start-up events first if they have not already been queued.  This is done
            // once we handle an initial subscribed event so that the idProvider is available to set the
            // next transaction Id for these start-up events (for accurate sorting).
            if (!_startupEventsQueued)
            {
                _queuedEvents.Enqueue(ValueTuple.Create((IEvent)null, DateTime.UtcNow)); // Software verified startup event
                _queuedEvents.Enqueue(ValueTuple.Create((IEvent)null, DateTime.UtcNow)); // NVRAM integrity
                _startupEventsQueued = true;
            }

            _queuedEvents.Enqueue(ValueTuple.Create(receivedEvent, DateTime.UtcNow));
        }

        private void AddGameEventLog(GameEventLogEntry eventLog)
        {
            while (_gameEvents.Count >= MaxLoggedGameEvents)
            {
                _gameEvents.RemoveAt(0);
            }

            _gameEvents.Add(eventLog);
        }

        private void StopTimer()
        {
            lock (_locker)
            {
                if (_eventUpdateTimer != null && _eventUpdateTimer.Enabled)
                {
                    _eventUpdateTimer.Enabled = false;
                }
            }
        }

        private void StartTimer()
        {
            lock (_locker)
            {
                if (_eventUpdateTimer != null && !_eventUpdateTimer.Enabled)
                {
                    _eventUpdateTimer.Enabled = true;
                }
            }
        }
    }
}
