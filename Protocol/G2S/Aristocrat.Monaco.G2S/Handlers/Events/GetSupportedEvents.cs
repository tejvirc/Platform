namespace Aristocrat.Monaco.G2S.Handlers.Events
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using DeviceClass = Aristocrat.G2S.DeviceClass;

    /// <summary>
    ///     Handles the v21.getSupportedEvents G2S message
    /// </summary>
    public class GetSupportedEvents : ICommandHandler<eventHandler, getSupportedEvents>
    {
        private readonly IG2SEgm _egm;
        private readonly IEventPersistenceManager _eventPersistenceManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetSupportedEvents" /> class.
        ///     Creates a new instance of the GetSupportedEvents handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="eventPersistenceManager">Event persistence manager.</param>
        public GetSupportedEvents(IG2SEgm egm, IEventPersistenceManager eventPersistenceManager)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventPersistenceManager =
                eventPersistenceManager ?? throw new ArgumentException(nameof(eventPersistenceManager));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<eventHandler, getSupportedEvents> command)
        {
            return await Sanction.OwnerAndGuests<IEventHandlerDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<eventHandler, getSupportedEvents> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var eventHandlerDevice = _egm.GetDevice<IEventHandlerDevice>(command.HostId);
                if (eventHandlerDevice == null)
                {
                    return;
                }

                var response = command.GenerateResponse<supportedEvents>();
                var supportedEvents = response.Command;
                var events = _eventPersistenceManager.SupportedEvents.Cast<SupportedEvent>();
                var result =
                    command.Command.deviceClass == DeviceClass.G2S_all && command.Command.deviceId == DeviceId.All
                        ? events
                        : command.Command.deviceClass != DeviceClass.G2S_all && command.Command.deviceId == DeviceId.All
                            ? events.Where(
                                a => a.DeviceClass == command.Command.deviceClass)
                            : command.Command.deviceClass == DeviceClass.G2S_all &&
                              command.Command.deviceId != DeviceId.All
                                ? events.Where(a => a.DeviceId == command.Command.deviceId)
                                : events.Where(
                                    a => a.DeviceId == command.Command.deviceId &&
                                         a.DeviceClass == command.Command.deviceClass);

                supportedEvents.supportedEvent = result.Select(
                    _ => new supportedEvent
                    {
                        deviceClass = _.DeviceClass,
                        deviceId = _.DeviceId,
                        eventCode = _.EventCode,
                        eventText = EventHandlerExtensions.GetEventText(_.EventCode)
                    }).ToArray();
            }

            await Task.CompletedTask;
        }
    }
}