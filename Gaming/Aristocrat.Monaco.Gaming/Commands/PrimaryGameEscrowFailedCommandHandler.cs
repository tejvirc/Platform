namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Progressives;
    using Runtime;
    using Runtime.Client;
    using Vgt.Client12.Application.OperatorMenu;

    public class PrimaryGameEscrowFailedCommandHandler : ICommandHandler<PrimaryGameEscrowFailed>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IGameHistory _gameHistory;
        private readonly IPlayerBank _bank;
        private readonly IRuntime _runtime;
        private readonly IGameRecovery _gameRecovery;
        private readonly IPersistentStorageManager _storage;
        private readonly IOperatorMenuLauncher _operatorMenu;
        private readonly IProgressiveGameProvider _progressiveGameProvider;
        private readonly IPropertiesManager _properties;

        public PrimaryGameEscrowFailedCommandHandler(
            IGameHistory gameHistory,
            IPlayerBank bank,
            IRuntime runtime,
            IGameRecovery gameRecovery,
            IPersistentStorageManager storage,
            IOperatorMenuLauncher operatorMenu,
            IProgressiveGameProvider progressiveGameProvider,
            IPropertiesManager properties)
        {
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _gameRecovery = gameRecovery ?? throw new ArgumentNullException(nameof(gameRecovery));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _operatorMenu = operatorMenu ?? throw new ArgumentNullException(nameof(operatorMenu));
            _progressiveGameProvider = progressiveGameProvider ??
                                       throw new ArgumentNullException(nameof(progressiveGameProvider));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public void Handle(PrimaryGameEscrowFailed command)
        {
            using (var scope = _storage.ScopedTransaction())
            {
                _gameHistory.Fail();

                _bank.Unlock();

                _gameRecovery.EndRecovery();

                scope.Complete();
            }

            _progressiveGameProvider.SetProgressiveWagerAmounts(new List<long>());
            _operatorMenu.EnableKey(GamingConstants.OperatorMenuDisableKey);

            var (game, _) = _properties.GetActiveGame();
            Logger.Debug($"PrimaryGameEscrowFailed: ActiveGame={game}");
            if (game != null)
            {
                _runtime.UpdateFlag(RuntimeCondition.AllowGameRound, true);
            }
        }
    }
}