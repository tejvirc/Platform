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

    /// <summary>
    ///     Handles the v21.clearEventSub G2S message
    /// </summary>
    public class ClearEventSub : ICommandHandler<eventHandler, clearEventSub>
    {
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ClearEventSub" /> class.
        ///     Creates a new instance of the ClearEventSub handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="eventLift">Event lift.</param>
        public ClearEventSub(IG2SEgm egm, IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<eventHandler, clearEventSub> command)
        {
            return await Sanction.OnlyOwner<IEventHandlerDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<eventHandler, clearEventSub> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var eventHandlerDevice = _egm.GetDevice<IEventHandlerDevice>(command.IClass.deviceId);
                if (eventHandlerDevice == null)
                {
                    return;
                }

                var eventSubs = eventHandlerDevice.GetAllRegisteredEventSub().ToList();
                var removeEvents = new List<eventHostSubscription>();
                bool matchClass;
                bool matchDevice;
                bool matchCode;
                if (command.Command.eventSelect != null)
                {
                    foreach (var eventSelect in command.Command.eventSelect)
                    {
                        try
                        {
                            matchClass = eventSelect.deviceClass != DeviceClass.G2S_all;
                            matchDevice = eventSelect.deviceId != DeviceId.All;
                            matchCode = eventSelect.eventCode != DeviceClass.G2S_all;

                            removeEvents.AddRange(
                                eventSubs.Where(sub => IsMatch(eventSelect, sub, matchClass, matchDevice, matchCode)));
                        }
                        catch (ArgumentNullException)
                        {
                        }
                    }

                    eventHandlerDevice.RemoveRegisteredEventSubscriptions(removeEvents);
                }

                _eventLift.Report(eventHandlerDevice, EventCode.G2S_EHE101);

                var response = command.GenerateResponse<clearEventSubAck>();
                response.Command.listStateDateTime = DateTime.UtcNow;
            }

            await Task.CompletedTask;
        }

        private static bool IsMatch(
            c_eventSelect select,
            c_eventSelect sub,
            bool matchClass,
            bool matchDevice,
            bool matchCode)
        {
            if (matchClass && sub.deviceClass != select.deviceClass)
            {
                return false;
            }

            if (matchDevice && sub.deviceId != select.deviceId)
            {
                return false;
            }

            return !matchCode || sub.eventCode == select.eventCode;
        }
    }
}