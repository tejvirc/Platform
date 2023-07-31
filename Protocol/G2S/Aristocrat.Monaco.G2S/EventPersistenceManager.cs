namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.EventHandler;
    using Data.Model;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Handles event data persistence.
    /// </summary>
    public class EventPersistenceManager : IEventPersistenceManager
    {
        private readonly object _addUpdateLock = new object();
        private readonly IMonacoContextFactory _contextFactory;

        private readonly IEventHandlerLogRepository _eventHandlerLogRepository;
        private readonly IEventSubscriptionRepository _eventSubscriptionRepository;
        private readonly IIdProvider _idProvider;
        private readonly ISupportedEventRepository _supportedEventRepository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EventPersistenceManager" /> class.
        /// </summary>
        /// <param name="contextFactory">An <see cref="IMonacoContextFactory" /> instance.</param>
        /// <param name="eventHandlerLogRepository">Event Handler repository</param>
        /// <param name="eventSubscriptionRepository">Event subscription repository</param>
        /// <param name="supportedEventRepository">Supported event repository</param>
        /// <param name="idProvider">Id Provider</param>
        public EventPersistenceManager(
            IMonacoContextFactory contextFactory,
            IEventHandlerLogRepository eventHandlerLogRepository,
            IEventSubscriptionRepository eventSubscriptionRepository,
            ISupportedEventRepository supportedEventRepository,
            IIdProvider idProvider)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _eventHandlerLogRepository = eventHandlerLogRepository ??
                                         throw new ArgumentNullException(nameof(eventHandlerLogRepository));
            _eventSubscriptionRepository = eventSubscriptionRepository ??
                                           throw new ArgumentNullException(nameof(eventSubscriptionRepository));
            _supportedEventRepository =
                supportedEventRepository ?? throw new ArgumentNullException(nameof(supportedEventRepository));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
        }

        /// <inheritdoc />
        public IReadOnlyCollection<object> SupportedEvents
        {
            get
            {
                using (var context = _contextFactory.CreateDbContext())
                { 
                    return _supportedEventRepository.GetAll(context).ToList();
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<eventHostSubscription> GetRegisteredEvents(int hostId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var result =
                    _eventSubscriptionRepository.GetAll(context).Where(
                        e => e.HostId == hostId && e.SubType == EventSubscriptionType.Host).ToList();

                return result.Select(GetHostSubscription);
            }
        }

        /// <inheritdoc />
        public void RegisteredEvents(IEnumerable<eventHostSubscription> subscriptions, int hostId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var currentSubscriptions = _eventSubscriptionRepository.GetAll(context)
                    .Where(e => e.HostId == hostId && e.SubType == EventSubscriptionType.Host).ToList();

                foreach (var subscription in subscriptions)
                {
                    var current = currentSubscriptions.FirstOrDefault(
                        s => s.EventCode == subscription.eventCode && s.DeviceId == subscription.deviceId);
                    if (current != null)
                    {
                        current.EventPersist = subscription.eventPersist;
                        current.SendTransaction = subscription.sendTransaction;
                        current.SendDeviceMeters = subscription.sendDeviceMeters;
                        current.SendClassMeters = subscription.sendClassMeters;
                        current.SendUpdatableMeters = subscription.sendUpdatableMeters;
                        current.SendDeviceStatus = subscription.sendDeviceStatus;
                    }
                    else
                    {
                        var sub = new EventSubscription
                        {
                            HostId = hostId,
                            DeviceId = subscription.deviceId,
                            EventCode = subscription.eventCode,
                            SubType = EventSubscriptionType.Host,
                            DeviceClass = subscription.deviceClass,
                            EventPersist = subscription.eventPersist,
                            SendTransaction = subscription.sendTransaction,
                            SendDeviceMeters = subscription.sendDeviceMeters,
                            SendClassMeters = subscription.sendClassMeters,
                            SendUpdatableMeters = subscription.sendUpdatableMeters,
                            SendDeviceStatus = subscription.sendDeviceStatus
                        };

                        context.Set<EventSubscription>().Add(sub);
                    }
                }

                context.SaveChanges();
            }
        }

        /// <inheritdoc />
        public void RemoveRegisteredEventSubscriptions(IEnumerable<eventHostSubscription> subscriptions, int hostId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var currentSubscriptions = _eventSubscriptionRepository.GetAll(context)
                    .Where(e => e.HostId == hostId && e.SubType == EventSubscriptionType.Host).ToList();

                var mapped = currentSubscriptions.Where(
                    c => subscriptions.Any(s => c.DeviceId == s.deviceId && c.EventCode == s.eventCode));

                _eventSubscriptionRepository.DeleteAll(context, mapped);
            }
        }

        /// <inheritdoc />
        public forcedSubscription GetForcedEvent(string eventCode, int hostId, int deviceId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                forcedSubscription result = null;
                var sub = _eventSubscriptionRepository.GetAll(context).FirstOrDefault(
                    a => a.EventCode == eventCode && a.DeviceId == deviceId && a.HostId == hostId &&
                         (a.SubType == EventSubscriptionType.Forced || a.SubType == EventSubscriptionType.Permanent));
                if (sub != null)
                {
                    result = GetForcedSubscription(sub);
                }

                return result;
            }
        }

        /// <inheritdoc />
        public void AddForcedEvent(string eventCode, forcedSubscription subscription, int hostId, int deviceId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var sub = _eventSubscriptionRepository.Get(
                    context,
                    eventCode,
                    hostId,
                    deviceId,
                    EventSubscriptionType.Forced);
                if (sub == null)
                {
                    sub = new EventSubscription
                    {
                        HostId = hostId,
                        DeviceId = deviceId,
                        EventCode = eventCode,
                        SubType = EventSubscriptionType.Forced,
                        DeviceClass = subscription.deviceClass,
                        EventPersist = subscription.forcePersist,
                        SendTransaction = subscription.forceTransaction,
                        SendDeviceMeters = subscription.forceDeviceMeters,
                        SendClassMeters = subscription.forceClassMeters,
                        SendUpdatableMeters = subscription.forceUpdatableMeters,
                        SendDeviceStatus = subscription.forceDeviceStatus
                    };

                    _eventSubscriptionRepository.Add(context, sub);
                }
                else
                {

                    sub.EventPersist = subscription.forcePersist;
                    sub.SendTransaction = subscription.forceTransaction;
                    sub.SendDeviceMeters = subscription.forceDeviceMeters;
                    sub.SendClassMeters = subscription.forceClassMeters;
                    sub.SendUpdatableMeters = subscription.forceUpdatableMeters;
                    sub.SendDeviceStatus = subscription.forceDeviceStatus;
                    _eventSubscriptionRepository.Update(context, sub);
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<object> GetAllEventSubscriptions(int hostId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var result = _eventSubscriptionRepository.GetAll(context).Where(e => e.HostId == hostId).ToList();

                return result;
            }
        }

        /// <inheritdoc />
        public IEnumerable<eventHostSubscription> GetAllRegisteredEventSub(int hostId)
        {
            return AllRegisteredEventSubs(hostId).Select(
                    sub => new eventHostSubscription
                    {
                        deviceClass = sub.DeviceClass,
                        deviceId = sub.DeviceId,
                        eventCode = sub.EventCode,
                        sendClassMeters = sub.SendClassMeters,
                        sendDeviceMeters = sub.SendDeviceMeters,
                        sendDeviceStatus = sub.SendDeviceStatus,
                        eventPersist = sub.EventPersist,
                        sendTransaction = sub.SendTransaction,
                        sendUpdatableMeters = sub.SendUpdatableMeters
                    })
                .ToList();
        }

        /// <inheritdoc />
        public IEnumerable<forcedSubscription> GetAllForcedEventSub(int hostId)
        {
            return AllForcedEventSubs(hostId).Select(
                    sub => new forcedSubscription
                    {
                        deviceClass = sub.DeviceClass,
                        deviceId = sub.DeviceId,
                        eventCode = sub.EventCode,
                        forceClassMeters = sub.SendClassMeters,
                        forceDeviceMeters = sub.SendDeviceMeters,
                        forceDeviceStatus = sub.SendDeviceStatus,
                        forcePersist = sub.EventPersist,
                        forceTransaction = sub.SendTransaction,
                        forceUpdatableMeters = sub.SendUpdatableMeters
                    })
                .ToList();
        }

        /// <inheritdoc />
        public long GetEventId()
        {
            return _idProvider.GetNextLogSequence<EventHandlerLog>();
        }

        /// <inheritdoc />
        public void AddSupportedEvents(string deviceClass, int deviceId, string eventCode)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var se = _supportedEventRepository.Get(context, eventCode, deviceId);

                if (se == null)
                {
                    var see = new SupportedEvent
                    {
                        DeviceClass = deviceClass,
                        DeviceId = deviceId,
                        EventCode = eventCode
                    };

                    _supportedEventRepository.Add(context, see);
                }
            }
        }

        /// <inheritdoc />
        public void RemoveSupportedEvents(int deviceId, string eventCode)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                _supportedEventRepository.Delete(context, eventCode, deviceId);
            }
        }

        /// <inheritdoc />
        public void AddEventLog(eventReport log, int hostId, int maxEntries)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                lock (_addUpdateLock)
                {
                    _eventHandlerLogRepository.Add(context, ToEventHandlerLog(log, new EventHandlerLog { HostId = hostId }));

                    var logCount = _eventHandlerLogRepository.Count(context, l => l.HostId == hostId);
                    if (logCount > maxEntries)
                    {
                        var count = logCount - maxEntries;

                        _eventHandlerLogRepository.DeleteOldest(context, hostId, count);
                    }
                }
            }
        }

        /// <inheritdoc />
        public void UpdateEventLog(eventReport report, int hostId, bool hostAck)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                lock (_addUpdateLock)
                {
                    var result = _eventHandlerLogRepository.Get(context, report.eventId, hostId);
                    if (result == null)
                    {
                        return;
                    }

                    var log = ToEventHandlerLog(report, result);
                    log.EventAck = hostAck;

                    _eventHandlerLogRepository.Update(context, log);
                }
            }
        }

        /// <inheritdoc />
        public IReadOnlyCollection<eventReport> GetUnsentEvents(int hostId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return _eventHandlerLogRepository.Get(context, e => e.HostId == hostId && !e.EventAck)
                    .ToList().Select(ToEventReport).ToList();
            }
        }

        /// <inheritdoc />
        public void AddDefaultEvents(int hostId)
        {
            AddPermanentEvent(
                EventCode.G2S_CBE321,
                "G2S_cabinet",
                true,
                hostId,
                1);
            AddPermanentEvent(
                EventCode.G2S_CBE322,
                "G2S_cabinet",
                true,
                hostId,
                1);
            AddPermanentEvent(
                EventCode.G2S_CBE324,
                "G2S_cabinet",
                true,
                hostId,
                1);
        }

        /// <inheritdoc />
        public void SetEventReportConfigs(int deviceId, Dictionary<string, HashSet<EventReportConfig>> eventConfigs)
        {
            var forcedEvents = AllForcedEventSubs(deviceId).ToList();
            var registered = AllRegisteredEventSubs(deviceId).ToList();

            using (var context = _contextFactory.CreateDbContext())
            {
                foreach (var supportedEvent in _supportedEventRepository.GetAll(context).ToList())
                {
                    var fs = forcedEvents.FirstOrDefault(
                        e => e.DeviceId == supportedEvent.DeviceId && e.EventCode == supportedEvent.EventCode);

                    var hs = registered.FirstOrDefault(
                        e => e.DeviceId == supportedEvent.DeviceId && e.EventCode == supportedEvent.EventCode);

                    if (fs != null || hs != null)
                    {
                        if (!eventConfigs.ContainsKey(supportedEvent.EventCode))
                        {
                            eventConfigs[supportedEvent.EventCode] = new HashSet<EventReportConfig>();
                        }

                        eventConfigs[supportedEvent.EventCode].Add(GetReportConfig(fs, hs, supportedEvent));
                    }
                }
            }
        }

        private static EventHandlerLog ToEventHandlerLog(c_eventReport report, EventHandlerLog data)
        {
            data.DeviceClass = report.deviceClass;
            data.DeviceId = report.deviceId;
            data.EventCode = report.eventCode;
            data.EventDateTime = report.eventDateTime;
            data.EventId = report.eventId;
            data.TransactionId = report.transactionId;
            data.DeviceList = report.deviceList != null ? EventHandlerExtensions.ToXml(report.deviceList) : null;
            data.MeterList = report.meterList != null ? EventHandlerExtensions.ToXml(report.meterList) : null;
            data.TransactionList = report.transactionList != null ? EventHandlerExtensions.ToXml(report.transactionList) : null;

            return data;
        }

        private static eventReport ToEventReport(EventHandlerLog data)
        {
            return new eventReport
            {
                deviceClass = data.DeviceClass,
                transactionId = data.TransactionId,
                eventCode = data.EventCode,
                deviceId = data.DeviceId,
                eventDateTime = data.EventDateTime,
                eventId = data.EventId,
                eventText = EventHandlerExtensions.GetEventText(data.EventCode),
                transactionList = !string.IsNullOrEmpty(data.TransactionList) ? EventHandlerExtensions.ParseXml<transactionList>(data.TransactionList) : null,
                deviceList = !string.IsNullOrEmpty(data.DeviceList) ? EventHandlerExtensions.ParseXml<deviceList1>(data.DeviceList) : null,
                meterList = !string.IsNullOrEmpty(data.MeterList) ? EventHandlerExtensions.ParseXml<meterList>(data.MeterList) : null,
            };
        }

        private void AddPermanentEvent(string eventCode, string deviceClass, bool forcePersist, int hostId, int deviceId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var sub = _eventSubscriptionRepository.Get(
                    context,
                    eventCode,
                    hostId,
                    deviceId,
                    EventSubscriptionType.Permanent);
                if (sub == null)
                {
                    sub = new EventSubscription
                    {
                        HostId = hostId,
                        DeviceId = deviceId,
                        EventCode = eventCode,
                        SubType = EventSubscriptionType.Permanent,
                        DeviceClass = deviceClass,
                        EventPersist = forcePersist
                    };

                    _eventSubscriptionRepository.Add(context, sub);
                }
            }
        }

        private IEnumerable<EventSubscription> AllRegisteredEventSubs(int hostId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return _eventSubscriptionRepository.GetAll(context)
                    .Where(e => e.HostId == hostId && e.SubType == EventSubscriptionType.Host).ToList();
            }
        }


        private IEnumerable<EventSubscription> AllForcedEventSubs(int hostId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return _eventSubscriptionRepository.GetAll(context).Where(
                        e => e.HostId == hostId &&
                             (e.SubType == EventSubscriptionType.Forced ||
                              e.SubType == EventSubscriptionType.Permanent))
                    .ToList();
            }
        }

        private eventHostSubscription GetHostSubscription(EventSubscription sub)
        {
            if (sub == null)
            {
                return null;
            }

            return new eventHostSubscription
            {
                eventCode = sub.EventCode,
                deviceClass = sub.DeviceClass,
                deviceId = sub.DeviceId,
                eventPersist = sub.EventPersist,
                sendClassMeters = sub.SendClassMeters,
                sendDeviceMeters = sub.SendDeviceMeters,
                sendDeviceStatus = sub.SendDeviceStatus,
                sendTransaction = sub.SendTransaction,
                sendUpdatableMeters = sub.SendUpdatableMeters
            };
        }

        private forcedSubscription GetForcedSubscription(EventSubscription sub)
        {
            if (sub == null)
            {
                return null;
            }

            return new forcedSubscription
            {
                eventCode = sub.EventCode,
                deviceClass = sub.DeviceClass,
                deviceId = sub.DeviceId,
                forcePersist = sub.EventPersist,
                forceClassMeters = sub.SendClassMeters,
                forceDeviceMeters = sub.SendDeviceMeters,
                forceDeviceStatus = sub.SendDeviceStatus,
                forceTransaction = sub.SendTransaction,
                forceUpdatableMeters = sub.SendUpdatableMeters
            };
        }

        private static EventReportConfig GetReportConfig(EventSubscription forced, EventSubscription host, SupportedEvent se)
        {
            var useForced = forced != null;
            var useHost = host != null;

            return new EventReportConfig(
                useForced && forced.SendDeviceStatus || useHost && host.SendDeviceStatus,
                useForced && forced.SendClassMeters || useHost && host.SendClassMeters,
                useForced && forced.SendDeviceMeters || useHost && host.SendDeviceMeters,
                useForced && forced.SendTransaction || useHost && host.SendTransaction,
                useForced && forced.SendUpdatableMeters || useHost && host.SendUpdatableMeters,
                useForced && forced.EventPersist || useHost && host.EventPersist,
                se.DeviceClass,
                se.DeviceId,
                useForced && forced.EventPersist);
        }
    }
}
