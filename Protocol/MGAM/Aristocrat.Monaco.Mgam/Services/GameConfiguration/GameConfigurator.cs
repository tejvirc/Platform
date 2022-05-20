namespace Aristocrat.Monaco.Mgam.Services.GameConfiguration
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client.Logging;
    using Common;
    using Common.Events;
    using GamePlay;
    using Gaming.Contracts.Events.OperatorMenu;
    using Kernel;

    /// <summary>
    ///     Manages remote configuration of the game.
    /// </summary>
    public class GameConfigurator : IGameConfigurator, IService
    {
        private readonly ILogger _logger;
        private readonly IEventBus _eventBus;
        private readonly IProgressiveController _progressiveController;

        /// <summary>
        ///     Initializes and instance of the <see cref="GameConfigurator"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger{TCategory}"/>.</param>
        /// <param name="eventBus"><see cref="IEventBus"/>.</param>
        /// <param name="progressiveController"><see cref="IProgressiveController"/>.</param>
        public GameConfigurator(
            ILogger<GameConfigurator> logger,
            IEventBus eventBus,
            IProgressiveController progressiveController)
        {
            _logger = logger;
            _eventBus = eventBus;
            _progressiveController = progressiveController;

            SubscribeToEvents();
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IGameConfigurator) };

        /// <inheritdoc />
        public void Initialize()
        {
            //SupportedCommands.Add(
            //    new CommandInfo
            //    {
            //        CommandId = (int)CustomCommandCode.Configure,
            //        Description = "CONFIGURE",
            //        ParameterName = "Use Game Package",
            //        DefaultValue = false,
            //        Type = CommandValueType.Boolean,
            //        ControlType = CommandControlType.CheckBox,
            //        AccessType = CommandAccessType.SiteController | CommandAccessType.Management
            //    });
        }

        public async Task Configure()
        {
            _logger.LogInfo("Configuration requested.");

            // TODO: Configure games

            _progressiveController.Configure();

            _eventBus.Publish(new ForceDisconnectEvent(DisconnectReason.GamesChanged));

            await Task.CompletedTask;
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<GameConfigurationSaveCompleteEvent>(this, Handle);
        }

        private Task Handle(GameConfigurationSaveCompleteEvent evt, CancellationToken cancellationToken)
        {
            // TODO: Lockup for invalid game configuration with appropriate message
            //if (!await ValidateConfiguration(cancellationToken))
            //{
            //}

            _progressiveController.Configure();

            _eventBus.Publish(new ForceDisconnectEvent(DisconnectReason.GamesChanged));

            return Task.CompletedTask;
        }

        // TODO: Get clarification on how games are configured and validated in NYL. Commenting out below code for now.
        //private async Task<bool> ValidateConfiguration(CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (!cancellationToken.IsCancellationRequested)
        //        {
        //            return true;
        //        }

        //        var assignments = await GetGameAssignments(cancellationToken);

        //        var configurations = _gameProvider
        //            .GetConfiguredGames()
        //            .SelectMany(
        //                g => g.WagerCategories,
        //                (g, c) => new
        //                {
        //                    GameId = g.Id,
        //                    UpcNumber = g.ProductCode,
        //                    PayTableIndex = int.Parse(c.Id),
        //                    Denomination = g.Denominations.First().Value.MillicentsToCents()
        //                });

        //        var valid = true;

        //        foreach (var c in configurations)
        //        {
        //            if (!assignments.Any(
        //                a => c.UpcNumber == a.UpcNumber && c.PayTableIndex == a.PayTableIndex &&
        //                     c.Denomination == a.Denomination))
        //            {
        //                _logger.LogError(
        //                    "Invalid game configuration, Game ID: {0}, Pay Table Index: {1}, Denomination: {2}",
        //                    c.GameId,
        //                    c.PayTableIndex,
        //                    c.Denomination);
        //                valid = false;
        //            }
        //        }

        //        return valid;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Validating game configuration failed");
        //        return false;
        //    }
        //}

        //private async Task<IReadOnlyList<GameAssignment>> GetGameAssignments(CancellationToken cancellationToken)
        //{
        //    return await _egm.GetService<IRegistration>().GetGameAssignments(cancellationToken);
        //}
    }
}
