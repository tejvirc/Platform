namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.ConfigWizard;
    using Application.UI.Events;
    using Kernel.Contracts;
    using Contracts;
    using Kernel;
    using Localization.Properties;
    using Mono.Addins;
    using System.ComponentModel.DataAnnotations;

    [CLSCompliant(false)]
    public class LimitsPageViewModel : ConfigWizardViewModelBase
    {
        private const string PropertyProvidersPath = "/Accounting/PropertyProviders";

        private decimal _creditLimit;
        private decimal _handpayLimit;
        private decimal _largeWinLimit;
        private decimal _largeWinRatio;
        private decimal _largeWinRatioThreshold;
        private decimal _maxBetLimit;
        private decimal _celebrationLockupLimit;
        private decimal _billAcceptanceLimit;
        private decimal _handCountPayoutLimit;

        private decimal _initialCreditLimit;
        private decimal _initialHandpayLimit;
        private decimal _initialLargeWinLimit;
        private decimal _initialLargeWinRatio;
        private decimal _initialLargeWinRatioThreshold;
        private decimal _initialMaxBetLimit;
        private decimal _initialCelebrationLockupLimit;
        private decimal _initialGambleWagerLimit;
        private decimal _initialGambleWinLimit;
        private decimal _initialBillAcceptanceLimit;
        private decimal _initialHandCountPayoutLimit;

        private string _selectedLargeWinHandpayResetMethod;
        private bool _allowRemoteHandpayReset;

        private bool _creditLimitIsChecked;
        private bool _handpayLimitIsChecked;
        private bool _largeWinLimitIsChecked;
        private bool _largeWinRatioIsChecked;
        private bool _largeWinRatioThresholdIsChecked;
        private bool _celebrationLockupLimitIsChecked;
        private bool _maxBetLimitIsChecked;
        private bool _creditLimitCheckboxEnabled;
        private bool _handpayLimitCheckboxEnabled;
        private bool _handCountLimitCheckboxEnabled;
        private bool _billAcceptanceLimitCheckboxEnabled;
        private bool _handCountPayoutLimitIsChecked;
        private bool _billAcceptanceLimitIsChecked;

        private bool _overwriteAllowRemoteHandpayReset;
        private bool _overwriteLargeWinLimit;
        private bool _overwriteLargeWinRatio;
        private bool _overwriteLargeWinRatioThreshold;
        private bool _overwriteMaxBetLimit;
        private bool _pageEnabled;
        private bool _gambleWagerLimitVisible;
        private bool _gambleWinLimitVisible;

        private long _maxCreditMeter;
        private decimal _gambleWagerLimit;
        private decimal _gambleWinLimit;

        private long _incrementThreshold;
        private long _initialIncrementThreshold;
        private bool _incrementThresholdIsChecked;
        private bool _creditLimitEditable;

        private string _limitsPopUpInfoText;
        private bool _popupOpen;
        private UIElement _placementTarget;

        public LimitsPageViewModel(bool isWizardPage = false) : base(isWizardPage)
        {
            if (isWizardPage)
            {
                var nodes = MonoAddinsHelper.GetSelectedNodes<TypeExtensionNode>(PropertyProvidersPath);
                foreach (var node in nodes)
                {
                    PropertiesManager.AddPropertyProvider((IPropertyProvider)node.CreateInstance());
                }
            }

            LargeWinHandpayResetMethods = new List<string>
            {
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PayByHand),
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PayByMenuSelection),
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PayByHostSystem)
            };

            var selectedLargeWinHandpayResetMethod = PropertiesManager.GetValue(AccountingConstants.LargeWinHandpayResetMethod, (int)LargeWinHandpayResetMethod.PayByHand);
            _selectedLargeWinHandpayResetMethod = LargeWinHandpayResetMethods[selectedLargeWinHandpayResetMethod];
            LargeWinLimitMaxValue = PropertiesManager.GetValue(
                AccountingConstants.LargeWinLimitMaxValue,
                AccountingConstants.DefaultHandpayLimit);

            IncrementThresholdVisible = PropertiesManager.GetValue(AccountingConstants.LaunderingMonitorVisible, false);

            if (IsWizardPage)
            {
                HandpayLimitVisible = (bool)PropertiesManager.GetProperty(ApplicationConstants.ConfigWizardHandpayLimitVisible, true);
                LargeWinLimitVisible = true;
                BillAcceptanceLimitVisible = (bool)PropertiesManager.GetProperty(AccountingConstants.BillAcceptanceLimitVisible, false);
                HandCountPayoutLimitVisible = (bool)PropertiesManager.GetProperty(AccountingConstants.HandCountPayoutLimitVisible, false);
                LargeWinRatioVisible = (bool)PropertiesManager.GetProperty(AccountingConstants.DisplayLargeWinRatio, false);
                LargeWinRatioThresholdVisible = (bool)PropertiesManager.GetProperty(AccountingConstants.DisplayLargeWinRatioThreshold, false);
                MaxBetLimitVisible = true;
                CelebrationLockupLimitVisible = false;
                GambleAllowed = false;  // don't show gamble info on config wizard
                AllowRemoteHandpayResetVisible = LargeWinHandpayResetMethodVisible =
                    (bool)PropertiesManager.GetProperty(AccountingConstants.DisplayHandpayResetOptions, true);
            }
            else
            {
                BillAcceptanceLimitVisible = GetConfigSetting(OperatorMenuSetting.BillAcceptanceLimitVisible, false);
                HandCountPayoutLimitVisible = GetConfigSetting(OperatorMenuSetting.HandCountPayoutLimitVisible, false);
                HandpayLimitVisible = GetConfigSetting(OperatorMenuSetting.HandpayLimitVisible, true);
                LargeWinLimitVisible = GetConfigSetting(OperatorMenuSetting.LargeWinLimitVisible, true);
                LargeWinRatioVisible = GetConfigSetting(OperatorMenuSetting.LargeWinRatioVisible, false);
                LargeWinRatioThresholdVisible = GetConfigSetting(OperatorMenuSetting.LargeWinRatioThresholdVisible, false);
                MaxBetLimitVisible = GetConfigSetting(OperatorMenuSetting.MaxBetLimitVisible, true);
                CelebrationLockupLimitVisible = GetConfigSetting(OperatorMenuSetting.CelebrationLockupLimitVisible, false);
                AllowRemoteHandpayResetVisible = GetConfigSetting(OperatorMenuSetting.AllowRemoteHandpayResetVisible, true);
                LargeWinHandpayResetMethodVisible = GetConfigSetting(OperatorMenuSetting.LargeWinHandpayResetMethodVisible, true);
                GambleAllowed = (bool)PropertiesManager.GetProperty(GamingConstants.GambleAllowed, true);
                GambleWagerLimitVisible = GetConfigSetting(OperatorMenuSetting.GambleWagerLimitVisible, true);
                GambleWinLimitVisible = GetConfigSetting(OperatorMenuSetting.GambleWinLimitVisible, false);
            }
        }

        public UIElement PlacementTarget
        {
            get => _placementTarget;
            set
            {
                _placementTarget = value;
                OnPropertyChanged(nameof(PlacementTarget));
            }
        }

        public bool ConfigPopupOpen
        {
            get => _popupOpen;
            set
            {
                _popupOpen = value;

                OnPropertyChanged(nameof(PlacementTarget));
                OnPropertyChanged(nameof(ConfigPopupOpen));
            }
        }

        public string LimitsPopUpInfoText
        {
            get => _limitsPopUpInfoText;
            set
            {
                _limitsPopUpInfoText = value;
                OnPropertyChanged(nameof(LimitsPopUpInfoText));
            }
        }

        public bool PageEnabled
        {
            get => _pageEnabled && InputEnabled;
            set
            {
                if (_pageEnabled == value)
                {
                    return;
                }
                
                _pageEnabled = value;
                OnPropertyChanged(nameof(PageEnabled));
            }
        }

        public bool CreditLimitEditable
        {
            get { return _creditLimitEditable; }
            set
            {
                _creditLimitEditable = value;
                OnPropertyChanged(nameof(CreditLimitEditable));
            }
        }

        [CustomValidation(typeof(LimitsPageViewModel), nameof(ValidateCreditLimit))]
        public decimal CreditLimit
        {
            get => CreditLimitIsChecked ? _creditLimit : _maxCreditMeter.MillicentsToDollars();
            set => SetProperty(ref _creditLimit, value, nameof(CreditLimit));
        }

        public bool CreditLimitIsChecked
        {
            get => _creditLimitIsChecked;
            set
            {
                if (SetProperty(ref _creditLimitIsChecked, value, nameof(CreditLimitIsChecked)))
                {
                    if (value)
                    {
                        CreditLimit = _initialCreditLimit;
                    }
                }
            }
        }

        [CustomValidation(typeof(LimitsPageViewModel), nameof(ValidateHandpayLimit))]
        public decimal HandpayLimit
        {
            get => HandpayLimitIsChecked ? _handpayLimit : AccountingConstants.DefaultHandpayLimit.MillicentsToDollars();
            set => SetProperty(ref _handpayLimit, value, nameof(HandpayLimit));
        }

        public bool HandpayLimitIsChecked
        {
            get => _handpayLimitIsChecked;
            set
            {
                if (SetProperty(ref _handpayLimitIsChecked, value, nameof(HandpayLimitIsChecked)))
                {
                    ValidateFields(nameof(HandpayLimit));
                    if (value)
                    {
                        HandpayLimit = _initialHandpayLimit;
                    }
                }
            }
        }

        [CustomValidation(typeof(LimitsPageViewModel), nameof(ValidateIncrementThreshold))]
        public long IncrementThreshold
        {
            get => IncrementThresholdIsChecked ? _incrementThreshold : (long)AccountingConstants.DefaultIncrementThreshold.MillicentsToDollars();
            set => SetProperty(ref _incrementThreshold, value, nameof(IncrementThreshold));
        }

        public bool IncrementThresholdIsChecked
        {
            get => _incrementThresholdIsChecked;
            set
            {
                if (SetProperty(ref _incrementThresholdIsChecked, value, nameof(IncrementThresholdIsChecked)))
                {
                    // don't follow the existing pattern in this case
                    // validation is needed only if it's checked 
                    if (_incrementThresholdIsChecked)
                    {
                        ValidateFields(nameof(IncrementThreshold));
                    }
                }
            }
        }

        public bool IncrementThresholdVisible { get; }

        [CustomValidation(typeof(LimitsPageViewModel), nameof(ValidateLargeWinLimit))]
        public decimal LargeWinLimit
        {
            get => LargeWinLimitIsChecked ? _largeWinLimit : AccountingConstants.DefaultLargeWinLimit.MillicentsToDollars();
            set => SetProperty(ref _largeWinLimit, value, nameof(LargeWinLimit));
        }

        public bool OverwriteLargeWinLimit
        {
            get => _overwriteLargeWinLimit;
            set => SetProperty(
                ref _overwriteLargeWinLimit,
                value,
                nameof(OverwriteLargeWinLimit),
                nameof(LargeWinLimitEditable),
                nameof(LargeWinLimitCheckboxIsEnabled));
        }

        public bool LargeWinLimitEditable => OverwriteLargeWinLimit && PageEnabled;

        public bool LargeWinLimitCheckboxIsEnabled => OverwriteLargeWinLimit && PageEnabled &&
                                                      LargeWinLimitMaxValue >= AccountingConstants.DefaultHandpayLimit;

        public bool LargeWinLimitIsChecked
        {
            get => _largeWinLimitIsChecked;
            set
            {
                if (SetProperty(ref _largeWinLimitIsChecked, value, nameof(LargeWinLimitIsChecked)))
                {
                    if (value)
                    {
                        LargeWinLimit = _initialLargeWinLimit;
                    }
                }
            }
        }

        public bool HandCountPayoutLimitIsChecked
        {
            get => _handCountPayoutLimitIsChecked;
            set
            {
                if (SetProperty(ref _handCountPayoutLimitIsChecked, value, nameof(HandCountPayoutLimitIsChecked)))
                {
                    if (value)
                    {
                        HandCountPayoutLimit = _initialHandCountPayoutLimit;
                    }
                }
            }
        }

        public bool BillAcceptanceLimitIsChecked
        {
            get => _billAcceptanceLimitIsChecked;
            set
            {
                if (SetProperty(ref _billAcceptanceLimitIsChecked, value, nameof(BillAcceptanceLimitIsChecked)))
                {
                    BillAcceptanceLimit = value ? _initialBillAcceptanceLimit : ApplicationConstants.DefaultMaxCreditsIn.MillicentsToDollars();
                }
            }
        }

        [CustomValidation(typeof(LimitsPageViewModel), nameof(ValidateLargeWinRatio))]
        public decimal LargeWinRatio
        {
            get => _largeWinRatio;
            set => SetProperty(ref _largeWinRatio, value, nameof(LargeWinRatio));
        }

        public bool OverwriteLargeWinRatio
        {
            get => _overwriteLargeWinRatio;
            set => SetProperty(ref _overwriteLargeWinRatio, value, nameof(OverwriteLargeWinRatio));
        }

        public bool LargeWinRatioCheckboxIsEnabled => OverwriteLargeWinRatio && PageEnabled;

        public bool LargeWinRatioIsChecked
        {
            get => _largeWinRatioIsChecked;
            set
            {
                if (SetProperty(ref _largeWinRatioIsChecked, value, nameof(LargeWinRatioIsChecked)))
                {
                    if (value)
                    {
                        LargeWinRatio = _initialLargeWinRatio;
                    }
                }
            }
        }

        public decimal MaximumLargeWinRatio => AccountingConstants.MaximumLargeWinRatio / 100.0m;

        [CustomValidation(typeof(LimitsPageViewModel), nameof(ValidateLargeWinRatioThreshold))]
        public decimal LargeWinRatioThreshold
        {
            get => _largeWinRatioThreshold;
            set => SetProperty(ref _largeWinRatioThreshold, value, nameof(LargeWinRatioThreshold));
        }

        public bool OverwriteLargeWinRatioThreshold
        {
            get => _overwriteLargeWinRatioThreshold;
            set => SetProperty(ref _overwriteLargeWinRatioThreshold, value, nameof(OverwriteLargeWinRatioThreshold));
        }

        public bool LargeWinRatioThresholdCheckboxIsEnabled => OverwriteLargeWinRatioThreshold && PageEnabled;

        public bool LargeWinRatioThresholdIsChecked
        {
            get => _largeWinRatioThresholdIsChecked;
            set
            {
                if (SetProperty(ref _largeWinRatioThresholdIsChecked, value, nameof(LargeWinRatioThresholdIsChecked)))
                {
                    if (value)
                    {
                        LargeWinRatioThreshold = _initialLargeWinRatioThreshold;
                    }
                }
            }
        }

        [CustomValidation(typeof(LimitsPageViewModel), nameof(ValidateMaxBetLimit))]
        public decimal MaxBetLimit
        {
            get => MaxBetLimitIsChecked ? _maxBetLimit : AccountingConstants.DefaultMaxBetLimit.MillicentsToDollars();
            set => SetProperty(ref _maxBetLimit, value, nameof(MaxBetLimit));
        }

        [CustomValidation(typeof(LimitsPageViewModel), nameof(ValidateBillAcceptanceLimit))]
        public decimal BillAcceptanceLimit
        {
            get => _billAcceptanceLimit;
            set => SetProperty(ref _billAcceptanceLimit, value, nameof(BillAcceptanceLimit));
        }

        [CustomValidation(typeof(LimitsPageViewModel), nameof(ValidateHandCountPayoutLimit))]
        public decimal HandCountPayoutLimit
        {
            get => HandCountPayoutLimitIsChecked ? _handCountPayoutLimit : 0m;
            set => SetProperty(ref _handCountPayoutLimit, value, nameof(HandCountPayoutLimit));
        }

        public bool OverwriteMaxBetLimit
        {
            get => _overwriteMaxBetLimit;
            set => SetProperty(ref _overwriteMaxBetLimit, value, nameof(OverwriteMaxBetLimit));
        }

        public bool MaxBetLimitCheckboxIsEnabled => OverwriteMaxBetLimit && PageEnabled;

        public bool MaxBetLimitIsChecked
        {
            get => _maxBetLimitIsChecked;
            set
            {
                if (SetProperty(ref _maxBetLimitIsChecked, value, nameof(MaxBetLimitIsChecked)))
                {
                    if (value)
                    {
                        MaxBetLimit = _initialMaxBetLimit;
                    }
                }
            }
        }

        public bool CreditLimitCheckboxEnabled
        {
            get => _creditLimitCheckboxEnabled && PageEnabled;
            set => SetProperty(ref _creditLimitCheckboxEnabled, value, nameof(CreditLimitCheckboxEnabled));
        }

        public bool HandpayLimitCheckboxEnabled
        {
            get => _handpayLimitCheckboxEnabled && PageEnabled;
            set => SetProperty(ref _handpayLimitCheckboxEnabled, value, nameof(HandpayLimitCheckboxEnabled));
        }

        public bool BillAcceptanceLimitCheckboxEnabled
        {
            get => _billAcceptanceLimitCheckboxEnabled && InputEnabled;
            set => SetProperty(ref _billAcceptanceLimitCheckboxEnabled, value, nameof(BillAcceptanceLimitCheckboxEnabled));
        }

        public bool HandCountPayoutLimitCheckboxEnabled
        {
            get => _handCountLimitCheckboxEnabled && PageEnabled;
            set => SetProperty(ref _handCountLimitCheckboxEnabled, value, nameof(HandCountPayoutLimitCheckboxEnabled));
        }

        [CustomValidation(typeof(LimitsPageViewModel), nameof(ValidateCelebrationLockupLimit))]
        public decimal CelebrationLockupLimit
        {
            get => CelebrationLockupLimitIsChecked ? _celebrationLockupLimit : 0;
            set => SetProperty(ref _celebrationLockupLimit, value, nameof(CelebrationLockupLimit));
        }

        public bool CelebrationLockupLimitCheckboxIsEnabled => LargeWinLimitCheckboxIsEnabled;

        public bool CelebrationLockupLimitIsChecked
        {
            get => _celebrationLockupLimitIsChecked;
            set
            {
                var currentCelebrationLockupLimit = CelebrationLockupLimit;
                if (SetProperty(ref _celebrationLockupLimitIsChecked, value, nameof(CelebrationLockupLimitIsChecked)))
                {
                    if (value)
                    {
                        CelebrationLockupLimit = _celebrationLockupLimit == 0 ? _initialCelebrationLockupLimit : currentCelebrationLockupLimit;
                    }
                    else
                    {
                        CelebrationLockupLimit = 0;
                    }
                }
            }
        }

        public List<string> LargeWinHandpayResetMethods { get; }

        public string SelectedLargeWinHandpayResetMethod
        {
            get => _selectedLargeWinHandpayResetMethod;
            set
            {
                _selectedLargeWinHandpayResetMethod = value;
                OnPropertyChanged(nameof(SelectedLargeWinHandpayResetMethod));
            }
        }

        public bool AllowRemoteHandpayReset
        {
            get => _allowRemoteHandpayReset;
            set
            {
                _allowRemoteHandpayReset = value;
                OnPropertyChanged(nameof(AllowRemoteHandpayReset));
            }
        }

        public bool ShowGambleWagerLimit => GambleAllowed && GambleWagerLimitVisible;

        public bool GambleWagerLimitVisible
        {
            get => _gambleWagerLimitVisible;
            set
            {
                _gambleWagerLimitVisible = value;
                OnPropertyChanged(nameof(GambleWagerLimitVisible));
                OnPropertyChanged(nameof(ShowGambleWagerLimit));
            }
        }

        public bool GambleWinLimitVisible
        {
            get => _gambleWinLimitVisible;
            set
            {
                _gambleWinLimitVisible = value;
                OnPropertyChanged(nameof(GambleWinLimitVisible));
                OnPropertyChanged(nameof(ShowGambleWinLimit));
            }
        }

        public bool ShowGambleWinLimit => GambleAllowed && GambleWinLimitVisible;

        [CustomValidation(typeof(LimitsPageViewModel), nameof(ValidateGambleWagerLimit))]
        public decimal GambleWagerLimit
        {
            get => _gambleWagerLimit;
            set => SetProperty(ref _gambleWagerLimit, value, nameof(GambleWagerLimit));
        }

        [CustomValidation(typeof(LimitsPageViewModel), nameof(ValidateGambleWinLimit))]
        public decimal GambleWinLimit
        {
            get => _gambleWinLimit;
            set => SetProperty(ref _gambleWinLimit, value, nameof(GambleWinLimit));
        }

        public bool GambleAllowed { get; }

        public bool GambleWagerLimitConfigurable => PropertiesManager.GetValue(
            GamingConstants.GambleWagerLimitConfigurable,
            false);

        public bool GambleWinLimitConfigurable => PropertiesManager.GetValue(
            GamingConstants.GambleWinLimitConfigurable,
            false);

        public bool OverwriteAllowRemoteHandpayReset
        {
            get => _overwriteAllowRemoteHandpayReset;
            set
            {
                _overwriteAllowRemoteHandpayReset = value;
                OnPropertyChanged(nameof(AllowRemoteHandpayResetIsEnabled));
            }
        }

        public bool AllowRemoteHandpayResetIsEnabled => OverwriteAllowRemoteHandpayReset && PageEnabled;

        public bool HandpayLimitVisible { get; }

        public bool LargeWinLimitVisible { get; }

        public bool BillAcceptanceLimitVisible { get; }

        public bool HandCountPayoutLimitVisible { get; }

        public bool LargeWinRatioVisible { get; }

        public bool LargeWinRatioThresholdVisible { get; }

        public bool MaxBetLimitVisible { get; }

        public bool CelebrationLockupLimitVisible { get; }

        public bool AllowRemoteHandpayResetVisible { get; }

        public bool LargeWinHandpayResetMethodVisible { get; }

        private long LargeWinLimitMaxValue { get; }

        private decimal CurrentMaximumLockupLimit => Math.Min(LargeWinLimit, Math.Min(HandpayLimit, CreditLimit));

        protected override void Loaded()
        {
            base.Loaded();

            ClearValidationOnUnload = true;

            PageEnabled = PropertiesManager.GetValue(AccountingConstants.ConfigWizardLimitsPageEnabled, true);

            CreditLimitEditable = HandCountPayoutLimitVisible ? PageEnabled && CreditLimitCheckboxEnabled : PageEnabled;

            IncrementThresholdIsChecked = PropertiesManager.GetValue(AccountingConstants.IncrementThresholdIsChecked, false);
            IncrementThreshold = _initialIncrementThreshold = (long)PropertiesManager.GetValue(AccountingConstants.IncrementThreshold, AccountingConstants.DefaultIncrementThreshold).MillicentsToDollars();
            CreditLimitIsChecked = PropertiesManager.GetValue(AccountingConstants.CreditLimitEnabled, true);
            HandpayLimitIsChecked = PropertiesManager.GetValue(AccountingConstants.HandpayLimitEnabled, false);
            LargeWinLimitIsChecked = PropertiesManager.GetValue(AccountingConstants.LargeWinLimitEnabled, true);
            LargeWinRatioIsChecked = PropertiesManager.GetValue(AccountingConstants.LargeWinRatioEnabled, false);
            LargeWinRatioThresholdIsChecked = PropertiesManager.GetValue(AccountingConstants.LargeWinRatioThresholdEnabled, false);
            CelebrationLockupLimit = _initialCelebrationLockupLimit = PropertiesManager.GetValue(AccountingConstants.CelebrationLockupLimit, 0L).MillicentsToDollars();
            CelebrationLockupLimitIsChecked = _celebrationLockupLimit > 0;
            MaxBetLimitIsChecked = PropertiesManager.GetValue(AccountingConstants.MaxBetLimitEnabled, true);
            HandCountPayoutLimitIsChecked = PropertiesManager.GetValue(AccountingConstants.HandCountPayoutLimitVisible, false);

            _maxCreditMeter = PropertiesManager.GetValue(AccountingConstants.MaxCreditMeterMaxAllowed, long.MaxValue);
            CreditLimit = _initialCreditLimit = PropertiesManager.GetValue(AccountingConstants.MaxCreditMeter, _maxCreditMeter).MillicentsToDollars();
            HandpayLimit = _initialHandpayLimit = PropertiesManager.GetValue(AccountingConstants.HandpayLimit, AccountingConstants.DefaultHandpayLimit).MillicentsToDollars();
            OverwriteLargeWinLimit = PropertiesManager.GetValue(AccountingConstants.OverwriteLargeWinLimit, false);
            LargeWinLimit = _initialLargeWinLimit = PropertiesManager.GetValue(AccountingConstants.LargeWinLimit, AccountingConstants.DefaultLargeWinLimit).MillicentsToDollars();
            OverwriteLargeWinRatio = PropertiesManager.GetValue(AccountingConstants.OverwriteLargeWinRatio, false);
            LargeWinRatio = _initialLargeWinRatio = PropertiesManager.GetValue(AccountingConstants.LargeWinRatio, AccountingConstants.DefaultLargeWinRatio) / 100.0m;
            OverwriteLargeWinRatioThreshold = PropertiesManager.GetValue(AccountingConstants.OverwriteLargeWinRatioThreshold, false);
            LargeWinRatioThreshold = _initialLargeWinRatioThreshold = PropertiesManager.GetValue(AccountingConstants.LargeWinRatioThreshold, AccountingConstants.DefaultLargeWinRatioThreshold).MillicentsToDollars();
            OverwriteMaxBetLimit = PropertiesManager.GetValue(AccountingConstants.OverwriteMaxBetLimit, false);
            MaxBetLimit = _initialMaxBetLimit = PropertiesManager.GetValue(AccountingConstants.MaxBetLimit, AccountingConstants.DefaultMaxBetLimit).MillicentsToDollars();
            CelebrationLockupLimit = _celebrationLockupLimit;
            GambleWagerLimit = PropertiesManager.GetValue(GamingConstants.GambleWagerLimit, 0L).MillicentsToDollars();
            GambleWinLimit = PropertiesManager.GetValue(GamingConstants.GambleWinLimit, GamingConstants.DefaultGambleWinLimit).MillicentsToDollars();
            _initialGambleWagerLimit = GambleWagerLimit;
            _initialGambleWinLimit = GambleWinLimit;
            BillAcceptanceLimit = _initialBillAcceptanceLimit = PropertiesManager.GetValue(PropertyKey.MaxCreditsIn, ApplicationConstants.DefaultMaxCreditsIn).MillicentsToDollars();
            BillAcceptanceLimitCheckboxEnabled = PropertiesManager.GetValue(AccountingConstants.BillAcceptanceLimitVisible, false);
            BillAcceptanceLimitIsChecked = BillAcceptanceLimit < ApplicationConstants.DefaultMaxCreditsIn.MillicentsToDollars() && PropertiesManager.GetValue(AccountingConstants.BillAcceptanceLimitVisible, false);
            HandCountPayoutLimit = _initialHandCountPayoutLimit = PropertiesManager.GetValue(AccountingConstants.HandCountPayoutLimit, 0L).MillicentsToDollars();
            OnInputStatusChanged();

            UpdateLimits();

            AllowRemoteHandpayReset = PropertiesManager.GetValue(AccountingConstants.RemoteHandpayResetAllowed, true);
            OverwriteAllowRemoteHandpayReset = PropertiesManager.GetValue(AccountingConstants.RemoteHandpayResetConfigurable, true);
            CreditLimitIsChecked = CreditLimit < long.MaxValue.MillicentsToDollars();
            CreditLimitCheckboxEnabled = PropertiesManager.GetValue(ApplicationConstants.ConfigWizardCreditLimitCheckboxEditable, true)
                & _maxCreditMeter == long.MaxValue;
            HandpayLimitCheckboxEnabled = PropertiesManager.GetValue(ApplicationConstants.ConfigWizardHandpayLimitCheckboxEditable, true);
            LargeWinLimitIsChecked = LargeWinLimit < AccountingConstants.DefaultLargeWinLimit.MillicentsToDollars();
            LargeWinRatioIsChecked = LargeWinRatio > AccountingConstants.DefaultLargeWinRatio;
            LargeWinRatioThresholdIsChecked = LargeWinRatioThreshold > AccountingConstants.DefaultLargeWinRatioThreshold.MillicentsToDollars();
            MaxBetLimitIsChecked = MaxBetLimit < long.MaxValue.MillicentsToDollars();

            EventBus?.Subscribe<PropertyChangedEvent>(this, HandleEvent);
            if (IsWizardPage)
            {
                EventBus?.Subscribe<OperatorMenuPopupEvent>(this, OnShowPopup);
            }

            CheckNavigation();
        }

        protected override void OnOperatorCultureChanged(OperatorCultureChangedEvent evt)
        {
            if (UseOperatorCultureForCurrencyFormatting)
            {
                OnPropertyChanged(nameof(CurrencyDisplayCulture));
            }

            UpdateLimits();
            base.OnOperatorCultureChanged(evt);
        }

        protected override void LoadAutoConfiguration()
        {
            string value = null;

            AutoConfigurator.GetValue("LargeWinLimit", ref value);
            if (value != null && OverwriteLargeWinLimit && decimal.TryParse(value, out var limit))
            {
                LargeWinLimit = limit;
                if (LargeWinLimitCheckboxIsEnabled)
                {
                    LargeWinLimitIsChecked = true;
                }
            }

            AutoConfigurator.GetValue("LargeWinRatio", ref value);
            if (value != null && OverwriteLargeWinRatio && decimal.TryParse(value, out var ratio))
            {
                LargeWinRatio = ratio / 100.0m;
                if (LargeWinRatioCheckboxIsEnabled)
                {
                    LargeWinRatioIsChecked = true;
                }
            }

            AutoConfigurator.GetValue("LargeWinRatioThreshold", ref value);
            if (value != null && OverwriteLargeWinRatioThreshold && decimal.TryParse(value, out var threshold))
            {
                LargeWinRatioThreshold = threshold;
                if (LargeWinRatioThresholdCheckboxIsEnabled)
                {
                    LargeWinRatioThresholdIsChecked = true;
                }
            }

            AutoConfigurator.GetValue("CreditLimit", ref value);
            if (value != null && decimal.TryParse(value, out var creditLimit))
            {
                CreditLimit = creditLimit;
                if (CreditLimitCheckboxEnabled)
                {
                    CreditLimitIsChecked = true;
                }
            }

            AutoConfigurator.GetValue("MaxBetLimit", ref value);
            if (value != null && OverwriteMaxBetLimit && decimal.TryParse(value, out var maxBet))
            {
                MaxBetLimit = maxBet;
                if (MaxBetLimitCheckboxIsEnabled)
                {
                    MaxBetLimitIsChecked = true;
                }
            }

            base.LoadAutoConfiguration();
        }

        private void OnShowPopup(OperatorMenuPopupEvent evt)
        {
            ConfigPopupOpen = evt.PopupOpen;
            LimitsPopUpInfoText = evt.PopupText;
            PlacementTarget = evt.TargetElement;
        }

        private void CheckNavigation()
        {
            if (IsWizardPage && WizardNavigator != null)
            {
                WizardNavigator.CanNavigateForward =
                    (!IncrementThresholdVisible || !IncrementThresholdIsChecked || ((decimal)IncrementThreshold).Validate(false, CreditLimit.DollarsToMillicents(), AccountingConstants.MinimumIncrementThreshold) is null) &&
                    CreditLimit.Validate(false, CreditLimitIsChecked ? _maxCreditMeter : long.MaxValue) is null &&
                    LargeWinLimit.Validate(false, LargeWinLimitMaxValue) is null &&
                    (!HandpayLimitVisible || HandpayLimit.Validate(false, AccountingConstants.DefaultHandpayLimit, LargeWinLimit.DollarsToMillicents()) is null) &&
                    LargeWinRatio.ValidateDecimal(AccountingConstants.DefaultLargeWinRatio, AccountingConstants.MaximumLargeWinRatio) is null &&
                    LargeWinRatioThreshold.Validate(true) is null &&
                    MaxBetLimit.Validate(false, PropertiesManager.GetValue(AccountingConstants.HighestMaxBetLimitAllowed, long.MaxValue)) is null;
            }
        }

        protected override void OnUnloaded()
        {
            if (IsWizardPage)
            {
                EventBus?.Unsubscribe<OperatorMenuPopupEvent>(this);
            }

            SaveChanges();
        }

        protected override void SaveChanges()
        {
            PropertiesManager.SetProperty(AccountingConstants.HandpayLimitEnabled, HandpayLimitIsChecked);
            PropertiesManager.SetProperty(AccountingConstants.LargeWinLimitEnabled, LargeWinLimitIsChecked);
            PropertiesManager.SetProperty(AccountingConstants.LargeWinRatioEnabled, LargeWinRatioIsChecked);
            PropertiesManager.SetProperty(AccountingConstants.LargeWinRatioThresholdEnabled, LargeWinRatioThresholdIsChecked);
            PropertiesManager.SetProperty(AccountingConstants.CreditLimitEnabled, CreditLimitIsChecked);
            PropertiesManager.SetProperty(AccountingConstants.MaxBetLimitEnabled, MaxBetLimitIsChecked);

            var hasChanges = false;

            if (PropertyHasErrors(nameof(CreditLimit)))
            {
                CreditLimitIsChecked = true;
                CreditLimit = _initialCreditLimit;
            }

            if (HandpayLimitVisible && PropertyHasErrors(nameof(HandpayLimit)))
            {
                HandpayLimitIsChecked = true;
                HandpayLimit = _initialHandpayLimit;
            }

            if (LargeWinLimitVisible && PropertyHasErrors(nameof(LargeWinLimit)))
            {
                LargeWinLimitIsChecked = true;
                LargeWinLimit = Math.Min(_initialLargeWinLimit, HandpayLimit);
            }

            if (LargeWinRatioVisible && PropertyHasErrors(nameof(LargeWinRatio)))
            {
                LargeWinRatioIsChecked = true;
                LargeWinRatio = _initialLargeWinRatio;
            }

            if (LargeWinRatioThresholdVisible && PropertyHasErrors(nameof(LargeWinRatioThreshold)))
            {
                LargeWinRatioThresholdIsChecked = true;
                LargeWinRatioThreshold = _initialLargeWinRatioThreshold;
            }

            if (MaxBetLimitVisible && PropertyHasErrors(nameof(MaxBetLimit)))
            {
                MaxBetLimitIsChecked = true;
                MaxBetLimit = _initialMaxBetLimit;
            }

            if (MaxBetLimit != _initialMaxBetLimit)
            {
                hasChanges = true;
                PropertiesManager.SetProperty(AccountingConstants.MaxBetLimit, MaxBetLimit.DollarsToMillicents());
            }

            if (HandCountPayoutLimitVisible && PropertyHasErrors(nameof(HandCountPayoutLimit)))
            {
                HandCountPayoutLimitIsChecked = true;
                HandCountPayoutLimit = _initialHandCountPayoutLimit;
            }

            if (BillAcceptanceLimitVisible && PropertyHasErrors(nameof(BillAcceptanceLimit)))
            {
                BillAcceptanceLimitIsChecked = true;
                BillAcceptanceLimit = _initialBillAcceptanceLimit;
            }

            if (HandCountPayoutLimit != _initialHandCountPayoutLimit)
            {
                hasChanges = true;
                PropertiesManager.SetProperty(AccountingConstants.HandCountPayoutLimit, HandCountPayoutLimit.DollarsToMillicents());
            }

            if (BillAcceptanceLimit != _initialBillAcceptanceLimit)
            {
                hasChanges = true;
                PropertiesManager.SetProperty(PropertyKey.MaxCreditsIn, BillAcceptanceLimit.DollarsToMillicents());
            }

            if (CelebrationLockupLimitVisible && PropertyHasErrors(nameof(CelebrationLockupLimit)))
            {
                CelebrationLockupLimitIsChecked = true;
                CelebrationLockupLimit = Math.Min(_initialCelebrationLockupLimit, CurrentMaximumLockupLimit);
            }

            if (CreditLimit != _initialCreditLimit)
            {
                hasChanges = true;
                PropertiesManager.SetProperty(AccountingConstants.MaxCreditMeter, CreditLimit.DollarsToMillicents());
                EventBus.Publish(new CreditLimitUpdatedEvent());
            }

            if (HandpayLimitVisible && HandpayLimit != _initialHandpayLimit)
            {
                hasChanges = true;
                PropertiesManager.SetProperty(AccountingConstants.HandpayLimit, HandpayLimit.DollarsToMillicents());
            }

            if (LargeWinLimitVisible && LargeWinLimit != _initialLargeWinLimit)
            {
                hasChanges = true;
                PropertiesManager.SetProperty(AccountingConstants.LargeWinLimit, LargeWinLimit.DollarsToMillicents());
            }

            if (LargeWinRatioVisible && LargeWinRatio != _initialLargeWinRatio)
            {
                hasChanges = true;
                PropertiesManager.SetProperty(AccountingConstants.LargeWinRatio, (long)(LargeWinRatio * 100.0m));
            }

            if (LargeWinRatioThresholdVisible && LargeWinRatioThreshold != _initialLargeWinRatioThreshold)
            {
                hasChanges = true;
                PropertiesManager.SetProperty(AccountingConstants.LargeWinRatioThreshold, LargeWinRatioThreshold.DollarsToMillicents());
            }

            if (CelebrationLockupLimitVisible && CelebrationLockupLimit != _initialCelebrationLockupLimit)
            {
                hasChanges = true;
                PropertiesManager.SetProperty(AccountingConstants.CelebrationLockupLimit, CelebrationLockupLimit.DollarsToMillicents());
            }

            if (AllowRemoteHandpayResetVisible && AllowRemoteHandpayReset != PropertiesManager.GetValue(AccountingConstants.RemoteHandpayResetAllowed, true))
            {
                hasChanges = true;
                PropertiesManager.SetProperty(AccountingConstants.RemoteHandpayResetAllowed, AllowRemoteHandpayReset);
            }

            if (LargeWinHandpayResetMethodVisible)
            {
                var selectedLargeWinHandpayResetMethod = 0;
                foreach (var item in LargeWinHandpayResetMethods)
                {
                    if (item.Equals(SelectedLargeWinHandpayResetMethod))
                    {
                        break;
                    }

                    selectedLargeWinHandpayResetMethod++;
                }

                if (selectedLargeWinHandpayResetMethod != PropertiesManager.GetValue(
                        AccountingConstants.LargeWinHandpayResetMethod,
                        (int)LargeWinHandpayResetMethod.PayByHand))
                {
                    hasChanges = true;
                    PropertiesManager.SetProperty(
                        AccountingConstants.LargeWinHandpayResetMethod,
                        selectedLargeWinHandpayResetMethod < LargeWinHandpayResetMethods.Count
                            ? selectedLargeWinHandpayResetMethod
                            : (int)LargeWinHandpayResetMethod.PayByHand);
                }
            }

            if (GambleAllowed)
            {
                if (GambleWagerLimitConfigurable && GambleWagerLimit != _initialGambleWagerLimit)
                {
                    PropertiesManager.SetProperty(GamingConstants.GambleWagerLimit, GambleWagerLimit.DollarsToMillicents());
                    hasChanges = true;
                }

                if (GambleWinLimitConfigurable && GambleWinLimit != _initialGambleWinLimit)
                {
                    PropertiesManager.SetProperty(GamingConstants.GambleWinLimit, GambleWinLimit.DollarsToMillicents());
                    hasChanges = true;
                }
            }

            if (IncrementThresholdVisible)
            {
                var previousCheckedStatus = (bool)PropertiesManager.GetProperty(AccountingConstants.IncrementThresholdIsChecked, IncrementThresholdIsChecked);

                if (IncrementThresholdIsChecked != previousCheckedStatus)
                {
                    PropertiesManager.SetProperty(AccountingConstants.IncrementThresholdIsChecked, IncrementThresholdIsChecked);
                    hasChanges = true;
                }

                if (IncrementThresholdIsChecked && IncrementThreshold != _initialIncrementThreshold)
                {
                    if (PropertyHasErrors(nameof(IncrementThreshold)))
                    {
                        PropertiesManager.SetProperty(AccountingConstants.IncrementThreshold, IncrementThreshold.DollarsToMillicents());
                        hasChanges = true;
                    }
                    else
                    {
                        IncrementThreshold = _initialIncrementThreshold;
                    }
                }
            }

            if (hasChanges)
            {
                EventBus.Publish(new OperatorMenuSettingsChangedEvent());
            }
        }

        private void ValidateFields(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(CreditLimit):
                    ValidateProperty(CreditLimit, nameof(CreditLimit));
                    ValidateProperty(CelebrationLockupLimit, nameof(CelebrationLockupLimit));
                    break;
                case nameof(HandpayLimit):
                    ValidateProperty(HandpayLimit, nameof(HandpayLimit));
                    ValidateProperty(CelebrationLockupLimit, nameof(CelebrationLockupLimit));
                    break;
                case nameof(LargeWinLimit):
                    ValidateProperty(HandpayLimit, nameof(HandpayLimit));
                    ValidateProperty(LargeWinLimit, nameof(LargeWinLimit));
                    ValidateProperty(CelebrationLockupLimit, nameof(CelebrationLockupLimit));
                    break;
                case nameof(LargeWinRatio):
                    ValidateProperty(LargeWinRatio, nameof(LargeWinRatio));
                    break;
                case nameof(LargeWinRatioThreshold):
                    ValidateProperty(LargeWinRatioThreshold, nameof(LargeWinRatioThreshold));
                    break;
                case nameof(MaxBetLimit):
                    ValidateProperty(MaxBetLimit, nameof(MaxBetLimit));
                    break;
                case nameof(CelebrationLockupLimit):
                    ValidateProperty(CelebrationLockupLimit, nameof(CelebrationLockupLimit));
                    break;
                case nameof(GambleWagerLimit):
                    ValidateProperty(GambleWagerLimit, nameof(GambleWagerLimit));
                    break;
                case nameof(GambleWinLimit):
                    ValidateProperty(GambleWinLimit, nameof(GambleWinLimit));
                    break;
                case nameof(IncrementThreshold):
                    ValidateProperty(IncrementThreshold, nameof(IncrementThreshold));
                    break;
                case nameof(HandCountPayoutLimit):
                    ValidateProperty(HandCountPayoutLimit, nameof(HandCountPayoutLimit));
                    break;
                case nameof(BillAcceptanceLimit):
                    ValidateProperty(BillAcceptanceLimit, nameof(BillAcceptanceLimit));
                    break;
            }

            UpdateLimits();
            OnPropertyChanged(nameof(LargeWinLimitCheckboxIsEnabled));
            OnPropertyChanged(nameof(LargeWinLimitEditable));
            OnPropertyChanged(nameof(LargeWinRatioCheckboxIsEnabled));
            OnPropertyChanged(nameof(LargeWinRatioThresholdCheckboxIsEnabled));
            OnPropertyChanged(nameof(MaxBetLimitCheckboxIsEnabled));
            OnPropertyChanged(nameof(CelebrationLockupLimitCheckboxIsEnabled));
        }

        public static ValidationResult ValidateCreditLimit(decimal creditLimit, ValidationContext context)
        {
            LimitsPageViewModel instance = (LimitsPageViewModel)context.ObjectInstance;
            var errors = creditLimit.Validate(false, instance._maxCreditMeter);

            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }

            return new(errors);
        }

        public static ValidationResult ValidateIncrementThreshold(decimal incrementThreshhold, ValidationContext context)
        {
            LimitsPageViewModel instance = (LimitsPageViewModel)context.ObjectInstance;
            var errors = incrementThreshhold.Validate(
                false,
                instance.CreditLimit.DollarsToMillicents(),
                AccountingConstants.MinimumIncrementThreshold);

            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }

            return new(errors);
        }

        public static ValidationResult ValidateHandpayLimit(decimal handpayLimit, ValidationContext context)
        {
            LimitsPageViewModel instance = (LimitsPageViewModel)context.ObjectInstance;
            var errors = handpayLimit.Validate(
                false,
                AccountingConstants.DefaultHandpayLimit,
                instance.LargeWinLimit.DollarsToMillicents());

            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }

            return new(errors);
        }

        public static ValidationResult ValidateLargeWinLimit(decimal largeWinLimit, ValidationContext context)
        {
            LimitsPageViewModel instance = (LimitsPageViewModel)context.ObjectInstance;
            var errors = largeWinLimit.Validate(false, instance.LargeWinLimitMaxValue);
            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }

            return new(errors);
        }

        public static ValidationResult ValidateLargeWinRatio(decimal largeWinRatio, ValidationContext context)
        {
            LimitsPageViewModel instance = (LimitsPageViewModel)context.ObjectInstance;
            var errors = largeWinRatio.ValidateDecimal(AccountingConstants.DefaultLargeWinRatio / 100.0m, AccountingConstants.MaximumLargeWinRatio / 100.0m);
            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }

            return new(errors);
        }

        public static ValidationResult ValidateLargeWinRatioThreshold(decimal largeWinRatioThreshold, ValidationContext context)
        {
            var errors = largeWinRatioThreshold.Validate(true);
            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }

            return new(errors);
        }

        public static ValidationResult ValidateMaxBetLimit(decimal maxBetLimit, ValidationContext context)
        {
            LimitsPageViewModel instance = (LimitsPageViewModel)context.ObjectInstance;
            var errors = maxBetLimit.Validate(false, instance.PropertiesManager.GetValue(AccountingConstants.HighestMaxBetLimitAllowed, long.MaxValue));
            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }

            return new(errors);
        }

        public static ValidationResult ValidateCelebrationLockupLimit(decimal celebrationLockupLimit, ValidationContext context)
        {
            LimitsPageViewModel instance = (LimitsPageViewModel)context.ObjectInstance;
            var errors = celebrationLockupLimit.Validate(true, instance.CurrentMaximumLockupLimit.DollarsToMillicents());
            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }

            return new(errors);
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        public static ValidationResult ValidateGambleWagerLimit(decimal gambleWagerLimit, ValidationContext context)
        {
            var errors = gambleWagerLimit.Validate();
            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }

            return new(errors);
        }

        public static ValidationResult ValidateBillAcceptanceLimit(decimal billAcceptanceLimit, ValidationContext context)
        {
            return billAcceptanceLimit switch
            {
                < ApplicationConstants.MaxCreditsInMin => new(Localizer.For(CultureFor.Player).GetString(ResourceKeys.MaxCreditsInInvalid)),
                > ApplicationConstants.MaxCreditsInMax => new(string.Format(Localizer.For(CultureFor.Player).GetString(ResourceKeys.LessThanOrEqualErrorMessage), ApplicationConstants.MaxCreditsInMax.FormattedCurrencyString())),
                _ => ValidationResult.Success
            };
        }

        public static ValidationResult ValidateHandCountPayoutLimit(decimal handCountPayoutLimit, ValidationContext context)
        {
            var errors = handCountPayoutLimit.Validate(true, AccountingConstants.MaximumHandCountPayoutLimit);
            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }

            return new(errors);
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        public static ValidationResult ValidateGambleWinLimit(decimal gambleWinLimit, ValidationContext context)
        {
            var errors = gambleWinLimit.Validate();
            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }

            return new(errors);
        }

        private void UpdateLimits()
        {
            OnPropertyChanged(nameof(MaxBetLimit));
            OnPropertyChanged(nameof(CreditLimit));
            OnPropertyChanged(nameof(HandpayLimit));
            OnPropertyChanged(nameof(HandCountPayoutLimit));
            OnPropertyChanged(nameof(BillAcceptanceLimit));
            OnPropertyChanged(nameof(LargeWinLimit));
            OnPropertyChanged(nameof(LargeWinRatio));
            OnPropertyChanged(nameof(LargeWinRatioThreshold));
            OnPropertyChanged(nameof(CelebrationLockupLimit));
            OnPropertyChanged(nameof(GambleWagerLimit));
            OnPropertyChanged(nameof(GambleWinLimit));
        }

        protected override void OnInputStatusChanged()
        {
            if (!InputEnabled)
            {
                CloseTouchScreenKeyboard();
            }

            OnPropertyChanged(nameof(PageEnabled));
            OnPropertyChanged(nameof(LargeWinLimitEditable));
            OnPropertyChanged(nameof(LargeWinLimitCheckboxIsEnabled));
            OnPropertyChanged(nameof(LargeWinRatioCheckboxIsEnabled));
            OnPropertyChanged(nameof(LargeWinRatioThresholdCheckboxIsEnabled));
            OnPropertyChanged(nameof(BillAcceptanceLimitCheckboxEnabled));
            OnPropertyChanged(nameof(MaxBetLimitCheckboxIsEnabled));
            OnPropertyChanged(nameof(CelebrationLockupLimitCheckboxIsEnabled));
            OnPropertyChanged(nameof(AllowRemoteHandpayResetIsEnabled));
            OnPropertyChanged(nameof(CreditLimitCheckboxEnabled));
            OnPropertyChanged(nameof(HandpayLimitCheckboxEnabled));
            OnPropertyChanged(nameof(GambleWagerLimit));
            OnPropertyChanged(nameof(GambleWinLimit));
        }

        protected override void OnInputEnabledChanged()
        {
            OnInputStatusChanged();
        }

        private void HandleEvent(PropertyChangedEvent @event)
        {
            switch (@event.PropertyName)
            {
                case AccountingConstants.MaxCreditMeter:
                    CreditLimit = ((long)PropertiesManager.GetProperty(AccountingConstants.MaxCreditMeter, long.MaxValue))
                        .MillicentsToDollars();
                    break;
            }
        }
    }
}