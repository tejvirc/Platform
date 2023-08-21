namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using System.Linq;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Gaming.Contracts;

    /// <summary>
    ///     Handles the <see cref="GameRemovedEvent" /> event.
    /// </summary>
    public class GameRemovedConsumer : Consumes<GameRemovedEvent>
    {
        private readonly IGameProvider _gameProvider;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameRemovedConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance</param>
        /// <param name="gameProvider">An <see cref="IGameProvider"/> instance</param>
        public GameRemovedConsumer(IG2SEgm egm, IGameProvider gameProvider)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public override void Consume(GameRemovedEvent theEvent)
        {
            if (_gameProvider.GetAllGames().All(g => !g.Enabled))
            {
                var cabinet = _egm.GetDevice<ICabinetDevice>();
                cabinet?.AddCondition(cabinet, EgmState.EgmDisabled, (int)CabinetFaults.NoGamesEnabled);
            }

            var device = _egm.GetDevice<IGamePlayDevice>(theEvent.GameId);
            if (device != null)
            {
                device.HostEnabled = false;
                _egm.RemoveDevice(device);
                ////device.Deactivate();
                ////_profiles.Save(device);
            }
        }
    }
}
