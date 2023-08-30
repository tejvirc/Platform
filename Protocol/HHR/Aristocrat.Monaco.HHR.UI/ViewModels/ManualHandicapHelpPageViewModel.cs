namespace Aristocrat.Monaco.Hhr.UI.ViewModels
{
    using System;
    using Menu;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Gaming.Contracts;
    using Hardware.Contracts.Button;
    using Kernel;

    public class ManualHandicapHelpPageViewModel : HhrMenuPageViewModelBase
    {
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;
        private readonly IGameProvider _gameProvider;
        private readonly IBank _bank;

        public ManualHandicapHelpPageViewModel(
            IEventBus eventBus,
            IBank bank,
            IGameProvider gameProvider,
            IPropertiesManager properties)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public override Task Init(Command command)
        {
            Commands.Add(new HhrPageCommand(PageCommandHandler, true, Command.Bet));
            Commands.Add(new HhrPageCommand(PageCommandHandler, true, Command.ManualHandicap));
            Commands.Add(new HhrPageCommand(PageCommandHandler, true, Command.Help));
            Commands.Add(new HhrPageCommand(PageCommandHandler, true, Command.ReturnToGame));

            return Task.CompletedTask;
        }


        private void PageCommandHandler(object obj)
        {
            switch ((Command)obj)
            {
                case Command.BetDown:
                    if (HostPageViewModelManager.BetButtonDelayLimit())
                    {
                        _eventBus.Publish(new DownEvent((int)ButtonLogicalId.BetDown));
                        OnHhrButtonClicked(Command.BetDown);
                    }
                    break;
                case Command.BetUp:
                    if (HostPageViewModelManager.BetButtonDelayLimit())
                    {
                        _eventBus.Publish(new DownEvent((int)ButtonLogicalId.BetUp));
                        OnHhrButtonClicked(Command.BetUp);
                    }
                    break;
                case Command.ManualHandicap:
                    if (SufficientCredits())
                    {
                        OnHhrButtonClicked(Command.ManualHandicap);
                    }
                    else
                    {
                        OnHhrButtonClicked(Command.ReturnToGame);
                    }

                    break;
                case Command.Help:
                    OnHhrButtonClicked(Command.Help);
                    break;
                case Command.ReturnToGame:
                    OnHhrButtonClicked(Command.ReturnToGame);
                    break;
            }
        }

        public bool SufficientCredits()
        {
            var (currentGame, currentDenom) = _gameProvider.GetActiveGame();

            if (currentGame != null && currentDenom != null)
            {
                var credits = _bank.QueryBalance() / currentDenom.Value;
                var selectedBet = _properties.GetValue(GamingConstants.SelectedBetCredits, 0L);
                return credits >= selectedBet;
            }
            
            Logger.Error("Could not fetch the current game and/or denom");

            return false;
        }

    }
}