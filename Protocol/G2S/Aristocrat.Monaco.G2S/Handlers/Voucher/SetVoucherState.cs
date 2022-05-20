namespace Aristocrat.Monaco.G2S.Handlers.Voucher
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Services;

    /// <summary>
    ///     Handles the v21.setVoucherState G2S message
    /// </summary>
    public class SetVoucherState : ICommandHandler<voucher, setVoucherState>
    {
        private readonly ICommandBuilder<IVoucherDevice, voucherStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IVoucherDataService _voucherDataService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetVoucherState" /> class.
        /// </summary>
        public SetVoucherState(
            IG2SEgm egm,
            IVoucherDataService voucherDataService,
            ICommandBuilder<IVoucherDevice, voucherStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _voucherDataService = voucherDataService ?? throw new ArgumentNullException(nameof(voucherDataService));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<voucher, setVoucherState> command)
        {
            return await Sanction.OnlyOwner<IVoucherDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<voucher, setVoucherState> command)
        {
            if (command.Class.sessionType != t_sessionTypes.G2S_request)
            {
                return;
            }

            var voucher = _egm.GetDevice<IVoucherDevice>(command.IClass.deviceId);
            if (voucher == null)
            {
                return;
            }

            if (voucher.HostEnabled != command.Command.enable)
            {
                voucher.DisableText = command.Command.disableText;
                voucher.HostEnabled = command.Command.enable;
            }

            _voucherDataService.VoucherStateChanged();

            var response = command.GenerateResponse<voucherStatus>();
            await _commandBuilder.Build(voucher, response.Command);
        }
    }
}