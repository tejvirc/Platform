namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Windows.Input;
    using CommunityToolkit.Mvvm.Input;
    using ConfigWizard;
    using Contracts;
    using Contracts.Extensions;
    using Hardware.Contracts.Bell;
    using Kernel;
    using Monaco.Localization.Properties;

    [CLSCompliant(false)]
    public class BellPageViewModel : InspectionWizardViewModelBase
    {
        private readonly IBell _bell;
        private readonly long _maxBellValue;
        private decimal _previousInitialBellValue;
        private decimal _previousIntervalBellValue;
        private string _status;
        private bool _testEnabled = true;
        private decimal _initialBellValue;
        private decimal _intervalBellValue;

        public BellPageViewModel(bool isWizard)
            : this(ServiceManager.GetInstance().TryGetService<IBell>(), isWizard)
        {
        }

        public BellPageViewModel(IBell bell, bool isWizard) : base(isWizard)
        {
            _bell = bell;
            _maxBellValue = PropertiesManager.GetValue(ApplicationConstants.MaxBellRing, 0L);

            RingBellClicked = new RelayCommand<object>(RingBell_Click);
        }

        public bool ToggleBell
        {
            get => IsRinging;

            set
            {
                if (IsRinging == value)
                {
                    return;
                }

                if (value)
                {
                    StartBell();
                }
                else
                {
                    StopBell();
                }

                UpdateProperties();
            }
        }

        public bool Enabled => _bell?.Enabled ?? false;

        public bool IsToggleEnabled => Enabled && InputStatusText == string.Empty;

        public bool ShowToggle => !Enabled;

        public bool IsRinging => _bell?.IsRinging ?? false;

        public bool RingBellEnabled => IsToggleEnabled && !IsRinging;

        public string Status
        {
            get => _status;

            set
            {
                if (_status == value)
                {
                    return;
                }

                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public bool TestEnabled
        {
            get => _testEnabled && TestModeEnabled;
            set => SetProperty(ref _testEnabled, value, nameof(TestEnabled));
        }

        [CustomValidation(typeof(BellPageViewModel), nameof(ValidateBellValue))]
        public decimal InitialBellValue
        {
            get => _initialBellValue;
            set => SetProperty(ref _initialBellValue, value, true);
        }

        [CustomValidation(typeof(BellPageViewModel), nameof(ValidateBellValue))]
        public decimal IntervalBellValue
        {
            get => _intervalBellValue;
            set => SetProperty(ref _intervalBellValue, value, true);
        }

        public ICommand RingBellClicked { get; }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            EventBus.Subscribe<EnabledEvent>(this, _ => UpdateProperties());
            EventBus.Subscribe<DisabledEvent>(this, _ => UpdateProperties());
            EventBus.Subscribe<RingStoppedEvent>(this, _ => UpdateProperties());

            InitialBellValue = -1;
            IntervalBellValue = -1;

            InitialBellValue = _previousInitialBellValue = ((long)PropertiesManager.GetProperty(ApplicationConstants.InitialBellRing, 0))
                .MillicentsToDollars();
            IntervalBellValue = _previousIntervalBellValue = ((long)PropertiesManager.GetProperty(ApplicationConstants.IntervalBellRing, 0))
                .MillicentsToDollars();

            UpdateProperties();
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

        protected override void OnInputStatusChanged()
        {
            if (!InputEnabled)
            {
                StopBell();
            }

            UpdateProperties();
        }

        protected override void OnTestModeEnabledChanged()
        {
            OnPropertyChanged(nameof(TestEnabled));
        }

        private void UpdateStatus()
        {
            if (!Enabled)
            {
                Status = ResourceKeys.Disabled;
            }
            else if (IsRinging)
            {
                Status = ResourceKeys.On;
            }
            else
            {
                Status = ResourceKeys.Off;
            }
        }

        private void StartBell()
        {
            Inspection?.SetTestName("On");
            _bell?.RingBell();
        }

        private void StopBell()
        {
            Inspection?.SetTestName("Off");
            _bell?.StopBell();
        }

        private void RingBell_Click(object o)
        {
            Inspection?.SetTestName($"Ring at {InitialBellValue}sec, repeat after {IntervalBellValue}sec");

            _bell?.RingBell(TimeSpan.FromSeconds(3));
            UpdateProperties();
        }

        private void UpdateProperties()
        {
            OnPropertyChanged(nameof(Enabled));
            OnPropertyChanged(nameof(IsRinging));
            OnPropertyChanged(nameof(ToggleBell));
            OnPropertyChanged(nameof(ShowToggle));
            OnPropertyChanged(nameof(IsToggleEnabled));
            OnPropertyChanged(nameof(RingBellEnabled));
            UpdateStatus();
        }

        protected override void DisposeInternal()
        {
            StopBell();

            base.DisposeInternal();
        }

        protected override void OnUnloaded()
        {
            StopBell();
            EventBus.UnsubscribeAll(this);

            if (InitialBellValue.Validate(maximum: _maxBellValue) is null)
            {
                if (_previousInitialBellValue != InitialBellValue)
                {
                    PropertiesManager.SetProperty(ApplicationConstants.InitialBellRing, InitialBellValue.DollarsToMillicents());
                }
            }
            else
            {
                InitialBellValue = _previousInitialBellValue;
            }

            if (IntervalBellValue.Validate(maximum: _maxBellValue) is null)
            {
                if (_previousIntervalBellValue != IntervalBellValue)
                {
                    PropertiesManager.SetProperty(ApplicationConstants.IntervalBellRing, IntervalBellValue.DollarsToMillicents());
                }
            }
            else
            {
                IntervalBellValue = _previousIntervalBellValue;
            }
        }
        public static ValidationResult ValidateBellValue(decimal bellValue, ValidationContext context)
        {
            var validationErrorMessage = bellValue.Validate(
                maximum: ((BellPageViewModel)context.ObjectInstance)._maxBellValue
            );

            return validationErrorMessage == null
                ? ValidationResult.Success
                : new(validationErrorMessage);
        }
    }
}
