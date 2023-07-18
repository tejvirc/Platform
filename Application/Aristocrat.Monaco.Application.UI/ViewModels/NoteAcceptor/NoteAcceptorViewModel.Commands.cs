namespace Aristocrat.Monaco.Application.UI.ViewModels.NoteAcceptor
{
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Common;
    using Contracts.HardwareDiagnostics;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.SharedDevice;
    using Monaco.Localization.Properties;
    using Kernel;
    using Views;

    public partial class NoteAcceptorViewModel
    {
        private void InitCommands()
        {
            StackButtonCommand = new RelayCommand<object>(HandleStackButtonCommand);

            InspectButtonCommand = new RelayCommand<object>(HandleInspectButtonCommand);

            SelfTestButtonCommand = new RelayCommand<object>(HandleSelfTestButtonCommand);
            SelfTestClearButtonCommand = new RelayCommand<object>(HandleSelfTestClearNvmButtonCommand);
            ReturnButtonCommand = new RelayCommand<object>(HandleReturnButtonCommand);
            NoteAcceptorTestCommand = new RelayCommand<object>(HandleNoteAcceptorTestCommand);
        }

        public ICommand InspectButtonCommand { get; set; }

        public ICommand ReturnButtonCommand { get; set; }

        public ICommand StackButtonCommand { get; set; }

        public ICommand NoteAcceptorTestCommand { get; set; }

        protected override void OnLoaded()
        {
            ChangeFocus = false;
            EnabledDenominationsText = string.Empty;
            LastDocumentResultText = string.Empty;
            StackerStateText = string.Empty;
            VariantNameText = string.Empty;
            VariantVersionText = string.Empty;

            SetDenominationsVisible(IsDenominationsVisible);
            ConfigureDenominations();

            SelfTestStatusVisible = true;
            SelfTestCurrentState = SelfTestState.None;
            StatusCurrentMode = StatusMode.None;

            EventBus.Publish(new NoteAcceptorMenuEnteredEvent());

            base.OnLoaded();

            UpdateWarningMessage();
        }

        protected override void UpdateWarningMessage()
        {
            if (!(NoteAcceptor?.Connected ?? false) || (NoteAcceptor?.DisabledByError ?? false))
            {
                TestWarningText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TestModeDisabledStatusDevice);
            }
            else
            {
                base.UpdateWarningMessage();
            }
            OnPropertyChanged(nameof(TestModeToolTipDisabled));
        }

        protected override void OnTestModeEnabledChanged()
        {
            if (TestModeEnabled && !(NoteAcceptor?.Enabled ?? true))
            {
                NoteAcceptor?.Enable(EnabledReasons.Operator);
            }
            OnPropertyChanged(nameof(TestModeToolTipDisabled));
        }

        /// <summary>Checks the denomination checkboxes to reset if all, none, or no vouchers selected.</summary>
        private void HandleDenominationChangeCommand(bool state, int denom)
        {
            if (NoteAcceptor == null)
            {
                Logger.Warn("Note acceptor service unavailable");
                return;
            }

            NoteAcceptor.UpdateDenom(denom, state);

            EventBus.Publish(new OperatorMenuSettingsChangedEvent());

            if (NoteAcceptor.Denominations.Count == 0)
            {
                AllowBillIn = false;
            }
        }

        private void HandleInspectButtonCommand(object obj)
        {
            Logger.Debug("Inspect btn clicked");
            Inspection?.SetTestName("Inspection");

            if (NoteAcceptor == null)
            {
                Logger.Warn("Note acceptor service unavailable");
                return;
            }

            StartInspecting();
        }

        private void HandleNoteAcceptorTestCommand(object obj)
        {
            var dialogService = ServiceManager.GetInstance().GetService<IDialogService>();

            var viewModel = new NoteAcceptorTestViewModel();

            EventBus.Publish(new HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory.NoteAcceptor));

            _inNoteAcceptorTest = true;
            dialogService.ShowInfoDialog<NoteAcceptorTestView>(
                this,
                viewModel,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoteAcceptorTest));
            _inNoteAcceptorTest = false;

            EventBus.Publish(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.NoteAcceptor));
        }

        private void HandleReturnButtonCommand(object obj)
        {
            Inspection?.SetTestName("Return note");
            NoteAcceptor?.Return();
        }

        private void HandleSelfTestButtonCommand(object obj)
        {
            Inspection?.SetTestName("Self test");
            RunSelfTest(false);
        }

        private void HandleSelfTestClearNvmButtonCommand(object obj)
        {
            Inspection?.SetTestName("Self test clear NVM");
            RunSelfTest(true);
        }

        private void RunSelfTest(bool clearNvm)
        {
            if (NoteAcceptor == null)
            {
                Logger.Warn("NoteAcceptor service unavailable");
                return;
            }

            if (NoteAcceptor.IsEscrowed)
            {
                Task.Run(async () =>
                {
                    await NoteAcceptor.Return();

                    Execute.OnUIThread(SelfTest);
                });
            }
            else
            {
                SelfTest();
            }

            void SelfTest()
            {
                EventBus.Publish(new HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory.NoteAcceptor));
                SetDiagnosticButtonsEnabled(false);
                SelfTestCurrentState = SelfTestState.Running;

                NoteAcceptor.SelfTest(clearNvm);
                EventBus.Publish(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.NoteAcceptor));
            }
        }

        private void HandleStackButtonCommand(object obj)
        {
            if (NoteAcceptor == null)
            {
                Logger.Warn("Note acceptor service unavailable");
            }

#if USE_STACK_BUTTON
            Inspection?.SetTestName("Stack");
            if (_noteAcceptorDiagnosticsEnabled)
            {
                if (ExtendTimeoutTimer.Enabled)
                {
                    ExtendTimeoutTimer.Stop();
                }
            }

            NoteAcceptor.StackDocument();
#endif
        }
    }
}
