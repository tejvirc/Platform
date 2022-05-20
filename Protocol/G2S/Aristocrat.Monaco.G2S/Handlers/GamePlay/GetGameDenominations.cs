namespace Aristocrat.Monaco.G2S.Handlers.GamePlay
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     An implementation of <see cref="ICommandHandler{TClass,TCommand}" />
    /// </summary>
    public class GetGameDenominations : ICommandHandler<gamePlay, getGameDenoms>
    {
        private readonly ICommandBuilder<IGamePlayDevice, gameDenomList> _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetGameDenominations" /> class.
        /// </summary>
        /// <param name="egm">An instance of an <see cref="IG2SEgm" /></param>
        /// <param name="commandBuilder">An instance of an <see cref="ICommandBuilder{TDevice,TCommand}" /></param>
        public GetGameDenominations(IG2SEgm egm, ICommandBuilder<IGamePlayDevice, gameDenomList> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<gamePlay, getGameDenoms> command)
        {
            return await Sanction.OwnerAndGuests<IGamePlayDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<gamePlay, getGameDenoms> command)
        {
            var device = _egm.GetDevice<IGamePlayDevice>(command.IClass.deviceId);

            var response = command.GenerateResponse<gameDenomList>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}