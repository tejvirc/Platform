namespace Aristocrat.Monaco.G2S.Handlers.Analytics
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.setAnalyticsState G2S message
    /// </summary>
    public class SetAnalyticsState : ICommandHandler<analytics, setAnalyticsState>
    {
        private readonly ICommandBuilder<IAnalyticsDevice, analyticsStatus> _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetAnalyticsState" /> class.
        /// </summary>
        public SetAnalyticsState(
            IG2SEgm egm,
            ICommandBuilder<IAnalyticsDevice, analyticsStatus> commandBuilder)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<analytics, setAnalyticsState> command)
        {
            return await Sanction.OnlyOwner<IAnalyticsDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<analytics, setAnalyticsState> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var device = _egm.GetDevice<IAnalyticsDevice>(command.IClass.deviceId);
                if (device == null)
                {
                    return;
                }

                var enabled = command.Command.enable;

                if (device.HostEnabled != enabled)
                {
                    device.DisableText = command.Command.disableText;
                    device.HostEnabled = enabled;
                }

                var response = command.GenerateResponse<analyticsStatus>();

                await _commandBuilder.Build(device, response.Command);
            }
        }
    }
}
