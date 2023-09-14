namespace Aristocrat.Monaco.Accounting.UI.ViewModels
{
    using System;
    using System.Windows.Input;
    using Accounting.Contracts.Hopper;
    using Accounting.Contracts.Transactions;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.HardwareDiagnostics;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.ViewModels;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Hopper;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Monaco.Localization.Properties;
    using MVVM.Command;
    using Views;

    /// <summary>
    ///     A HopperViewModel contains the logic for HopperPageViewModel.xaml.cs
    /// </summary>
    /// <seealso cref="DeviceViewModel" />
    [CLSCompliant(false)]
    public class HopperPageViewModel : DeviceViewModel
    {
        private readonly IMeterManager _meterManager;
        private readonly ITime _time;
        private long _initialHopperLevel;
        private decimal _voucherOutLimit;
        private const int ThresholdMultiple = 10;
        private readonly decimal _maxHopperRefillValue;
        private readonly decimal _minHopperRefillValue;
        private readonly decimal _minHopperCollectLimit = 0;
        private readonly decimal _maxHopperCollectLimit = 0;
        private readonly decimal _defaultHopperCollectLimit = 0;
        private readonly decimal _minHopperTicketThreshold = 0;
        private decimal _maxHopperTicketThreshold = 0;
        private readonly decimal _defaultHopperTicketThreshold = 0;

        private string _timeOfLastRefill;
        private string _valueOfLastRefill;
        private long _refillCount;
        private string _hopperRefill;
        private string _hopperLevel;
        private string _currentRefillValue;
        private decimal _hopperRefillValue;
        private bool _applyCommandEnabled;
        private bool _hopperTicketSplitSupported;
        private bool _hopperTicketSplitConfigurable;
        private bool _showHopperCollectLimit;
        private bool _hopperRefillWarningEnabled;
        private bool _hopperRefillEnabled;
        private bool _hopperTicketSplitEnabled;
        private bool _showHopperTicketSplit;
        private decimal _hopperTicketThreshold;
        private bool _showHopperTicketThreshold;
        private decimal _hopperCollectLimit;
        public string TimeOfLastRefill
        {
            get => _timeOfLastRefill;
            set => SetProperty(ref _timeOfLastRefill, value, nameof(TimeOfLastRefill));
        }
        public string ValueOfLastRefill
        {
            get => _valueOfLastRefill;
            set => SetProperty(ref _valueOfLastRefill, value, nameof(ValueOfLastRefill));
        }
        public long RefillCount
        {
            get => _refillCount;
            set => SetProperty(ref _refillCount, value, nameof(RefillCount));
        }
        public string HopperRefill
        {
            get => _hopperRefill;
            set => SetProperty(ref _hopperRefill, value, nameof(HopperRefill));
        }
        public string HopperLevel
        {
            get => _hopperLevel;
            set => SetProperty(ref _hopperLevel, value, nameof(HopperLevel));
        }
        public string CurrentRefillValue
        {
            get => _currentRefillValue;
            set => SetProperty(ref _currentRefillValue, value, nameof(CurrentRefillValue));
        }
        public decimal HopperRefillValue
        {
            get => _hopperRefillValue;
            set
            {
                _hopperRefillValue = value;
                ValidateFields(nameof(HopperRefillValue));
                RaisePropertyChanged(nameof(HopperRefillValue));
            }
        }
        public bool ApplyCommandEnabled
        {
            get => _applyCommandEnabled;
            set => SetProperty(ref _applyCommandEnabled, value, nameof(ApplyCommandEnabled));
        }
        public bool HopperTicketSplitSupported
        {
            get => _hopperTicketSplitSupported;
            set => SetProperty(ref _hopperTicketSplitSupported, value, nameof(HopperTicketSplitSupported));
        }
        public bool HopperTicketSplitConfigurable
        {
            get => _hopperTicketSplitConfigurable;
            set => SetProperty(ref _hopperTicketSplitConfigurable, value, nameof(HopperTicketSplitConfigurable));
        }

        public bool HopperCollectVisibility => HopperTicketSplitSupported && (HopperTicketSplitConfigurable || !HopperTicketSplitEnabled);

        public bool HopperThresholdVisibility => HopperTicketSplitSupported && (HopperTicketSplitConfigurable || HopperTicketSplitEnabled);

        public decimal HopperCollectLimit
        {
            get => _hopperCollectLimit;
            set
            {
                _hopperCollectLimit = value;
                ValidateFields(nameof(HopperCollectLimit));
                RaisePropertyChanged(nameof(HopperCollectLimit));
            }
        }
        public bool ShowHopperCollectLimit
        {
            get => _showHopperCollectLimit;
            set => SetProperty(ref _showHopperCollectLimit, value, nameof(ShowHopperCollectLimit));
        }

        public bool HopperRefillWarningEnabled
        {
            get => _hopperRefillWarningEnabled;
            set => SetProperty(ref _hopperRefillWarningEnabled, value, nameof(HopperRefillWarningEnabled));
        }

        public bool HopperRefillEnabled
        {
            get => _hopperRefillEnabled;
            set => SetProperty(ref _hopperRefillEnabled, value, nameof(HopperRefillEnabled));
        }

        public string HopperRefillWarningText => Resources.HopperEmptyText;

        public bool HopperTicketSplitEnabled
        {
            get => _hopperTicketSplitEnabled;
            set
            {
                _hopperTicketSplitEnabled = value;
                CheckSplitEnabled();
                CanApplyExecute();
                RaisePropertyChanged(nameof(HopperTicketSplitEnabled));
            }
        }

        public bool ShowHopperTicketSplit
        {
            get => _showHopperTicketSplit;
            set => SetProperty(ref _showHopperTicketSplit, value, nameof(ShowHopperTicketSplit));
        }

        public decimal HopperTicketThreshold
        {
            get => _hopperTicketThreshold;
            set
            {
                _hopperTicketThreshold = value;
                ValidateFields(nameof(HopperTicketThreshold));
                RaisePropertyChanged(nameof(HopperTicketThreshold));
            }
        }

        public bool ShowHopperTicketThreshold
        {
            get => _showHopperTicketThreshold;
            set => SetProperty(ref _showHopperTicketThreshold, value, nameof(ShowHopperTicketThreshold));
        }


        public ICommand PerformHopperRefillCommand { get; set; }
        public ICommand ApplyCollectOptionsCommand { get; set; }
        public ICommand HopperTestCommand { get; set; }

        public HopperPageViewModel(bool isWizard) : base(DeviceType.Hopper, isWizard)
        {
            _minHopperRefillValue = PropertiesManager.GetValue(AccountingConstants.HopperRefillMinValue, 0L).MillicentsToDollars();
            _maxHopperRefillValue = PropertiesManager.GetValue(AccountingConstants.HopperRefillMaxValue, 0L).MillicentsToDollars();
            _defaultHopperCollectLimit = PropertiesManager.GetValue(AccountingConstants.HopperCollectDefaultValue, 0L).MillicentsToDollars();
            _minHopperCollectLimit = PropertiesManager.GetValue(AccountingConstants.HopperCollectMinValue, 0L).MillicentsToDollars();
            _maxHopperCollectLimit = PropertiesManager.GetValue(AccountingConstants.HopperCollectMaxValue, 0L).MillicentsToDollars();
            _defaultHopperTicketThreshold = PropertiesManager.GetValue(AccountingConstants.HopperThresholdDefaultValue, 0L).MillicentsToDollars();
            _minHopperTicketThreshold = PropertiesManager.GetValue(AccountingConstants.HopperThresholdMinValue, 0L).MillicentsToDollars();
            HopperTicketSplitSupported = PropertiesManager.GetValue(AccountingConstants.HopperTicketSplitSupported, false);
            HopperTicketSplitConfigurable = PropertiesManager.GetValue(AccountingConstants.HopperTicketSplitConfigurable, false);
            HopperTicketSplitEnabled = PropertiesManager.GetValue(AccountingConstants.HopperTicketSplit, false);
            _meterManager = ServiceManager.GetInstance().GetService<IMeterManager>();
            ModelText = ServiceManager.GetInstance().GetService<IHopper>().DeviceConfiguration.Model;
            // The current culture will be used to display the date/time.
            _time = ServiceManager.GetInstance().GetService<ITime>();
            Init();
        }

        private void Init()
        {
            PerformHopperRefillCommand = new ActionCommand<object>(obj => PerformHopperRefill());
            ApplyCollectOptionsCommand = new ActionCommand<object>(obj => SaveHopperLimits());
            HopperTestCommand = new ActionCommand<object>(obj => HandleHopperTestCommand());
        }
        private void CanApplyExecute()
        {
            if (HopperTicketSplitSupported && HopperTicketSplitConfigurable)
            {
                var validateLimits = HopperTicketSplitEnabled
                    ? ValidateHopperTicketThreshold()
                    : ValidateHopperCollectLimit();

                var checkPreviousLimits = HopperTicketSplitEnabled
                    ? HopperTicketThreshold != PropertiesManager
                        .GetValue(AccountingConstants.HopperTicketThreshold, 0L).MillicentsToDollars()
                    : HopperCollectLimit != PropertiesManager.GetValue(AccountingConstants.HopperCollectLimit, 0L)
                        .MillicentsToDollars();

                ApplyCommandEnabled = validateLimits && ValidateHopperRefillValue() &&
                                      (
                                          (HopperTicketSplitEnabled != PropertiesManager.GetValue(
                                              AccountingConstants.HopperTicketSplit,
                                              false)) ||
                                          checkPreviousLimits ||
                                          (HopperRefillValue != PropertiesManager.GetValue(
                                                  AccountingConstants.HopperCurrentRefillValue,
                                                  0L)
                                              .MillicentsToDollars())
                                      );
            }
            else
            {
                ApplyCommandEnabled = HopperRefillValue != PropertiesManager.GetValue(
                        AccountingConstants.HopperCurrentRefillValue,
                        0L)
                    .MillicentsToDollars();
            }
        }
        private void PerformHopperRefill()
        {
            var dialogService = ServiceManager.GetInstance().GetService<IDialogService>();

            var result = dialogService.ShowYesNoDialog(this, Resources.PerformHopperRefillDialog, string.Empty);
            if (result != null && (bool)result)
            {
                EventBus.Publish(new HopperRefillStartedEvent());
            }
        }

        private void Handle(HopperRefillCompletedEvent evt)
        {
            ValueOfLastRefill = CurrentRefillValue;
            var dateTime = TimeZoneInfo.ConvertTime(evt.LastRefillTime, _time.TimeZoneInformation);
            TimeOfLastRefill = _time.GetFormattedLocationTime(dateTime, ApplicationConstants.LongDateTimeFormat);

            GetHopperRefillValues();
        }

        private void GetHopperRefillValues()
        {
            var hopperRefillMeter = _meterManager.GetMeter(AccountingMeters.HopperRefillAmount);
            var refillCountMeter = _meterManager.GetMeter(AccountingMeters.HopperRefillCount);
            var hopperLevelMeter = _meterManager.GetMeter(AccountingMeters.CurrentHopperLevelCount);
            HopperRefill = hopperRefillMeter.Lifetime.MillicentsToDollars().FormattedCurrencyString();
            RefillCount = refillCountMeter.Lifetime;
            HopperLevel = hopperLevelMeter.Lifetime.FormattedCurrencyString();
        }

        private void SaveHopperLimits()
        {
            if (HopperTicketSplitSupported && HopperTicketSplitConfigurable)
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.HopperCurrentRefillValue,
                    HopperRefillValue.DollarsToMillicents());
                PropertiesManager.SetProperty(AccountingConstants.HopperTicketSplit, HopperTicketSplitEnabled);
                if (!HopperTicketSplitEnabled)
                {
                    PropertiesManager.SetProperty(
                        AccountingConstants.HopperCollectLimit,
                        HopperCollectLimit.DollarsToMillicents());
                    PropertiesManager.SetProperty(AccountingConstants.HopperTicketThreshold, 0L);
                }
                else
                {
                    PropertiesManager.SetProperty(
                        AccountingConstants.HopperTicketThreshold,
                        HopperTicketThreshold.DollarsToMillicents());
                    PropertiesManager.SetProperty(AccountingConstants.HopperCollectLimit, 0L);
                }
            }
            else
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.HopperCurrentRefillValue,
                    HopperRefillValue.DollarsToMillicents());
            }

            CurrentRefillValue = HopperRefillValue.FormattedCurrencyString();
            ApplyCommandEnabled = false;
        }

        private void CheckSplitEnabled()
        {
            if (!PropertiesManager.GetValue(ApplicationConstants.HopperEnabled, false) || !HopperTicketSplitSupported || !HopperTicketSplitConfigurable)
            {
                return;
            }

            if (HopperTicketSplitEnabled)
            {
                ShowHopperCollectLimit = false;
                ShowHopperTicketThreshold = true;
                HopperCollectLimit = _defaultHopperCollectLimit;
            }
            else
            {
                ShowHopperTicketThreshold = false;
                ShowHopperCollectLimit = true;
                HopperTicketThreshold = _defaultHopperTicketThreshold;
            }
        }

        private void ValidateHopperPrinter()
        {
            var hopperInstalled = PropertiesManager.GetValue(ApplicationConstants.HopperEnabled, false);
            var printerInstalled = PropertiesManager.GetValue(ApplicationConstants.PrinterEnabled, false);
            if (!hopperInstalled)
            {
                ShowHopperTicketSplit = false;
                ShowHopperTicketThreshold = false;
                ShowHopperCollectLimit = false;
            }
            else if (!printerInstalled)
            {
                ShowHopperTicketSplit = false;
                ShowHopperTicketThreshold = false;
                ShowHopperCollectLimit = true;
            }
            else
            {
                ShowHopperTicketSplit = true;
                ShowHopperTicketThreshold = true;
                ShowHopperCollectLimit = true;
            }
        }

        private void ValidateFields(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(HopperCollectLimit):
                    ValidateHopperCollectLimit();
                    break;
                case nameof(HopperTicketThreshold):
                case nameof(HopperRefillValue):
                    ValidateHopperTicketThreshold();
                    break;
            }

            CanApplyExecute();
        }

        private bool ValidateHopperCollectLimit()
        {
            var errorInfo = string.Empty;
            if (HopperCollectLimit < _minHopperCollectLimit)
            {
                errorInfo = $"{Resources.HopperCollectLimit} {Resources.CannotLessThanText} {_minHopperCollectLimit}";
            }
            else if (HopperCollectLimit > _maxHopperCollectLimit)
            {
                errorInfo = $"{Resources.HopperCollectLimit} {Resources.MustLessThanText} {_maxHopperCollectLimit}";
            }
            else if (HopperCollectLimit > _voucherOutLimit)
            {
                errorInfo = $"{Resources.HopperCollectLimit} {Resources.MustLessThanText} {Resources.VoucherOutLimit}";
            }
            else
            {
                // do nothing
            }
            SetError(nameof(HopperCollectLimit), errorInfo);
            return string.IsNullOrEmpty(errorInfo);
        }

        private bool ValidateHopperTicketThreshold()
        {
            var errorInfo = string.Empty;
            if (HopperTicketThreshold < _minHopperTicketThreshold)
            {
                errorInfo = $"{Resources.HopperTicketThreshold} {Resources.CannotLessThanText} {_minHopperTicketThreshold}";
            }
            else if (HopperTicketThreshold > _maxHopperTicketThreshold)
            {
               errorInfo = $"{Resources.HopperTicketThreshold} {Resources.MustLessThanText} {_maxHopperTicketThreshold}";
            }
            else if (HopperTicketThreshold % ThresholdMultiple != 0)
            {
               errorInfo = $"{Resources.EnteredValueMultipleOf} {ThresholdMultiple}";
            }
            else
            {
                // do nothing.
            }

            SetError(nameof(HopperTicketThreshold), errorInfo);
            return string.IsNullOrEmpty(errorInfo);
        }

        private bool ValidateHopperRefillValue()
        {
            string errorInfo = string.Empty;
            if (HopperRefillValue < _minHopperRefillValue)
            {
                errorInfo = Resources.HopperRefillInvalidValueText;

            }
            else if (HopperRefillValue > ((long)_maxHopperRefillValue))
            {
                errorInfo = $"{Resources.MaxHopperRefillValueText} {((long)_maxHopperRefillValue).FormattedCurrencyString()}";
            }
            else
            {
                //do nothing
            }
            SetError(nameof(HopperRefillValue), errorInfo);
            return string.IsNullOrEmpty(errorInfo);
        }

        private void GetLastRefill()
        {
            var transactions = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var transaction = transactions?.GetLast<HopperRefillTransaction>();
            if (transaction != null)
            {
                var dateTime = TimeZoneInfo.ConvertTime(transaction.TransactionDateTime, _time.TimeZoneInformation);
                TimeOfLastRefill = _time.GetFormattedLocationTime(dateTime, ApplicationConstants.LongDateTimeFormat);
                ValueOfLastRefill = transaction.LastRefillValue.MillicentsToDollars().FormattedCurrencyString();
            }
            GetHopperRefillValues();
        }

        protected override void OnLoaded()
        {
            EventBus.Subscribe<HopperRefillCompletedEvent>(this, Handle);
            var voucherOutLimitCents = PropertiesManager.GetValue(AccountingConstants.VoucherOutLimit, AccountingConstants.DefaultVoucherOutLimit);
            // Hopper Ticket Threshold should be multiple of 10
            _maxHopperTicketThreshold = ((voucherOutLimitCents - AccountingConstants.HopperTicketSplitMinTicketValue) / AccountingConstants.HopperTicketSplitIncrement) * ThresholdMultiple;
            _voucherOutLimit = voucherOutLimitCents.MillicentsToDollars();
            HopperTicketSplitEnabled = PropertiesManager.GetValue(AccountingConstants.HopperTicketSplit, false);
            HopperCollectLimit = PropertiesManager.GetValue(AccountingConstants.HopperCollectLimit, 0L).MillicentsToDollars();
            HopperTicketThreshold = PropertiesManager.GetValue(AccountingConstants.HopperTicketThreshold, 0L).MillicentsToDollars();
            CurrentRefillValue = PropertiesManager.GetValue(AccountingConstants.HopperCurrentRefillValue, 0L).MillicentsToDollars().FormattedCurrencyString();
            HopperRefillValue = PropertiesManager.GetValue(AccountingConstants.HopperCurrentRefillValue, 0L).MillicentsToDollars();
            _initialHopperLevel = _meterManager.GetMeter(AccountingMeters.CurrentHopperLevelCount).Lifetime;
            HopperRefillWarningEnabled = _initialHopperLevel > 0 && InputEnabled;
            HopperRefillEnabled = _initialHopperLevel == 0 && InputEnabled;
            ValidateHopperPrinter();
            CheckSplitEnabled();
            GetLastRefill();
            if (PropertiesManager.GetValue(HardwareConstants.HopperDiagnosticMode, false))
            {
                HandleHopperTestCommand();
            }
            base.OnLoaded();
        }

        protected override void OnUnloaded()
        {
            EventBus.UnsubscribeAll(this);
            base.OnUnloaded();
        }

        protected override void OnInputEnabledChanged()
        {
            HopperRefillWarningEnabled = _initialHopperLevel > 0 && InputEnabled;
            HopperRefillEnabled = _initialHopperLevel == 0 && InputEnabled;
            CheckSplitEnabled();
        }
        protected override void StartEventHandler()
        {
        }

        protected override void SubscribeToEvents()
        {
        }

        protected override void UpdateScreen()
        {
        }

        /// <summary>
        ///     Hopper Test View
        /// </summary>
        private void HandleHopperTestCommand()
        {
            var dialogService = ServiceManager.GetInstance().GetService<IDialogService>();

            var viewModel = new HopperTestViewModel();

            EventBus.Publish(new HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory.Hopper));

            dialogService.ShowDialog<HopperTestView>(
                this,
                viewModel,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HopperTest),
                DialogButton.None);

            EventBus.Publish(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.Hopper));
        }
    }
}
