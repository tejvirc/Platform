namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using System.Reflection;
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="CashOutButtonPressedConsumer" /> class.
        /// </summary>
        /// <param name="playerBank">An <see cref="IPlayerBank" /> instance.</param>
        /// <param name="gamePlayState">An <see cref="IGamePlayState" /> instance.</param>
        public CashOutButtonPressedConsumer(IPlayerBank playerBank, IGamePlayState gamePlayState)
        {
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _playerBank = playerBank ?? throw new ArgumentNullException(nameof(playerBank));
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

            if (!_playerBank.CashOut())
            {
                Logger.Error("Player bank cashout failed");
            }
        }
    }
}
