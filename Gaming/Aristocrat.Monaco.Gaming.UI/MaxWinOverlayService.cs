namespace Aristocrat.Monaco.Gaming.UI
{
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Aristocrat.MVVM;
    using System.Threading;
    using Aristocrat.Monaco.Gaming.UI.Views.Overlay;
    using Aristocrat.Monaco.Gaming.UI.ViewModels;
    using System.Globalization;
    using Aristocrat.Cabinet.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Gaming.Runtime;
    using Aristocrat.Monaco.Gaming.Runtime.Client;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Button;

    public class MaxWinOverlayService : IMaxWinOverlayService, IService, IDisposable
    {
        private Timer _maxWinShowTimer;
        private MaxWinDialog _maxWinDialog;
        private MaxWinDialogViewModel _maxWinDialogViewModel;
        private bool _disposed;
        private IEventBus _eventBus;
        private readonly IPropertiesManager _properties;
        private readonly IBank _bank;

        private const int MaxWinDialogDisplayTimeMS = 5000;

        public string Name { get; } = "MaxWinOverlayService";

        public ICollection<Type> ServiceTypes => new[] { typeof(IService), typeof(IMaxWinOverlayService) };
    

    public bool ShowingMaxWinWarning { get; set;}

        public MaxWinOverlayService() : this(ServiceManager.GetInstance().GetService<IEventBus>(),
            ServiceManager.GetInstance().TryGetService<IPropertiesManager>(),
            ServiceManager.GetInstance().TryGetService<IBank>())
        { }

        public MaxWinOverlayService(IEventBus eventBus, IPropertiesManager properties, IBank bank)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
        }

        public void Initialize()
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    _maxWinDialogViewModel = new MaxWinDialogViewModel();
                    _maxWinDialog = new MaxWinDialog(_maxWinDialogViewModel);
                });
            _eventBus.Subscribe<MaxWinReachedEvent>(this, HandleEvent);
            _eventBus.Subscribe<DownEvent>(this, Handle);
        }

        private void Handle(DownEvent obj)
        {
           if(ShowingMaxWinWarning)
            {
                CloseMaxWinDialog();
            }
        }

        private void HandleEvent(MaxWinReachedEvent obj)
        {
            ShowingMaxWinWarning = true;
            var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
            var game = _properties.GetValues<IGameDetail>(GamingConstants.Games).SingleOrDefault(g => g.Id == gameId);
            var denomination = game.Denominations.Single(d => d.Value == _properties.GetValue(GamingConstants.SelectedDenom, 0L));
            _maxWinDialogViewModel.MaxWinAmount = (game.BetOptionList.FirstOrDefault(x => x.Name == denomination.BetOption)?.MaxWin * denomination.Value)?.MillicentsToDollars().ToString(CultureInfo.InvariantCulture) ?? "";

            _eventBus.Publish(new ViewInjectionEvent(_maxWinDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Add));
            
            _maxWinShowTimer = new Timer(_ =>
            {
                CloseMaxWinDialog();
            }
            , null, MaxWinDialogDisplayTimeMS, Timeout.Infinite);
        }

        private void CloseMaxWinDialog()
        {
            _maxWinShowTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _maxWinShowTimer.Dispose();
            _maxWinShowTimer = null;
            _eventBus.Publish(new ViewInjectionEvent(_maxWinDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Remove));

            var runtime = ServiceManager.GetInstance().GetService<IContainerService>().Container.GetInstance<IRuntime>();
            runtime.UpdateBalance(_bank.QueryBalance().MillicentsToCents());
            runtime.UpdateFlag(RuntimeCondition.AllowGameRound, true);
            ShowingMaxWinWarning = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _maxWinShowTimer?.Dispose();
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }
    }
}
