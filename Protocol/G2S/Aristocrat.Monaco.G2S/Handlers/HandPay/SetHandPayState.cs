namespace Aristocrat.Monaco.G2S.Handlers.Handpay
{
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class SetHandpayState : ICommandHandler<handpay, setHandpayState>
    {
        private readonly ICommandBuilder<IHandpayDevice, handpayStatus> _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetHandpayState" /> class.
        /// </summary>
        public SetHandpayState(
            IG2SEgm egm,
            ICommandBuilder<IHandpayDevice, handpayStatus> commandBuilder)
        {
            _egm = egm;
            _commandBuilder = commandBuilder;
        }

        public async Task<Error> Verify(ClassCommand<handpay, setHandpayState> command)
        {
            return await Sanction.OnlyOwner<IHandpayDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<handpay, setHandpayState> command)
        {
            var device = _egm.GetDevice<IHandpayDevice>(command.IClass.deviceId);
            if (device == null)
            {
                return;
            }

            if (device.HostEnabled != command.Command.enable)
            {
                device.DisableText = command.Command.disableText;
                device.HostEnabled = command.Command.enable;
            }

            var response = command.GenerateResponse<handpayStatus>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}