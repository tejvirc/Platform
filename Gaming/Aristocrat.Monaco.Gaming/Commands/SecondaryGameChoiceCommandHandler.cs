namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts;

    public class SecondaryGameChoiceCommandHandler : ICommandHandler<SecondaryGameChoice>
    {
        private readonly IGameHistory _gameHistory;

        public SecondaryGameChoiceCommandHandler(IGameHistory gameHistory)
        {
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
        }

        public void Handle(SecondaryGameChoice command)
        {
            _gameHistory.SecondaryGameChoice();
        }
    }
}
