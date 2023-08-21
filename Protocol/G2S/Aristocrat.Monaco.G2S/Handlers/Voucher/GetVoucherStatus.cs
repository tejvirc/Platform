namespace Aristocrat.Monaco.G2S.Handlers.Voucher
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.getVoucherStatus G2S message
    /// </summary>
    public class GetVoucherStatus : ICommandHandler<voucher, getVoucherStatus>
    {
        private readonly IG2SEgm _egm;
        private readonly ICommandBuilder<IVoucherDevice, voucherStatus> _voucherStatusCommandBuilder;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetVoucherStatus" /> class.
        ///     Creates a new instance of the GetDownloadStatus handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="voucherStatusCommandBuilder">Voucher Data service.</param>
        public GetVoucherStatus(IG2SEgm egm, ICommandBuilder<IVoucherDevice, voucherStatus> voucherStatusCommandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _voucherStatusCommandBuilder = voucherStatusCommandBuilder ??
                                           throw new ArgumentNullException(nameof(voucherStatusCommandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<voucher, getVoucherStatus> command)
        {
            return await Sanction.OwnerAndGuests<IVoucherDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<voucher, getVoucherStatus> command)
        {
            var device = _egm.GetDevice<IVoucherDevice>(command.IClass.deviceId);
            if (device == null)
            {
                return;
            }

            var response = command.GenerateResponse<voucherStatus>();
            var status = response.Command;
            await _voucherStatusCommandBuilder.Build(device, status);
        }
    }
}