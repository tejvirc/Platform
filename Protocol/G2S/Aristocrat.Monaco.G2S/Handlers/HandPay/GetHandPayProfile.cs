namespace Aristocrat.Monaco.G2S.Handlers.Handpay
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using System.Threading.Tasks;

    public class GetHandpayProfile : ICommandHandler<handpay, getHandpayProfile>
    {
        private readonly IG2SEgm _egm;
        private readonly ICommandBuilder<IHandpayDevice, handpayProfile> _commandBuilder;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetHandpayProfile" /> class.
        /// </summary>
        /// <param name="egm">The <see cref="IG2SEgm" /> object</param>
        /// <param name="commandBuilder">An ICommandBuilder instance.</param>
        public GetHandpayProfile(IG2SEgm egm, ICommandBuilder<IHandpayDevice, handpayProfile> commandBuilder)
        {
            _egm = egm;
            _commandBuilder = commandBuilder;
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<handpay, getHandpayProfile> command)
        {
            return await Sanction.OwnerAndGuests<IHandpayDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<handpay, getHandpayProfile> command)
        {
            var device = _egm.GetDevice<IHandpayDevice>(command.IClass.deviceId);

            var response = command.GenerateResponse<handpayProfile>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}
