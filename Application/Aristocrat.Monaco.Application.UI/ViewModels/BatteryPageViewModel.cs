namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Windows.Input;
    using System.Windows.Media;
    using ConfigWizard;
    using Contracts.Localization;
    using Hardware.Contracts.Battery;
    using Kernel;
    using Monaco.Localization.Properties;

    [CLSCompliant(false)]
    public class BatteryPageViewModel : InspectionWizardViewModelBase
    {
        private readonly IBattery _battery;
        private readonly bool?[] _batteryStatus = new bool?[2];

        public BatteryPageViewModel(bool isWizard) : base(isWizard)
        {
            _battery = ServiceManager.GetInstance().GetService<IBattery>();
        }

        public ICommand TestCommand { get; set; }

        public string Battery1Label => $"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BatteryLabel)} 1";

        public string Battery2Label => $"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BatteryLabel)} 2";

        public string Battery1StatusText => GetBatteryStatusText(0);

        public string Battery2StatusText => GetBatteryStatusText(1);

        public Brush Battery1Background => GetBatteryStatusBackground(0);

        public Brush Battery2Background => GetBatteryStatusBackground(1);

        protected override void OnLoaded()
        {
            TestBatteries();

            base.OnLoaded();
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

        private void TestBatteries()
        {
            var results = _battery.Test();
            _batteryStatus[0] = results.Item1;
            _batteryStatus[1] = results.Item2;
            OnPropertyChanged(nameof(Battery1StatusText));
            OnPropertyChanged(nameof(Battery2StatusText));
            OnPropertyChanged(nameof(Battery1Background));
            OnPropertyChanged(nameof(Battery2Background));
        }

        private string GetBatteryStatusText(int batteryIndex)
        {
            return _batteryStatus[batteryIndex].HasValue
                ? _batteryStatus[batteryIndex].Value
                    ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Good).ToUpper()
                    : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Failed).ToUpper()
                : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BatteryInitialStatus);
        }

        private Brush GetBatteryStatusBackground(int batteryIndex)
        {
            Inspection?.SetTestName($"Battery{batteryIndex + 1}");

            Brush background = Brushes.Transparent;
            if (_batteryStatus[batteryIndex].HasValue && !_batteryStatus[batteryIndex].Value)
            {
                background = Brushes.Red;

                Inspection?.ReportTestFailure();
            }

            return background;
        }
    }
}