namespace Aristocrat.Monaco.Accounting.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Kernel;
    using Kernel.Contracts;
    using Kernel.MarketConfig.Models.Application;
    using Localization.Properties;
    using Models;
    using MVVM;

    [CLSCompliant(false)]
    public class VoucherSettingsPageViewModel : OperatorMenuPageViewModelBase
    {
        private bool _allowNonCashVoucherOut;
        private bool _allowVoucherOut;
        private bool _allowVoucherOutIsEnabled;
        private bool _allowVoucherIn;
        private int _nonCashVoucherExpirationDays;
        private bool _printHandpayReceipt;
        private bool _handpayReceiptEditable;
        private string _propertyAddress1;
        private string _propertyAddress2;
        private string _propertyName;
        private BarcodeTypeData _selectedBarcodeType;
        private string _selectedLayoutType;
        private string _selectedValidationLength;
        private int _voucherExpirationDays;
        private decimal _voucherInLimit;
        private bool _voucherInLimitEnabledChecked;
        private bool _voucherInLimitCheckboxEnabled;
        private decimal _voucherOutLimit;
        private bool _voucherOutLimitEnabledChecked;
        private bool _voucherOutLimitCheckboxEnabled;
        private bool _voucherExpirationEditable;
        private bool _isNonCashableVoucherOutVisible;
        private bool _isCashableVoucherExpirationVisible;
        private bool _arePropertyFieldsEnabled;
        private bool _printerEnabled;

        public VoucherSettingsPageViewModel()
        {
            LoadLocalizedLists();

            VoucherInLimitEditable = (bool)PropertiesManager.GetProperty(AccountingConstants.VoucherInLimitEditable, true);
            VoucherOutLimitEditable = (bool)PropertiesManager.GetProperty(AccountingConstants.VoucherOutLimitEditable, true);
        }

        public bool AllowVoucherIn
        {
            get => _allowVoucherIn;
            set
            {
                _allowVoucherIn = value;
                RaisePropertyChanged(nameof(AllowVoucherIn));
            }
        }

        public decimal VoucherInLimit
        {
            get => _voucherInLimit;
            set
            {
                if (PreviousVoucherInLimit != value)
                {
                    PreviousVoucherInLimit = _voucherInLimit;
                }

                if (SetProperty(ref _voucherInLimit, value, nameof(VoucherInLimit)))
                {
                    SetError(nameof(VoucherInLimit), _voucherInLimit.Validate(true, MaxVoucherInAllowed));
                }
            }
        }

        public decimal PreviousVoucherInLimit { get; set; }

        public bool VoucherInLimitCheckboxEnabled
        {
            get => _voucherInLimitCheckboxEnabled;
            set
            {
                _voucherInLimitCheckboxEnabled = value;
                RaisePropertyChanged(nameof(VoucherInLimitCheckboxEnabled));
            }
        }

        public bool VoucherInLimitEnabledChecked
        {
            get => _voucherInLimitEnabledChecked;
            set
            {
                _voucherInLimitEnabledChecked = value;

                var voucherInLimit = ((long)PropertiesManager.GetProperty(AccountingConstants.VoucherInLimit, 0))
                    .MillicentsToDollars();
                if (_voucherInLimitEnabledChecked && voucherInLimit == MaxVoucherInAllowed.MillicentsToDollars())
                {
                    voucherInLimit = PreviousVoucherInLimit;
                }

                VoucherInLimit = _voucherInLimitEnabledChecked && voucherInLimit <= MaxVoucherInAllowed
                    ? voucherInLimit
                    : MaxVoucherInAllowed.MillicentsToDollars();

                RaisePropertyChanged(nameof(VoucherInLimitEnabledChecked));
                PropertiesManager.SetProperty(AccountingConstants.VoucherInLimitEnabled, value);
            }
        }

        public bool VoucherInLimitEditable { get; }

        public bool AllowVoucherOutIsEnabled
        {
            get => _allowVoucherOutIsEnabled;
            set => SetProperty(ref _allowVoucherOutIsEnabled, value, nameof(AllowVoucherOutIsEnabled));
        }

        public bool AllowVoucherOut
        {
            get => _allowVoucherOut;
            set
            {
                _allowVoucherOut = value;
                if (!_allowVoucherOut)
                {
                    AllowNonCashVoucherOut = false;
                }

                RaisePropertyChanged(nameof(AllowVoucherOut));
            }
        }

        public bool PrinterEnabled
        {
            get => _printerEnabled;
            set
            {
                _printerEnabled = value;
                if (!_printerEnabled)
                {
                    AllowVoucherOut = false;
                    PrintHandpayReceipt = false;
                }

                RaisePropertyChanged(nameof(PrinterEnabled));
            }
        }

        public string PrinterDisabledWarningText => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Printer_Disabled);

        public bool IsCashableVoucherExpirationVisible
        {
            get => _isCashableVoucherExpirationVisible;
            set
            {
                _isCashableVoucherExpirationVisible = value;
                RaisePropertyChanged(nameof(IsCashableVoucherExpirationVisible));
            }
        }

        public bool IsNonCashableVoucherOutVisible
        {
            get => _isNonCashableVoucherOutVisible;
            set
            {
                _isNonCashableVoucherOutVisible = value;
                RaisePropertyChanged(nameof(IsNonCashableVoucherOutVisible));
            }
        }

        public bool ArePropertyFieldsEnabled
        {
            get => _arePropertyFieldsEnabled;
            set
            {
                _arePropertyFieldsEnabled = value;
                RaisePropertyChanged(nameof(ArePropertyFieldsEnabled));
            }
        }

        public decimal VoucherOutLimit
        {
            get => _voucherOutLimit;
            set
            {
                if (value != Math.Min(MaxCreditMeterMaxAllowed, long.MaxValue).MillicentsToDollars() &&
                    PreviousVoucherOutLimit != value)
                {
                    PreviousVoucherOutLimit = _voucherOutLimit;
                }

                if (SetProperty(ref _voucherOutLimit, value, nameof(VoucherOutLimit)))
                {
                    SetError(nameof(VoucherOutLimit),
                        _voucherOutLimit.Validate(maximum: MaxVoucherOutAllowed));
                }
            }
        }

        public decimal PreviousVoucherOutLimit { get; set; }

        public bool VoucherOutLimitCheckboxEnabled
        {
            get => _voucherOutLimitCheckboxEnabled;
            set
            {
                _voucherOutLimitCheckboxEnabled = value;
                RaisePropertyChanged(nameof(VoucherOutLimitCheckboxEnabled));
            }
        }

        public bool VoucherOutLimitEnabledChecked
        {
            get => _voucherOutLimitEnabledChecked;
            set
            {
                _voucherOutLimitEnabledChecked = value;


                var voucherOutLimit = ((long)PropertiesManager.GetProperty(AccountingConstants.VoucherOutLimit, 0))
                    .MillicentsToDollars();
                if (_voucherOutLimitEnabledChecked && voucherOutLimit == MaxVoucherOutAllowed.MillicentsToDollars())
                {
                    voucherOutLimit = PreviousVoucherOutLimit;
                }

                VoucherOutLimit = _voucherOutLimitEnabledChecked && voucherOutLimit <= MaxVoucherOutAllowed
                    ? voucherOutLimit
                    : MaxVoucherOutAllowed.MillicentsToDollars();

                RaisePropertyChanged(nameof(VoucherOutLimitEnabledChecked));
                PropertiesManager.SetProperty(AccountingConstants.VoucherOutLimitEnabled, value);
            }
        }

        public bool VoucherOutLimitEditable { get; }

        public bool AllowNonCashVoucherOut
        {
            get => _allowNonCashVoucherOut;
            set
            {
                _allowNonCashVoucherOut = value;
                RaisePropertyChanged(nameof(AllowNonCashVoucherOut));
            }
        }

        public bool HandpayReceiptEditable
        {
            get => _handpayReceiptEditable;
            set
            {
                _handpayReceiptEditable = value;
                RaisePropertyChanged(nameof(HandpayReceiptEditable));
            }
        }

        public bool PrintHandpayReceipt
        {
            get => _printHandpayReceipt;
            set
            {
                _printHandpayReceipt = value;
                RaisePropertyChanged(nameof(PrintHandpayReceipt));
            }
        }

        public ObservableCollection<BarcodeTypeData> BarcodeTypes { get; } =
            new ObservableCollection<BarcodeTypeData>();

        public BarcodeTypeData SelectedBarcodeType
        {
            get => _selectedBarcodeType;
            set
            {
                if (SetProperty(ref _selectedBarcodeType, value))
                {
                    PropertiesManager.SetProperty(ApplicationConstants.BarCodeType, value.Value);
                }
            }
        }

        public ObservableCollection<string> ValidationLengths { get; } = new ObservableCollection<string>();

        public string SelectedValidationLength
        {
            get => _selectedValidationLength;
            set
            {
                _selectedValidationLength = value;
                RaisePropertyChanged(nameof(SelectedValidationLength));
            }
        }

        public ObservableCollection<string> LayoutTypes { get; } = new ObservableCollection<string>();

        public string SelectedLayoutType
        {
            get => _selectedLayoutType;
            set
            {
                _selectedLayoutType = value;
                RaisePropertyChanged(nameof(SelectedLayoutType));
            }
        }

        public int VoucherExpirationDays
        {
            get => _voucherExpirationDays;
            set
            {
                _voucherExpirationDays = value;
                RaisePropertyChanged(nameof(VoucherExpirationDays));
            }
        }

        public int NonCashVoucherExpirationDays
        {
            get => _nonCashVoucherExpirationDays;
            set
            {
                _nonCashVoucherExpirationDays = value;
                RaisePropertyChanged(nameof(NonCashVoucherExpirationDays));
            }
        }

        public string PropertyName
        {
            get => _propertyName;
            set
            {
                _propertyName = value;
                RaisePropertyChanged(nameof(PropertyName));
            }
        }

        public string PropertyAddress1
        {
            get => _propertyAddress1;
            set
            {
                _propertyAddress1 = value;
                RaisePropertyChanged(nameof(PropertyAddress1));
            }
        }

        public string PropertyAddress2
        {
            get => _propertyAddress2;
            set
            {
                _propertyAddress2 = value;
                RaisePropertyChanged(nameof(PropertyAddress2));
            }
        }

        public bool VoucherExpirationEditable
        {
            get => _voucherExpirationEditable;
            set
            {
                _voucherExpirationEditable = value;
                RaisePropertyChanged(nameof(VoucherExpirationEditable));
            }
        }

        private decimal CreditLimit { get; set; }

        public long MaxCreditMeterMaxAllowed { get; set; }

        private long MaxVoucherInAllowed { get; set; }

        private long MaxVoucherOutAllowed { get; set; }

        protected override void OnFieldAccessRestrictionChange()
        {
            switch (FieldAccessRestriction)
            {
                case OperatorMenuAccessRestriction.LogicDoor:
                    FieldAccessStatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OpenLogicDoorOptionsText);
                    break;
                case OperatorMenuAccessRestriction.MainDoor:
                case OperatorMenuAccessRestriction.MainOpticDoor:
                    FieldAccessStatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OpenMainDoorOptionsText);
                    break;
            }
        }

        protected override void OnLoaded()
        {
            ClearValidationOnUnload = true;

            MaxCreditMeterMaxAllowed = (long)PropertiesManager.GetProperty(AccountingConstants.MaxCreditMeterMaxAllowed, long.MaxValue);
            MaxVoucherInAllowed = Math.Min(MaxCreditMeterMaxAllowed, (long)PropertiesManager.GetProperty(
                AccountingConstants.VoucherInMaxAllowed, long.MaxValue));
            MaxVoucherOutAllowed = Math.Min(MaxCreditMeterMaxAllowed, (long)PropertiesManager.GetProperty(
                AccountingConstants.VoucherOutMaxAllowed, long.MaxValue));

            CreditLimit = ((long)PropertiesManager.GetProperty(AccountingConstants.MaxCreditMeter, long.MaxValue))
                .MillicentsToDollars();

            AllowVoucherIn = (bool)PropertiesManager.GetProperty(PropertyKey.VoucherIn, false);
            var voucherInLimit = Math.Min((long)PropertiesManager.GetProperty(AccountingConstants.VoucherInLimit, 0), MaxVoucherInAllowed)
                .MillicentsToDollars();
            PreviousVoucherInLimit = voucherInLimit;
            VoucherInLimitEnabledChecked = (bool)PropertiesManager.GetProperty(AccountingConstants.VoucherInLimitEnabled, true);
            VoucherInLimitCheckboxEnabled = PropertiesManager.GetValue(AccountingConstants.VoucherInMaxAllowed, long.MaxValue) == long.MaxValue;

            AllowVoucherOut = PropertiesManager.GetValue(AccountingConstants.VoucherOut, false);
            var voucherOutLimit = Math.Min((long)PropertiesManager.GetProperty(AccountingConstants.VoucherOutLimit, 0), MaxVoucherOutAllowed)
                .MillicentsToDollars();
            PreviousVoucherOutLimit = voucherOutLimit;
            VoucherOutLimitEnabledChecked = (bool)PropertiesManager.GetProperty(AccountingConstants.VoucherOutLimitEnabled, true);
            VoucherOutLimitCheckboxEnabled = PropertiesManager.GetValue(AccountingConstants.VoucherOutMaxAllowed, long.MaxValue) == long.MaxValue;

            AllowVoucherOutIsEnabled = GetConfigSetting(OperatorMenuSetting.EnableAllowVoucherOut, true);
            IsCashableVoucherExpirationVisible = GetConfigSetting(OperatorMenuSetting.ShowCashableVoucherExpiration, true);
            IsNonCashableVoucherOutVisible = GetConfigSetting(OperatorMenuSetting.ShowNonCashableVoucherOut, true);
            ArePropertyFieldsEnabled = GetConfigSetting(OperatorMenuSetting.EnablePropertyFields, true);

            VoucherExpirationDays = (int)PropertiesManager.GetProperty(
                AccountingConstants.VoucherOutExpirationDays,
                AccountingConstants.DefaultVoucherExpirationDays);

            NonCashVoucherExpirationDays = (int)PropertiesManager.GetProperty(
                AccountingConstants.VoucherOutNonCashExpirationDays,
                AccountingConstants.DefaultVoucherExpirationDays);

            PropertyName = (string)PropertiesManager.GetProperty(PropertyKey.TicketTextLine1, string.Empty);
            PropertyAddress1 = (string)PropertiesManager.GetProperty(PropertyKey.TicketTextLine2, string.Empty);
            PropertyAddress2 = (string)PropertiesManager.GetProperty(PropertyKey.TicketTextLine3, string.Empty);
            HandpayReceiptEditable = PropertiesManager.GetValue(AccountingConstants.ValidateHandpays, false) &&
                PropertiesManager.GetValue(AccountingConstants.EditableReceipts, true);
            PrintHandpayReceipt = PropertiesManager.GetValue(AccountingConstants.EnableReceipts, false);
            AllowNonCashVoucherOut = AllowVoucherOut && PropertiesManager.GetValue(
                                         AccountingConstants.VoucherOutNonCash,
                                         false);

            VoucherExpirationEditable = PropertiesManager.GetValue(AccountingConstants.EditableExpiration, true);

            PrinterEnabled = PropertiesManager.GetValue(ApplicationConstants.PrinterEnabled, false);

            EventBus?.Subscribe<PropertyChangedEvent>(this, HandleEvent);
        }

        protected override void OnUnloaded()
        {
            var hasChanges = HasChanges();

            PropertiesManager.SetProperty(PropertyKey.VoucherIn, AllowVoucherIn);

            if (AllowVoucherOut != PropertiesManager.GetValue(AccountingConstants.VoucherOut, false))
            {
                PropertiesManager.SetProperty(AccountingConstants.VoucherOut, AllowVoucherOut);
            }

            PropertiesManager.SetProperty(AccountingConstants.EnableReceipts, PrintHandpayReceipt);
            PropertiesManager.SetProperty(AccountingConstants.VoucherOutNonCash, AllowNonCashVoucherOut);

            if (VoucherInLimit.Validate(true, MaxVoucherInAllowed) is null &&
                PropertiesManager.GetValue(AccountingConstants.VoucherInLimit, 0L).MillicentsToDollars() !=
                VoucherInLimit)
            {
                PropertiesManager.SetProperty(AccountingConstants.VoucherInLimit, VoucherInLimit.DollarsToMillicents());
            }

            if (VoucherOutLimit.Validate(false, MaxVoucherOutAllowed) is null &&
                PropertiesManager.GetValue(AccountingConstants.VoucherOutLimit, 0L).MillicentsToDollars() !=
                VoucherOutLimit)
            {
                PropertiesManager.SetProperty(AccountingConstants.VoucherOutLimit, VoucherOutLimit.DollarsToMillicents());
            }

            if ((int)PropertiesManager.GetProperty(AccountingConstants.VoucherOutExpirationDays, AccountingConstants.DefaultVoucherExpirationDays) != VoucherExpirationDays)
            {
                PropertiesManager.SetProperty(AccountingConstants.VoucherOutExpirationDays, VoucherExpirationDays);
            }

            if ((int)PropertiesManager.GetProperty(AccountingConstants.VoucherOutNonCashExpirationDays, AccountingConstants.DefaultVoucherExpirationDays) != NonCashVoucherExpirationDays)
            {
                PropertiesManager.SetProperty(AccountingConstants.VoucherOutNonCashExpirationDays, NonCashVoucherExpirationDays);
            }

            PropertiesManager.SetProperty(PropertyKey.TicketTextLine1, PropertyName);
            PropertiesManager.SetProperty(PropertyKey.TicketTextLine2, PropertyAddress1);
            PropertiesManager.SetProperty(PropertyKey.TicketTextLine3, PropertyAddress2);

            if (hasChanges)
            {
                EventBus.Publish(new OperatorMenuSettingsChangedEvent());
            }

            EventBus?.Unsubscribe<PropertyChangedEvent>(this);
        }

        protected override void SetError(string propertyName, string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                ClearErrors(propertyName);
            }
            else
            {
                base.SetError(propertyName, error);
            }
        }

        private void LoadLocalizedLists()
        {
            BarcodeTypes.Clear();
            BarcodeTypes.Add(
                new BarcodeTypeData(
                    BarcodeTypeOptions.Interleave2of5,
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BarcodeInterleave2Of5)));
            var barcodeType = PropertiesManager.GetValue(ApplicationConstants.BarCodeType, BarcodeTypeOptions.Interleave2of5);
            _selectedBarcodeType = BarcodeTypes.FirstOrDefault(b => b.Value == barcodeType);

            ValidationLengths.Clear();
            ValidationLengths.Add(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.System));
            _selectedValidationLength = ValidationLengths.FirstOrDefault();

            LayoutTypes.Clear();
            LayoutTypes.Add(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExtendedLayout));
            _selectedLayoutType = LayoutTypes.FirstOrDefault();
        }

        protected override void OnOperatorCultureChanged(OperatorCultureChangedEvent evt)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                LoadLocalizedLists();
                RaisePropertyChanged(nameof(SelectedBarcodeType), nameof(SelectedValidationLength), nameof(SelectedLayoutType), nameof(PrinterDisabledWarningText));
            });

            if (UseOperatorCultureForCurrencyFormatting)
            {
                RaisePropertyChanged(nameof(CurrencyDisplayCulture));
            }

            RaisePropertyChanged(nameof(VoucherInLimit));
            RaisePropertyChanged(nameof(VoucherOutLimit));

            base.OnOperatorCultureChanged(evt);
        }

        private void HandleEvent(PropertyChangedEvent @event)
        {
            switch (@event.PropertyName)
            {
                case AccountingConstants.MaxCreditMeterMaxAllowed:
                    MaxCreditMeterMaxAllowed = (long)PropertiesManager.GetProperty(AccountingConstants.MaxCreditMeterMaxAllowed, long.MaxValue);
                    VoucherInLimitCheckboxEnabled = CreditLimit == MaxCreditMeterMaxAllowed.MillicentsToDollars();
                    VoucherOutLimitCheckboxEnabled = PropertiesManager.GetValue(AccountingConstants.VoucherOutMaxAllowed, long.MaxValue) == long.MaxValue;
                    break;
                case AccountingConstants.MaxCreditMeter:
                    CreditLimit = ((long)PropertiesManager.GetProperty(AccountingConstants.MaxCreditMeter, long.MaxValue))
                        .MillicentsToDollars();
                    VoucherInLimitCheckboxEnabled = CreditLimit == MaxCreditMeterMaxAllowed.MillicentsToDollars();
                    VoucherOutLimitCheckboxEnabled = PropertiesManager.GetValue(AccountingConstants.VoucherOutMaxAllowed, long.MaxValue) == long.MaxValue;
                    break;
                case PropertyKey.VoucherIn:
                    AllowVoucherIn = (bool)PropertiesManager.GetProperty(PropertyKey.VoucherIn, false);
                    break;
                case AccountingConstants.VoucherInLimit:
                    var voucherInLimit = Math.Min((long)PropertiesManager.GetProperty(AccountingConstants.VoucherInLimit, 0), MaxVoucherInAllowed)
                        .MillicentsToDollars();
                    PreviousVoucherInLimit = voucherInLimit;
                    VoucherInLimitEnabledChecked = (bool)PropertiesManager.GetProperty(AccountingConstants.VoucherInLimitEnabled, true);
                    break;
                case AccountingConstants.VoucherOut:
                    AllowVoucherOut = PropertiesManager.GetValue(AccountingConstants.VoucherOut, false);
                    break;
                case AccountingConstants.VoucherOutLimit:
                    var voucherOutLimit = Math.Min((long)PropertiesManager.GetProperty(AccountingConstants.VoucherOutLimit, 0), MaxVoucherOutAllowed)
                        .MillicentsToDollars();
                    PreviousVoucherOutLimit = voucherOutLimit;
                    VoucherOutLimitEnabledChecked = (bool)PropertiesManager.GetProperty(AccountingConstants.VoucherOutLimitEnabled, true);
                    break;
                case AccountingConstants.VoucherOutExpirationDays:
                    VoucherExpirationDays = (int)PropertiesManager.GetProperty(
                        AccountingConstants.VoucherOutExpirationDays,
                        AccountingConstants.DefaultVoucherExpirationDays);
                    break;
                case AccountingConstants.VoucherOutNonCashExpirationDays:
                    NonCashVoucherExpirationDays = (int)PropertiesManager.GetProperty(
                        AccountingConstants.VoucherOutNonCashExpirationDays,
                        AccountingConstants.DefaultVoucherExpirationDays);
                    break;
                case PropertyKey.TicketTextLine1:
                    PropertyName = (string)PropertiesManager.GetProperty(PropertyKey.TicketTextLine1, string.Empty);
                    break;
                case PropertyKey.TicketTextLine2:
                    PropertyAddress1 = (string)PropertiesManager.GetProperty(PropertyKey.TicketTextLine2, string.Empty);
                    break;
                case PropertyKey.TicketTextLine3:
                    PropertyAddress2 = (string)PropertiesManager.GetProperty(PropertyKey.TicketTextLine3, string.Empty);
                    break;
                case AccountingConstants.ValidateHandpays:
                    HandpayReceiptEditable = PropertiesManager.GetValue(AccountingConstants.ValidateHandpays, false);
                    break;
                case AccountingConstants.EnableReceipts:
                    PrintHandpayReceipt = PropertiesManager.GetValue(AccountingConstants.EnableReceipts, false);
                    break;
                case AccountingConstants.VoucherOutNonCash:
                    AllowNonCashVoucherOut = AllowVoucherOut && PropertiesManager.GetValue(
                                                 AccountingConstants.VoucherOutNonCash,
                                                 false);
                    break;
                case AccountingConstants.EditableExpiration:
                    VoucherExpirationEditable = PropertiesManager.GetValue(AccountingConstants.EditableExpiration, true);
                    break;
            }
        }

        private bool HasChanges()
        {
            if (PropertiesManager.GetValue(PropertyKey.VoucherIn, false) != AllowVoucherIn)
            {
                return true;
            }

            if (PropertiesManager.GetValue(AccountingConstants.VoucherOut, false) != AllowVoucherOut)
            {
                return true;
            }

            if (PropertiesManager.GetValue(AccountingConstants.EnableReceipts, false) != PrintHandpayReceipt)
            {
                return true;
            }

            if (PropertiesManager.GetValue(AccountingConstants.VoucherOutNonCash, false) != AllowNonCashVoucherOut)
            {
                return true;
            }


            if (VoucherInLimit.Validate(true, MaxVoucherInAllowed) is null &&
                PropertiesManager.GetValue(AccountingConstants.VoucherInLimit, 0L).MillicentsToDollars() !=
                VoucherInLimit)
            {
                return true;
            }

            if (VoucherOutLimit.Validate(true, MaxVoucherOutAllowed) is null &&
                PropertiesManager.GetValue(AccountingConstants.VoucherOutLimit, 0L).MillicentsToDollars() !=
                VoucherOutLimit)
            {
                return true;
            }

            if (VoucherExpirationDays > 0 && PropertiesManager.GetValue(
                    AccountingConstants.VoucherOutExpirationDays,
                    AccountingConstants.DefaultVoucherExpirationDays) != VoucherExpirationDays)
            {
                return true;
            }

            if (NonCashVoucherExpirationDays > 0 && PropertiesManager.GetValue(
                    AccountingConstants.VoucherOutNonCashExpirationDays,
                    AccountingConstants.DefaultVoucherExpirationDays) != NonCashVoucherExpirationDays)
            {
                return true;
            }

            if (PropertiesManager.GetValue(PropertyKey.TicketTextLine1, string.Empty) != PropertyName ||
                PropertiesManager.GetValue(PropertyKey.TicketTextLine2, string.Empty) != PropertyAddress1 ||
                PropertiesManager.GetValue(PropertyKey.TicketTextLine3, string.Empty) != PropertyAddress2)
            {
                return true;
            }

            return false;
        }
    }
}