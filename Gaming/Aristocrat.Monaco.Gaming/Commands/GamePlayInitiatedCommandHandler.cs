namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Diagnostics;
    using Common.PerformanceCounters;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Vgt.Client12.Application.OperatorMenu;

    [CounterDescription("Game Play Initiated", PerformanceCounterType.AverageTimer32)]
    public class GamePlayInitiatedCommandHandler : ICommandHandler<GamePlayInitiated>
    {
        private readonly IPlayerBank _bank;
        private readonly IPropertiesManager _properties;
        private readonly IOperatorMenuLauncher _operatorMenu;
        private readonly IPersistentStorageManager _persistentStorage;

        public GamePlayInitiatedCommandHandler(
            IPlayerBank bank,
            IPropertiesManager properties,
            IOperatorMenuLauncher operatorMenu,
            IPersistentStorageManager persistentStorage)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _operatorMenu = operatorMenu ?? throw new ArgumentNullException(nameof(operatorMenu));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
        }

        public void Handle(GamePlayInitiated command)
        {
            using (var scope = _persistentStorage.ScopedTransaction())
            {
                command.Success = _bank.Lock();
                if (!command.Success)
                {
                    return;
                }

                scope.Complete();
            }

            if (_properties.GetValue(GamingConstants.OperatorMenuDisableDuringGame, false))
            {
                _operatorMenu.DisableKey(GamingConstants.OperatorMenuDisableKey);
            }
        }
    }
}
