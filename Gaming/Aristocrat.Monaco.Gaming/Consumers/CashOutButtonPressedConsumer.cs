namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Hardware.Contracts.Button;
    using Aristocrat.Monaco.Kernel;
    using Contracts;
    using log4net;

    /// <summary>
    ///     Handles the <see cref="CashOutButtonPressedEvent" />
    /// </summary>
    public class CashOutButtonPressedConsumer : Consumes<CashOutButtonPressedEvent>
    {
        /// <summary>Create a logger for use in this class.</summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IGamePlayState _gamePlayState;
        private readonly IPlayerBank _playerBank;
        private readonly IEventBus _bus;
        private readonly ISystemDisableManager _systemDisableManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CashOutButtonPressedConsumer" /> class.
        /// </summary>
        /// <param name="playerBank">An <see cref="IPlayerBank" /> instance.</param>
        /// <param name="gamePlayState">An <see cref="IGamePlayState" /> instance.</param>
        public CashOutButtonPressedConsumer(
            IPlayerBank playerBank,
            IEventBus bus,
            IGamePlayState gamePlayState,
            ISystemDisableManager systemDisableManager
        )
        {
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _playerBank = playerBank ?? throw new ArgumentNullException(nameof(playerBank));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _systemDisableManager =
                systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
        }

        /// <inheritdoc />
        public override void Consume(CashOutButtonPressedEvent theEvent)
        {
            // If we're not idle we shouldn't be handling this event
            if (!_gamePlayState.Idle && !_gamePlayState.InPresentationIdle)
            {
                Logger.Warn($"Unable to cashout. The game play state is not idle. ({_gamePlayState.CurrentState})");
                return;
            }

            Task.Run(CashOutAsync);
        }

        private async Task CashOutAsync()
        {
            if (_playerBank.Balance > 2000000)
            {
                var keyOff = Initiate();
                await keyOff.Task;

                _systemDisableManager.Enable(ApplicationConstants.LargePayoutDisableKey);
            }

            if (!_playerBank.CashOut())
            {
                Logger.Error("Player bank cashout failed");
            }
        }

        private TaskCompletionSource<object> Initiate()
        {
            var keyOff = new TaskCompletionSource<object>();

            _bus.Subscribe<DownEvent>(
                this,
                _ =>
                {
                    keyOff.TrySetResult(null);
                },
                evt => evt.LogicalId == (int)ButtonLogicalId.Button30);

            _systemDisableManager.Disable(
                ApplicationConstants.LargePayoutDisableKey,
                SystemDisablePriority.Immediate,
                () => "Cashout Amount more than $20, call attendant",
                true,
                () => "Cashout Amount more than $20");

            return keyOff;
        }
    }
}
