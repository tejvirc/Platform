namespace Aristocrat.Monaco.Hhr.UI.ViewModels
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Hardware.Contracts.Button;
    using Kernel;
    using Menu;

    public class BetHelpPageViewModel : HhrMenuPageViewModelBase
    {
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;

        public BetHelpPageViewModel(
            IEventBus eventBus,
            IPropertiesManager propertiesManager,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ??
                                                throw new ArgumentNullException(
                                                    nameof(protocolLinkedProgressiveAdapter));

            ShowFooterText = false;
        }

        public override Task Init(Command command)
        {
            Commands.Add(new HhrPageCommand(PageCommandHandler, true, Command.Bet));
            Commands.Add(new HhrPageCommand(PageCommandHandler, true, Command.ExitHelp));
            Commands.Add(new HhrPageCommand(PageCommandHandler, true, Command.Next));

            return Task.CompletedTask;
        }



        private void PageCommandHandler(object command)
        {
            switch ((Command)command)
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
                case Command.ExitHelp:
                    OnHhrButtonClicked(Command.ExitHelp);
                    break;
                case Command.Next:
                    var cmd = CanShowProgressivePage()
                        ? Command.CurrentProgressive: Command.WinningCombination;
                    OnHhrButtonClicked(cmd);
                    break;
            }
        }

        private bool CanShowProgressivePage()
        {
            var progressivePoolCreationType = (ProgressivePoolCreation)_propertiesManager.GetProperty(
                GamingConstants.ProgressivePoolCreationType,
                ProgressivePoolCreation.Default);

            if (progressivePoolCreationType != ProgressivePoolCreation.WagerBased)
            {
                return false;
            }

            var creationType = _protocolLinkedProgressiveAdapter?.GetActiveProgressiveLevels()?.FirstOrDefault()?.CreationType ??
                               LevelCreationType.Default;

            return creationType != LevelCreationType.Default;
        }
    }
}