namespace Aristocrat.Monaco.G2S.Handlers.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using log4net;
    using DeviceClass = Aristocrat.G2S.DeviceClass;

    /// <summary>
    ///     Handles the v21.setEventSub G2S message
    /// </summary>
    public class SetEventSub : ICommandHandler<eventHandler, setEventSub>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IEventPersistenceManager _eventPersistenceManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetEventSub" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="eventLift">Event lift.</param>
        /// <param name="eventPersistenceManager">Event persistence manager.</param>
        public SetEventSub(IG2SEgm egm, IEventLift eventLift, IEventPersistenceManager eventPersistenceManager)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _eventPersistenceManager =
                eventPersistenceManager ?? throw new ArgumentException(nameof(eventPersistenceManager));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<eventHandler, setEventSub> command)
        {
            return await Sanction.OnlyOwner<IEventHandlerDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<eventHandler, setEventSub> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var eventHandlerDevice = _egm.GetDevice<IEventHandlerDevice>(command.HostId);
                if (eventHandlerDevice == null)
                {
                    return;
                }

                var events = _eventPersistenceManager.SupportedEvents?.Cast<SupportedEvent>().ToList() ?? new List<SupportedEvent>();

                if (events.Count != 0 && command.Command.eventHostSubscription != null
                    && command.Command.eventHostSubscription.Length > 0
                    && !command.Command.eventHostSubscription.All(sub => ValidateSubscription(sub, events, command)))
                {
                    command.Error.SetErrorCode(ErrorCode.G2S_EHX001);
                }
                else if (command.Command.eventHostSubscription != null)
                {
                    var subscriptions = new List<eventHostSubscription>();

                    foreach (var eventSub in command.Command.eventHostSubscription)
                    {
                        var allClass = eventSub.deviceClass == DeviceClass.G2S_all;
                        var allDevices = eventSub.deviceId == DeviceId.All;
                        var allEvents = eventSub.eventCode == DeviceClass.G2S_all;
                        if (allClass || allDevices || allEvents)
                        {
                            try
                            {
                                subscriptions.AddRange(
                                    events.Where(
                                            a =>
                                                IsMatchingSupported(
                                                    a,
                                                    eventSub,
                                                    !allClass,
                                                    !allDevices,
                                                    !allEvents)
                                                &&
                                                CheckDevice(
                                                    a.DeviceClass,
                                                    a.DeviceId,
                                                    command.IClass.deviceId))
                                        .Select(
                                            se =>
                                                GetEventHostSub(
                                                    se.DeviceClass,
                                                    se.DeviceId,
                                                    se.EventCode,
                                                    eventSub)));
                            }
                            catch (ArgumentNullException)
                            {
                            }
                        }
                        else
                        {
                            if (CheckDevice(eventSub.deviceClass, eventSub.deviceId, command.IClass.deviceId))
                            {
                                subscriptions.Add(eventSub);
                            }
                        }
                    }

                    if (eventHandlerDevice.SetEventSubscriptions(subscriptions))
                    {
                        if (command.Command.eventHostSubscription.Length > 0)
                        {
                            _eventLift.Report(eventHandlerDevice, EventCode.G2S_EHE101);
                        }

                        var response = command.GenerateResponse<setEventSubAck>();
                        response.Command.listStateDateTime = DateTime.UtcNow;
                        response.Command.listStateDateTimeSpecified = true;
                    }
                    else
                    {
                        command.Error.SetErrorCode(ErrorCode.G2S_EHX003);
                    }
                }
            }

            await Task.CompletedTask;
        }

        private bool ValidateSubscription(
            eventHostSubscription sub,
            List<SupportedEvent> events,
            ClassCommand<eventHandler, setEventSub> command)
        {
            if (sub.deviceClass != DeviceClass.G2S_all && sub.deviceId != DeviceId.All)
            {
                var device = _egm.GetDevice(sub.deviceClass.TrimmedDeviceClass(), sub.deviceId);
                if (device == null || !device.Active)
                {
                    Logger.Debug($"Failed to find an active device for {sub.deviceClass}[{sub.deviceId}]");

                    return false;
                }

                if (!device.IsGuest(command.HostId) && !device.IsOwner(command.HostId))
                {
                    Logger.Debug($"Host Id {command.HostId} does not have permissions to subscribe to {sub.deviceClass}[{sub.deviceId}]");

                    return false;
                }

                if (!events.Exists(e => e.DeviceClass == sub.deviceClass && e.DeviceId == sub.deviceId))
                {
                    Logger.Debug($"Device {sub.deviceClass}[{sub.deviceId}] does not have any advertised events");

                    return false;
                }

                if (sub.eventCode != DeviceClass.G2S_all && 
                    !events.Exists(e => e.DeviceClass == sub.deviceClass && e.EventCode == sub.eventCode))
                {
                    Logger.Debug($"Event {sub.eventCode} on {sub.deviceClass} is not available");

                    return false;
                }
            }

            return sub.deviceClass == DeviceClass.G2S_all || events.Exists(e => e.DeviceClass == sub.deviceClass);
        }

        private static bool IsMatchingSupported(
            SupportedEvent select,
            c_eventSelect sub,
            bool matchClass,
            bool matchDevice,
            bool matchCode)
        {
            if (matchClass && sub.deviceClass != select.DeviceClass)
            {
                return false;
            }

            if (matchDevice && sub.deviceId != select.DeviceId)
            {
                return false;
            }

            if (matchCode && sub.eventCode != select.EventCode)
            {
                return false;
            }

            return true;
        }

        private static eventHostSubscription GetEventHostSub(
            string deviceClass,
            int deviceId,
            string eventCode,
            c_hostSubscription eventSub)
        {
            return new eventHostSubscription
            {
                deviceClass = deviceClass,
                deviceId = deviceId,
                eventCode = eventCode,
                eventPersist = eventSub.eventPersist,
                sendClassMeters = eventSub.sendClassMeters,
                sendDeviceMeters = eventSub.sendDeviceMeters,
                sendDeviceStatus = eventSub.sendDeviceStatus,
                sendTransaction = eventSub.sendTransaction,
                sendUpdatableMeters = eventSub.sendUpdatableMeters
            };
        }

        private bool CheckDevice(string deviceClass, int deviceId, int hostId)
        {
            var device = _egm.GetDevice(deviceClass.TrimmedDeviceClass(), deviceId);

            return device != null && (device.IsGuest(hostId) || device.IsOwner(hostId));
        }
    }
}