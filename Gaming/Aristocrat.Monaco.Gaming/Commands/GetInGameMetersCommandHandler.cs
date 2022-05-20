namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts;
    using Contracts.Models;
    using Kernel;

    /// <summary>
    ///     Command handler for the <see cref="GetInGameMeters" /> command.
    /// </summary>
    public class GetInGameMetersCommandHandler : ICommandHandler<GetInGameMeters>
    {
        private readonly IGameStorage _gameStorage;
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetInGameMetersCommandHandler" /> class.
        /// </summary>
        /// <param name="properties">An <see cref="IPropertiesManager" /> instance</param>
        /// <param name="gameStorage">An <see cref="IGameStorage" /> instance</param>
        public GetInGameMetersCommandHandler(
            IPropertiesManager properties,
            IGameStorage gameStorage)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _gameStorage = gameStorage ?? throw new ArgumentNullException(nameof(gameStorage));
        }

        /// <inheritdoc />
        public void Handle(GetInGameMeters command)
        {
            var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
            var denomId = _properties.GetValue(GamingConstants.SelectedDenom, 0L);

            foreach (var meter in _gameStorage.GetValues<InGameMeter>(gameId, denomId, GamingConstants.InGameMeters))
            {
                command.MeterValues.Add(meter.MeterName, (ulong)meter.Value);
            }
        }
    }
}
