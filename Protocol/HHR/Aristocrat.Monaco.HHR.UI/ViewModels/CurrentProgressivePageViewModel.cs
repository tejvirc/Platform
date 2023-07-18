namespace Aristocrat.Monaco.Hhr.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using Application.Contracts.Extensions;
    using Accounting.Contracts;
    using Gaming.Contracts.Progressives;
    using Hardware.Contracts.Button;
    using Kernel;
    using Menu;
    using Models;
    using System.Threading.Tasks;

    public class CurrentProgressivePageViewModel : HhrMenuPageViewModelBase
    {
        private readonly IEventBus _bus;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;

        // Used to guard against multiple threads trying to update the progressive info.
        private readonly SemaphoreSlim _updateLock = new SemaphoreSlim(1);

        private bool _disposed;

        public CurrentProgressivePageViewModel(IEventBus bus, IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter)
        {
            ProgressivePools = new ObservableCollection<ProgressivePoolModel>();

            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));

            ShowFooterText = false;
        }

        public ObservableCollection<ProgressivePoolModel> ProgressivePools { get; set; }

        public override Task Init(Command command)
        {
            if (command == Command.CurrentProgressiveMoneyIn)
            {
                _bus.Subscribe<TransferOutCompletedEvent>(this, _ => OnHhrButtonClicked(Command.ReturnToGame));
                Commands.Add(new HhrPageCommand(PageCommandHandler, true, Command.PlayNow));
            }
            else
            {
                Commands.Add(new HhrPageCommand(PageCommandHandler, true, Command.Bet));
                Commands.Add(new HhrPageCommand(PageCommandHandler, true, Command.ExitHelp));
                Commands.Add(new HhrPageCommand(PageCommandHandler, true, Command.Next));
            }

            Application.Current.Dispatcher.Invoke(UpdateProgressiveInfo);

            return Task.CompletedTask;
        }

        public override void Reset()
        {
            base.Reset();
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    ProgressivePools.Clear();
                });

            _bus.Unsubscribe<TransferOutCompletedEvent>(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _bus.UnsubscribeAll(this);
                _updateLock.Dispose();
            }

            _disposed = true;

            base.Dispose(disposing);
        }

        private void PageCommandHandler(object command)
        {
            var cmd = (Command)command;

            switch ((Command)command)
            {
                case Command.BetDown:
                    if (HostPageViewModelManager.BetButtonDelayLimit())
                    {
                        _bus.Publish(new DownEvent((int)ButtonLogicalId.BetDown));
                        OnHhrButtonClicked(Command.BetDown);
                    }
                    break;
                case Command.BetUp:
                    if (HostPageViewModelManager.BetButtonDelayLimit())
                    {
                        _bus.Publish(new DownEvent((int)ButtonLogicalId.BetUp));
                        OnHhrButtonClicked(Command.BetUp);
                    }
                    break;
                case Command.Next:
                    cmd = Command.WinningCombination;
                    break;
            }

            OnHhrButtonClicked(cmd);
        }

        private void UpdateProgressiveInfo()
        {
            _updateLock.WaitAsync();

            try
            {
                var progressives = _protocolLinkedProgressiveAdapter.GetActiveProgressiveLevels().ToList();

                if (!progressives.Any())
                {
                    Logger.Error("Could not fetch any progressives moving to WinningCombinations Page");
                    OnHhrButtonClicked(Command.WinningCombination);
                    return;
                }

                var progressiveGroups = progressives.GroupBy(progressive => progressive.WagerCredits);

                ProgressivePools.Clear();

                foreach (var progressiveGroup in progressiveGroups)
                {
                    var levels = progressiveGroup.OrderBy(p => p.LevelId)
                        .Select(prog => (double)prog.CurrentValue.MillicentsToDollars()).ToList();

                    var pool = new ProgressivePoolModel { Bet = (int)progressiveGroup.Key, CurrentAmount = levels };

                    ProgressivePools.Add(pool);
                }

            }
            catch (Exception ex)
            {
                Logger.Error("Failed to update progressive info.", ex);
            }
            finally
            {
                _updateLock.Release();
            }
        }
    }
}
