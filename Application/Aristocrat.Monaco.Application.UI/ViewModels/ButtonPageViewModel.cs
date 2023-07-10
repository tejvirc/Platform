namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Application.Settings;
    using ButtonTestDeck;
    using ConfigWizard;
    using Contracts.HardwareDiagnostics;
    using Contracts.Localization;
    using Hardware.Contracts.Button;
	using Hardware.Contracts.ButtonDeck;
    using Hardware.Contracts.Cabinet;
    using Kernel;
    using Models;
    using Monaco.UI.Common.Extensions;
    using MVVM;

    [CLSCompliant(false)]
    public class ButtonPageViewModel : InspectionWizardViewModelBase
    {
        private readonly ICabinetDetectionService _cabinet;
        private const int PlayButtonId = 113;
        private const int JackPotId = 130;

        private readonly object _context = new();
        private readonly IButtonService _buttonService;
        private readonly IButtonDeckDisplay _buttonDeckDisplay;
        private readonly LCDButtonTestDeck _buttonDeck;

        private bool _isLcdButtonDeckEnabled = true;

        private string _firmwareCrc;
        private string _crcSeed;

        /// <summary>
        ///     Creates an instance of <see cref="ButtonPageViewModel"/>
        /// </summary>
        /// <param name="isWizard">Whether or not this is for the configuration wizard</param>
        public ButtonPageViewModel(bool isWizard)
            : this(
                ServiceManager.GetInstance().GetService<ICabinetDetectionService>(),
                ServiceManager.GetInstance().GetService<IButtonService>(),
                ServiceManager.GetInstance().GetService<IButtonDeckDisplay>(),
                isWizard)
        {
        }

        /// <summary>
        ///     Creates an instance of <see cref="ButtonPageViewModel"/>
        /// </summary>
        /// <param name="cabinet">An instance of <see cref="ICabinetDetectionService"/></param>
        /// <param name="buttonService">An instance of <see cref="IButtonService"/></param>
        /// <param name="buttonDeckService">An instance of <see cref="IButtonDeckDisplay"/></param>
        /// <param name="isWizard">Whether or not this is for the configuration wizard</param>
        public ButtonPageViewModel(
            ICabinetDetectionService cabinet,
            IButtonService buttonService,
            IButtonDeckDisplay buttonDeckService,
            bool isWizard) : base(isWizard)
        {
            _cabinet = cabinet ?? throw new ArgumentNullException(nameof(cabinet));
            _buttonService = buttonService ?? throw new ArgumentNullException(nameof(buttonService));
            _buttonDeckDisplay = buttonDeckService ?? throw new ArgumentNullException(nameof(buttonDeckService));
            if (_cabinet.GetButtonDeckType(PropertiesManager) == ButtonDeckType.LCD)
            {
                _buttonDeck = new LCDButtonTestDeck();
            }
        }

        public override bool CanCalibrateTouchScreens => false;

        public ObservableCollection<ButtonViewModel> Buttons { get; } = new();

        public ObservableCollection<PressedButtonData> PressedButtonsData { get; } = new();

        public bool IsLcdPanelEnabled
        {
            get => _isLcdButtonDeckEnabled;
            set
            {
                _isLcdButtonDeckEnabled = value;
                RaisePropertyChanged(nameof(IsLcdPanelEnabled));
            }
        }

        public string FirmwareCrc
        {
            get => _firmwareCrc;
            set
            {
                if (_firmwareCrc == value)
                {
                    return;
                }

                _firmwareCrc = value;
                RaisePropertyChanged(nameof(FirmwareCrc));
            }
        }

        public string CrcSeed
        {
            get => _crcSeed;
            set
            {
                if (_crcSeed == value)
                {
                    return;
                }

                _crcSeed = value;
                RaisePropertyChanged(nameof(CrcSeed));
            }
        }

        protected sealed override void OnLoaded()
        {
            EventBus.Publish(new HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory.Buttons));

            (FirmwareCrc, CrcSeed) = _buttonDeckDisplay.GetButtonDeckFirmwareCrc(PropertiesManager);

            IsLcdPanelEnabled = IsButtonDeckLcd();

            Subscribe();

            _buttonDeck?.OnLoaded();

            LoadButtonViewModels();

            Inspection?.SetFirmwareVersion(
                MachineSettingsUtilities.GetButtonDeckIdentification(Localizer.For(CultureFor.Operator)));

            base.OnLoaded();
        }

        protected override void OnUnloaded()
        {
            EventBus.Publish(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.Buttons));

            Unsubscribe();

            _buttonDeck?.OnUnloaded();

            DisposeInternal();

            base.OnUnloaded();
        }

        protected override void SetupNavigation()
        {
            if (WizardNavigator != null)
            {
                WizardNavigator.CanNavigateForward = true;
            }
        }

        protected override void SaveChanges()
        {
        }

        protected override void DisposeInternal()
        {
            foreach (var button in Buttons)
            {
                button.OnUnloaded();
            }

            _buttonService.ExitButtonTestMode();

            base.DisposeInternal();
        }

        private void Subscribe()
        {
            EventBus.Subscribe<DownEvent>(_context, HandleEvent);
            EventBus.Subscribe<UpEvent>(_context, HandleEvent);
        }

        private void Unsubscribe()
        {
            EventBus.UnsubscribeAll(_context);
        }

        private bool IsButtonDeckLcd()
        {
            var deckType = _cabinet.GetButtonDeckType(PropertiesManager);
            return deckType is ButtonDeckType.LCD or ButtonDeckType.SimulatedLCD;
        }

        private void LoadButtonViewModels()
        {
            Buttons.Clear();

            _buttonService.EnterButtonTestMode();

            var buttonIds = _buttonService.LogicalButtons
                .Where(button => button.Key == PlayButtonId)
                .Select(button => button.Key);

            foreach (var buttonId in buttonIds)
            {
                var viewModel = new ButtonViewModel(buttonId, null);
                Buttons.Add(viewModel);
                viewModel.OnLoaded();
            }
        }

        private void HandleEvent(DownEvent evt)
        {
            if (!TestModeEnabled || evt.LogicalId == JackPotId)
            {
                return;
            }

            _buttonDeck?.Pressed(evt.LogicalId);

            var pressedData = new PressedButtonData(
                _buttonDeck?.ResourceKey(evt.LogicalId),
                _buttonService.GetButtonPhysicalId(evt.LogicalId),
                _buttonService.GetLocalizedButtonName(evt.LogicalId, Localizer.For(CultureFor.Operator).GetString),
                _buttonService.GetButtonName(evt.LogicalId));

            MvvmHelper.ExecuteOnUI(() => PressedButtonsData.Insert(0, pressedData));
            Inspection?.SetTestName(_buttonService.GetButtonName(evt.LogicalId));
        }

        private void HandleEvent(UpEvent evt)
        {
            _buttonDeck?.Released(evt.LogicalId);
        }

        protected override void OnOperatorCultureChanged(OperatorCultureChangedEvent evt)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                var coll = new ObservableCollection<PressedButtonData>();
                foreach (var button in PressedButtonsData)
                {
                    button.RefreshFields();
                    coll.Add(button);
                }
                PressedButtonsData.Clear();
                PressedButtonsData.AddRange(coll);
                RaisePropertyChanged(nameof(PressedButtonsData));
            });

            base.OnOperatorCultureChanged(evt);
        }
    }
}