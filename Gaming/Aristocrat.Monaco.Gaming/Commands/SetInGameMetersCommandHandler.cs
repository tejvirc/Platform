namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Linq;
    using Contracts;
    using Contracts.Models;
    using Kernel;

    /// <summary>
    ///     Command handler for the <see cref="SetInGameMeters" /> command.
    /// </summary>
    public class SetInGameMetersCommandHandler : ICommandHandler<SetInGameMeters>
    {
        private readonly IGameStorage _gameStorage;
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetInGameMetersCommandHandler" /> class.
        /// </summary>
        /// <param name="properties">An <see cref="IPropertiesManager" /> instance</param>
        /// <param name="gameStorage">An <see cref="IGameStorage" /> instance</param>
        public SetInGameMetersCommandHandler(IPropertiesManager properties, IGameStorage gameStorage)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _gameStorage = gameStorage ?? throw new ArgumentNullException(nameof(gameStorage));
        }

        /// <inheritdoc />
        public void Handle(SetInGameMeters command)
        {
            if (command.MeterValues.Count <= 0)
            {
                return;
            }

            var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
            var denomId = _properties.GetValue(GamingConstants.SelectedDenom, 0L);

            var meters = _gameStorage.GetValues<InGameMeter>(gameId, denomId, GamingConstants.InGameMeters).ToList();

            foreach (var meterValues in command.MeterValues)
            {
                var meter = meters.FirstOrDefault(m => m.MeterName == meterValues.Key);
                if (meter == null)
                {
                    meters.Add(new InGameMeter { MeterName = meterValues.Key, Value = (long)meterValues.Value });
                }
                else
                {
                    meter.Value = (long)meterValues.Value;
                }
            }

            _gameStorage.SetValue(gameId, denomId, GamingConstants.InGameMeters, meters);
        }
    }
}
