namespace Aristocrat.Monaco.G2S.Handlers.GamePlay
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
    public class GetGamePlayStatus : ICommandHandler<gamePlay, getGamePlayStatus>
    {
        private readonly ICommandBuilder<IGamePlayDevice, gamePlayStatus> _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetGamePlayStatus" /> class.
        /// </summary>
        /// <param name="egm">An instance of an <see cref="IG2SEgm" /></param>
        /// <param name="commandBuilder">An instance of an <see cref="ICommandBuilder{TDevice,TCommand}" /></param>
        public GetGamePlayStatus(IG2SEgm egm, ICommandBuilder<IGamePlayDevice, gamePlayStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<gamePlay, getGamePlayStatus> command)
        {
            return await Sanction.OwnerAndGuests<IGamePlayDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<gamePlay, getGamePlayStatus> command)
        {
            var device = _egm.GetDevice<IGamePlayDevice>(command.IClass.deviceId);

            var response = command.GenerateResponse<gamePlayStatus>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}