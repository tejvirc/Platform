namespace Aristocrat.Monaco.Accounting.UI.ViewModels
{
    using System;
    using System.Linq;
    using System.Windows.Input;
    using Hopper;
    using Application.Contracts;
    using Application.Contracts.OperatorMenu;
    using Application.UI.OperatorMenu;
    using Hardware.Contracts.CoinAcceptor;
    using Common;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Hopper;
    using Kernel;
    using Monaco.Localization.Properties;
    using MVVM.Command;
    using CoinFaultEvent = Hardware.Contracts.CoinAcceptor.HardwareFaultEvent;
    using HardwareFaultEvent = Hardware.Contracts.Hopper.HardwareFaultEvent;

    [CLSCompliant(false)]
    public class HopperTestViewModel : OperatorMenuSaveViewModelBase
    {
        private const int HopperTestPayoutLevel = 10;
        private const int DivertTolerance = 5;   //small no of badly directed coins is allowed 
        private readonly ICoinAcceptor _coinAcceptorService;
        private readonly IHopper _hopperService;
        private readonly IMeterManager _meters;
        private readonly ISystemDisableManager _disableManager;
        private int _diverterErrors = 0;
        private long _hopperTestPayout;
        private long _coinsReinserted;
        private string _lastHopperEvent;
        private string _lastDivertorEvent;
        private long _extraPayoutDuringTest;
        private long _coinsReinsertedForExtraPayout;
        private bool _canActivatePayout;
        private bool _canHopperTestClose;
        private string _status;
        public long HopperTestPayout
        {
            get => _hopperTestPayout;
            set => SetProperty(ref _hopperTestPayout, value, nameof(HopperTestPayout));
        }
        
        public long CoinsReinserted
        {
            get => _coinsReinserted;
            set => SetProperty(ref _coinsReinserted, value, nameof(CoinsReinserted));
        }
        public string LastHopperEvent
        {
            get => _lastHopperEvent;
            set => SetProperty(ref _lastHopperEvent, value, nameof(LastHopperEvent));
        }
        
        public string LastDivertorEvent
        {
            get => _lastDivertorEvent;
            set => SetProperty(ref _lastDivertorEvent, value, nameof(LastDivertorEvent));
        }
        
        public long ExtraPayoutDuringTest
        {
            get => _extraPayoutDuringTest;
            set => SetProperty(ref _extraPayoutDuringTest, value, nameof(ExtraPayoutDuringTest));
        }
        
        public long CoinsReinsertedForExtraPayout
        {
            get => _coinsReinsertedForExtraPayout;
            set => SetProperty(ref _coinsReinsertedForExtraPayout, value, nameof(CoinsReinsertedForExtraPayout));
        }

        public long TotalPayout => HopperTestPayout + ExtraPayoutDuringTest - CoinsReinserted - CoinsReinsertedForExtraPayout;
        
        public bool CanActivatePayout
        {
            get => _canActivatePayout;
            set => SetProperty(ref _canActivatePayout, value, nameof(CanActivatePayout));
        }
        
        public bool CanHopperTestClose
        {
            get => _canHopperTestClose;
            set => SetProperty(ref _canHopperTestClose, value, nameof(CanHopperTestClose));
        }
        
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value, nameof(Status));
        }

        public ICommand ActivatePayoutCommand { get; set; }

        public ICommand CloseHopperTestCommand { get; set; }
        public HopperTestViewModel()
            : this(ServiceManager.GetInstance().TryGetService<IHopper>(),
                  ServiceManager.GetInstance().TryGetService<ICoinAcceptor>(),
                  ServiceManager.GetInstance().GetService<IMeterManager>(),
                  ServiceManager.GetInstance().GetService<ISystemDisableManager>())
        {
        }

        public HopperTestViewModel(IHopper hopperService,
            ICoinAcceptor coinAcceptorService,
            IMeterManager meters,
            ISystemDisableManager disableManager)
        {
            _hopperService = hopperService ?? throw new ArgumentNullException(nameof(hopperService));
            _coinAcceptorService = coinAcceptorService ?? throw new ArgumentNullException(nameof(coinAcceptorService));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            
            ActivatePayoutCommand = new ActionCommand<object>(obj => ActivatePayout());
            CloseHopperTestCommand = new ActionCommand<object>(obj => CloseHopperTestDialog());
            CanHopperTestClose = true;
            CanActivatePayout = true;
        }

        private bool CheckForFake()
        {
            if (!_hopperService.Name.Contains("Fake"))
            {
                return false;
            }

            CanActivatePayout = true;
            CanHopperTestClose = true;
            Status = "Fake Hopper test done";
            return true;

        }

        protected override void OnLoaded()
        {
            LoadTestCoins();
            base.OnLoaded();
            SubscribeToEvents();
        }

        protected override void OnUnloaded()
        {
            EventBus.UnsubscribeAll(this);
            CheckHopperTestStatus();
            base.OnUnloaded();
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<DownEvent>(
                this,
                _ => HardwareFaultClear(),
                evt => evt.LogicalId == (int)ButtonLogicalId.Button30);
            EventBus.Subscribe<CoinInEvent>(this, HandleCoinInEvent);
            EventBus.Subscribe<CoinOutEvent>(this, HandleCoinOutEvent);
            EventBus.Subscribe<CoinFaultEvent>(this, GetLastDiverterEvent);
            EventBus.Subscribe<HardwareFaultEvent>(this, GetLastHopperEvent);
            EventBus.Subscribe<CoinToCashboxInsteadOfHopperEvent>(this, Handle);
        }

        private void HandleCoinInEvent(CoinInEvent evt)
        {
            if (HopperTestPayout > CoinsReinserted)
            {
                _meters.GetMeter(AccountingMeters.HopperTestCoinsIn).Increment(1);
                CoinsReinserted += 1;
            }
            else
            {
                _meters.GetMeter(AccountingMeters.ExtraCoinInWhenHopperTest).Increment(1);
                CoinsReinsertedForExtraPayout += 1;
            }
            UpdateStatus();

            if (HopperTestPayout <= CoinsReinserted && ExtraPayoutDuringTest <= CoinsReinsertedForExtraPayout)
            {
                _coinAcceptorService.CoinRejectMechOn();
                ClearPeriodMeters();
                UpdateHopperTest();
            }
        }

        private void Handle(CoinToCashboxInsteadOfHopperEvent evt)
        {
            _meters.GetMeter(AccountingMeters.CoinToCashBoxInsteadHopperCount).Increment(1);

            _diverterErrors += 1;
            if (_diverterErrors >= DivertTolerance)
            {
                _diverterErrors = 0;
                EventBus.Publish(new CoinFaultEvent(CoinFaultTypes.Divert));
                LastDivertorEvent = CoinFaultTypes.Divert.GetDescription();
            }
        }

        private void HandleCoinOutEvent(CoinOutEvent evt)
        {
            _meters.GetMeter(AccountingMeters.HopperTestCoinsOut).Increment(1);
            HopperTestPayout += 1;
            if (HopperTestPayoutLevel <= HopperTestPayout)
            {
                _hopperService.StopHopperMotor();
                UpdateStatus();
            }
        }

        private void HandleIllegalCoinOutEvent()
        {
            _meters.GetMeter(AccountingMeters.ExtraCoinOutWhenHopperTest).Increment(1);
            ExtraPayoutDuringTest += 1;
            UpdateStatus();
        }

        public void ActivatePayout()
        {
            if (CheckForFake()) return;
            InitHopperTest();
            _hopperService.SetMaxCoinoutAllowed(HopperTestPayoutLevel);
            _hopperService.StartHopperMotor();
        }

        public void CloseHopperTestDialog()
        {
            ServiceManager.GetInstance().GetService<IDialogService>().DismissOpenedDialog();
            UpdateHopperTest();
        }

        private void LoadTestCoins()
        {
            if (!PropertiesManager.GetValue(ApplicationConstants.HopperEnabled, false) &&
                !PropertiesManager.GetValue(ApplicationConstants.CoinAcceptorEnabled, false))
            {
                Status = Resources.HopperTestNotAvailable;
                CanActivatePayout = false;
            }

            HopperTestPayout = _meters.GetMeter(AccountingMeters.HopperTestCoinsOut).Period;
            CoinsReinserted = _meters.GetMeter(AccountingMeters.HopperTestCoinsIn).Period;
            ExtraPayoutDuringTest = _meters.GetMeter(AccountingMeters.ExtraCoinOutWhenHopperTest).Period;
            CoinsReinsertedForExtraPayout = _meters.GetMeter(AccountingMeters.ExtraCoinInWhenHopperTest).Period;

            if (PropertiesManager.GetValue(HardwareConstants.HopperDiagnosticMode, false))
            {
                CanHopperTestClose = false;
                CanActivatePayout = false;
                PropertiesManager.SetProperty(HardwareConstants.CoinAcceptorDiagnosticMode, true);
                _disableManager.Enable(HardwareConstants.HopperTestLockKey);
                UpdateInputStatus();
                _coinAcceptorService.DivertToHopper();
                UpdateStatus();
                UpdateHopperTest();
            }
            else
            {
                CanHopperTestClose = true;
                CanActivatePayout = !CheckHopperLockup();
            }
        }

        protected override void OnInputEnabledChanged()
        {
            UpdateInputStatus();
            UpdateStatus();
        }

        private void CheckHopperTestStatus()
        {
            if (PropertiesManager.GetValue(HardwareConstants.HopperDiagnosticMode, false))
            {
                CreateHopperTestLockup();
            }
        }

        private void CreateHopperTestLockup()
        {
            _disableManager.Disable(HardwareConstants.HopperTestLockKey,
                SystemDisablePriority.Immediate,
                () => Hardware.Contracts.Properties.Resources.HopperTestFault,
                true,
                () => Hardware.Contracts.Properties.Resources.HopperTestFaultHelp);
        }
        private void InitHopperTest()
        {
            HopperTestPayout = 0;
            CoinsReinserted = 0;
            ExtraPayoutDuringTest = 0;
            CoinsReinsertedForExtraPayout = 0;
            PropertiesManager.SetProperty(HardwareConstants.HopperDiagnosticMode, true);
            PropertiesManager.SetProperty(HardwareConstants.CoinAcceptorDiagnosticMode, true);
            CanHopperTestClose = false;
            CanActivatePayout = false;
            UpdateInputStatus();
            _coinAcceptorService.DivertToHopper();
        }

        private void GetLastHopperEvent(HardwareFaultEvent evt)
        {
            if (evt.Fault == HopperFaultTypes.IllegalCoinOut)
            {
                HandleIllegalCoinOutEvent();
            }
            _hopperService.StopHopperMotor();
            LastHopperEvent = evt.Fault.GetDescription(typeof(HopperFaultTypes));
            Status = $"{Resources.HopperFault} {Resources.TurnResetKey}";
            UpdateHopperTest();
        }

        private void GetLastDiverterEvent(CoinFaultEvent evt)
        {
            if (evt.Fault == CoinFaultTypes.Divert)
            {
                LastDivertorEvent = evt.Fault.GetDescription(typeof(CoinFaultTypes));
                _coinAcceptorService.CoinRejectMechOn();
                Status = $"{Resources.CoinEntryFault} {Resources.TurnResetKey}";
            }
        }

        private void HardwareFaultClear()
        {
            foreach (CoinFaultTypes fault in Enum.GetValues(typeof(CoinFaultTypes)))
            {
                if (_disableManager.CurrentDisableKeys.Contains(fault.GetAttribute<ErrorGuidAttribute>().Id))
                {
                    _disableManager.Enable(fault.GetAttribute<ErrorGuidAttribute>().Id);
                }

            }
            foreach (HopperFaultTypes fault in Enum.GetValues(typeof(HopperFaultTypes)))
            {
                if (_disableManager.CurrentDisableKeys.Contains(fault.GetAttribute<ErrorGuidAttribute>().Id))
                {
                    _disableManager.Enable(fault.GetAttribute<ErrorGuidAttribute>().Id);
                }

            }

            UpdateStatus();
            UpdateInputStatus();
        }

        private void UpdateStatus()
        {
            var msg = InputEnabled ? Resources.CloseDoorReinsert : Resources.ReinsertCoins;
            Status = TotalPayout > 0 ? string.Format(msg, TotalPayout) : string.Empty;
        }

        private bool CheckHopperLockup()
        {
            var lockupExists = false;
            foreach (HopperFaultTypes fault in Enum.GetValues(typeof(HopperFaultTypes)))
            {
                if (_disableManager.CurrentImmediateDisableKeys.Contains(fault.GetAttribute<ErrorGuidAttribute>().Id))
                {
                    lockupExists = true;
                    break;
                }
            }

            Status = lockupExists ? $"{Resources.HopperFault} {Resources.TurnResetKey}" : string.Empty;
            return lockupExists;
        }

        private void UpdateHopperTest()
        {
            if (TotalPayout == 0)
            {
                CanHopperTestClose = true;
                CanActivatePayout = true;
                PropertiesManager.SetProperty(HardwareConstants.HopperDiagnosticMode, false);
                PropertiesManager.SetProperty(HardwareConstants.CoinAcceptorDiagnosticMode, false);
                Status = string.Empty;
            }
        }

        private void UpdateInputStatus()
        {
            if (InputEnabled)
            {
                _coinAcceptorService.CoinRejectMechOn();
            }
            else if (TotalPayout > 0)
            {
                _coinAcceptorService.CoinRejectMechOff();
            }
            else
            {
                //do nothing
            }
        }

        private void ClearPeriodMeters()
        {
            //Clearing Test Period meters.
            _meters.ClearPeriodMeters(typeof(CoinOutTestMetersProvider).ToString());
        }
    }
}
