namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Application.Settings;
    using Aristocrat.Monaco.Hardware.Contracts.ButtonDeck;
    using ButtonTestDeck;
    using ConfigWizard;
    using Contracts.HardwareDiagnostics;
    using Contracts.Localization;
    using Hardware.Contracts.Button;
    using Kernel;
    using MVVM;

    [CLSCompliant(false)]
    public class ButtonPageViewModel : InspectionWizardViewModelBase
    {
        private const int PlayButtonId = 113;
        private const int JackPotId = 130;

        private readonly object _context = new object();
        private readonly IButtonService _buttonService;
        private readonly LCDButtonTestDeck _buttonDeck;

        private bool _isLcdButtonDeckEnabled = true;

        private string _firmwareCrc;
        private string _crcSeed;

        public ButtonPageViewModel(bool isWizard) : base(isWizard)
        {
            _buttonService = ServiceManager.GetInstance().GetService<IButtonService>();
            if (ButtonDeckUtilities.GetButtonDeckType() == ButtonDeckUtilities.ButtonDeckType.LCD)
            {
                _buttonDeck = new LCDButtonTestDeck();
            }
        }

        public override bool CanCalibrateTouchScreens => false;

        public ObservableCollection<ButtonViewModel> Buttons { get; } = new ObservableCollection<ButtonViewModel>();

        /// <summary>
        ///     A collection of information about the pressed button.
        ///         <list type="bullet">
        ///             <item><see cref="Tuple{T1,T2,T3}.Item1"/>:
        ///                 The value of <see cref="LCDButtonTestDeck.ResourceKey(int)"/>.
        ///                 If the <see cref="LCDButtonTestDeck"/> is null, then use <see cref="Tuple{T1,T2,T3}.Item2"/>.
        ///             </item>
        ///             <item><see cref="Tuple{T1,T2,T3}.Item2"/>:
        ///                 The physical ID of the button. This is used if <see cref="LCDButtonTestDeck"/> is null.
        ///             </item>
        ///             <item><see cref="Tuple{T1,T2,T3}.Item3"/>:
        ///                 The name of the button.
        ///             </item>
        ///         </list>
        /// </summary>
        public ObservableCollection<Tuple<string, int, string>> PressedButtonsData { get; }
            = new ObservableCollection<Tuple<string, int, string>>();

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

            (FirmwareCrc, CrcSeed) = ButtonDeckUtilities.GetButtonDeckFirmwareCrc();

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

        private static bool IsButtonDeckLcd()
        {
            var deckType = ButtonDeckUtilities.GetButtonDeckType();

            return deckType == ButtonDeckUtilities.ButtonDeckType.LCD ||
                   deckType == ButtonDeckUtilities.ButtonDeckType.SimulatedLCD;
        }

        private void LoadButtonViewModels()
        {
            Buttons.Clear();

            _buttonService.EnterButtonTestMode();

            IEnumerable<int> buttonIds = _buttonService.LogicalButtons
                .Where(button => button.Key == PlayButtonId)
                .Select(button => button.Key);

            foreach (int buttonId in buttonIds)
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

            var pressedData = new Tuple<string, int, string>(
                _buttonDeck?.ResourceKey(evt.LogicalId),
                _buttonService.GetButtonPhysicalId(evt.LogicalId),
                _buttonService.GetLocalizedButtonName(evt.LogicalId));

            MvvmHelper.ExecuteOnUI(() => PressedButtonsData.Insert(0, pressedData));

            Inspection?.SetTestName(_buttonService.GetButtonName(evt.LogicalId));
        }

        private void HandleEvent(UpEvent evt)
        {
            _buttonDeck?.Released(evt.LogicalId);
        }
    }
}