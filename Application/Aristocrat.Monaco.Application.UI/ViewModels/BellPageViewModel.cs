namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Windows.Input;
    using Aristocrat.Monaco.Localization.Properties;
    using CefSharp.DevTools.Accessibility;
    using CommunityToolkit.Mvvm.Input;
    using Contracts;
    using Contracts.Extensions;
    using Hardware.Contracts.Bell;
    using Kernel;
    using OperatorMenu;

    [CLSCompliant(false)]
    public class BellPageViewModel : OperatorMenuPageViewModelBase
    {
        private readonly IBell _bell;
        private readonly long _maxBellValue;
        private decimal _previousInitialBellValue;
        private decimal _previousIntervalBellValue;
        private string _status;
        private bool _testEnabled = true;
        private decimal _initialBellValue;
        private decimal _intervalBellValue;

        public BellPageViewModel()
            :this(ServiceManager.GetInstance().TryGetService<IBell>())
        {
        }

        public BellPageViewModel(IBell bell)
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

        [CustomValidation(typeof(BellPageViewModel), nameof(InitialBellValueValidate))]
        public decimal InitialBellValue
        {
            get => _initialBellValue;
            set => SetProperty(ref _initialBellValue, value, true);
        }

        private ValidationResult InitialBellValueValidate(decimal initialBellValue, ValidationContext context)
        {
            var errors = initialBellValue.Validate(maximum: _maxBellValue);

            if (errors == null)
            {
                return ValidationResult.Success;
            }

            return new(errors);
        }
        /*public decimal InitialBellValue
        {
            get => _initialBellValue;

            set
            {
                if (_initialBellValue == value)
                {
                    return;
                }

                if (SetProperty(ref _initialBellValue, value, nameof(InitialBellValue)))
                {
                    SetError(nameof(InitialBellValue), _initialBellValue.Validate(maximum: _maxBellValue));
                }
            }
        }*/



        [CustomValidation(typeof(BellPageViewModel), nameof(IntervalBellValueValidate))]
        public decimal IntervalBellValue
        {
            get => _intervalBellValue;
            set => SetProperty(ref _intervalBellValue, value, true);
        }

        private ValidationResult IntervalBellValueValidate(decimal intervalBellValue, ValidationContext context)
        {
            var errors = intervalBellValue.Validate(maximum: _maxBellValue);

            if (errors == null)
            {
                return ValidationResult.Success;
            }

            return new(errors);
        }
        /*public decimal IntervalBellValue
        {
            get => _intervalBellValue;

            set
            {
                if (_intervalBellValue == value)
                {
                    return;
                }

                if (SetProperty(ref _intervalBellValue, value, nameof(IntervalBellValue)))
                {
                    SetError(nameof(IntervalBellValue),
                        _intervalBellValue.Validate(maximum: _maxBellValue));
                }
            }
        }*/

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
            _bell?.RingBell();
        }

        private void StopBell()
        {
            _bell?.StopBell();
        }

        private void RingBell_Click(object o)
        {
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

            if(InitialBellValue.Validate(maximum: _maxBellValue) is null)
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

            if(IntervalBellValue.Validate(maximum: _maxBellValue) is null)
            {
                if(_previousIntervalBellValue != IntervalBellValue)
                {
                    PropertiesManager.SetProperty(ApplicationConstants.IntervalBellRing, IntervalBellValue.DollarsToMillicents());
                }
            }
            else
            {
                IntervalBellValue = _previousIntervalBellValue;
            }
        }
    }
}
