namespace Aristocrat.Monaco.G2S.Handlers.Printer
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Implementation of <see cref="setPrinterState" /> for the <see cref="printer" /> class
    /// </summary>
    public class SetPrinterState : ICommandHandler<printer, setPrinterState>
    {
        private readonly ICommandBuilder<IPrinterDevice, printerStatus> _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetPrinterState" /> class.
        /// </summary>
        public SetPrinterState(IG2SEgm egm, ICommandBuilder<IPrinterDevice, printerStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<printer, setPrinterState> command)
        {
            return await Sanction.OnlyOwner<IPrinterDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<printer, setPrinterState> command)
        {
            var device = _egm.GetDevice<IPrinterDevice>(command.IClass.deviceId);

            var enabled = command.Command.enable;

            if (device.HostEnabled != enabled)
            {
                device.DisableText = command.Command.disableText;
                device.HostEnabled = enabled;
            }

            var response = command.GenerateResponse<printerStatus>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}
