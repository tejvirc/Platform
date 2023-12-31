﻿namespace Aristocrat.Monaco.Gaming.Commands.RuntimeEvents
{
    using Kernel;
    using Runtime.Client;
    using System;
    using Contracts;

    public class ReplayRuntimeEventHandler : IReplayRuntimeEventHandler
    {
        private readonly IPropertiesManager _properties;
        private readonly IEventBus _bus;
        private readonly IGameDiagnostics _gameDiagnostics;

        public ReplayRuntimeEventHandler(
            IPropertiesManager properties,
            IEventBus bus,
            IGameDiagnostics gameDiagnostics)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _gameDiagnostics = gameDiagnostics ?? throw new ArgumentNullException(nameof(gameDiagnostics));
        }

        public void HandleEvent(ReplayGameRoundEvent replayGameRoundEvent)
        {
            if (_gameDiagnostics.Context is not IDiagnosticContext<IGameHistoryLog> context)
            {
                return;
            }

            switch (replayGameRoundEvent.State)
            {
                case GameRoundEventState.Present:
                    var (game, denomination) = _properties.GetActiveGame();
                    var wagerCategory = _properties.GetValue<IWagerCategory>(GamingConstants.SelectedWagerCategory, null);

                    if (replayGameRoundEvent.Action == GameRoundEventAction.Begin)
                    {
                        _bus.Publish(new GamePresentationStartedEvent(game.Id, denomination.Value, wagerCategory.Id, context.Arguments));
                    }
                    else if (replayGameRoundEvent.Action == GameRoundEventAction.Pending)
                    {
                        _bus.Publish(new GameWinPresentationStartedEvent(game.Id, denomination.Value, wagerCategory.Id, context.Arguments));
                    }
                    else if (replayGameRoundEvent.Action == GameRoundEventAction.Completed)
                    {
                        _bus.Publish(new GamePresentationEndedEvent(game.Id, denomination.Value, wagerCategory.Id, context.Arguments));
                    }
                    else if (replayGameRoundEvent.Action == GameRoundEventAction.Invoked)
                    {
                        _bus.Publish(new GameReplayCompletedEvent());
                    }

                    break;
            }
        }
    }
}
