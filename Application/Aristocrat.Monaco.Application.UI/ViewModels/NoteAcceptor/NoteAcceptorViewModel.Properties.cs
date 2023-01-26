namespace Aristocrat.Monaco.Application.UI.ViewModels.NoteAcceptor
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;
    using Common;
    using Contracts;
    using Contracts.Extensions;
    using Contracts.HardwareDiagnostics;
    using Contracts.Localization;
    using Events;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Monaco.Localization.Properties;

    public partial class NoteAcceptorViewModel
    {
        private const int InspectionTimeout = 30000; // 30 seconds
        private readonly Dictionary<string, StatusMode> _statusMessages = new Dictionary<string, StatusMode>();

        private bool _billAcceptanceLimitIsChecked;
        private bool _canEditBillAcceptanceLimit;
        private decimal _initialBillAcceptanceLimit;
        private decimal _billAcceptanceLimit;
        private string _enabledDenominationsText;
        private bool _inspectButtonFocused;
        private bool _inspectButtonVisible;
        private bool _inspecting;
        private string _lastDocumentResultText;
        private bool _noteAcceptorDiagnosticsEnabled;
        private bool _returnButtonEnabled;
        private bool _returnButtonVisible;
        private bool _selfTestButtonVisible;
        private bool _selfTestClearNvmButtonVisible;
        private bool _selfTestStatusVisible;
        private bool _setDenominationsButtonVisible;
        private bool _stackButtonEnabled;
        private bool _stackButtonVisible;
        private string _stackerStateText;
        private string _variantNameText;
        private string _variantVersionText;
        private bool _isDenomEditable;
        private bool _excessiveRejectDisable;
        private bool _excessiveRejectDisableIsVisible;
        private bool _changeFocus;
        private bool _allowBillIn;
        private bool _allowBillInEnabled;
        private string _voucherInEnabledText;
        private bool _inTestMode;

        public bool IsNoteAcceptorConnected => NoteAcceptor != null;

        public bool ChangeFocus
        {
            get => _changeFocus;
            set
            {
                _changeFocus = value;
                RaisePropertyChanged(nameof(ChangeFocus));
            }
        }

        public bool IsDenomEditable
        {
            get => _isDenomEditable;
            set
            {
                if (_isDenomEditable != value)
                {
                    _isDenomEditable = value;
                    RaisePropertyChanged(nameof(IsDenomEditable));
                    RaisePropertyChanged(nameof(CanEgmModifyDenominations));
                }
            }
        }

        public bool ExcessiveRejectDisable
        {
            get => _excessiveRejectDisable;
            set
            {
                if (_excessiveRejectDisable != value)
                {
                    _excessiveRejectDisable = value;
                    if (!_excessiveRejectDisable)
                    {
                        PropertiesManager.SetProperty(ApplicationConstants.ExcessiveDocumentRejectCount, -1);
                    }
                    else
                    {
                        var excessiveDocumentRejectCountDefault = PropertiesManager.GetValue(ApplicationConstants.ExcessiveDocumentRejectCountDefault, -1);
                        PropertiesManager.SetProperty(ApplicationConstants.ExcessiveDocumentRejectCount, excessiveDocumentRejectCountDefault);
                    }

                    RaisePropertyChanged(nameof(ExcessiveRejectDisable));
                }
            }
        }

        public bool ExcessiveRejectDisableIsVisible
        {
            get => _excessiveRejectDisableIsVisible;
            set
            {
                if (_excessiveRejectDisableIsVisible != value)
                {
                    _excessiveRejectDisableIsVisible = value;
                    RaisePropertyChanged(nameof(ExcessiveRejectDisableIsVisible));
                }
            }
        }

        public string VoucherInEnabledText
        {
            get => _voucherInEnabledText;
            set
            {
                if (_voucherInEnabledText != value)
                {
                    _voucherInEnabledText = value;
                    RaisePropertyChanged(nameof(VoucherInEnabledText));
                }
            }
        }

        public Brush VoucherInEnabledTextForeground => Brushes.White;

        public bool CanEgmModifyDenominations => IsDenomEditable && InputEnabled;  // bound to view

        public bool AllowBillIn
        {
            get => _allowBillIn;
            set
            {
                if (_allowBillIn == value)
                {
                    return;
                }

                _allowBillIn = value;

                foreach (var denom in Denominations.Where(denom => denom.Enabled))
                {
                    denom.Selected = _allowBillIn;
                }

                RaisePropertyChanged(nameof(AllowBillIn));
            }
        }

        public bool AllowBillInEnabled
        {
            get => _allowBillInEnabled;
            set
            {
                if (_allowBillInEnabled == value)
                {
                    return;
                }

                _allowBillInEnabled = value;
                RaisePropertyChanged(nameof(AllowBillInEnabled));
            }
        }

        public bool IsVoucherInEnabled { get; set; }

        private bool IsDenominationsVisible =>
            NoteAcceptor != null && NoteAcceptor.LogicalState != NoteAcceptorLogicalState.Uninitialized &&
            NoteAcceptor.LogicalState != NoteAcceptorLogicalState.Inspecting &&
            _noteAcceptorDiagnosticsEnabled;

        public decimal BillAcceptanceLimit
        {
            get => _billAcceptanceLimit;
            set
            {
                if (_billAcceptanceLimit == value)
                {
                    return;
                }

                if (SetProperty(ref _billAcceptanceLimit, value, nameof(BillAcceptanceLimit)))
                {
                    SetError(nameof(BillAcceptanceLimit), ValidateBillAcceptanceLimit(value));
                }
            }
        }

        // Flag which specifies whether to show the Bill Acceptor Limit Field
        public bool HideBillAcceptorLimitField { get; private set; }

        public bool CanEditBillAcceptanceLimit
        {
            get => _canEditBillAcceptanceLimit;
            set => SetProperty(
                ref _canEditBillAcceptanceLimit,
                value,
                nameof(CanEditBillAcceptanceLimit),
                nameof(BillAcceptanceLimitCheckboxIsEnabled));
        }

        public bool BillAcceptanceLimitIsChecked
        {
            get => _billAcceptanceLimitIsChecked;
            set
            {
                if (SetProperty(ref _billAcceptanceLimitIsChecked, value, nameof(BillAcceptanceLimitIsChecked)))
                {
                    if (value)
                    {
                        BillAcceptanceLimit = _initialBillAcceptanceLimit;
                    }
                    else
                    {
                        BillAcceptanceLimit = ApplicationConstants.DefaultMaxCreditsIn.MillicentsToDollars();
                    }
                }
            }
        }

        public bool BillAcceptanceLimitCheckboxIsEnabled => CanEditBillAcceptanceLimit && InputEnabled;

        public string StackerStateText
        {
            get => _stackerStateText;

            set
            {
                _stackerStateText = value;
                RaisePropertyChanged(nameof(StackerStateText));
            }
        }

        public string EnabledDenominationsText
        {
            get => _enabledDenominationsText;

            set
            {
                _enabledDenominationsText = value;
                RaisePropertyChanged(nameof(EnabledDenominationsText));
            }
        }

        public string LastDocumentResultText
        {
            get => _lastDocumentResultText;

            set
            {
                _lastDocumentResultText = value;
                RaisePropertyChanged(nameof(LastDocumentResultText));
            }
        }

        public List<ConfigurableDenomination> Denominations { get; }


        public bool ReturnButtonEnabled
        {
            get => _returnButtonEnabled;

            set
            {
                _returnButtonEnabled = value;
                RaisePropertyChanged(nameof(ReturnButtonEnabled));
            }
        }

        public bool StackButtonEnabled
        {
            get => _stackButtonEnabled;

            set
            {
                _stackButtonEnabled = value;
                RaisePropertyChanged(nameof(StackButtonEnabled));
            }
        }

        public bool StackButtonVisible
        {
            get => _stackButtonVisible;

            set
            {
                _stackButtonVisible = value;
                RaisePropertyChanged(nameof(StackButtonVisible));
            }
        }

        public string VariantNameText
        {
            get => _variantNameText;

            set
            {
                _variantNameText = value;
                RaisePropertyChanged(nameof(VariantNameText));
                RaisePropertyChanged(nameof(VariantNameForeground));
            }
        }

        public string VariantVersionText
        {
            get => _variantVersionText;

            set
            {
                _variantVersionText = value;
                RaisePropertyChanged(nameof(VariantVersionText));
                RaisePropertyChanged(nameof(VariantVersionForeground));
            }
        }

        public SolidColorBrush VariantNameForeground => Brushes.White;

        public SolidColorBrush VariantVersionForeground => Brushes.White;

        public bool SelfTestButtonVisible
        {
            get => _selfTestButtonVisible;

            set
            {
                _selfTestButtonVisible = value;
                RaisePropertyChanged(nameof(SelfTestButtonVisible));
            }
        }

        public bool SelfTestClearNvmButtonVisible
        {
            get => _selfTestClearNvmButtonVisible;

            set
            {
                _selfTestClearNvmButtonVisible = value;
                RaisePropertyChanged(nameof(SelfTestClearNvmButtonVisible));
            }
        }

        public bool SelfTestStatusVisible
        {
            get => _selfTestStatusVisible;

            set
            {
                _selfTestStatusVisible = value;
                RaisePropertyChanged(nameof(SelfTestStatusVisible));
            }
        }

        public bool InspectButtonVisible
        {
            get => _inspectButtonVisible;

            set
            {
                _inspectButtonVisible = value;
                RaisePropertyChanged(nameof(InspectButtonVisible));
            }
        }

        public bool ReturnButtonVisible
        {
            get => _returnButtonVisible;

            set
            {
                _returnButtonVisible = value;
                RaisePropertyChanged(nameof(ReturnButtonVisible));
            }
        }

        public bool SetDenominationsButtonVisible
        {
            get => _setDenominationsButtonVisible;

            set
            {
                _setDenominationsButtonVisible = value;
                RaisePropertyChanged(nameof(SetDenominationsButtonVisible));
            }
        }

        public bool InspectButtonFocused
        {
            get => _inspectButtonFocused;

            set
            {
                _inspectButtonFocused = value;
                RaisePropertyChanged(nameof(InspectButtonFocused));
            }
        }

        private INoteAcceptor NoteAcceptor => ServiceManager.GetInstance().TryGetService<INoteAcceptor>();

        private bool HasDocumentCheckFault => NoteAcceptor?.WasStackingOnLastPowerUp ?? false;

        public override bool TestModeEnabledSupplementary =>  (NoteAcceptor?.Connected ?? false) &&
            NoteAcceptor?.StackerState != NoteAcceptorStackerState.Removed;


        public bool TestModeToolTipDisabled => (NoteAcceptor?.ReasonDisabled.HasFlag(DisabledReasons.GamePlay) ?? false) || TestModeEnabled;

        public string ValidateBillAcceptanceLimit(decimal billAcceptanceLimit)
        {
            string error = string.Empty;
            if (billAcceptanceLimit > ApplicationConstants.MaxCreditsInMax)
            {
                error = string.Format(Localizer.For(CultureFor.Player).GetString(ResourceKeys.LessThanOrEqualErrorMessage), ApplicationConstants.MaxCreditsInMax.FormattedCurrencyString());
            }

            if (billAcceptanceLimit < ApplicationConstants.MaxCreditsInMin)
            {
                error = Localizer.For(CultureFor.Player).GetString(ResourceKeys.MaxCreditsInInvalid);
            }

            return error;
        }

        public NoteAcceptorTestViewModel TestViewModel { get; } = new NoteAcceptorTestViewModel();

        public bool InTestMode
        {
            get => _inTestMode;
            set
            {
                if (_inTestMode == value)
                {
                    return;
                }

                TestViewModel.TestMode = value;
                if (!value)
                {
                    if (_inTestMode)
                    {
                        EventBus.Publish(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.NoteAcceptor));
                    }

                    UpdateStatusText();
                }
                else
                {
                    EventBus.Publish(new HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory.NoteAcceptor));
                    EventBus.Publish(new OperatorMenuWarningMessageEvent(""));
                }

                SetProperty(ref _inTestMode, value, nameof(InTestMode));
            }
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
    }
}