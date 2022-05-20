namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Reflection;
    using Contracts;
    using Kernel;
    using log4net;
    using Runtime.Client;

    public class RuntimeFlagChangedCommandHandler : ICommandHandler<RuntimeFlagChanged>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _bus;
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly IAutoPlayStatusProvider _autoPlayStatusProvider;

        private readonly bool _meterFreeGames;
        private readonly bool _replayPauseEnabled;

        public RuntimeFlagChangedCommandHandler(
            IEventBus bus,
            IPropertiesManager properties,
            IGameDiagnostics gameDiagnostics,
            IAutoPlayStatusProvider autoPlayStatusProvider)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _gameDiagnostics = gameDiagnostics ?? throw new ArgumentNullException(nameof(gameDiagnostics));
            _autoPlayStatusProvider = autoPlayStatusProvider ?? throw new ArgumentNullException(nameof(autoPlayStatusProvider));

            _meterFreeGames = properties.GetValue(GamingConstants.MeterFreeGamesIndependently, false);
            _replayPauseEnabled = properties.GetValue(GamingConstants.ReplayPauseActive, true);
        }

        public void Handle(RuntimeFlagChanged command)
        {
            Logger.Debug($"{command.Condition}={command.State}");
            switch (command.Condition)
            {
                case RuntimeCondition.ReplayPause:
                    // This essentially means, we're splitting free games into their own game.  If that's the case we don't want the event
                    if (!_meterFreeGames || _gameDiagnostics.AllowResume)
                    {
                        Logger.Debug("Client Requested ReplayPauseInput");
                        _bus.Publish(new GameReplayPauseInputEvent(command.State));
                    }

                    break;
                case RuntimeCondition.DisplayingTimeRemaining:
                    Logger.Debug("Runtime updated DisplayingTimeRemaining Flag");
                    _bus.Publish(new DisplayingTimeRemainingChangedEvent(command.State));
                    break;
                case RuntimeCondition.AllowReplayResume:
                    // don't resume the replay when the pause is disabled
                    // when the replay is completed, one prompt should be there to ask whether to resume the replay.
                    // for now, nothing is displayed so the operator just exits after all is done
                    if (_replayPauseEnabled)
                    {
                        _gameDiagnostics.AllowResume = command.State;

                        if (!command.State)
                        {
                            _bus.Publish(new GameReplayPauseInputEvent(false));
                        }
                    }

                    break;
                case RuntimeCondition.StartSystemDrivenAutoPlay:
                    // we only care if the Runtime turned off auto play
                    if (!command.State)
                    {
                        _autoPlayStatusProvider.StopSystemAutoPlay();
                    }

                    break;
                case RuntimeCondition.AllowGameRound:
                    _bus.Publish(new AllowGameRoundChangedEvent(command.State));

                    break;
                case RuntimeCondition.AwaitingPlayerSelection:
                    _bus.Publish(new AwaitingPlayerSelectionChangedEvent(command.State));

                    break;
                case RuntimeCondition.Class2MultipleOutcomeSpins:
                    _bus.Publish(new Class2MultipleOutcomeSpinsChangedEvent(command.State));
                    break;
            }
        }
    }
}