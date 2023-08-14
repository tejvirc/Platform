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
    using Aristocrat.Monaco.Gaming.UI.Views.Overlay;
    using Aristocrat.Monaco.Gaming.UI.ViewModels;
    using System.Globalization;
    using Aristocrat.Cabinet.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Gaming.Runtime;
    using Aristocrat.Monaco.Gaming.Runtime.Client;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Button;
    using System.Timers;
    using log4net;
    using System.Reflection;
    using System.Windows;

    public class MaxWinOverlayService : IMaxWinOverlayService, IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const double MaxWinDialogDisplaySeconds = 5.0;
        private const double ResetTimerIntervalSeconds = 1.0;
        private readonly Timer _maxWinShowTimer;
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;
        private readonly IBank _bank;
        private MaxWinDialog _maxWinDialog;
        private MaxWinDialogViewModel _maxWinDialogViewModel;
        private bool _disposed;

        public string Name { get; } = "MaxWinOverlayService";

        public ICollection<Type> ServiceTypes => new[] { typeof(IService), typeof(IMaxWinOverlayService) };

        private TimeSpan MaxWinDialogDispalyTime = TimeSpan.FromSeconds(MaxWinDialogDisplaySeconds);

        private TimeSpan oneSecondElapsed = TimeSpan.FromSeconds(ResetTimerIntervalSeconds);

        private TimeSpan TimeLeft { get; set; }

        public bool ShowingMaxWinWarning { get; set; }

        public MaxWinOverlayService()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().TryGetService<IPropertiesManager>(),
                ServiceManager.GetInstance().TryGetService<IBank>())
        {
        }

        public MaxWinOverlayService(IEventBus eventBus, IPropertiesManager properties, IBank bank)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));

            _maxWinShowTimer = new Timer(oneSecondElapsed.TotalMilliseconds);
            _maxWinShowTimer.Elapsed += resetTimer_Tick;

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
            _eventBus.Subscribe<SystemDisabledEvent>(this, Handle);
            _eventBus.Subscribe<SystemEnabledEvent>(this, Handle);
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
                _maxWinShowTimer.Elapsed -= resetTimer_Tick;
                _maxWinShowTimer.Stop();
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void Handle(SystemEnabledEvent obj)
        {
            if (TimeLeft.TotalSeconds > 0)
            {
                ShowMaxWinDialog();
                _maxWinShowTimer.Start();
            }
        }

        private void Handle(SystemDisabledEvent obj)
        {
            _maxWinShowTimer.Stop();
            CloseMaxWinDialog();
        }

        private void Handle(DownEvent obj)
        {
            if (ShowingMaxWinWarning)
            {
                _maxWinShowTimer.Stop();
                CloseMaxWinDialog();
                Logger.Info($"MaxWin dialog is closed as a result of button press");
                UpdateRuntime();
                TimeLeft = TimeSpan.Zero;
            }
        }

        private void resetTimer_Tick(object sender, EventArgs e)
        {
            TimeLeft = TimeLeft.Subtract(oneSecondElapsed);
            if (TimeLeft.Seconds == 0 && TimeLeft.Minutes == 0)
            {
                _maxWinShowTimer.Stop();
                CloseMaxWinDialog();
                Logger.Info($"MaxWin dialog is closed after 5 seconds");
                UpdateRuntime();
            }
        }

        private void HandleEvent(MaxWinReachedEvent obj)
        {
            ShowingMaxWinWarning = true;
            ShowMaxWinDialog();
            TimeLeft = MaxWinDialogDispalyTime;
            _maxWinShowTimer.Start();
        }

        private void ShowMaxWinDialog()
        {
            var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
            var game = _properties.GetValues<IGameDetail>(GamingConstants.Games).SingleOrDefault(g => g.Id == gameId);
            var denomination = game.Denominations.Single(d => d.Value == _properties.GetValue(GamingConstants.SelectedDenom, 0L));
            MvvmHelper.ExecuteOnUI(
            () =>
            {
                //MaxWin dialog visibility is set to Hidden and then to Visible once the dialog is added to the main view
                //as there is a delay in rendering the maxWin amount value in the dialog
                //Please refer to https://jerry.aristocrat.com/browse/TXM-13316
                _maxWinDialog.Visibility = Visibility.Hidden;
                _maxWinDialogViewModel.MaxWinAmount = (game.BetOptionList.FirstOrDefault(x => x.Name == denomination.BetOption)?.MaxWin * denomination.Value)?.MillicentsToDollars().ToString(CultureInfo.InvariantCulture) ?? "";

                _eventBus.Publish(new ViewInjectionEvent(_maxWinDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Add));
                _maxWinDialog.Visibility = Visibility.Visible;
            });
        }

        private void CloseMaxWinDialog()
        {
            _eventBus.Publish(new ViewInjectionEvent(_maxWinDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Remove));
        }

        private void UpdateRuntime()
        {
            var runtime = ServiceManager.GetInstance().GetService<IContainerService>().Container.GetInstance<IRuntime>();
            runtime.UpdateBalance(_bank.QueryBalance().MillicentsToCents());
            runtime.UpdateFlag(RuntimeCondition.AllowGameRound, true);
            ShowingMaxWinWarning = false;
        }
    }
}
