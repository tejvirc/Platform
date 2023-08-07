namespace Aristocrat.Monaco.Hhr.Services
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Events;
    using Gaming.Contracts;
    using Kernel;
    using log4net;

    public class GameSelectionVerificationService : IGameSelectionVerificationService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IGameProvider _gameProvider;
        private readonly IGameDataService _gameDataService;

        private bool _disposedValue;

        private bool _protocolInitializationComplete;

        public GameSelectionVerificationService(
            IEventBus eventBus,
            IGameProvider gameProvider,
            IGameDataService gameDataService)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _gameDataService = gameDataService ?? throw new ArgumentNullException(nameof(gameDataService));

            _eventBus.Subscribe<ProtocolInitializationInProgress>(this, x => _protocolInitializationComplete = false);
            _eventBus.Subscribe<ProtocolInitializationComplete>(this, Handle);
            _eventBus.Subscribe<GameEnabledEvent>(this, async x => await Verify());
            _eventBus.Subscribe<GameDisabledEvent>(this, async x => await Verify());
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task Verify()
        {
            if (!_protocolInitializationComplete) return;

            var gamesOnServer = (await _gameDataService.GetGameInfo()).ToList();

            var invalidSelection = false;

            // From the games that were enabled by the operator, verify they are on the server
            foreach (var game in _gameProvider.GetEnabledGames())
            {
                if (!gamesOnServer.Exists(g => g.GameId.ToString() == game.ReferenceId))
                {
                    invalidSelection = true;
                    Logger.Error(
                        $"Invalid selection - Id: {game.Id}, Var: {game.VariationId}, PayTable: {game.PaytableId}, RefId: {game.ReferenceId}");
                }
            }

            if (invalidSelection)
            {
                Logger.Error(
                    $"Invalid games selected. Valid choices: {string.Join(",", gamesOnServer.Select(g => g.GameId.ToString()).ToArray())}");

                _eventBus.Publish(new GameSelectionMismatchEvent());
            }
            else
            {
                _eventBus.Publish(new GameSelectionVerificationCompletedEvent());
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _eventBus.UnsubscribeAll(this);
                }
                
                _disposedValue = true;
            }
        }

        private async void Handle(ProtocolInitializationComplete theEvent)
        {
            _protocolInitializationComplete = true;

            await Verify();
        }
    }
}