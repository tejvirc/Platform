namespace Aristocrat.Monaco.G2S.Handlers.CoinAcceptor
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Defines a new instance of an <see cref="ICommandHandler" />
    /// </summary>
    public class SetCoinAcceptorState : ICommandHandler<coinAcceptor, setCoinAcceptorState>
    {
        private readonly ICommandBuilder<ICoinAcceptor, coinAcceptorStatus> _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetCoinAcceptorState" /> class.
        /// </summary>
        public SetCoinAcceptorState(
            IG2SEgm egm,
            ICommandBuilder<ICoinAcceptor, coinAcceptorStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<coinAcceptor, setCoinAcceptorState> command)
        {
            return await Sanction.OnlyOwner<ICoinAcceptor>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<coinAcceptor, setCoinAcceptorState> command)
        {
            var device = _egm.GetDevice<ICoinAcceptor>(command.IClass.deviceId);

            SetState(device, command.Command.enable, command.Command.disableText);

            var response = command.GenerateResponse<coinAcceptorStatus>();

            await _commandBuilder.Build(device, response.Command);
        }

        private void SetState(ICoinAcceptor device, bool enabled, string message)
        {
            if (device.HostEnabled == enabled)
            {
                return;
            }

            device.DisableText = message;
            device.HostEnabled = enabled;
        }
    }
}