namespace Aristocrat.Monaco.G2S.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.OperatorMenu;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.Extensions.CommunityToolkit;
    using Common.Events;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Data.Profile;
    using G2S;
    using Kernel;
    using Localization.Properties;
    using Monaco.UI.Common.Extensions;
    using Views;

    [CLSCompliant(false)]
    public class DeviceManagerViewModel : OperatorMenuPageViewModelBase
    {
        private IProfileService _profileService;
        private readonly IDialogService _dialogService;
        private IG2SEgm _egm;
        private IContainerService _containerService;
        private List<ClientDeviceBase> _devices;
        private readonly List<EditableDevice> _editableDevices;
        private string _deviceName;
        private int _deviceOwner;
        private bool _deviceChangeEnabled;
        private EditableDevice _selectedDevice;
        private bool _isDirty;
        private bool _saveInProgress;
        private readonly ILocalizer _operatorLocalizer;

        public ObservableCollection<EditableDevice> ActiveDevices { get; }

        public EditableDevice SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                EditCommand.NotifyCanExecuteChanged();
            }
        }

        public List<ClientDeviceBase> Devices
        {
            get => _devices;
            set
            {
                _devices = value;
                OnPropertyChanged(nameof(Devices));
            }
        }

        public string DeviceName
        {
            get => _deviceName;

            set
            {
                if (_deviceName != value)
                {
                    _deviceName = value;
                    OnPropertyChanged(nameof(DeviceName));
                }

            }
        }

        public int DeviceOwner
        {
            get => _deviceOwner;

            set
            {
                if (_deviceOwner != value)
                {
                    _deviceOwner = value;
                    OnPropertyChanged(nameof(DeviceOwner));
                }
            }
        }

        public bool DeviceChangeEnabled
        {
            get => _deviceChangeEnabled;

            set
            {
                if (_deviceChangeEnabled != value)
                {
                    _deviceChangeEnabled = value;
                    OnPropertyChanged(nameof(DeviceChangeEnabled));
                }
            }
        }

        private bool IsDirty
        {
            get => _isDirty;

            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    SaveChangesCommand.NotifyCanExecuteChanged();
                    CancelChangesCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private bool SaveInProgress
        {
            get => _saveInProgress;

            set
            {
                if (_saveInProgress != value)
                {
                    _saveInProgress = value;
                    SaveChangesCommand.NotifyCanExecuteChanged();
                    CancelChangesCommand.NotifyCanExecuteChanged();
                    BulkChangesCommand.NotifyCanExecuteChanged();
                    EditCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private bool CanSaveChanges => GameIdle && IsDirty && !SaveInProgress;

        private bool CanCancelChanges => IsDirty && !SaveInProgress;

        private bool CanEditSelected => SelectedDevice != null && GameIdle && !SaveInProgress;

        private bool CanBulkChanges => GameIdle && !SaveInProgress;

        public RelayCommand<object> EditCommand { get; }

        public RelayCommand<object> CancelChangesCommand { get; }

        public RelayCommand<object> SaveChangesCommand { get; }

        public RelayCommand<object> BulkChangesCommand { get; }


        public DeviceManagerViewModel()
        {
            Logger.Debug("Loading Device Manager ViewModel");

            _dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
            _editableDevices = new List<EditableDevice>();
            _operatorLocalizer = Localizer.For(CultureFor.Operator);
            ActiveDevices = new ObservableCollection<EditableDevice>();

            EditCommand = new RelayCommand<object>(EditDevice, _ => CanEditSelected);
            CancelChangesCommand = new RelayCommand<object>(CancelChanges, _ => CanCancelChanges);
            SaveChangesCommand = new RelayCommand<object>(SaveChanges, _ => CanSaveChanges);
            BulkChangesCommand = new RelayCommand<object>(BulkChanges, _ => CanBulkChanges);
            EventBus.Subscribe<ProtocolsInitializedEvent>(this, HandleEvent);
        }

        protected override void OnLoaded()
        {
            Logger.Debug("OnLoaded() Device Manager ViewModel - reloading services and devices");
            _containerService = ServiceManager.GetInstance().TryGetService<IContainerService>();
            _profileService = _containerService?.Container.GetInstance<IProfileService>();

            LoadDevices();
            ReloadEditableView();

            IsDirty = false;
            InputStatusText =
                GameIdle ? string.Empty : _operatorLocalizer.GetString(ResourceKeys.EndGameRoundBeforeChange);
        }

        private void LoadDevices()
        {
            Devices = new List<ClientDeviceBase>();
            _egm = _containerService?.Container.GetInstance<IG2SEgm>();
            if (_egm != null)
            {
                Devices = _egm.GetDevices<ClientDeviceBase>().OrderBy(d => d.DeviceClass).ThenBy(i => i.Id).ToList();
            }

            DeviceChangeEnabled = Devices.Count > 0;
        }

        private void ReloadEditableView()
        {
            _editableDevices.Clear();
            foreach (var d in Devices)
            {
                // edited is set to false in ctor
                var editableDevice = new EditableDevice
                {
                    DeviceClass = d.DeviceClass,
                    Id = d.Id,
                    Owner = d.Owner,
                    Enabled = d.Enabled,
                    HostEnabled = d.HostEnabled,
                    Active = d.Active,
                    IsHostOriented = d.IsHostOriented(),
                    ActiveDisplayText = GetBooleanDisplayText(d.Active),
                    EnabledDisplayText = GetBooleanDisplayText(d.Enabled)
                };
                _editableDevices.Add(editableDevice);
            }

            ActiveDevices.Clear();
            ActiveDevices.AddRange(_editableDevices.OrderBy(h => h.Id));
        }

        private void EditDevice(object obj)
        {
            var device = SelectedDevice;
            if (device.IsHostOriented)
            {
                Logger.Info(
                    "Device Manager - Editing device '" + device.DeviceClass +
                    "' is host-oriented and cannot be edited.");
                ShowPopup(_operatorLocalizer.GetString(ResourceKeys.DeviceManagerPopupIsHostOriented), 4);
                return;
            }

            var viewModel = new EditDeviceViewModel
            {
                DeviceName = device.DeviceClass,
                DeviceId = device.Id,
                OwnerId = device.Owner,
                Enabled = device.Enabled,
                HostEnabled = device.HostEnabled,
                Active = device.Active,
            };

            var result = _dialogService.ShowDialog<EditDeviceView>(
                this,
                viewModel,
                _operatorLocalizer.GetString(ResourceKeys.EditDevice));


            if (result == false)
                return; // user canceled out

            if (device.Owner == viewModel.OwnerId && device.Enabled == viewModel.Enabled &&
                device.Active == viewModel.Active)
                return; //do nothing if nothing changed

            if (device.Owner != viewModel.OwnerId)
            {
                Logger.Info(
                    "Device Manager - Editing device '" + device.DeviceClass + "' set Owner to ->' " +
                    viewModel.OwnerId + "'.");
                device.Owner = viewModel.OwnerId;
                device.Edited = true;
            }

            if (device.Active != viewModel.Active)
            {
                Logger.Info(
                    "Device Manager - Editing device '" + device.DeviceClass + "' set Active to -> '" +
                    viewModel.Active + "'.");
                device.Active = viewModel.Active;
                device.Edited = true;
            }

            OnPropertyChanged(nameof(ActiveDevices));

            IsDirty = true;
            InputStatusText =
                (GameIdle ? string.Empty : _operatorLocalizer.GetString(ResourceKeys.EndGameRoundBeforeChange));
        }

        private void SaveChanges(object obj)
        {
            if (_profileService == null)
            {
                Logger.Warn("Device Manager - Profile Service is unavailable, changes not saved.");
                ShowPopup(_operatorLocalizer.GetString(ResourceKeys.DeviceManagerPopupProfileServiceUnavailable));
                return;
            }

            SaveInProgress = true;

            Logger.Info("Device Manager - Saving changes");
            ShowPopup(_operatorLocalizer.GetString(ResourceKeys.DeviceManagerPopupSaveChanges));

            var restart = false;

            foreach (var editableDevice in _editableDevices)
            {
                if (!editableDevice.Edited) // only edit the devices with actual changes
                    continue;

                restart = true; // if we catch at least one edit then trigger a protocol restart below
                var edits = Devices.Where(
                    d => d.DeviceClass == editableDevice.DeviceClass && d.Id == editableDevice.Id);

                foreach (var device in edits)
                {
                    if (device.Owner != editableDevice.Owner)
                    {
                        Logger.Info(
                            "Device Manager - Saving changes to device '" + editableDevice.DeviceClass +
                            "' Owner from '" + device.Owner + "' to '" + editableDevice.Owner + "'.");
                    }

                    if (device.Active != editableDevice.Active)
                    {
                        Logger.Info(
                            "Device Manager - Saving changes to device '" + editableDevice.DeviceClass +
                            "' Active from '" + device.Owner + "' to '" + editableDevice.Owner + "'.");
                    }

                    device.HasOwner(editableDevice.Owner, editableDevice.Active);
                    _profileService.Save(device);
                }
            }

            if (restart)
            {
                EventBus.Publish(new OperatorMenuSettingsChangedEvent());
                EventBus.Publish(new RestartProtocolEvent());
            }

            IsDirty = false;
            if (!restart)
            {
                // if we are restarting the protocol, don't clear SaveInProgress till the protocol restarts.
                SaveInProgress = false;
            }
        }

        private void CancelChanges(object obj)
        {
            Logger.Info("Device Manager - Canceling changes");
            ShowPopup(_operatorLocalizer.GetString(ResourceKeys.DeviceManagerPopupCancelChanges));

            ReloadEditableView(); // reset the view to initial load state

            IsDirty = false;
        }

        private void BulkChanges(object obj)
        {
            try
            {
                var viewModel = new EditBulkDeviceViewModel();
                var result = _dialogService.ShowDialog<EditBulkDeviceView>(
                    this,
                    viewModel,
                    _operatorLocalizer.GetString(ResourceKeys.BulkEditDevices)
                );

                if (result == false)
                    return; // user canceled out

                ShowPopup(_operatorLocalizer.GetString(ResourceKeys.DeviceManagerPopupBulkChanges));


                if (viewModel.SelectedField ==
                    _operatorLocalizer.GetString(ResourceKeys.DeviceManagerOwnerSelection))
                {
                    Logger.Info(
                        "Device Manager - Bulk changes set all Owner Ids to '" + viewModel.SelectedHostId + "'.");
                    foreach (var d in _editableDevices)
                    {
                        if (d.IsHostOriented)
                        {
                            Logger.Info(
                                "Device Manager - Editing device '" + d.DeviceClass + "' under Host '"
                                + d.Id + "'is host-oriented and Owner cannot be edited.");
                            continue;
                        }

                        d.Owner = viewModel.SelectedHostId;
                        d.Edited = true;
                    }
                }

                if (viewModel.SelectedField ==
                    _operatorLocalizer.GetString(ResourceKeys.DeviceManagerActiveSelection))
                {
                    Logger.Info(
                        "Device Manager - Bulk changes set all Active to '" + viewModel.SelectedActive + "'.");
                    foreach (var d in _editableDevices)
                    {
                        if (d.IsHostOriented)
                        {
                            Logger.Info(
                                "Device Manager - Editing device '" + d.DeviceClass + "' under Host '"
                                + d.Id + "' is host-oriented and Active cannot be edited.");
                            continue;
                        }

                        d.Active = viewModel.SelectedActive;
                        d.Edited = true;
                    }
                }

                OnPropertyChanged(nameof(ActiveDevices));

                EventBus.Publish(new OperatorMenuSettingsChangedEvent());
                IsDirty = true;
                InputStatusText = (GameIdle
                    ? string.Empty
                    : _operatorLocalizer.GetString(ResourceKeys.EndGameRoundBeforeChange));
            }
            catch (NullReferenceException)
            {
                ShowPopup("Profile Manager restarting...     Retry bulk edit");
            }
        }

        private void HandleEvent(ProtocolsInitializedEvent evt)
        {
            Execute.OnUIThread(() => SaveInProgress = false);
        }

        protected override void OnOperatorCultureChanged(OperatorCultureChangedEvent evt)
        {
            Execute.OnUIThread(ReloadEditableView);
            base.OnOperatorCultureChanged(evt);
        }
    }

    public class EditableDevice : ObservableObject
    {
        private string _deviceClass;
        private int _id;
        private int _owner;
        private bool _active;
        private bool _enabled;
        private bool _hostEnabled;
        private bool _isHostOriented;
        private bool _edited;
        private string _activeDisplayText;
        private string _enabledDisplayText;

        internal EditableDevice()
        {
            _edited = false;
        }

        public string DeviceClass
        {
            get => _deviceClass;
            set
            {
                if (_deviceClass == value)
                    return;

                _deviceClass = value;
            }
        }

        public int Id
        {
            get => _id;
            set
            {
                if (_id == value)
                    return;

                _id = value;
            }
        }

        public int Owner
        {
            get => _owner;
            set
            {
                if (_owner == value)
                    return;

                _owner = value;
                OnPropertyChanged(nameof(Owner));
            }
        }

        public bool Active
        {
            get => _active;
            set
            {
                if (_active == value)
                    return;

                _active = value;
                OnPropertyChanged(nameof(Active));
                OnPropertyChanged(nameof(ActiveDisplayText));
            }
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value)
                    return;

                _enabled = value;
                OnPropertyChanged(nameof(Enabled));
            }
        }

        public bool HostEnabled
        {
            get => _hostEnabled;
            set
            {
                if (_hostEnabled == value)
                    return;

                _hostEnabled = value;
                OnPropertyChanged(nameof(HostEnabled));
            }
        }

        public string ActiveDisplayText
        {
            get => _activeDisplayText;
            set
            {
                if (_activeDisplayText == value)
                {
                    return;
                }

                _activeDisplayText = value;
                OnPropertyChanged(nameof(ActiveDisplayText));
            }
        }

        public string EnabledDisplayText
        {
            get => _enabledDisplayText;

            set
            {
                if (_enabledDisplayText == value)
                {
                    return;
                }

                _enabledDisplayText = value;
                OnPropertyChanged(nameof(EnabledDisplayText));
            }
        }

        public bool IsHostOriented
        {
            get => _isHostOriented;

            set
            {
                if (_isHostOriented == value)
                    return;

                _isHostOriented = value;
            }
        }

        public bool Edited
        {
            get => _edited;
            set
            {
                if (_edited == value)
                    return;

                _edited = value;
            }
        }
    }
}
