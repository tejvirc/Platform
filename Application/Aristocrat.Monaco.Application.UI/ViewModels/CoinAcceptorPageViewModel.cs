namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using Monaco.Localization.Properties;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Events;
    using Hardware.Contracts.PWM;
    using Kernel;
    using Hardware.Contracts;
    using OperatorMenu;
    using System.Linq;
    using System.Collections.Generic;
    using Contracts.HardwareDiagnostics;
    using Monaco.Common;

    /// <summary>
    ///     A CoinAcceptorViewModel contains the logic for CoinAcceptorViewModel.xaml.cs
    /// </summary>
    /// <seealso cref="DeviceViewModel" />
    [CLSCompliant(false)]
    public class CoinAcceptorPageViewModel : OperatorMenuPageViewModelBase
    {
        private DivertorState _selectedDiverterDirection;
        private AcceptorState _selectedCoinEntryStates;
        private string _infoText;
        private bool _resetInfo; //to keep info text label value for hardware warnings

        private readonly ICoinAcceptor _coinAcceptor;
        private string _coinEntry;
        public Dictionary<DivertorState, string> DiverterDirections { get; }
        private int _coinToHopper;
        private int _coinToCashbox;
        private int _totalCoinIn;

        public CoinAcceptorPageViewModel()
        {
            _coinAcceptor = ServiceManager.GetInstance().TryGetService<ICoinAcceptor>();

            DiverterDirections = Enum.GetValues(typeof(DivertorState))
                .Cast<DivertorState>()
                .Where(x => x != DivertorState.None)
                .ToDictionary(k => k, v => v.GetDescription());
        }
        protected override void OnLoaded()
        {
            EventBus.Publish(new HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory.CoinAcceptor));
            PropertiesManager.SetProperty(HardwareConstants.CoinAcceptorDiagnosticMode, true);

            EventBus.Subscribe<CoinToCashboxInEvent>(this, _ => { InfoText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CoinToCashBox); CoinToCashbox++; });
            EventBus.Subscribe<CoinToHopperInEvent>(this, _ => { InfoText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CoinToHopper); CoinToHopper++; });
            EventBus.Subscribe<CoinToCashboxInsteadOfHopperEvent>(this, _ => { InfoText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CoinToCashBoxInsteadHopper); CoinToCashbox++; });
            EventBus.Subscribe<CoinToHopperInsteadOfCashboxEvent>(this, _ => { InfoText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CoinToHopperInsteadCashBox); CoinToHopper++; });
            EventBus.Subscribe<HardwareFaultEvent>(this, HandleEvent);
            EventBus.Subscribe<OperatorMenuExitingEvent>(this, HandleEvent);

            CoinEntry = "CC_62";//_coinAcceptor.DeviceName;
            SelectedDiverterDirection = _coinAcceptor.DiverterDirection;
            SelectedCoinEntryStates = AcceptorState.Reject;
            CoinToHopper = 0;
            CoinToCashbox = 0;
            TotalCoinIn = 0;
        }
        private void HandleEvent(HardwareFaultEvent e)
        {
            HandleHardwareWarningEvents(e.Fault);
            _resetInfo = true;
            SelectedCoinEntryStates = AcceptorState.Reject;
            _resetInfo = false;
            _coinAcceptor.Reset();
        }
        private void HandleEvent(OperatorMenuExitingEvent e)
        {
            PropertiesManager.SetProperty(HardwareConstants.CoinAcceptorDiagnosticMode, false);         //when user exits from operator menu while selected tab is 'Coin Acceptor'
            SelectedCoinEntryStates = AcceptorState.Reject;
        }
        protected override void OnUnloaded()
        {
            if (PropertiesManager.GetValue(HardwareConstants.CoinAcceptorDiagnosticMode, false))        //when user exits from operator menu while selected tab is not 'Coin Acceptor'
            {
                PropertiesManager.SetProperty(HardwareConstants.CoinAcceptorDiagnosticMode, false);
                SelectedCoinEntryStates = AcceptorState.Reject;
            }
            EventBus.UnsubscribeAll(this);
            _coinAcceptor.DivertMechanismOnOff();
            EventBus.Publish(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.CoinAcceptor));
        }
        private void HandleHardwareWarningEvents(CoinFaultTypes warning)
        {
            switch (warning)
            {
                case CoinFaultTypes.Optic:
                    InfoText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CoinWarningOptic);
                    break;
                case CoinFaultTypes.Invalid:
                    InfoText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CoinWarningInvalid);
                    break;
                case CoinFaultTypes.YoYo:
                    InfoText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CoinWarningYoYo);
                    break;
                case CoinFaultTypes.Divert:
                    InfoText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CoinWarningDivert);
                    break;
            }
        }
        public string CoinEntry
        {
            get => _coinEntry;
            set
            {
                if (_coinEntry == value)
                {
                    return;
                }

                _coinEntry = value;
                RaisePropertyChanged(nameof(CoinEntry));
            }
        }
        private string InfoText
        {
            get => _infoText;
            set
            {
                if (_infoText != value)
                {
                    _infoText = value;
                    UpdateStatusText();
                }
            }
        }
        protected override void UpdateStatusText()
        {
            if (!string.IsNullOrEmpty(InfoText))
            {
                EventBus.Publish(new OperatorMenuWarningMessageEvent(InfoText));
            }
            else
            {
                base.UpdateStatusText();
            }
        }
        protected override void OnTestModeEnabledChanged()
        {
            UpdateStatusText();
        }
        public DivertorState SelectedDiverterDirection
        {
            get => _selectedDiverterDirection;
            set
            {
                if (_selectedDiverterDirection == value)
                {
                    return;
                }

                _selectedDiverterDirection = value;
                RaisePropertyChanged(nameof(SelectedDiverterDirection));
                SetDiverterDirection(_selectedDiverterDirection);
                if (!_resetInfo)
                {
                    InfoText = string.Empty;
                }
            }
        }
        public AcceptorState SelectedCoinEntryStates
        {
            get => _selectedCoinEntryStates;
            set
            {
                if (_selectedCoinEntryStates == value)
                {
                    return;
                }

                _selectedCoinEntryStates = value;
                RaisePropertyChanged(nameof(SelectedCoinEntryStates));
                SetCoinInState(_selectedCoinEntryStates);
                if (!_resetInfo)
                {
                    InfoText = string.Empty;
                }
                if (_selectedCoinEntryStates == AcceptorState.Accept)                   //if any fault occurs while diverter direction was towards cashbox so for next coin-in set it.
                {
                    SelectedDiverterDirection = _coinAcceptor.DiverterDirection;
                }
            }
        }
        private void SetCoinInState(AcceptorState coinAcceptorState)
        {
            switch (coinAcceptorState)
            {
                case AcceptorState.Accept:
                    _coinAcceptor.CoinRejectMechOff();
                    break;
                case AcceptorState.Reject:
                    _coinAcceptor.CoinRejectMechOn();
                    break;
            }
        }
        private void SetDiverterDirection(DivertorState diverterState)
        {
            switch (diverterState)
            {
                case DivertorState.DivertToHopper:
                    _coinAcceptor.DivertToHopper();
                    break;
                case DivertorState.DivertToCashbox:
                    _coinAcceptor.DivertToCashbox();
                    break;
            }
        }
        public int CoinToHopper
        {
            get => _coinToHopper;
            set
            {
                if (_coinToHopper == value)
                {
                    return;
                }
                _coinToHopper = value;
                TotalCoinIn++;
                RaisePropertyChanged(nameof(CoinToHopper));
            }
        }
        public int CoinToCashbox
        {
            get => _coinToCashbox;
            set
            {
                if (_coinToCashbox == value)
                {
                    return;
                }
                _coinToCashbox = value;
                TotalCoinIn++;
                RaisePropertyChanged(nameof(CoinToCashbox));
            }
        }
        public int TotalCoinIn
        {
            get => _totalCoinIn;
            set
            {
                if (_totalCoinIn == value)
                {
                    return;
                }
                _totalCoinIn = value;
                RaisePropertyChanged(nameof(TotalCoinIn));
            }
        }
    }
}
