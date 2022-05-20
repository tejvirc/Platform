namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.getCommConfigProfile G2S message
    /// </summary>
    public class GetOptionConfigProfile : ICommandHandler<optionConfig, getOptionConfigProfile>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetOptionConfigProfile" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm.</param>
        public GetOptionConfigProfile(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<optionConfig, getOptionConfigProfile> command)
        {
            return await Sanction.OwnerAndGuests<IOptionConfigDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<optionConfig, getOptionConfigProfile> command)
        {
            if (command.Class.sessionType != t_sessionTypes.G2S_request)
            {
                return;
            }

            var device = _egm.GetDevice<IOptionConfigDevice>(command.IClass.deviceId);

            var response = command.GenerateResponse<optionConfigProfile>();

            response.Command.configurationId = device.ConfigurationId;
            if (device.MinLogEntries > 0)
            {
                response.Command.minLogEntries = device.MinLogEntries;
            }

            response.Command.noResponseTimer = (int)device.NoResponseTimer.TotalMilliseconds;
            if (device.ConfigDateTime != default(DateTime))
            {
                response.Command.configDateTimeSpecified = true;
                response.Command.configDateTime = device.ConfigDateTime;
            }

            response.Command.configComplete = device.ConfigComplete;

            await Task.CompletedTask;
        }
    }
}