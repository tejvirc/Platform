namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Linq;
    using Aristocrat.Monaco.Application.Localization;
    using Contracts;
    using Hardware.Contracts.SharedDevice;

    [CLSCompliant(false)]
    public class HardwareConfigPageViewModel : HardwareConfigBaseViewModel
    {
        private bool _autoNavigated;

        public HardwareConfigPageViewModel() : base(true) { }

        protected override void SetupNavigation()
        {
            WizardNavigator.CanNavigateForward = EnabledDevices.All(div => div.StatusType == DeviceState.ConnectedText) && !IsValidating;

            WizardNavigator.CanNavigateBackward = false;
        }

        protected override void OnUnloaded()
        {
            // If configurable devices are disabled, validation may not occur so persist configs on unload just in case
            SaveCurrentHardwareConfig();
            base.OnUnloaded();
        }

        protected override void CheckValidatedStatus()
        {
            if (WizardNavigator != null)
            {
                WizardNavigator.CanNavigateForward = Validated;

                if (WizardNavigator.CanNavigateForward && !_autoNavigated && AutoConfigurator != null &&
                    AutoConfigurator.AutoConfigurationExists)
                {
                    _autoNavigated = true;
                    WizardNavigator.NavigateForward();
                }
            }
        }

        protected override void LoadAutoConfiguration()
        {
            var autoConfigured = Devices.Aggregate(true, (current, device) => current & LoadAutoConfiguration(device.DeviceType));

            LoadAutoConfigurationHardMeter();

            if (!autoConfigured)
            {
                return;
            }

            // Only proceed forward if a valid configuration was found.  No need to validate the hardware just save and move for auto configuration
            SaveCurrentHardwareConfig();
            base.LoadAutoConfiguration();
        }

        private void LoadAutoConfigurationHardMeter()
        {
            if (AutoConfigurator == null)
            {
                return;
            }

            var enabled = false;

            var enabledString = "HardMetersEnabled";
            if (AutoConfigurator.GetValue(enabledString, ref enabled))
            {
                HardMetersEnabled = enabled;
            }
        }

        private bool LoadAutoConfiguration(DeviceType type)
        {
            if (AutoConfigurator == null)
            {
                return false;
            }

            var configured = false;
            var enabled = false;
            var device = Devices.FirstOrDefault(d => d.DeviceType == type);

            if (device == null)
            {
                return false;
            }

            var enabledString = type + "Enabled";
            if (AutoConfigurator.GetValue(enabledString, ref enabled))
            {
                device.Enabled = enabled;
                configured = true;
            }

            if (enabled)
            {
                var make = string.Empty;
                if (AutoConfigurator.GetValue(type + "Make", ref make) && device.Manufacturers.Contains(make))
                {
                    device.Manufacturer = make;
                }

                if (!make.Contains(ApplicationConstants.Fake))
                {
                    var protocol = string.Empty;
                    if (AutoConfigurator.GetValue(type + "Protocol", ref protocol))
                    {
                        device.Protocol = protocol;
                    }

                    var port = string.Empty;
                    if (AutoConfigurator.GetValue(type + "Port", ref port) && device.Ports.Contains(port))
                    {
                        device.Port = port;
                    }
                }
            }

            // All three fields must be set for auto config to succeed for this type
            return (configured || enabled) && !string.IsNullOrEmpty(device.Manufacturer) && !string.IsNullOrEmpty(device.Protocol) &&
                !string.IsNullOrEmpty(device.Port) || !device.Enabled && !enabled;
        }
    }
}
