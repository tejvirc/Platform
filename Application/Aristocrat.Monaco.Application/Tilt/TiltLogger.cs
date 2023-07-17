namespace Aristocrat.Monaco.Application.Tilt
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Timers;
    using Common;
    using Contracts;
    using Contracts.Authentication;
    using Contracts.Localization;
    using Contracts.TiltLogger;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Localization;
    using log4net;
    using Monaco.Localization.Properties;
    using Vgt.Client12.Application.OperatorMenu;
    using Timer = System.Timers.Timer;

    /// <summary>
    ///     TiltLogger collects tilts and errors in order to record them.
    /// </summary>
    public sealed class TiltLogger : BaseEventLogAdapter, IService, ITiltLogger, IDisposable
    {
        private const string EventStringDelimiter = " -- ";
        private const int DefaultEventStringCount = 6;
        private const string Index = "Index";
        private const string Events = "Events";
        private const string SoftwareVerified = "Software verified";
        private const string IntegrityCheckEvent = "NVRAM Integrity check";
        private const string Passed = " - Passed.";
        private const string Failed = " - Failed.";
        private const string Info = "info";
        private const double EventUpdateTimerInterval = 1000;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string _blockIndexName = $"{nameof(TiltLogger)}Current";
        private readonly string _blockName = $"{nameof(TiltLogger)}Format";

        private readonly ConcurrentQueue<(IEvent Event, DateTime DateTime, string Role)> _queuedEvents = new();

        private readonly string _tiltLoggerConfigurationPath = "/TiltLogger/Configuration";

        private readonly Dictionary<string, (int Max, string Combined, bool AppendSecurityLevel)> _eventTypes =
            new Dictionary<string, (int Max, string Combined, bool AppendSecurityLevel)>();

        private readonly Dictionary<string, int> _eventTypeIndex = new Dictionary<string, int>();

        private readonly Dictionary<string, IPersistentStorageAccessor> _indexAccessor = new Dictionary<string, IPersistentStorageAccessor>();

        private readonly Dictionary<string, IPersistentStorageAccessor> _dataAccessor = new Dictionary<string, IPersistentStorageAccessor>();

        private readonly Dictionary<string, int> _eventsToSubscribe = new Dictionary<string, int>();

        private readonly Dictionary<string, int> _eventsSubscribed = new Dictionary<string, int>();

        private readonly Dictionary<string, (bool Reload, bool Rolled)> _reloadEventHistory = new Dictionary<string, (bool Reload, bool Rolled)>();

        private readonly Dictionary<string, bool> _eventTypeRolled = new Dictionary<string, bool>();

        private ITime _timeService;

        private IOperatorMenuLauncher _operatorMenuLauncher;

        private IEventBus _eventBus;

        private IPersistentStorageManager _persistentStorage;

        private IIdProvider _idProvider;

        private IPropertiesManager _properties;

        private ILocalization _localization;

        private bool _disposed;

        private bool _startupEventsQueued;

        private bool _reloadAllView;

        private List<EventDescription> _eventDescriptionsToSubscribe = new List<EventDescription>();

        private Timer _eventUpdateTimer;

        private bool _logBootVerification = true;
        private bool _logCriticalMemoryIntegrityCheck = true;

        /// <summary>Gets a value indicating whether the service is initialized.</summary>
        public bool Initialized { get; private set; }

        /// <summary>Realizing the IDispose interface to dispose of module.</summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _eventBus.UnsubscribeAll(this);

            if (_eventUpdateTimer != null)
            {
                _eventUpdateTimer.Stop();
                _eventUpdateTimer.Dispose();
                _eventUpdateTimer = null;
            }
        }

        /// <summary>Gets the Name.</summary>
        public string Name => GetType().Name;

        /// <summary>Gets the Service types.</summary>
        public ICollection<Type> ServiceTypes => new[] { typeof(ITiltLogger) };

        /// <inheritdoc />
        public void Initialize()
        {
            if (Initialized)
            {
                const string errorMessage = "Cannot initialize more than once.";
                Logger.Fatal(errorMessage);
                throw new ServiceException(errorMessage);
            }

            _persistentStorage = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            _timeService = ServiceManager.GetInstance().TryGetService<ITime>();
            _operatorMenuLauncher = ServiceManager.GetInstance().GetService<IOperatorMenuLauncher>();
            _properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _localization = ServiceManager.GetInstance().GetService<ILocalization>();

            ConfigurationParse();

            CreatePersistence();

            SubscribeToEvents();

            _eventUpdateTimer = new Timer(EventUpdateTimerInterval);
            _eventUpdateTimer.Elapsed += EventUpdateTimerElapsed;
            _eventUpdateTimer.Start();

            // Set service initialized.
            Initialized = true;

            Logger.Debug("TiltLogger initialized");
        }

        /// <inheritdoc />
        public event EventHandler<TiltLogAppendedEventArgs> TiltLogAppendedTilt;

        /// <inheritdoc />
        public bool ReloadEventHistory(string type, bool onLoaded)
        {
            var reloadEventHistory = false;

            if (string.IsNullOrEmpty(type))
            {
                reloadEventHistory = onLoaded && _reloadAllView;
                _reloadAllView = false;
            }
            else if (_reloadEventHistory.TryGetValue(type, out var reload))
            {
                var rolled = reload.Rolled;
                if (!rolled)
                {
                    if (_eventTypes.TryGetValue(type, out var value))
                    {
                        if (!string.IsNullOrEmpty(value.Combined) && _reloadEventHistory.TryGetValue(value.Combined, out var combined))
                        {
                            rolled = combined.Rolled;
                        }
                    }
                }

                reloadEventHistory = reload.Reload && (onLoaded || rolled);
                _reloadEventHistory[type] = ValueTuple.Create(false, false);
            }

            return reloadEventHistory;
        }

        /// <inheritdoc />
        public int GetEventsToSubscribe(string type)
        {
            var eventsToSubscribe = 0;
            if (string.IsNullOrEmpty(type))
            {
                foreach (var e in _eventTypes)
                {
                    if (_eventsToSubscribe.TryGetValue(e.Key, out var eventTypesToSubscribe))
                    {
                        eventsToSubscribe += eventTypesToSubscribe;
                    }
                }
            }
            else if (_eventsToSubscribe.TryGetValue(type, out var eventTypesToSubscribe))
            {
                eventsToSubscribe += eventTypesToSubscribe;
            }

            return eventsToSubscribe;
        }

        /// <inheritdoc />
        public int GetEventsSubscribed(string type)
        {
            var eventsSubscribed = 0;
            if (string.IsNullOrEmpty(type))
            {
                foreach (var e in _eventTypes)
                {
                    if (_eventsSubscribed.TryGetValue(e.Key, out var eventTypesSubscribed))
                    {
                        eventsSubscribed += eventTypesSubscribed;
                    }
                }
            }
            else if (_eventsSubscribed.TryGetValue(type, out var eventTypesSubscribed))
            {
                eventsSubscribed += eventTypesSubscribed;
            }

            return eventsSubscribed;
        }

        /// <inheritdoc />
        public IEnumerable<EventDescription> GetEvents(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                var unsortedList = new List<EventDescription>();
                foreach (var e in _eventTypes)
                {
                    unsortedList.AddRange(GetTypeEvents(e.Key).ToList());
                }

                return unsortedList;
            }

            if (type.Contains('|'))
            {
                var unsortedList = new List<EventDescription>();
                var eventTypes = type.Split('|');
                foreach (var e in eventTypes)
                {
                    unsortedList.AddRange(GetTypeEvents(e).ToList());
                }

                return unsortedList;
            }

            return GetTypeEvents(type);
        }

        /// <inheritdoc />
        public int GetMax(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return _eventTypes.Sum(e => GetTypeMax(e.Key, true));
            }

            if (!type.Contains('|'))
            {
                return GetTypeMax(type);
            }

            var combinedList = new List<string>();
            var max = 0;
            var eventTypes = type.Split('|');
            foreach (var e in eventTypes)
            {
                max += GetTypeMax(e, IgnoreCombined(e, eventTypes, combinedList));
                var combined = GetCombined(e);
                if (!string.IsNullOrEmpty(combined) && !combinedList.Contains(combined))
                {
                    combinedList.Add(combined);
                }
            }

            return max;
        }

        public List<string> GetCombinedTypes(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return null;
            }

            // Get all of the types that are combined to reach the specified max
            var combinedTypes = _eventTypes.Where(e => e.Value.Combined == type)
                .Select(e => e.Key)
                .ToList();

            if (!combinedTypes.Contains(type))
            {
                combinedTypes.Add(type);
            }

            return combinedTypes;
        }

        private IEnumerable<EventDescription> GetTypeEvents(string type)
        {
            var list = new List<EventDescription>();

            if (!_eventTypes.TryGetValue(type, out (int Max, string Combined, bool AppendSecurityLevel) typeValue))
            {
                Logger.Error($"GetTypeEvents _eventTypes key {type} NOT FOUND");
                return list;
            }

            var key = type;
            if (!string.IsNullOrEmpty(typeValue.Combined))
            {
                if (!_eventTypes.ContainsKey(key))
                {
                    Logger.Error($"GetTypeEvents _eventTypes key {typeValue.Combined} NOT FOUND FOR KEY {key}");
                    return list;
                }

                key = typeValue.Combined;
            }

            if (!_eventTypes.TryGetValue(key, out var eventTypesValue))
            {
                Logger.Error($"GetTypeEvents _eventTypes key {key} NOT FOUND");
                return list;
            }

            if (!_indexAccessor.TryGetValue(key, out var indexAccessor))
            {
                Logger.Error($"GetEvents _indexAccessor key {key} NOT FOUND");
                return list;
            }

            if (!_dataAccessor.TryGetValue(key, out var dataAccessor))
            {
                Logger.Error($"GetEvents _dataAccessor key {key} NOT FOUND");
                return list;
            }

            var unsortedList = new List<EventDescription>();
            var records = new List<(int Index, string EventString)>(eventTypesValue.Max);

            var latestIndex = (int)indexAccessor[Index];
            for (var i = 0; i < eventTypesValue.Max; i++)
            {
                var eventString = (string)dataAccessor[i, Events];
                if (string.IsNullOrEmpty(eventString))
                {
                    break;
                }

                var index = (i - latestIndex - 1) % eventTypesValue.Max;
                if (index < 0)
                {
                    index += eventTypesValue.Max;
                }

                records.Add(ValueTuple.Create(index, eventString));
            }

            unsortedList.AddRange(records.OrderByDescending(r => r.Index).Select(r => r.EventString).Select(FromDelimitedString));

            list.AddRange(unsortedList.Where(u => u.Type.Equals(type)));

            return list;
        }

        private string GetCombined(string type)
        {
            if (_eventTypes.TryGetValue(type, out var eventTypesValue))
            {
                return eventTypesValue.Combined;
            }

            Logger.Error($"GetCombined _eventTypes key {type} NOT FOUND");
            return string.Empty;
        }

        private bool IgnoreCombined(string type, string[] eventTypes, List<string> combinedList)
        {
            if (!_eventTypes.TryGetValue(type, out var typeValue))
            {
                Logger.Error($"IgnoreCombined _eventTypes key {type} NOT FOUND");
                return false;
            }

            if (string.IsNullOrEmpty(typeValue.Combined))
            {
                return false;
            }

            if (eventTypes.Contains(typeValue.Combined))
            {
                return true;
            }

            if (combinedList.Contains(typeValue.Combined))
            {
                return true;
            }

            return false;
        }

        private int GetTypeMax(string type, bool ignoreCombined = false)
        {
            if (!_eventTypes.TryGetValue(type, out var typeValue))
            {
                Logger.Error($"GetTypeMax _eventTypes key {type} NOT FOUND");
                return 0;
            }

            var key = type;
            if (!string.IsNullOrEmpty(typeValue.Combined))
            {
                if (ignoreCombined)
                {
                    return 0;
                }

                if (!_eventTypes.ContainsKey(key))
                {
                    Logger.Error($"GetTypeMax _eventTypes key {typeValue.Combined} NOT FOUND FOR KEY {key}");
                    return 0;
                }

                key = typeValue.Combined;
            }

            if (_eventTypes.TryGetValue(key, out var eventTypesValue))
            {
                return eventTypesValue.Max;
            }

            Logger.Error($"GetTypeMax _eventTypes key {key} NOT FOUND");
            return 0;
        }

        private void ConfigurationParse()
        {
            try
            {
                GetTiltLoggerConfiguration();
            }
            catch (Exception ex)
            {
                Logger.Error("Invalid or missing configuration for TiltLogger.", ex);
            }
        }

        private void CreatePersistence()
        {
            foreach (var e in _eventTypes)
            {
                var type = e.Key;
                if (!string.IsNullOrEmpty(e.Value.Combined))
                {
                    type = e.Value.Combined;
                }

                var blockIndexName = $"{_blockIndexName}_{type}";
                if (_persistentStorage.BlockExists(blockIndexName))
                {
                    if (!_indexAccessor.ContainsKey(type))
                    {
                        _indexAccessor.Add(type, _persistentStorage.GetBlock(blockIndexName));
                    }

                    if (!_indexAccessor.TryGetValue(type, out var indexAccessor))
                    {
                        Logger.Error($"CreatePersistence _indexAccessor key {type} NOT FOUND");
                        continue;
                    }

                    if (!_eventTypeIndex.ContainsKey(type))
                    {
                        _eventTypeIndex.Add(type, (int)indexAccessor[Index]);
                    }
                }
                else
                {
                    // Create and init to -1, since the initial log entry will increment it making the first entry 0.
                    var indexAccessor = _persistentStorage.CreateBlock(PersistenceLevel.Critical, blockIndexName, 1);
                    indexAccessor[Index] = -1;
                    _indexAccessor.Add(type, indexAccessor);
                    _eventTypeIndex.Add(type, (int)indexAccessor[Index]);
                }

                var blockName = _blockName + "_" + type;
                if (_persistentStorage.BlockExists(blockName))
                {
                    if (_dataAccessor.ContainsKey(type))
                    {
                        continue;
                    }

                    _dataAccessor.Add(type, _persistentStorage.GetBlock(blockName));
                }
                else
                {
                    _dataAccessor.Add(type, _persistentStorage.CreateBlock(PersistenceLevel.Critical, blockName, e.Value.Max));
                }
            }
        }

        private void GetTiltLoggerConfiguration()
        {
            var config = ConfigurationUtilities.GetConfiguration(_tiltLoggerConfigurationPath,
                () => new TiltLoggerConfiguration
                {
                    ArrayOfEventTypes = Array.Empty<EventType>(),
                    ArrayOfEventDescription = Array.Empty<EventDescription>()
                });

            var eventTypes = config.ArrayOfEventTypes.ToList();

            // Add all configured event types that do not contain a combined element first.
            foreach (var e in eventTypes.Where(e => string.IsNullOrEmpty(e.Combined)))
            {
                _eventTypes.Add(e.Type, ValueTuple.Create(e.Max, string.Empty, e.AppendSecurityLevel));
            }

            // Add all configured event types that contain a combined element. 
            foreach (var e in eventTypes)
            {
                if (string.IsNullOrEmpty(e.Combined))
                {
                    continue;
                }

                var max = e.Max;
                var combined = string.Empty;
                var combinations = e.Combined.Split('|');
                foreach (var c in combinations)
                {
                    if (!_eventTypes.TryGetValue(c, out var eventTypesValue))
                    {
                        continue;
                    }

                    if (eventTypesValue.Max <= max)
                    {
                        continue;
                    }

                    max = eventTypesValue.Max;
                    combined = c;
                }

                _eventTypes.Add(e.Type, ValueTuple.Create(max, combined, e.AppendSecurityLevel));
            }

            _eventsToSubscribe.Clear();
            _eventsSubscribed.Clear();
            _reloadEventHistory.Clear();
            _eventTypeRolled.Clear();

            foreach (var e in _eventTypes)
            {
                _eventsToSubscribe.Add(e.Key, 0);
                _eventsSubscribed.Add(e.Key, 0);
                _reloadEventHistory.Add(e.Key, ValueTuple.Create(false, false));
                _eventTypeRolled.Add(e.Key, false);
            }

            _eventDescriptionsToSubscribe = config.ArrayOfEventDescription.ToList();
            foreach (var e in _eventDescriptionsToSubscribe)
            {
                _eventsToSubscribe[e.Type]++;
            }
        }

        private void SubscribeToEvents()
        {
            foreach (var eventBeingSubscribed in _eventDescriptionsToSubscribe)
            {
                var theType = Type.GetType(eventBeingSubscribed.Name);
                if (theType != null)
                {
                    _eventBus.Subscribe(this, theType, ReceiveEvent);
                    Logger.Debug($"Subscribed to {eventBeingSubscribed.Name} {eventBeingSubscribed.Type}");
                    _eventsSubscribed[eventBeingSubscribed.Type]++;
                }
                else
                {
                    Logger.Error($"{eventBeingSubscribed.Name} not found");
                }
            }
        }

        private EventDescription FromDelimitedString(string eventString)
        {
            var eventStringParts = eventString.Split(new[] { EventStringDelimiter }, StringSplitOptions.None);
            string eventType;
            long transactionId;
            DateTime transactionDateTimeUtc;
            DateTime transactionDateTime;

            // Is this an old event string format (IE. older game event with no initial length to parse)?
            if (eventStringParts.Length < DefaultEventStringCount && eventStringParts.Length >= 3)
            {
                Logger.Warn($"Parsing old event string format {eventString} with {eventStringParts.Length} parts");
                eventType = "General";
                transactionId = 0;
                transactionDateTimeUtc = DateTime.Parse(eventStringParts[eventStringParts.Length - 1], CultureInfo.CurrentCulture);
                transactionDateTime = TimeZoneInfo.ConvertTimeFromUtc(transactionDateTimeUtc, _timeService.TimeZoneInformation);
                return new EventDescription(eventStringParts[0], eventStringParts[1], eventType, transactionId, Guid.NewGuid(), transactionDateTime);
            }

            var eventStringCount = int.Parse(eventStringParts[0]);
            if (eventStringParts.Length != eventStringCount)
            {
                throw new ArgumentException($@"Could not parse {eventString} with {eventStringParts.Length} parts into {eventStringCount} parts");
            }

            eventType = eventStringParts[3];
            transactionId = long.Parse(eventStringParts[4], CultureInfo.InvariantCulture);
            transactionDateTimeUtc = DateTime.Parse(eventStringParts[5], CultureInfo.CurrentCulture);
            transactionDateTime = TimeZoneInfo.ConvertTimeFromUtc(transactionDateTimeUtc, _timeService.TimeZoneInformation);

            return new EventDescription(eventStringParts[1], eventStringParts[2], eventType, transactionId, Guid.NewGuid(), transactionDateTime, GetAdditionalInfo(eventStringCount, eventStringParts));
        }

        private (string, string)[] GetAdditionalInfo(int eventStringCount, IReadOnlyList<string> eventStringParts)
        {
            var additionalInfo = default(List<(string, string)>);
            var additionalInfoCount = eventStringCount - DefaultEventStringCount;
            if (additionalInfoCount > 0)
            {
                var additionalInfoNameKeys = new List<string>
                {
                    ResourceKeys.DateAndTimeHeader,
                    ResourceKeys.ComponentId,
                    ResourceKeys.SignatureType,
                    ResourceKeys.Seed,
                    ResourceKeys.Result
                };

                additionalInfo = new List<(string, string)>();
                for (var i = 0; i < eventStringCount - DefaultEventStringCount; i++)
                {
                    additionalInfo.Add((additionalInfoNameKeys[i], eventStringParts[DefaultEventStringCount + i]));
                }
            }

            return additionalInfo?.ToArray();
        }

        private (string, string)[] GetEventData((IEvent Event, DateTime DateTime, string Role) unformattedEvent)
        {
            var @event = unformattedEvent.Event;
            var additionalInfo = new List<(string, string)>();

            var componentHashCompleteEvent = @event as ComponentHashCompleteEvent;
            if (componentHashCompleteEvent?.ComponentVerification != null)
            {
                var formattedSeed = componentHashCompleteEvent.ComponentVerification.Seed.FormatBytes();
                additionalInfo.Add(GetDateAndTimeHeader(ResourceKeys.DateAndTimeHeader, unformattedEvent.DateTime));
                additionalInfo.Add((ResourceKeys.ComponentId, componentHashCompleteEvent.ComponentVerification.ComponentId));
                additionalInfo.Add((ResourceKeys.SignatureType, componentHashCompleteEvent.ComponentVerification.AlgorithmType.ToString().ToUpper()));
                additionalInfo.Add((ResourceKeys.Seed, string.IsNullOrEmpty(formattedSeed) ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable) : formattedSeed));
                additionalInfo.Add((ResourceKeys.Result, componentHashCompleteEvent.ComponentVerification.Result.FormatBytes()));
            }

            return additionalInfo.ToArray();
        }

        private void LogToPersistence()
        {
            while (!_queuedEvents.IsEmpty)
            {
                using (var scopedTransaction = _persistentStorage.ScopedTransaction())
                {
                    while (_queuedEvents.TryDequeue(out var localEvent))
                    {
                        var description = FormatEvent(localEvent);
                        if (description == null)
                        {
                            continue;
                        }

                        TiltLogAppendedTilt?.Invoke(this, new TiltLogAppendedEventArgs(description, localEvent.Event.GetType()));

                        var key = description.Type;
                        if (!_eventTypes.TryGetValue(key, out var eventTypesValue))
                        {
                            Logger.Error($"LogToPersistence _eventTypes key {key} NOT FOUND");
                            continue;
                        }

                        if (eventTypesValue.AppendSecurityLevel)
                        {
                            description.Name = $"{description.Name} {localEvent.Role}";
                        }

                        if (!string.IsNullOrEmpty(eventTypesValue.Combined))
                        {
                            if (!_eventTypes.ContainsKey(key))
                            {
                                Logger.Error($"LogToPersistence _eventTypes key {eventTypesValue.Combined} NOT FOUND FOR KEY {key}");
                                continue;
                            }

                            key = eventTypesValue.Combined;
                        }

                        if (!_indexAccessor.TryGetValue(key, out var indexAccessor))
                        {
                            Logger.Error($"LogToPersistence _indexAccessor key {key} NOT FOUND");
                            continue;
                        }

                        if (!_eventTypeIndex.TryGetValue(key, out var eventTypeIndex))
                        {
                            Logger.Error($"LogToPersistence _eventTypeIndex key {key} NOT FOUND");
                            continue;
                        }

                        var nextIndex = eventTypeIndex + 1;
                        var index = nextIndex % eventTypesValue.Max;
                        Debug.Assert(index >= 0 && index < eventTypesValue.Max, "_lastEventOutOfRange");

                        if (!_dataAccessor.TryGetValue(key, out var dataAccessor))
                        {
                            Logger.Error($"LogToPersistence _dataAccessor key {key} NOT FOUND");
                            continue;
                        }

                        if (_eventTypeRolled.TryGetValue(key, out var eventTypeRolled))
                        {
                            if (nextIndex >= eventTypesValue.Max)
                            {
                                _eventTypeRolled[key] = true;
                                eventTypeRolled = true;
                            }
                        }

                        if (_operatorMenuLauncher.IsShowing && eventTypeRolled)
                        {
                            var eventString = (string)dataAccessor[nextIndex - 1, Events];
                            if (!string.IsNullOrEmpty(eventString))
                            {
                                var eventDescription = FromDelimitedString(eventString);
                                if (_reloadEventHistory.TryGetValue(eventDescription.Type, out _))
                                {
                                    _reloadEventHistory[eventDescription.Type] = ValueTuple.Create(true, true);
                                    _reloadAllView = true;
                                }
                            }
                        }

                        _eventTypeIndex[key] = index;

                        using (var transaction = dataAccessor.StartTransaction())
                        {
                            var formattedDateTime = _timeService.FormatDateTimeString(description.Timestamp);

                            string eventString;
                            if (description.AdditionalInfos != null && description.AdditionalInfos.Any())
                            {
                                var eventStringCount = DefaultEventStringCount + description.AdditionalInfos.Length;
                                eventString = string.Join(
                                    EventStringDelimiter,
                                    eventStringCount,
                                    description.Name,
                                    description.Level,
                                    description.Type,
                                    description.TransactionId,
                                    formattedDateTime,
                                    description.GetAdditionalInfoString());
                            }
                            else
                            {
                                eventString = string.Join(
                                    EventStringDelimiter,
                                    DefaultEventStringCount,
                                    description.Name,
                                    description.Level,
                                    description.Type,
                                    description.TransactionId,
                                    formattedDateTime);
                            }

                            transaction.AddBlock(indexAccessor);
                            var blockIndexName = _blockIndexName + "_" + key;
                            transaction[blockIndexName, Index] = index;
                            var blockName = _blockName + "_" + key;
                            transaction[blockName, index, Events] = eventString;

                            var blockSize = dataAccessor.Format.GetFieldDescription(Events).Size;
                            var dataSize = eventString.Length;
                            Debug.Assert(dataSize <= blockSize, $"TiltLogger log data exceeded - {blockSize} blockSize");

                            if (_operatorMenuLauncher.IsShowing && _reloadEventHistory.TryGetValue(description.Type, out _))
                            {
                                _reloadEventHistory[description.Type] = ValueTuple.Create(true, nextIndex >= eventTypesValue.Max);
                                _reloadAllView = true;
                            }

                            Logger.Debug($"LogToPersistence - ADDING {key} INDEX {index} - {eventString.ToString(CultureInfo.InvariantCulture)}");
                        }
                    }

                    scopedTransaction.Complete();
                }
            }
        }

        private string GetCriticalMemoryIntegrityCheckText()
        {
            var eventName = IntegrityCheckEvent;

            if (_persistentStorage.VerifyIntegrity(true))
            {
                eventName += Passed;
            }
            else
            {
                eventName += Failed;
            }

            Logger.Info(eventName);
            return eventName;
        }

        private void QueueEvent(IEvent receivedEvent)
        {
            // We queue any start-up events first if they have not already been queued.  This is done
            // once we handle an initial subscribed event so that the idProvider is available to set the
            // next transaction Id for these start-up events (for accurate sorting).
            if (!_startupEventsQueued)
            {
                _queuedEvents.Enqueue(ValueTuple.Create((IEvent)null, DateTime.UtcNow, string.Empty)); // Software verified startup event
                _queuedEvents.Enqueue(ValueTuple.Create((IEvent)null, DateTime.UtcNow, string.Empty)); // NVRAM integrity
                _startupEventsQueued = true;
            }

            _queuedEvents.Enqueue(ValueTuple.Create(receivedEvent, DateTime.UtcNow, GetCurrentOperatorMenuRole()));
        }

        private void ProcessEvents()
        {
            if (!_queuedEvents.IsEmpty)
            {
                LogToPersistence();
            }
        }

        private void EventUpdateTimerElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _eventUpdateTimer?.Stop();
            ProcessEvents();
            _eventUpdateTimer?.Start();
        }

        private EventDescription GetStartupEventDescription(DateTime dateTime)
        {
            if (!_logBootVerification && !_logCriticalMemoryIntegrityCheck)
            {
                return null;
            }

            if (_idProvider is null)
            {
                _idProvider = ServiceManager.GetInstance().TryGetService<IIdProvider>();
            }

            string name;

            if (_logBootVerification)
            {
                name = SoftwareVerified;
                _logBootVerification = false;
            }
            else
            {
                name = GetCriticalMemoryIntegrityCheckText();
                _logCriticalMemoryIntegrityCheck = false;
            }

            return new EventDescription
            {
                Name = name,
                Level = Info,
                Type = "General",
                Guid = Guid.NewGuid(),
                TransactionId = _idProvider?.GetNextTransactionId() ?? 0,
                Timestamp = dateTime
            };
        }

        private EventDescription FormatEvent((IEvent Event, DateTime DateTime, string Role) unformattedEvent)
        {
            if (unformattedEvent.Event == null)
            {
                return GetStartupEventDescription(unformattedEvent.DateTime);
            }

            string targetName;

            if (unformattedEvent.Event is DoorBaseEvent doorEvent)
            {
                var doorMonitor = ServiceManager.GetInstance().TryGetService<IDoorMonitor>();
                var localizedName = doorMonitor.GetLocalizedDoorName(doorEvent.LogicalId, true);
                targetName = doorEvent.ToLocalizedString(localizedName);
            }
            else
            {
                targetName = unformattedEvent.Event.ToString();
            }


            if (string.IsNullOrEmpty(targetName))
            {
                return null;
            }

            if (_idProvider == null)
            {
                _idProvider = ServiceManager.GetInstance().TryGetService<IIdProvider>();
            }

            var subscribedEvent =
                (from eve in _eventDescriptionsToSubscribe
                 where eve.Name.Split(',')[0] == unformattedEvent.Event.GetType().ToString()
                 select eve).FirstOrDefault();

            var receivedEvent = new EventDescription(
                targetName,
                subscribedEvent?.Level,
                subscribedEvent?.Type,
                _idProvider?.GetNextTransactionId() ?? 0,
                unformattedEvent.Event.GloballyUniqueId,
                unformattedEvent.DateTime,
                GetEventData(unformattedEvent));
            return receivedEvent;
        }

        private void ReceiveEvent(IEvent data)
        {
            if (data == null)
            {
                Logger.Error("Received a null event.");
                return;
            }

            QueueEvent(data);
        }

        private string GetCurrentOperatorMenuRole()
        {
            // Use default operator culture for this as the value will get persisted and we don't want
            // to be able to affect what is persisted based on what the culture happens to be at the time.

            if (!_operatorMenuLauncher.IsShowing)
            {
                return string.Empty;
            }

            var provider = Localizer.For(CultureFor.Operator) as OperatorCultureProvider;
            var role = _properties.GetValue(ApplicationConstants.RolePropertyKey, string.Empty);

            return role == ApplicationConstants.DefaultRole || string.IsNullOrEmpty(role)
                ? provider?.GetString(provider.DefaultCulture, ResourceKeys.MenuTitleRoleAdmin) ?? string.Empty
                : provider?.GetString(provider.DefaultCulture, ResourceKeys.MenuTitleRoleTechnician) ?? string.Empty;
        }
    }
}