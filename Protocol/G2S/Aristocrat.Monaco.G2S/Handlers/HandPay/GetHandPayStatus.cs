namespace Aristocrat.Monaco.G2S.Handlers.Handpay
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using System.Threading.Tasks;

    public class GetHandpayStatus : ICommandHandler<handpay, getHandpayStatus>
    {
        private readonly IG2SEgm _egm;
        private readonly ICommandBuilder<IHandpayDevice, handpayStatus> _commandBuilder;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetHandpayStatus" /> class.
        /// </summary>
        /// <param name="egm">The <see cref="IG2SEgm" /> object</param>
        /// <param name="commandBuilder">Handpay Data service.</param>
        public GetHandpayStatus(
            IG2SEgm egm,
            ICommandBuilder<IHandpayDevice, handpayStatus> commandBuilder)
        {
            _egm = egm;
            _commandBuilder = commandBuilder;
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<handpay, getHandpayStatus> command)
        {
            return await Sanction.OwnerAndGuests<IHandpayDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<handpay, getHandpayStatus> command)
        {
            var device = _egm.GetDevice<IHandpayDevice>(command.IClass.deviceId);
            if (device == null)
            {
                return;
            }

            var response = command.GenerateResponse<handpayStatus>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}
