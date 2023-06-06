namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using System.Reflection;
    using Accounting.Contracts.HandCount;
    using Contracts;
    using Kernel;
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
        private readonly ICashOutAmountCalculator _calculator;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CashOutButtonPressedConsumer" /> class.
        /// </summary>
        /// <param name="playerBank">An <see cref="IPlayerBank" /> instance.</param>
        /// <param name="gamePlayState">An <see cref="IGamePlayState" /> instance.</param>
        public CashOutButtonPressedConsumer(IPlayerBank playerBank, IGamePlayState gamePlayState)
        {
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _playerBank = playerBank ?? throw new ArgumentNullException(nameof(playerBank));

            // Check if hand count calculations are active, and if so, fetch the calculator.
            var handCountService = ServiceManager.GetInstance().TryGetService<IHandCountService>();
            if (handCountService.HandCountServiceEnabled)
            {
                _calculator = ServiceManager.GetInstance().TryGetService<ICashOutAmountCalculator>();
            }
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

            if (_calculator != null)
            {
                var amountCashable = _calculator.GetCashableAmount(_playerBank.Balance);

                if (amountCashable > 0)
                {
                    if (!_playerBank.CashOut(amountCashable))
                    {
                        Logger.Error($"Player bank cashout ({amountCashable}) failed");
                    }
                }
                else
                {
                    // Do something to indicate we couldn't cash out anything?
                }
            }
            else
            {
                if (!_playerBank.CashOut())
                {
                    Logger.Error("Player bank cashout failed");
                }
            }
        }
    }
}