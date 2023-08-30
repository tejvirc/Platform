namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Contracts;
    using Contracts.Configuration;
    using Contracts.Models;
    using Contracts.Process;
    using Hardware.Contracts.Audio;
    using Kernel;
    using Kernel.Contracts;
    using Localization.Properties;
    using log4net;
    using Newtonsoft.Json;
    using Runtime;

    /// <summary>
    ///     Definition of the Game class.  This is intended to manage the state of a game (entry to exit).
    /// </summary>
    public class GameService : IGameService, IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IAudio _audio;
        private readonly IEventBus _eventBus;
        private readonly IProcessCommunication _ipc;
        private readonly IGameProcess _process;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IGameConfigurationProvider _gameConfiguration;
        private readonly IGameProvider _gameProvider;
        private readonly IGamePackConfigurationProvider _gamePackConfigProvider;
        private bool _disposed;
        private int _processId;
        private bool _running;
        private GameInitRequest _lastRequest;
        private readonly object _sync = new();

        public GameService(
            IEventBus eventBus,
            IGameProcess process,
            IProcessCommunication interProcessCommunication,
            IPropertiesManager propertiesManager,
            IAudio audio,
            IGameConfigurationProvider gameConfiguration,
            IGameProvider gameProvider,
            IGamePackConfigurationProvider gamePackConfigProvider)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _process = process ?? throw new ArgumentNullException(nameof(process));
            _ipc = interProcessCommunication ?? throw new ArgumentNullException(nameof(interProcessCommunication));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
            _gameConfiguration = gameConfiguration ?? throw new ArgumentNullException(nameof(gameConfiguration));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _gamePackConfigProvider =
                gamePackConfigProvider ?? throw new ArgumentNullException(nameof(gamePackConfigProvider));
        }

        public bool Running
        {
            get => _running;

            private set
            {
                if (_running != value)
                {
                    _running = value;
                    _propertiesManager.SetProperty(GamingConstants.IsGameRunning, _running);
                    if (!_lastRequest.IsReplay)
                    {
                        _propertiesManager.SetProperty(GamingConstants.LaunchGameAfterReboot, _running);
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Initialize(GameInitRequest request)
        {
            _lastRequest = request;
            Logger.Info("Ending game process from Initialize");
            _process.EndGameProcess(false);
            _processId = 0;

            if (!request.IsReplay)
            {
                ValidateRequest(request);
            }

            StoreSelectedGame(request);

            _ipc.StartComms();

            _processId = _process.StartGameProcess(request);

            Logger.Info("Game process Started.");
        }

        public void ReInitialize(GameInitRequest request)
        {
            if (_lastRequest != null && Running)
            {
                ValidateRequest(request);
                StoreSelectedGame(request);

                _lastRequest.Denomination = request.Denomination;
                _lastRequest.GameId = request.GameId;
                _lastRequest.BetOption = request.BetOption;
            }
        }

        public void CreateMiniDump()
        {
            _process.CreateMiniDump();
        }

        public void Terminate(int processId)
        {
            Terminate(processId, true);
        }

        public void Terminate(int processId, bool notifyExited)
        {
            InternalTerminate(notifyExited, true, processId);
        }

        public void TerminateAny()
        {
            TerminateAny(true, true);
        }

        public void TerminateAny(bool notifyExited, bool terminateExpected)
        {
            InternalTerminate(notifyExited, terminateExpected);
        }

        public void ShutdownBegin()
        {
            _process.Exiting();

            _eventBus.Publish(new GameShutdownStartedEvent());

            Running = false;
        }

        public void ShutdownEnd()
        {
            if (!Running)
            {
                _ipc.EndComms();
                _eventBus.Publish(new GameShutdownCompletedEvent());
            }
        }

        public void Connected()
        {
            Running = true;
            Logger.Info("Game Connected.");
        }

        public IVolume GetVolumeControl()
        {
            return _audio.GetVolumeControl(_processId);
        }

        public string Name => typeof(GameService).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(IGameService) };

        public void Initialize()
        {
            _eventBus.Subscribe<MediaAlteredEvent>(this, _ =>
            {
                _gameProvider.UpdateGameRuntimeTargets();
            }, evt =>
                evt.MediaType.Equals(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MediaTypeRuntime)) &&
                evt.ReasonForChange.Equals(
                    string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UninstallReason))));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void InternalTerminate(bool notifyExited, bool terminateExpected, int processId = -1)
        {
            Running = false;

            Logger.Info("Ending game process from InternalTerminate");

            if (processId == -1)
            {
                _process.EndGameProcess(notifyExited, terminateExpected);
            }
            else if (_process.IsRunning(processId))
            {
                _process.EndGameProcess(processId, notifyExited, terminateExpected);
            }

            _ipc.EndComms();

            _processId = 0;
            Logger.Info("All game processes and IPC terminated.");
        }

        private void ValidateRequest(GameInitRequest request)
        {
            var combos = _propertiesManager.GetValues<IGameCombo>(GamingConstants.GameCombos);

            if (!combos.Any(c => c.GameId == request.GameId && c.Denomination == request.Denomination && c.BetOption == request.BetOption))
            {
                Logger.Error(
                    $"Unable to find a valid game combo for game id {request.GameId} and a denomination of {request.Denomination}");
                throw new InvalidGameException(
                    $"Failed to launch game id: {request.GameId} with a {request.Denomination} denomination");
            }
        }

        private void StoreSelectedGame(GameInitRequest request)
        {
            // Store the validated selected game
            Logger.Info(
                $"New game selected, replay={request.IsReplay}. Game Id: {request.GameId} with a denom of {request.Denomination}");

            lock (_sync) // TXM-10879 Fixes a race-condition which causes a game to crash
            {
                _propertiesManager.SetProperty(GamingConstants.SelectedGameId, request.GameId);
                _propertiesManager.SetProperty(GamingConstants.SelectedDenom, request.Denomination);
            }
            
            _propertiesManager.SetProperty(GamingConstants.SelectedBetOption, request.BetOption);

            if (request.IsReplay)
            {
                return;
            }

            var currentGame = _gameProvider.GetGame(_propertiesManager.GetValue(GamingConstants.SelectedGameId, 0));
                
            var restrictions = _gameConfiguration.GetActive(currentGame.ThemeId);
            if (restrictions is null)
            {
                return;
            }

            var gameConfigPropValue =
                _gamePackConfigProvider.GetDenomRestrictionsByGameId(currentGame.Id);

            var json = JsonConvert.SerializeObject(gameConfigPropValue, Formatting.None);

            _propertiesManager.SetProperty(GamingConstants.GameConfiguration, json);
        }
    }
}