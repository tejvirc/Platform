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
    public class GetCoinAcceptorStatus : ICommandHandler<coinAcceptor, getCoinAcceptorStatus>
    {
        private readonly ICommandBuilder<ICoinAcceptor, coinAcceptorStatus> _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetCoinAcceptorStatus" /> class.
        /// </summary>
        /// <param name="egm">An instance of an <see cref="IG2SEgm" /></param>
        /// <param name="commandBuilder">An instance of an <see cref="ICommandBuilder{ICoinAcceptor,coinAcceptorStatus}" /></param>
        public GetCoinAcceptorStatus(IG2SEgm egm, ICommandBuilder<ICoinAcceptor, coinAcceptorStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<coinAcceptor, getCoinAcceptorStatus> command)
        {
            return await Sanction.OwnerAndGuests<ICoinAcceptor>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<coinAcceptor, getCoinAcceptorStatus> command)
        {
            var device = _egm.GetDevice<ICoinAcceptor>(command.IClass.deviceId);

            var response = command.GenerateResponse<coinAcceptorStatus>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}