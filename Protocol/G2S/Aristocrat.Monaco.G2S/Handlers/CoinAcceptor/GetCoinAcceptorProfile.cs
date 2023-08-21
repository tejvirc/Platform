namespace Aristocrat.Monaco.G2S.Handlers.CoinAcceptor
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Defines a new instance of an <see cref="ICommandHandler{TClass,TCommand}" />
    /// </summary>
    public class GetCoinAcceptorProfile : ICommandHandler<coinAcceptor, getCoinAcceptorProfile>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetCoinAcceptorProfile" /> class.
        /// </summary>
        /// <param name="egm">An instance of an <see cref="IG2SEgm" /></param>
        public GetCoinAcceptorProfile(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<coinAcceptor, getCoinAcceptorProfile> command)
        {
            return await Sanction.OwnerAndGuests<ICoinAcceptor>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<coinAcceptor, getCoinAcceptorProfile> command)
        {
            var device = _egm.GetDevice<ICoinAcceptor>(command.IClass.deviceId);

            var response = command.GenerateResponse<coinAcceptorProfile>();

            response.Command.restartStatus = device.RestartStatus;
            response.Command.useDefaultConfig = device.UseDefaultConfig;
            response.Command.requiredForPlay = device.RequiredForPlay;
            response.Command.promoSupported = t_g2sBoolean.G2S_false;
            response.Command.configurationId = device.ConfigurationId;
            response.Command.configComplete = device.ConfigComplete;
            response.Command.configDateTime = device.ConfigDateTime;

            // TODO
            response.Command.coinData = new coinData[0];

            await Task.CompletedTask;
        }
    }
}