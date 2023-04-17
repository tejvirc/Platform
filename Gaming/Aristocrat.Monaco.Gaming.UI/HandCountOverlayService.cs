namespace Aristocrat.Monaco.Gaming.UI
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using Aristocrat.Cabinet.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Events;
    using Aristocrat.Monaco.Gaming.UI.ViewModels;
    using Aristocrat.Monaco.Gaming.UI.Views.Overlay;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.MVVM;

    public class HandCountOverlayService : IService, IDisposable
    {
        private bool _disposed;
        private IEventBus _eventBus;
        private readonly IPropertiesManager _properties;
        private Timer _maxWinShowTimer;

        private const int MaxWinDialogDisplayTimeMS = 5000;

        private HandCountTimerDialog _timerDialog;
        private HandCountTimerDialogViewModel _timerDialogViewModel;

        private MaxWinDialog _maxWinDialog;
        private MaxWinDialogViewModel _maxWinDialogViewModel;

        private CashoutResetHandCount _cashoutResetHandCountDialog;
        private CashoutResetHandCountViewModel _cashoutResetHandCountViewModel;
        public string Name { get; } = "HandCountOverlayService";

        public ICollection<Type> ServiceTypes => new[] { typeof(IService) };

        public HandCountOverlayService() : this(ServiceManager.GetInstance().GetService<IEventBus>(),
            ServiceManager.GetInstance().TryGetService<IPropertiesManager>())
        { }

        public HandCountOverlayService(IEventBus eventBus, IPropertiesManager properties)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public void Initialize()
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    _timerDialogViewModel = new HandCountTimerDialogViewModel();
                    _timerDialog = new HandCountTimerDialog(_timerDialogViewModel);

                    _maxWinDialogViewModel = new MaxWinDialogViewModel();
                    _maxWinDialog = new MaxWinDialog(_maxWinDialogViewModel);

                    _cashoutResetHandCountViewModel = new CashoutResetHandCountViewModel();
                    _cashoutResetHandCountDialog = new CashoutResetHandCount(_cashoutResetHandCountViewModel);
                });

            _eventBus.Subscribe<HandCountResetTimerStartedEvent>(this, HandleEvent);
            _eventBus.Subscribe<HandCountResetTimerCancelledEvent>(this, HandleEvent);
            _eventBus.Subscribe<HandCountResetTimerElapsedEvent>(this, HandleEvent);

            _eventBus.Subscribe<MaxWinReachedEvent>(this, HandleEvent);

            _eventBus.Subscribe<CashoutAmountPlayerConfirmationRequestedEvent>(this, Handle);
            _eventBus.Subscribe<CashoutAmountPlayerConfirmationReceivedEvent>(this, Handle);
        }

        private void Handle(CashoutAmountPlayerConfirmationReceivedEvent obj)
        {
            _eventBus.Publish(new ViewInjectionEvent(_cashoutResetHandCountDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Remove));
        }

        private void Handle(CashoutAmountPlayerConfirmationRequestedEvent evt)
        {
            _eventBus.Publish(new ViewInjectionEvent(_cashoutResetHandCountDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Add));
        }

        private void HandleEvent(MaxWinReachedEvent obj)
        {
            var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
            var game = _properties.GetValues<IGameDetail>(GamingConstants.Games).SingleOrDefault(g => g.Id == gameId);
            var denomination = game.Denominations.Single(d => d.Value == _properties.GetValue(GamingConstants.SelectedDenom, 0L));
            _maxWinDialogViewModel.MaxWinAmount = (game.ActiveBetOption.MaxWin.Value * denomination.Value).MillicentsToDollars().ToString(CultureInfo.InvariantCulture);

            _eventBus.Publish(new ViewInjectionEvent(_maxWinDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Add));
            _maxWinShowTimer = new Timer(_ =>
            {
                _maxWinShowTimer.Dispose();
                _maxWinShowTimer = null;
                _eventBus.Publish(new ViewInjectionEvent(_maxWinDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Remove));
            }
            , null, MaxWinDialogDisplayTimeMS, Timeout.Infinite);
        }

        private void HandleEvent(HandCountResetTimerElapsedEvent obj)
        {
            _eventBus.Publish(new ViewInjectionEvent(_timerDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Remove));
        }

        private void HandleEvent(HandCountResetTimerCancelledEvent obj)
        {
            _timerDialogViewModel.OnHandCountTimerCancelled();
            _eventBus.Publish(new ViewInjectionEvent(_timerDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Remove));
        }

        private void HandleEvent(HandCountResetTimerStartedEvent obj)
        {
            _eventBus.Publish(new ViewInjectionEvent(_timerDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Add));
            _timerDialogViewModel.OnHandCountTimerStarted();
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
                _timerDialogViewModel.Dispose();
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }
    }
}
