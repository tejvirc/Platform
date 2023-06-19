namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Accounting.Contracts;
    using Contracts.OperatorMenu;
    using Contracts;
    using Contracts.Localization;
    using Contracts.MeterPage;
    using Kernel;
    using Monaco.Localization.Properties;
    using MVVM.Command;
    using OperatorMenu;
    using Vgt.Client12.Application.OperatorMenu;

    [CLSCompliant(false)]
    public class MetersMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string PagesExtensionPath = "/Application/OperatorMenu/MetersMenu";

        private static readonly AutoResetEvent Refresh = new AutoResetEvent(true);

        private readonly object _lock = new object();

        private string _currentPageHeader;

        private bool _isPeriodMasterButtonChecked = true;

        private readonly string _dateTimeFormat;

        private readonly ConcurrentDictionary<string, DateTime> _pageNameToPersonalizedPeriodClearDateTimeMap =
            new ConcurrentDictionary<string, DateTime>();

        public MetersMainPageViewModel(IOperatorMenuPageLoader mainPage)
            : base(mainPage, PagesExtensionPath)
        {
            IsVisibleChangedCommand = new ActionCommand<Page>(OnIsVisibleChanged);
            PeriodMasterButtonClickedCommand = new ActionCommand<object>(PeriodOrMasterButtonClicked);
            var dateFormat = PropertiesManager.GetValue(
                ApplicationConstants.LocalizationOperatorDateFormat,
                ApplicationConstants.DefaultDateTimeFormat);
            _dateTimeFormat = $"{dateFormat} {ApplicationConstants.DefaultTimeFormat}";
        }

        public override int MinButtonWidth => 150;

        public ICommand IsVisibleChangedCommand { get; set; }

        public ICommand PeriodMasterButtonClickedCommand { get; set; }

        /// <summary>
        ///     True if Master, False if Period
        /// </summary>
        public bool IsPeriodMasterButtonChecked
        {
            get => _isPeriodMasterButtonChecked;
            set
            {
                if (_isPeriodMasterButtonChecked == value)
                {
                    return;
                }

                _isPeriodMasterButtonChecked = value;
                RaisePropertyChanged(nameof(IsPeriodMasterButtonChecked));
            }
        }

        public string CurrentPageHeader
        {
            get => _currentPageHeader;
            set
            {
                _currentPageHeader = value;
                RaisePropertyChanged(nameof(CurrentPageHeader));
            }
        }

        /// <summary>
        ///     Dispose of managed objects used by this class
        /// </summary>
        protected override void DisposeInternal()
        {
            PropertyChanged -= OnPropertyChanged;

            base.DisposeInternal();
        }

        protected override void InitializeData()
        {
            PropertyChanged += OnPropertyChanged;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            EventBus.Publish(new MetersOperatorMenuEnteredEvent());

            SubscribeToEvents();
            ShowClearTime();
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();
            EventBus.Publish(new MetersOperatorMenuExitedEvent());
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedPage))
            {
                SetButtonsEnabled(true);
                PropertyChanged -= OnPropertyChanged;
            }
        }

        /// <summary>
        ///     Displays the last clear time of master/period meters on the proper thread.
        /// </summary>
        private void ShowClearTime()
        {
            // this method might be called from a different thread.
            // check if we're allowed to access the control on this thread.
            if (!Dispatcher.CurrentDispatcher.CheckAccess())
            {
                // call display method in proper thread
                Dispatcher.CurrentDispatcher.BeginInvoke(
                    new Action(DisplayClearTime),
                    null);
                return;
            }

            DisplayClearTime();
        }

        /// <summary>
        ///     Displays the last clear time of master/period meters.
        /// </summary>
        private void DisplayClearTime()
        {
            if (!Pages.Any())
            {
                return;
            }

            var meterManager = ServiceManager.GetInstance().GetService<IMeterManager>();

            // The current culture will be used to display the date/time.
            var time = ServiceManager.GetInstance().GetService<ITime>();
            DateTime dateTime;
            string currentMeterText;

            if (IsPeriodMasterButtonChecked)
            {
                dateTime = TimeZoneInfo.ConvertTime(meterManager.LastMasterClear, time.TimeZoneInformation);
                currentMeterText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MasterMeters);
            }
            else
            {
                dateTime =
                    _pageNameToPersonalizedPeriodClearDateTimeMap.TryGetValue(
                        SelectedPage.PageName,
                        out var lastPeriodicClearForSubPage)
                        ? TimeZoneInfo.ConvertTime(lastPeriodicClearForSubPage, time.TimeZoneInformation)
                        : TimeZoneInfo.ConvertTime(meterManager.LastPeriodClear, time.TimeZoneInformation);
                currentMeterText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PeriodMeters);
            }

            // VLT-4526 : Meter reset time needs updating on changing timezones
            string formattedDateTime = time.GetFormattedLocationTime(dateTime, _dateTimeFormat);

            CurrentPageHeader =
                $"{currentMeterText} {Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Since)}: {formattedDateTime}";
        }

        /// <summary>
        ///     Passes a period-or-master button click to the active page
        /// </summary>
        private void PeriodOrMasterButtonClicked(object obj)
        {
            lock (_lock)
            {
                Logger.Debug("Period/Master btn clicked");

                SetPageTitle();

                ShowClearTime();

                EventBus.Publish(new PeriodOrMasterButtonClickedEvent(IsPeriodMasterButtonChecked));
            }

            Refresh.Set();
        }

        private void RefreshPage()
        {
            ShowClearTime();
            Refresh.Set();
        }

        // VLT-12225
        // Handles an event fired when a specific meter page is loaded (switching tabs) so we can
        // synchronize the ShowLifetime and Master/period button status by re-firing Click event
        protected override void PageLoaded()
        {
            EventBus.Publish(new PeriodOrMasterButtonClickedEvent(IsPeriodMasterButtonChecked));
            RefreshPage();
        }

        protected override void SetPageTitle()
        {
            var periodTitle = IsPeriodMasterButtonChecked
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MasterButtonText)
                : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PeriodText);

            EventBus.Publish(
                new PageTitleEvent(
                    $"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MetersScreen)} - {SelectedPage.PageName} - {periodTitle}"));
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<PeriodMetersClearedEvent>(this, HandlePeriodMetersClearedEvent);
            EventBus.Subscribe<PeriodMetersDateTimeChangeRequestEvent>(
                this,
                HandlePeriodMetersDateTimeChangeRequestEvent);
            EventBus.Subscribe<OperatorCultureChangedEvent>(this, HandleOperatorCultureChanged);
        }

        private void HandlePeriodMetersClearedEvent(PeriodMetersClearedEvent evt)
        {
            RefreshPage();
        }

        private void HandlePeriodMetersDateTimeChangeRequestEvent(PeriodMetersDateTimeChangeRequestEvent evt)
        {
            _pageNameToPersonalizedPeriodClearDateTimeMap.AddOrUpdate(
                evt.PageName,
                evt.PeriodicClearDateTime,
                (_, _) => evt.PeriodicClearDateTime);
            RefreshPage();
        }

        private void HandleOperatorCultureChanged(OperatorCultureChangedEvent @event)
        {
            SetPageTitle();
            RefreshPage();
        }

        private void OnIsVisibleChanged(Page page)
        {
            if (page.IsVisible)
            {
                RefreshPage();
            }
        }
    }
}