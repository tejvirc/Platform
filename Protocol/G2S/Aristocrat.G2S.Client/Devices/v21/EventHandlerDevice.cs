namespace Aristocrat.G2S.Client.Devices.v21
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Diagnostics;
    using Newtonsoft.Json;
    using Protocol.v21;

    /// <summary>
    ///     The eventHandler class manages the event subscriptions for an EGM, providing the means to determine the
    ///     events supported by the EGM, to install and remove and inquire about event subscriptions, and to manage the
    ///     collection and delivery of events and associated affected data.
    /// </summary>
    /// <remarks>
    ///     The eventHandler class is a multiple device
    ///     class. Each registered host may own a single eventHandler device.Each eventHandler device has its own
    ///     event log.There is no class-level log for the eventHandler class
    ///     <para />
    ///     The device identifiers for devices within the eventHandler class MUST be equal to the host identifier of the
    ///     host that owns the device.When a host is registered, the EGM MUST create and remove eventHandler
    ///     devices, as required.When a host is unregistered, the EGM MUST remove any eventHandler devices owned
    ///     by the host.The EGM MUST NOT own eventHandler devices
    ///     <para />
    ///     An event represents an occurrence of an incident detected by a device in an EGM.The device can generate an
    ///     event in an unsolicited manner or in response to a host command.Some events may also have associated
    ///     commands that are generated for the owner of the device.
    ///     <para />
    ///     Events are always reported after the action has taken place; events are after-the-fact. The EGM MUST NOT
    ///     generate an event until after all EGM data(including device, meter, and log states) have been updated to reflect
    ///     the occurrence of the event. For each event, the EGM MUST NOT report the event and MUST NOT store
    ///     the event in an eventHandler log until all EGM data have been updated to reflect the occurrence of the event
    ///     and all events with lesser eventIds.
    /// </remarks>
    public class EventHandlerDevice : HostOrientedDevice<eventHandler>, IEventHandlerDevice
    {
        /// <summary>
        ///     The default disable behavior.
        /// </summary>
        public const t_disableBehaviors DefaultDisableBehavior = t_disableBehaviors.G2S_overwrite;

        private const bool DefaultRestartStatus = true;
        private const bool DefaultRequiredForPlay = false;

        // NOTE: This was set to 2500 due to the host incorrectly configuring the client
        private const int DefaultMinLogEntries = 2500;

        private static readonly List<EventHandlerDevice> Devices = new List<EventHandlerDevice>();

        private static readonly List<(string, int, string)> UnregisteredSupportedEvents =
            new List<(string, int, string)>();

        private static readonly object Lock = new object();
        private readonly Dictionary<string, HashSet<EventReportConfig>> _eventConfigs;
        private readonly object _eventLock = new object();
        private readonly IEventPersistenceManager _eventPersistenceManager;
        private readonly object _eventSubLock = new object();
        private readonly ConcurrentQueue<Action> _offlineEvents = new ConcurrentQueue<Action>();
        private readonly List<string> _overflowEvents;
        private readonly ConcurrentQueue<eventReport> _queuedEvents = new ConcurrentQueue<eventReport>();

        private bool _disposed;
        private volatile bool _open;

        private Thread _senderThread;
        private AutoResetEvent _timeoutExpired = new AutoResetEvent(false);

        /// <summary>
        ///     Initializes a new instance of the <see cref="EventHandlerDevice" /> class.
        /// </summary>
        /// <param name="deviceId">The device identifier, which for this class should be the host identifier.</param>
        /// <param name="deviceStateObserver">An <see cref="IDeviceObserver" /> instance.</param>
        /// <param name="eventPersistenceManager">The event persistence manager.</param>
        public EventHandlerDevice(
            int deviceId,
            IDeviceObserver deviceStateObserver,
            IEventPersistenceManager eventPersistenceManager)
            : base(deviceId, deviceStateObserver)
        {
            _eventPersistenceManager =
                eventPersistenceManager ?? throw new ArgumentNullException(nameof(eventPersistenceManager));

            lock (Lock)
            {
                Devices.Add(this);

                if (UnregisteredSupportedEvents.Count > 0)
                {
                    foreach (var supportedEvent in UnregisteredSupportedEvents)
                    {
                        _eventPersistenceManager.AddSupportedEvents(
                            supportedEvent.Item1,
                            supportedEvent.Item2,
                            supportedEvent.Item3);
                    }

                    UnregisteredSupportedEvents.Clear();
                }
            }

            SetDefaults();

            lock (_eventSubLock)
            {
                _eventConfigs = new Dictionary<string, HashSet<EventReportConfig>>();
            }

            _overflowEvents = new List<string>();
            _eventPersistenceManager.AddDefaultEvents(deviceId);
        }

        /// <inheritdoc />
        public bool RestartStatus { get; protected set; }

        /// <inheritdoc />
        public t_disableBehaviors DisableBehavior { get; protected set; }

        /// <inheritdoc />
        public t_queueBehaviors QueueBehavior { get; protected set; }

        /// <inheritdoc />
        public int TimeToLive { get; protected set; }

        /// <inheritdoc />
        public int MinLogEntries { get; protected set; }

        /// <inheritdoc />
        [JsonIgnore]
        public bool Overflow { get; set; }

        /// <inheritdoc />
        public override void Open(IStartupContext context)
        {
            SetEventReportConfigs();

            _open = true;

            lock (Lock)
            {
                while (_offlineEvents.TryDequeue(out var report))
                {
                    report.Invoke();
                }
            }

            DeviceStateChanged();

            LoadPersistedEvents();
        }

        /// <inheritdoc />
        public override void Close()
        {
            _open = false;

            _timeoutExpired.Set();

            if (_senderThread != null && _senderThread.IsAlive)
            {
                _senderThread.Join();
            }
        }

        /// <inheritdoc />
        public override void ApplyOptions(DeviceOptionConfigValues optionConfigValues)
        {
            base.ApplyOptions(optionConfigValues);

            SetDeviceValue(
                G2SParametersNames.RestartStatusParameterName,
                optionConfigValues,
                parameterId => { RestartStatus = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.UseDefaultConfigParameterName,
                optionConfigValues,
                parameterId => { UseDefaultConfig = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.RequiredForPlayParameterName,
                optionConfigValues,
                parameterId => { RequiredForPlay = optionConfigValues.BooleanValue(parameterId); });

            // See comment on DefaultMinLogEntries
            //SetDeviceValue(
            //    G2SParametersNames.EventHandlerDevice.MinLogEntriesParameterName,
            //    optionConfigValues,
            //    parameterId => { MinLogEntries = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.TimeToLiveParameterName,
                optionConfigValues,
                parameterId => { TimeToLive = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.EventHandlerDevice.QueueBehaviorParameterName,
                optionConfigValues,
                parameterId =>
                {
                    var queueBehavior = optionConfigValues.StringValue(parameterId);

                    switch (queueBehavior)
                    {
                        case "G2S_overwrite":
                            QueueBehavior = t_queueBehaviors.G2S_overwrite;
                            break;
                        case "G2S_disable":
                            QueueBehavior = t_queueBehaviors.G2S_disable;
                            break;
                        case "G2S_discard":
                            QueueBehavior = t_queueBehaviors.G2S_discard;
                            break;
                    }
                });

            SetDeviceValue(
                G2SParametersNames.EventHandlerDevice.DisableBehaviorParameterName,
                optionConfigValues,
                parameterId =>
                {
                    var disableBehavior = optionConfigValues.StringValue(parameterId);

                    switch (disableBehavior)
                    {
                        case "G2S_discard":
                            DisableBehavior = t_disableBehaviors.G2S_discard;
                            break;
                        case "G2S_overwrite":
                            DisableBehavior = t_disableBehaviors.G2S_overwrite;
                            break;
                    }
                });
        }

        /// <inheritdoc />
        public override void RegisterEvents()
        {
            var deviceClass = this.PrefixedDeviceClass();

            RegisterEvent(deviceClass, Id, EventCode.G2S_EHE001);
            RegisterEvent(deviceClass, Id, EventCode.G2S_EHE002);
            RegisterEvent(deviceClass, Id, EventCode.G2S_EHE003);
            RegisterEvent(deviceClass, Id, EventCode.G2S_EHE004);
            RegisterEvent(deviceClass, Id, EventCode.G2S_EHE005);
            //// TODO: G2S_EHE006 Configuration Changed by Operator impl
            RegisterEvent(deviceClass, Id, EventCode.G2S_EHE006);
            RegisterEvent(deviceClass, Id, EventCode.G2S_EHE101);
            RegisterEvent(deviceClass, Id, EventCode.G2S_EHE102);
            RegisterEvent(deviceClass, Id, EventCode.G2S_EHE103);
        }

        /// <inheritdoc />
        public IEnumerable<object> GetAllEventSubscriptions()
        {
            return _eventPersistenceManager.GetAllEventSubscriptions(Id);
        }

        /// <inheritdoc />
        public IEnumerable<eventHostSubscription> GetAllRegisteredEventSub()
        {
            return _eventPersistenceManager.GetAllRegisteredEventSub(Id);
        }

        /// <inheritdoc />
        public IEnumerable<forcedSubscription> GetAllForcedEventSub()
        {
            return _eventPersistenceManager.GetAllForcedEventSub(Id);
        }

        /// <inheritdoc />
        public void RemoveRegisteredEventSubscriptions(IEnumerable<eventHostSubscription> subscriptions)
        {
            _eventPersistenceManager.RemoveRegisteredEventSubscriptions(subscriptions, Id);
            SetEventReportConfigs();
        }

        /// <inheritdoc />
        public void RemoveRegisteredHostEventSubscriptions(string className, int deviceId)
        {
            RemoveRegisteredEventSubscriptions(
                GetAllRegisteredEventSub().Where(_ => _.deviceId == deviceId && _.deviceClass == className));
        }

        /// <inheritdoc />
        public bool SetEventSubscriptions(IEnumerable<eventHostSubscription> subscriptions)
        {
            var registeredEvents = new List<eventHostSubscription>();

            var currentSubscriptions = _eventPersistenceManager.GetRegisteredEvents(Id).ToList();
            foreach (var subscription in subscriptions)
            {
                var current = currentSubscriptions.FirstOrDefault(s => s.eventCode == subscription.eventCode);
                if (current != null)
                {
                    MergeSub(current, subscription);
                    registeredEvents.Add(current);
                }
                else
                {
                    registeredEvents.Add(subscription);
                }
            }

            _eventPersistenceManager.RegisteredEvents(registeredEvents, Id);

            SetEventReportConfigs();

            return registeredEvents.Count > 0;
        }

        /// <inheritdoc />
        public void DeviceStateChanged()
        {
            if (HostEnabled)
            {
                SendEventReports();
            }
        }

        /// <summary>
        ///     This command is initiated by the EGM and is the direct result of a subscription that was previously set. When
        ///     an event occurs on the EGM, the eventHandler device checks its subscriptions. If the event is to be sent to
        ///     the host, the eventHandler device includes all requested affected data that is available and generates the
        ///     command for the host.
        /// </summary>
        /// <param name="deviceClass">class that generates event</param>
        /// <param name="eventDeviceId">identifier of the device that generates event</param>
        /// <param name="eventCode">Event code of the sub event that caused the event to be generated</param>
        /// <param name="deviceList">Contains one or more statusInfo elements. This element has no attributes.</param>
        /// <param name="eventText">Text description of the event</param>
        /// <param name="meterList">Contains one or more meterInfo elements. This element has no attributes</param>
        /// <param name="transactionId">Transaction identifier of the initiating transaction; set to 0 (zero) otherwise</param>
        /// <param name="transactionList">Contains one or more transactionInfo elements. This element has no attributes</param>
        public static void EventReport(
            string deviceClass = "",
            int eventDeviceId = -1,
            string eventCode = "",
            deviceList1 deviceList = null,
            string eventText = "",
            meterList meterList = null,
            long transactionId = 0,
            transactionList transactionList = null)
        {
            EventHandlerDevice[] eventHandlerDevices;

            lock (Lock)
            {
                if (Devices.Count == 0)
                {
                    return;
                }

                eventHandlerDevices = new EventHandlerDevice[Devices.Count];
                Devices.CopyTo(eventHandlerDevices);
            }

            // Create the eventId for all instances of this event.  The EGM reserves the right to do this even if no one is subscribed to the event
            var eventId = eventHandlerDevices.First()._eventPersistenceManager.GetEventId();

            foreach (var device in eventHandlerDevices)
            {
                if (!device._open)
                {
                    lock (Lock)
                    {
                        device._offlineEvents.Enqueue(
                            () => device.InternalEventReport(
                                deviceClass,
                                eventDeviceId,
                                eventCode,
                                deviceList,
                                eventText,
                                meterList,
                                transactionId,
                                transactionList,
                                eventId));

                        while (device._offlineEvents.Count > DefaultMinLogEntries)
                        {
                            device._offlineEvents.TryDequeue(out var _);
                        }
                    }
                }
                else
                {
                    device.InternalEventReport(
                        deviceClass,
                        eventDeviceId,
                        eventCode,
                        deviceList,
                        eventText,
                        meterList,
                        transactionId,
                        transactionList,
                        eventId);
                }
            }
        }

        /// <summary>
        ///     Register supported events
        /// </summary>
        /// <param name="deviceClass">Device class type</param>
        /// <param name="deviceId">Device unique identifier</param>
        /// <param name="eventCode">Assigned G2S event code</param>
        public static void RegisterEvent(string deviceClass, int deviceId, string eventCode)
        {
            lock (Lock)
            {
                if (Devices.Count > 0)
                {
                    Devices[0]._eventPersistenceManager.AddSupportedEvents(deviceClass, deviceId, eventCode);
                }
                else
                {
                    UnregisteredSupportedEvents.Add((deviceClass, deviceId, eventCode));
                }
            }
        }

        /// <summary>
        ///     Unregister supported events
        /// </summary>
        /// <param name="deviceClass">Device class type</param>
        /// <param name="deviceId">Device unique identifier</param>
        /// <param name="eventCode">Assigned G2S event code</param>
        public static void UnregisterEvent(string deviceClass, int deviceId, string eventCode)
        {
            lock (Lock)
            {
                if (Devices.Count > 0)
                {
                    Devices[0]._eventPersistenceManager.RemoveSupportedEvents(deviceId, eventCode);
                }
            }
        }

        /// <inheritdoc />
        protected override void ConfigureDefaults()
        {
            base.ConfigureDefaults();

            SetDefaults();
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                lock (Lock)
                {
                    try
                    {
                        Devices.RemoveAll(a => a.Id == Id);
                    }
                    catch
                    {
                        // ignore
                    }
                }

                if (_timeoutExpired != null)
                {
                    _timeoutExpired.Set();
                    _timeoutExpired.Close();
                }
            }

            _timeoutExpired = null;

            _disposed = true;
        }

        private void SetEventReportConfigs()
        {
            lock (_eventSubLock)
            {
                _eventConfigs.Clear();

                _eventPersistenceManager.SetEventReportConfigs(Id, _eventConfigs);
            }
        }

        private void InternalEventReport(
            string deviceClass,
            int deviceId,
            string eventCode,
            deviceList1 deviceList,
            string eventText,
            meterList meterList,
            long transactionId,
            transactionList transactionList,
            long eventId)
        {
            EventReportConfig config;
            lock (_eventSubLock)
            {
                if (!_eventConfigs.TryGetValue(eventCode, out var subscription))
                {
                    return;
                }

                var configCheck = subscription.Cast<EventReportConfig?>().FirstOrDefault();
                if (configCheck == null)
                {
                    return;
                }

                config = configCheck.Value;
            }

            if (!HostEnabled && !config.ForcedPersist)
            {
                return;
            }

            if (string.IsNullOrEmpty(deviceClass))
            {
                deviceClass = this.PrefixedDeviceClass();
            }
            
            var report = new eventReport
            {
                deviceClass = string.IsNullOrEmpty(deviceClass) ? config.DeviceClass : deviceClass,
                deviceId = deviceId,
                eventCode = eventCode,
                eventDateTime = DateTime.UtcNow,
                eventId = eventId,
                eventText = string.IsNullOrEmpty(eventText) ? EventHandlerExtensions.GetEventText(eventCode) : eventText,
                transactionId = transactionId
            };

            if (config.SendTransaction)
            {
                report.transactionList = transactionList;
            }

            if (config.SendDeviceStatus)
            {
                report.deviceList = deviceList;
            }

            if ((config.SendClassMeters || config.SendDeviceMeters) && meterList != null)
            {
                // TODO: Need to handle sendUpdatableMeters, sendClassMeters, and sendDeviceMeters properly.  This is not currently doing that...
                if (config.SendClassMeters && config.SendDeviceMeters && !config.SendUpdatableMeters)
                {
                    report.meterList = meterList;
                }
                else
                {
                    var list = new List<meterInfo>();
                    foreach (var info in meterList.meterInfo)
                    {
                        if (info.deviceMeters != null && info.deviceMeters.Length > 0 && config.SendDeviceMeters ||
                            (info.deviceMeters == null || info.deviceMeters.Length == 0) && config.SendClassMeters)
                        {
                            list.Add(info);
                        }
                    }

                    report.meterList = new meterList { meterInfo = list.ToArray() };
                }
            }

            QueueForSend(report);
        }

        private void SetDefaults()
        {
            TimeToLive = (int)Constants.DefaultTimeout.TotalMilliseconds;
            RequiredForPlay = DefaultRequiredForPlay;
            RestartStatus = DefaultRestartStatus;

            MinLogEntries = DefaultMinLogEntries;
            DisableBehavior = DefaultDisableBehavior;
        }

        private void LoadPersistedEvents()
        {
            var list = new SortedList<long, eventReport>();
            var eventReports = _eventPersistenceManager.GetUnsentEvents(Id);
            if (eventReports.Count <= 0)
            {
                return;
            }

            foreach (var report in eventReports)
            {
                if (!list.ContainsKey(report.eventId))
                {
                    list[report.eventId] = report;
                }
            }

            foreach (var report in _queuedEvents)
            {
                if (!list.ContainsKey(report.eventId))
                {
                    list[report.eventId] = report;
                }
            }

            while (_queuedEvents.Count > 0)
            {
                _queuedEvents.TryDequeue(out var _);
            }

            foreach (var report in list)
            {
                _queuedEvents.Enqueue(report.Value);
            }

            SendEventReports();
        }

        private bool PersistedEvent(string eventCode, int deviceId)
        {
            lock (_eventSubLock)
            {
                if (!_eventConfigs.ContainsKey(eventCode))
                {
                    return false;
                }

                var config = _eventConfigs[eventCode].Cast<EventReportConfig?>().FirstOrDefault(a => a?.DeviceId == deviceId);

                return config != null && config.Value.EventPersist;
            }
        }

        private void QueueForSend(eventReport command)
        {
            lock (_eventLock)
            {
                if (_queuedEvents.Count >= MinLogEntries)
                {
                    SourceTrace.TraceInformation(
                        G2STrace.Source,
                        $@"EventHandlerDevice.QueueForSend : Queue overflow - current count {_queuedEvents.Count}");

                    switch (QueueBehavior)
                    {
                        case t_queueBehaviors.G2S_overwrite:
                            if (_queuedEvents.TryDequeue(out var evt))
                            {
                                SourceTrace.TraceInformation(
                                    G2STrace.Source,
                                    $@"EventHandlerDevice.QueueForSend : overwriting event {evt.eventCode}({evt.eventId}) for device {Id}");
                            }
                            break;
                        case t_queueBehaviors.G2S_disable:
                            if (Enabled)
                            {
                                Enabled = false;

                                _overflowEvents.Add(EventCode.G2S_EHE001);

                                DeviceStateChanged();
                            }

                            switch (DisableBehavior)
                            {
                                case t_disableBehaviors.G2S_overwrite:
                                    if (_queuedEvents.TryDequeue(out var evtDisable))
                                    {
                                        SourceTrace.TraceInformation(
                                            G2STrace.Source,
                                            $@"EventHandlerDevice.QueueForSend : overwriting event while disabled {evtDisable.eventCode}({evtDisable.eventId}) for device {Id}");
                                    }
                                    break;
                                case t_disableBehaviors.G2S_discard:
                                    return;
                            }

                            break;
                        case t_queueBehaviors.G2S_discard:
                            return;
                    }

                    if (!Overflow)
                    {
                        Overflow = true;
                        EventReport(
                            eventDeviceId: Id,
                            eventCode: EventCode.G2S_EHE102,
                            eventText: "Event Handler Queue Overflow",
                            deviceList: this.DeviceList());
                    }
                }
                else
                {
                    if (Overflow)
                    {
                        SourceTrace.TraceInformation(
                            G2STrace.Source,
                            $@"EventHandlerDevice.QueueForSend : Queue no longer overflowed - current count {_queuedEvents.Count}");

                        Overflow = false;
                        _overflowEvents.Add(EventCode.G2S_EHE103);

                        if (!Enabled)
                        {
                            Enabled = true;
                            _overflowEvents.Add(EventCode.G2S_EHE002);
                            DeviceStateChanged();
                        }
                    }
                }

                if (PersistedEvent(command.eventCode, Id))
                {
                    AddEventLog(command);
                }

                _queuedEvents.Enqueue(command);

                SourceTrace.TraceInformation(
                    G2STrace.Source,
                    $@"EventHandlerDevice.QueueForSend : Queued event - current count {_queuedEvents.Count}");

                SendEventReports();
            }
        }

        private void DequeueEvent(bool ack)
        {
            if (_queuedEvents.TryDequeue(out var evt) && ack)
            {
                _eventPersistenceManager.UpdateEventLog(evt, Id, true);
            }
        }

        private void SendEventReports()
        {
            if (_senderThread == null || !_senderThread.IsAlive)
            {
                _senderThread = new Thread(Send) { Name = GetType().ToString() };
                _senderThread.Start();
            }
        }

        private void Send()
        {
            while (_open && !Queue.CanSend)
            {
                Thread.Sleep(100);
            }

            while (_open && Queue.CanSend && _queuedEvents.Count > 0 && HostEnabled)
            {
                if (!_queuedEvents.TryPeek(out var er))
                {
                    continue;
                }

                if (PersistedEvent(er.eventCode, er.deviceId))
                {
                    if (!SendEventReportRequest(er))
                    {
                        continue;
                    }
                }
                else
                {
                    SendEventReportNotification(er);
                }

                if (_queuedEvents.Count < MinLogEntries - _overflowEvents.Count && _overflowEvents.Count > 0)
                {
                    var postEvents = _overflowEvents.ToArray();
                    _overflowEvents.Clear();
                    foreach (var overflowEvent in postEvents)
                    {
                        EventReport(
                            eventDeviceId: Id,
                            eventCode: overflowEvent,
                            deviceList: this.DeviceList());
                    }
                }
            }
        }

        private void SendEventReportNotification(eventReport er)
        {
            var request = InternalCreateClass();

            request.Item = er;

            SendNotification(request);

            DequeueEvent(false);
        }

        private bool SendEventReportRequest(eventReport er)
        {
            var request = InternalCreateClass();
            request.Item = er;

            var session = SendRequest(request, TimeSpan.FromMilliseconds(TimeToLive));
            session.WaitForCompletion();

            if (session.SessionState == SessionStatus.Success && session.Responses.Count > 0 &&
                session.Responses[0].IClass.Item is eventAck)
            {
                DequeueEvent(true);

                return true;
            }

            if (session.SessionState != SessionStatus.TimedOut)
            {
                SourceTrace.TraceInformation(
                    G2STrace.Source,
                    @"EventHandlerDevice.SendEventReportRequest : eventReport Failed.  Will try again
    Device Id : {0}",
                    Id);

                _timeoutExpired.WaitOne(TimeToLive);
            }

            return false;
        }

        private void AddEventLog(eventReport log)
        {
            _eventPersistenceManager.AddEventLog(log, Id, MinLogEntries);
        }

        private void MergeSub(eventHostSubscription org, eventHostSubscription eventSub)
        {
            org.eventPersist |= eventSub.eventPersist;
            org.sendClassMeters |= eventSub.sendClassMeters;
            org.sendDeviceMeters |= eventSub.sendDeviceMeters;
            org.sendDeviceStatus |= eventSub.sendDeviceStatus;
            org.sendTransaction |= eventSub.sendTransaction;
            org.sendUpdatableMeters |= eventSub.sendUpdatableMeters;
        }
    }
}