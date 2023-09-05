﻿namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using Common.PerformanceCounters;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Progressives;
    using Runtime;
    using Runtime.Client;
    using Vgt.Client12.Application.OperatorMenu;

    [CounterDescription("Game Idle", PerformanceCounterType.AverageTimer32)]
    public class GameIdleCommandHandler : ICommandHandler<GameIdle>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _bus;
        private readonly IProgressiveGameProvider _progressiveGameProvider;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly IGameHistory _gameHistory;
        private readonly IGameRecovery _gameRecovery;
        private readonly IOperatorMenuLauncher _operatorMenu;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly ICashoutController _cashoutController;
        private readonly IRuntime _runtime;
        private readonly IMaxWinOverlayService _maxWinOverlayService;

        public GameIdleCommandHandler(
            IGameHistory gameHistory,
            IGameRecovery gameRecovery,
            IRuntime runtime,
            ICommandHandlerFactory commandFactory,
            IOperatorMenuLauncher operatorMenu,
            IPersistentStorageManager persistentStorage,
            ICashoutController cashoutController,
            IEventBus bus,
            IProgressiveGameProvider progressiveGameProvider,
            IMaxWinOverlayService maxWinOverlayService)
        {
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _gameRecovery = gameRecovery ?? throw new ArgumentNullException(nameof(gameRecovery));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _operatorMenu = operatorMenu ?? throw new ArgumentNullException(nameof(operatorMenu));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
            _cashoutController = cashoutController ?? throw new ArgumentNullException(nameof(cashoutController));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _progressiveGameProvider = progressiveGameProvider ?? throw new ArgumentNullException(nameof(progressiveGameProvider));
            _maxWinOverlayService = maxWinOverlayService ?? throw new ArgumentNullException(nameof(maxWinOverlayService));
        }

        public void Handle(GameIdle command)
        {
            using (var scope = _persistentStorage.ScopedTransaction())
            {
                _gameHistory.End(_progressiveGameProvider.GetJackpotSnapshot(string.Empty));

                _gameRecovery.EndRecovery();

                scope.Complete();
            }

            var checkBalance = new CheckBalance();

            _commandFactory.Create<CheckBalance>().Handle(checkBalance);

            // Clear the wager amounts if they were set
            _progressiveGameProvider.SetProgressiveWagerAmounts(new List<long>());
            _operatorMenu.EnableKey(GamingConstants.OperatorMenuDisableKey);

            _bus.Publish(new DisableCountdownTimerEvent(false));

            Logger.Debug($"GameIdle: ForcedCashout={checkBalance.ForcedCashout}, PaperInChuteNotificationActive={_cashoutController.PaperInChuteNotificationActive}, ShowingMaxWinWarning={_maxWinOverlayService.ShowingMaxWinWarning}");
            if (!checkBalance.ForcedCashout && !_cashoutController.PaperInChuteNotificationActive && !_maxWinOverlayService.ShowingMaxWinWarning)
            {
                _runtime.UpdateFlag(RuntimeCondition.AllowGameRound, true);
            }
        }
    }
}