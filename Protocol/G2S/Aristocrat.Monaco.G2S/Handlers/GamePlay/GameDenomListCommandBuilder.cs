namespace Aristocrat.Monaco.G2S.Handlers.GamePlay
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;

    /// <summary>
    ///     An implementation of <see cref="ICommandBuilder{TDevice,TCommand}" />
    /// </summary>
    public class GameDenomListCommandBuilder : ICommandBuilder<IGamePlayDevice, gameDenomList>
    {
        private readonly IGameProvider _gameProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameDenomListCommandBuilder" /> class.
        /// </summary>
        /// <param name="gameProvider">An <see cref="IGameProvider" /></param>
        public GameDenomListCommandBuilder(IGameProvider gameProvider)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        /// <inheritdoc />
        public async Task Build(IGamePlayDevice device, gameDenomList command)
        {
            var game = _gameProvider.GetGame(device.Id);

            command.Items =
                game.SupportedDenominations.Select(
                        denom => new gameDenom { denomId = denom, active = game.ActiveDenominations.Contains(denom) })
                    .Cast<object>()
                    .ToArray();

            await Task.CompletedTask;
        }
    }
}