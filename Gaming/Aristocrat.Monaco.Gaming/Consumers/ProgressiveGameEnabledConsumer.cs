namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using System.Linq;
    using Contracts;
    using Contracts.Progressives;
    using Kernel;
    using Runtime;
    using Runtime.Client;

    public class ProgressiveGameEnabledConsumer : Consumes<ProgressiveGameEnabledEvent>
    {
        private readonly IRuntime _runtime;
        private readonly IPropertiesManager _properties;
        private readonly IGameProvider _gameProvider;

        public ProgressiveGameEnabledConsumer(IRuntime runtime, IPropertiesManager properties, IGameProvider gameProvider)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        public override void Consume(ProgressiveGameEnabledEvent theEvent)
        {
            if (IsActiveGame(theEvent))
            {
                _runtime.UpdateFlag(RuntimeCondition.ProgressiveError, false);
            }
        }

        private bool IsActiveGame(ProgressiveGameEnabledEvent theEvent)
        {
            var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
            var denom = _properties.GetValue(GamingConstants.SelectedDenom, 0L);
            return _runtime.Connected && theEvent.Denom == denom && theEvent.GameId == gameId &&
                   (string.IsNullOrEmpty(theEvent.BetOption) ||
                    _gameProvider.GetGame(gameId).Denominations.FirstOrDefault(x => x.Value == denom)?.BetOption ==
                    theEvent.BetOption);
        }
    }
}