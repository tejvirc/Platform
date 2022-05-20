namespace Aristocrat.Monaco.Hhr.UI.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using Hardware.Contracts.Button;
    using Kernel;
    using Menu;

    public class HelpPageViewModel : HhrMenuPageViewModelBase
    {
        private readonly IEventBus _eventBus;

        public HelpPageViewModel(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
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
                    OnHhrButtonClicked(Command.BetHelp);
                    break;
            }
        }
    }
}