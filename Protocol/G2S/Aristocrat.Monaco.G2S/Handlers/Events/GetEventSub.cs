namespace Aristocrat.Monaco.G2S.Handlers.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using DeviceClass = Aristocrat.G2S.DeviceClass;

    /// <summary>
    ///     Handles the v21.getEventSub G2S message
    /// </summary>
    public class GetEventSub : ICommandHandler<eventHandler, getEventSub>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetEventSub" /> class.
        ///     Creates a new instance of the GetEventSub handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        public GetEventSub(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<eventHandler, getEventSub> command)
        {
            return await Sanction.OwnerAndGuests<IEventHandlerDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<eventHandler, getEventSub> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var eventHandlerDevice = _egm.GetDevice<IEventHandlerDevice>(command.IClass.deviceId);
                if (eventHandlerDevice == null)
                {
                    return;
                }

                var response = command.GenerateResponse<eventSubList>();
                var eventSubscriptions = eventHandlerDevice.GetAllEventSubscriptions().Cast<EventSubscription>().ToList();
                var results = new List<eventSubscription>();
                if (command.Command.eventSelect != null)
                {
                    foreach (var select in command.Command.eventSelect)
                    {
                        if (command.Command.getForcedSubs)
                        {
                            results.AddRange(GetAllForcedSub(select, eventSubscriptions));
                        }

                        if (command.Command.getHostSubs)
                        {
                            results.AddRange(GetAllHostSub(select, eventSubscriptions));
                        }
                    }
                }

                response.Command.eventSubscription = results.ToArray();
            }

            await Task.CompletedTask;
        }

        /// <summary>
        ///     Checks for matching event subscriptions
        /// </summary>
        /// <param name="select">Event selection criteria</param>
        /// <param name="sub">Event to be checked</param>
        /// <param name="matchClass">Flag to check the class name</param>
        /// <param name="matchDevice">Flag to check the device ID</param>
        /// <param name="matchCode">Flag to check the event code</param>
        /// <returns>true if it's a match</returns>
        private static bool IsMatch(
            eventSelect select,
            EventSubscription sub,
            bool matchClass,
            bool matchDevice,
            bool matchCode)
        {
            if (matchClass && sub.DeviceClass != select.deviceClass)
            {
                return false;
            }

            if (matchDevice && sub.DeviceId != select.deviceId)
            {
                return false;
            }

            if (matchCode && sub.EventCode != select.eventCode)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Convert EventSubscription sub to eventSubscription
        /// </summary>
        /// <param name="sub">EventSubscription to convert</param>
        /// <returns>Converted eventSubscription</returns>
        private eventSubscription GetSub(EventSubscription sub)
        {
            if(sub.SubType == EventSubscriptionType.Host)
            {
                return new eventSubscription
                {
                    deviceClass = sub.DeviceClass,
                    deviceId = sub.DeviceId,
                    eventCode = sub.EventCode,
                    sendDeviceStatus = sub.SendDeviceStatus,
                    sendTransaction = sub.SendTransaction,
                    sendClassMeters = sub.SendClassMeters,
                    sendDeviceMeters = sub.SendDeviceMeters,
                    sendUpdatableMeters = sub.SendUpdatableMeters,
                    eventPersist = sub.EventPersist
                };
            }

            return new eventSubscription
            {
                deviceClass = sub.DeviceClass,
                deviceId = sub.DeviceId,
                eventCode = sub.EventCode,
                forceDeviceStatus = sub.SendDeviceStatus,
                forceTransaction = sub.SendTransaction,
                forceClassMeters = sub.SendClassMeters,
                forceDeviceMeters = sub.SendDeviceMeters,
                forceUpdatableMeters = sub.SendUpdatableMeters,
                forcePersist = sub.EventPersist
            };
        }

        /// <summary>
        ///     Gets all forced subscriptions that match the search criteria
        /// </summary>
        /// <param name="select">Subscription search criteria</param>
        /// <param name="eventSubscriptions">Event Subscriptions</param>
        /// <returns>Collection of the matching subscriptions</returns>
        private IEnumerable<eventSubscription> GetAllForcedSub(
            eventSelect select,
            IEnumerable<EventSubscription> eventSubscriptions)
        {
            var results = new List<eventSubscription>();

            try
            {
                var matchClass = select.deviceClass != DeviceClass.G2S_all;
                var matchDevice = select.deviceId != DeviceId.All;
                var matchCode = select.eventCode != DeviceClass.G2S_all;

                results.AddRange(
                    eventSubscriptions.Where(
                        e => e.SubType == EventSubscriptionType.Forced ||
                             e.SubType == EventSubscriptionType.Permanent).Where(
                        _ => IsMatch(select, _, matchClass, matchDevice, matchCode)).Select(
                        GetSub));
            }
            catch (ArgumentNullException)
            {
            }

            return results;
        }

        /// <summary>
        ///     Gets all host subscriptions that match the search criteria
        /// </summary>
        /// <param name="select">Subscription search criteria</param>
        /// <param name="eventSubscriptions">Event Subscriptions</param>
        /// <returns>Collection of the matching subscriptions</returns>
        private IEnumerable<eventSubscription> GetAllHostSub(eventSelect select, IEnumerable<EventSubscription> eventSubscriptions)
        {
            var results = new List<eventSubscription>();

            try
            {
                var matchClass = select.deviceClass != DeviceClass.G2S_all;
                var matchDevice = select.deviceId != DeviceId.All;
                var matchCode = select.eventCode != DeviceClass.G2S_all;
                results.AddRange(
                    eventSubscriptions.Where(
                        e => e.SubType == EventSubscriptionType.Host).Where(
                        _ => IsMatch(select, _, matchClass, matchDevice, matchCode)).Select(
                        GetSub));
            }
            catch (ArgumentNullException)
            {
            }

            return results;
        }
    }
}