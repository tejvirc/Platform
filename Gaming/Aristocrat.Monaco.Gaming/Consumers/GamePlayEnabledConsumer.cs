namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts;

    /// <summary>
    ///     Handles the GamePlayEnabledEvent
    /// </summary>
    public class GamePlayEnabledConsumer : Consumes<GamePlayEnabledEvent>
    {
        private readonly IGameRecovery _gameRecovery;
        private readonly IGameCashOutRecovery _gameCashOutRecovery;
        private bool _recoveryOnStartup;

        public GamePlayEnabledConsumer(
            IGameHistory gameHistory,
            IGameRecovery gameRecovery,
            IGameCashOutRecovery gameCashOutRecovery)
        {
            if (gameHistory == null)
            {
                throw new ArgumentNullException(nameof(gameHistory));
            }

            _gameRecovery = gameRecovery ?? throw new ArgumentNullException(nameof(gameRecovery));
            _gameCashOutRecovery = gameCashOutRecovery ?? throw new ArgumentNullException((nameof(gameCashOutRecovery)));

            _recoveryOnStartup = gameHistory.IsRecoveryNeeded;
        }

        /// <inheritdoc />
        public override void Consume(GamePlayEnabledEvent theEvent)
        {
            if (!_recoveryOnStartup && !_gameRecovery.IsRecovering)
            {
                _gameCashOutRecovery.Recover();
            }

            _recoveryOnStartup = false;
        }
    }
}
