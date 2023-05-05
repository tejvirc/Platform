namespace Aristocrat.Monaco.G2S.Handlers.Communications
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.getCommsProfile G2S message
    /// </summary>
    public class GetCommsProfile : ICommandHandler<communications, getCommsProfile>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetCommsProfile" /> class.
        ///     Creates a new instance of the GetCommsProfile handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        public GetCommsProfile(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<communications, getCommsProfile> command)
        {
            var device = _egm.GetDevice<ICommunicationsDevice>(command.IClass.deviceId);

            return await Task.FromResult(command.Validate(device, CommandRestrictions.RestrictedToOwnerAndGuests));
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<communications, getCommsProfile> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var device = _egm.GetDevice<ICommunicationsDevice>(command.IClass.deviceId);

                var response = command.GenerateResponse<commsProfile>();

                response.Command.configurationId = device.ConfigurationId;
                response.Command.useDefaultConfig = device.UseDefaultConfig;
                response.Command.requiredForPlay = device.RequiredForPlay;
                response.Command.timeToLive = device.TimeToLive;
                response.Command.noResponseTimer = (int)device.NoResponseTimer.TotalMilliseconds;
                response.Command.displayCommFault = device.DisplayFault;
                response.Command.allowMulticast = device.AllowMulticast;
            }

            await Task.CompletedTask;
        }
    }
}