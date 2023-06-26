namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Kernel;
    using Kernel.Contracts;
    using Localization;
    using MVVM;
    using MVVM.Command;
    using Vgt.Client12.Application.OperatorMenu;

    [CLSCompliant(false)]
    public class HardwareManagerPageViewModel : HardwareConfigBaseViewModel
    {
        private readonly IOperatorMenuLauncher _operatorMenuLauncher;

        private readonly List<DeviceConfigViewModel>
            _persistedDevices = new List<DeviceConfigViewModel>(); // used to store device settings on page start-up

        private readonly object _persistedDeviceLock = new object();
        private bool _isDirty;
        private readonly bool _initialDoorOpticSensor;
        private readonly bool _initialBellyPanelDoor;
        private readonly bool _initialBell;

        public HardwareManagerPageViewModel()
            : base(false)
        {
            _operatorMenuLauncher = ServiceManager.GetInstance().GetService<IOperatorMenuLauncher>();
            EventBus.Subscribe<OperatorCultureChangedEvent>(this, Handle);

            ApplyHardwareSettingsCommand = new ActionCommand<object>(Apply, _ => IsDirty);
            _initialDoorOpticSensor = DoorOpticSensorEnabled;
            _initialBellyPanelDoor = BellyPanelDoorEnabled;
            _initialBell = BellEnabled;
            UpdateChanges = () => IsDirty = AreChangesMade();
        }

        public ActionCommand<object> ApplyHardwareSettingsCommand { get; set; }

        public bool IsDirty
        {
            get => _isDirty && InputEnabled;
            set
            {
                if (_isDirty == value)
                {
                    return;
                }

                _isDirty = value;
                RaisePropertyChanged(nameof(IsDirty));
                ApplyHardwareSettingsCommand.RaiseCanExecuteChanged();
            }
        }

        protected override void InitializeData()
        {
            base.InitializeData();

            SetSavedPreferences();
        }

        protected override void Loaded()
        {
            base.Loaded();

            foreach (var device in Devices)
            {
                SetHardwareStatus(device);
            }

            IsValidating = false;
            IsDirty = AreChangesMade();
        }

        protected override void OnInputEnabledChanged()
        {
            ApplyHardwareSettingsCommand.RaiseCanExecuteChanged();
            base.OnInputEnabledChanged();
        }

        protected override void CheckValidatedStatus()
        {
        }

        protected override void Device_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IsDirty = AreChangesMade();

            if (!_isDirty) // reset status if change was backed-out
            {
                foreach (var device in Devices)
                {
                    SetHardwareStatus(device);
                }
            }

            base.Device_OnPropertyChanged(sender, e);
        }

        protected override void UndoSavedChanges()
        {
            lock (_persistedDeviceLock)
            {
                SaveCurrentHardwareConfig(_persistedDevices);
            }
        }

        private void Apply(object parameter)
        {
            IsDirty = false;

            if (AreChangesMade())
            {
                EventBus.Publish(new OperatorMenuSettingsChangedEvent());
                SaveCurrentHardwareConfig();
                _operatorMenuLauncher.Close();
                EventBus.Publish(new ExitRequestedEvent(ExitAction.Restart));
            }
        }

        // save start up preferences to fuel the cancel button
        private void SetSavedPreferences()
        {
            lock (_persistedDeviceLock)
            {
                _persistedDevices.Clear();

                foreach (var setting in Devices)
                {
                    var device = new DeviceConfigViewModel(setting.DeviceType, true)
                    {
                        Enabled = setting.Enabled,
                        Manufacturer = setting.Manufacturer,
                        Protocol = setting.Protocol,
                        Port = setting.Port
                    };

                    _persistedDevices.Add(device);
                }
            }
        }

        // called from the Cancel button to restore to initial state (display, not save)
        private void RestorePreferences()
        {
            lock (_persistedDeviceLock)
            {
                foreach (var device in _persistedDevices)
                {
                    var modifiedDevice = Devices.FirstOrDefault(d => d.DeviceType == device.DeviceType);
                    if (modifiedDevice != null)
                    {
                        modifiedDevice.Enabled = device.Enabled;
                        modifiedDevice.Manufacturer = device.Manufacturer;
                        modifiedDevice.Protocol = device.Protocol;
                        modifiedDevice.Port = device.Port;
                    }
                }
            }
        }

        private bool AreChangesMade()
        {
            if (InitialHardMeter != HardMetersEnabled || _initialDoorOpticSensor != DoorOpticSensorEnabled
                || _initialBellyPanelDoor != BellyPanelDoorEnabled || _initialBell != BellEnabled)
            {
                return true;
            }

            lock (_persistedDeviceLock)
            {
                foreach (var device in _persistedDevices)
                {
                    var modifiedDevice = Devices.FirstOrDefault(d => d.DeviceType == device.DeviceType);
                    if (modifiedDevice != null)
                    {
                        // Protocol/Port will be disabled for Fake so don't check those
                        if (modifiedDevice.Enabled != device.Enabled ||
                            modifiedDevice.Manufacturer != device.Manufacturer ||
                            modifiedDevice.ProtocolEnabled && modifiedDevice.Protocol != device.Protocol ||
                            modifiedDevice.PortEnabled && modifiedDevice.Port != device.Port)
                        {
                            modifiedDevice.Status = string.Empty;
                            modifiedDevice.StatusType = DeviceState.None;
                            return true;
                        }
                    }
                }

                return false; // falling through means no changes are detected
            }
        }

        private void Handle(OperatorCultureChangedEvent obj)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                foreach (var device in Devices)
                {
                    device.RefreshProps();
                }
            });
        }
    }
}