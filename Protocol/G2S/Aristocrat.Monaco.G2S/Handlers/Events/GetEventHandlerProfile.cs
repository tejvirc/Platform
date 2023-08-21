namespace Aristocrat.Monaco.G2S.Handlers.Events
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.getEventHandlerProfile G2S message
    /// </summary>
    public class GetEventHandlerProfile : ICommandHandler<eventHandler, getEventHandlerProfile>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetEventHandlerProfile" /> class.
        ///     Creates a new instance of the GetEventHandlerProfile handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        public GetEventHandlerProfile(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<eventHandler, getEventHandlerProfile> command)
        {
            return await Sanction.OwnerAndGuests<IEventHandlerDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<eventHandler, getEventHandlerProfile> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var eventHandlerDevice = _egm.GetDevice<IEventHandlerDevice>(command.IClass.deviceId);
                if (eventHandlerDevice == null)
                {
                    return;
                }

                var response = command.GenerateResponse<eventHandlerProfile>();
                var profile = response.Command;

                profile.configurationId = eventHandlerDevice.ConfigurationId;
                profile.restartStatus = eventHandlerDevice.RestartStatus;
                profile.useDefaultConfig = eventHandlerDevice.UseDefaultConfig;
                profile.requiredForPlay = eventHandlerDevice.RequiredForPlay;
                if (eventHandlerDevice.MinLogEntries > 0)
                {
                    profile.minLogEntries = eventHandlerDevice.MinLogEntries;
                }

                profile.timeToLive = eventHandlerDevice.TimeToLive;
                profile.queueBehavior = eventHandlerDevice.QueueBehavior;
                profile.configComplete = eventHandlerDevice.ConfigComplete;
                profile.disableBehavior = eventHandlerDevice.DisableBehavior;
                profile.forcedSubscription = eventHandlerDevice.GetAllForcedEventSub().ToArray();

                if (eventHandlerDevice.ConfigDateTime != default(DateTime))
                {
                    profile.configDateTime = eventHandlerDevice.ConfigDateTime;
                    profile.configDateTimeSpecified = true;
                }
            }

            await Task.CompletedTask;
        }
    }
}