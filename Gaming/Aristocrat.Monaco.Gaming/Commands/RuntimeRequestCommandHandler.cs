namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts.Media;
    using Common.PerformanceCounters;
    using Contracts;
    using Hardware.Contracts.Bell;
    using Kernel;
    using log4net;
    using Runtime.Client;

    [CounterDescription("Runtime Request", PerformanceCounterType.AverageTimer32)]
    public class RuntimeRequestCommandHandler : ICommandHandler<RuntimeRequest>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IBell _bell;
        private readonly IEventBus _eventBus;
        private readonly IGameRecovery _gameRecovery;
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly IPropertiesManager _properties;
        private readonly IMediaProvider _mediaProvider;
        private readonly IMediaPlayerResizeManager _resizeManager;
        private readonly IResponsibleGaming _responsibleGaming;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly ITransactionCoordinator _transactions;

        public RuntimeRequestCommandHandler(
            IBell bell,
            IEventBus eventBus,
            IResponsibleGaming responsibleGaming,
            IGameRecovery gameRecovery,
            IGameDiagnostics gameDiagnostics,
            IPropertiesManager properties,
            ISystemDisableManager systemDisableManager,
            IMediaPlayerResizeManager resizeManager,
            IMediaProvider mediaProvider,
            ITransactionCoordinator transactions)
        {
            _bell = bell ?? throw new ArgumentNullException(nameof(bell));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _responsibleGaming = responsibleGaming ?? throw new ArgumentNullException(nameof(responsibleGaming));
            _gameRecovery = gameRecovery ?? throw new ArgumentNullException(nameof(gameRecovery));
            _gameDiagnostics = gameDiagnostics ?? throw new ArgumentNullException(nameof(gameDiagnostics));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _systemDisableManager = systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _resizeManager = resizeManager ?? throw new ArgumentNullException(nameof(resizeManager));
            _mediaProvider = mediaProvider ?? throw new ArgumentNullException(nameof(mediaProvider));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
        }

        public void Handle(RuntimeRequest command)
        {
            switch (command.State)
            {
                case RuntimeRequestState.BeginGameRound:
                    if (_gameRecovery.IsRecovering || _gameDiagnostics.IsActive)
                    {
                        command.Result = true;
                    }
                    else
                    {
                        command.Result = IsPlayable();
                    }

                    if (command.Result)
                    {
                        _responsibleGaming.EngageSpinGuard();
                    }
                    break;
                case RuntimeRequestState.BeginAttract:
                case RuntimeRequestState.BeginLobby:
                    if (_gameRecovery.IsRecovering || _gameDiagnostics.IsActive)
                    {
                        command.Result = false;
                    }
                    else
                    {
                        command.Result = IsPlayable() && _properties.GetValue("Automation.HandleExitToLobby", true);
                    }

                    if (command.Result)
                    {
                        _eventBus.Publish(
                            new GameRequestedLobbyEvent(command.State == RuntimeRequestState.BeginAttract));
                    }
                    break;
                case RuntimeRequestState.BeginPlatformHelp:
                    command.Result = !_gameRecovery.IsRecovering && !_gameDiagnostics.IsActive;

                    if (command.Result)
                    {
                        _eventBus.Publish(new GameRequestedPlatformHelpEvent(true));
                    }
                    break;
                case RuntimeRequestState.EndPlatformHelp:
                    command.Result = !_gameRecovery.IsRecovering && !_gameDiagnostics.IsActive;

                    if (command.Result)
                    {
                        _eventBus.Publish(new GameRequestedPlatformHelpEvent(false));
                    }
                    break;
                case RuntimeRequestState.BeginCelebratoryNoise:
                    command.Result = !_gameRecovery.IsRecovering && !_gameDiagnostics.IsActive;

                    if (command.Result)
                    {
                        Task.Run(() => _bell.RingBell());
                    }
                    break;
                case RuntimeRequestState.EndCelebratoryNoise:
                    command.Result = true;

                    Task.Run(() => _bell.StopBell());

                    break;
                case RuntimeRequestState.BeginGameAttract:
                case RuntimeRequestState.EndGameAttract:
                    command.Result = true;
                    _eventBus.Publish(new GameDrivenAttractEvent(command.State == RuntimeRequestState.BeginGameAttract));
                    break;
                default:
                    // This is essentially undefined
                    command.Result = true;
                    break;
            }

            Logger.Debug($"Handled RuntimeRequest {command.State}: result {command.Result}");
        }

        private bool IsPlayable()
        {
            return !_transactions.IsTransactionActive &&
                   !_systemDisableManager.DisableImmediately &&
                   !_resizeManager.IsResizing &&
                   !_mediaProvider.IsPrimaryOverlayVisible &&
                   _responsibleGaming.CanSpinReels();
        }
    }
}
