namespace Aristocrat.Monaco.Application.UI.ViewModels.NoteAcceptor
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
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
    using Toolkit.Mvvm.Extensions;

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
                OnPropertyChanged(nameof(ChangeFocus));
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
                    OnPropertyChanged(nameof(IsDenomEditable));
                    OnPropertyChanged(nameof(CanEgmModifyDenominations));
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

                    OnPropertyChanged(nameof(ExcessiveRejectDisable));
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
                    OnPropertyChanged(nameof(ExcessiveRejectDisableIsVisible));
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
                    OnPropertyChanged(nameof(VoucherInEnabledText));
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

                OnPropertyChanged(nameof(AllowBillIn));
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
                OnPropertyChanged(nameof(AllowBillInEnabled));
            }
        }

        public bool IsVoucherInEnabled { get; set; }

        private bool IsDenominationsVisible =>
            NoteAcceptor != null && NoteAcceptor.LogicalState != NoteAcceptorLogicalState.Uninitialized &&
            NoteAcceptor.LogicalState != NoteAcceptorLogicalState.Inspecting &&
            _noteAcceptorDiagnosticsEnabled;

        [CustomValidation(typeof(NoteAcceptorViewModel), nameof(BillAcceptanceLimitValidate))]
        public decimal BillAcceptanceLimit
        {
            get => _billAcceptanceLimit;
            set => SetProperty(ref _billAcceptanceLimit, value, true);
        }

        // Flag which specifies whether to show the Bill Acceptor Limit Field
        public bool HideBillAcceptorLimitField { get; private set; }

        public bool CanEditBillAcceptanceLimit
        {
            get => _canEditBillAcceptanceLimit;
            set => this.SetProperty(
                ref _canEditBillAcceptanceLimit,
                value,
                OnPropertyChanged,
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
                OnPropertyChanged(nameof(StackerStateText));
            }
        }

        public string EnabledDenominationsText
        {
            get => _enabledDenominationsText;

            set
            {
                _enabledDenominationsText = value;
                OnPropertyChanged(nameof(EnabledDenominationsText));
            }
        }

        public string LastDocumentResultText
        {
            get => _lastDocumentResultText;

            set
            {
                _lastDocumentResultText = value;
                OnPropertyChanged(nameof(LastDocumentResultText));
            }
        }

        public List<ConfigurableDenomination> Denominations { get; }


        public bool ReturnButtonEnabled
        {
            get => _returnButtonEnabled;

            set
            {
                _returnButtonEnabled = value;
                OnPropertyChanged(nameof(ReturnButtonEnabled));
            }
        }

        public bool StackButtonEnabled
        {
            get => _stackButtonEnabled;

            set
            {
                _stackButtonEnabled = value;
                OnPropertyChanged(nameof(StackButtonEnabled));
            }
        }

        public bool StackButtonVisible
        {
            get => _stackButtonVisible;

            set
            {
                _stackButtonVisible = value;
                OnPropertyChanged(nameof(StackButtonVisible));
            }
        }

        public string VariantNameText
        {
            get => _variantNameText;

            set
            {
                _variantNameText = value;
                OnPropertyChanged(nameof(VariantNameText));
                OnPropertyChanged(nameof(VariantNameForeground));
            }
        }

        public string VariantVersionText
        {
            get => _variantVersionText;

            set
            {
                _variantVersionText = value;
                OnPropertyChanged(nameof(VariantVersionText));
                OnPropertyChanged(nameof(VariantVersionForeground));
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
                OnPropertyChanged(nameof(SelfTestButtonVisible));
            }
        }

        public bool SelfTestClearNvmButtonVisible
        {
            get => _selfTestClearNvmButtonVisible;

            set
            {
                _selfTestClearNvmButtonVisible = value;
                OnPropertyChanged(nameof(SelfTestClearNvmButtonVisible));
            }
        }

        public bool SelfTestStatusVisible
        {
            get => _selfTestStatusVisible;

            set
            {
                _selfTestStatusVisible = value;
                OnPropertyChanged(nameof(SelfTestStatusVisible));
            }
        }

        public bool InspectButtonVisible
        {
            get => _inspectButtonVisible;

            set
            {
                _inspectButtonVisible = value;
                OnPropertyChanged(nameof(InspectButtonVisible));
            }
        }

        public bool ReturnButtonVisible
        {
            get => _returnButtonVisible;

            set
            {
                _returnButtonVisible = value;
                OnPropertyChanged(nameof(ReturnButtonVisible));
            }
        }

        public bool SetDenominationsButtonVisible
        {
            get => _setDenominationsButtonVisible;

            set
            {
                _setDenominationsButtonVisible = value;
                OnPropertyChanged(nameof(SetDenominationsButtonVisible));
            }
        }

        public bool InspectButtonFocused
        {
            get => _inspectButtonFocused;

            set
            {
                _inspectButtonFocused = value;
                OnPropertyChanged(nameof(InspectButtonFocused));
            }
        }

        private INoteAcceptor NoteAcceptor => ServiceManager.GetInstance().TryGetService<INoteAcceptor>();

        private bool HasDocumentCheckFault => NoteAcceptor?.WasStackingOnLastPowerUp ?? false;

        public override bool TestModeEnabledSupplementary =>  (NoteAcceptor?.Connected ?? false) &&
            NoteAcceptor?.StackerState != NoteAcceptorStackerState.Removed;


        public bool TestModeToolTipDisabled => (NoteAcceptor?.ReasonDisabled.HasFlag(DisabledReasons.GamePlay) ?? false) || TestModeEnabled;

        public static ValidationResult BillAcceptanceLimitValidate(decimal billAcceptanceLimit)
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

            if (string.IsNullOrEmpty(error))
            {
                return ValidationResult.Success;
            }
            return new(error);
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
    }
}