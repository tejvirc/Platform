namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Barkeeper;
    using Contracts.Meters;
    using Kernel;

    public class WagerCommandHandler : ICommandHandler<Wager>
    {
        private readonly IPlayerBank _bank;
        private readonly IBarkeeperHandler _barkeeperHandler;
        private readonly IGameMeterManager _gameMeters;
        private readonly IPropertiesManager _properties;

        public WagerCommandHandler(
            IPlayerBank bank,
            IGameMeterManager gameMeters,
            IPropertiesManager properties,
            IBarkeeperHandler barkeeperHandler)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _gameMeters = gameMeters ?? throw new ArgumentNullException(nameof(gameMeters));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _barkeeperHandler = barkeeperHandler ?? throw new ArgumentNullException(nameof(barkeeperHandler));
        }

        public void Handle(Wager command)
        {
            _bank.Wager(command.Amount);

            var wageredAmount = command.Amount.CentsToMillicents();

            _gameMeters.GetMeter(command.GameId, command.Denomination, GamingMeters.WageredAmount)
                .Increment(wageredAmount);

            _barkeeperHandler.CreditsWagered(wageredAmount);

            var wagerCategory = _properties.GetValue<IWagerCategory>(GamingConstants.SelectedWagerCategory, null);
            if (wagerCategory != null)
            {
                _gameMeters.GetMeter(
                        command.GameId,
                        command.Denomination,
                        wagerCategory.Id,
                        GamingMeters.WagerCategoryWageredAmount)
                    .Increment(wageredAmount);
            }
        }
    }
}