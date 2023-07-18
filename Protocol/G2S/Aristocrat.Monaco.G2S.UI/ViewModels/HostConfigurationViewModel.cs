namespace Aristocrat.Monaco.G2S.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Windows.Data;
    using System.Windows.Input;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.ConfigWizard;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Common.DHCP;
    using Common.Events;
    using Data.Profile;
    using Kernel;
    using Localization.Properties;
    using Models;
    using Monaco.Common;
    using Newtonsoft.Json;
    using Views;
    using Constants = Constants;

    /// <summary>
    ///     A HostConfigurationViewModel contains the logic for configuring the host list for the G2S client.
    /// </summary>
    public partial class HostConfigurationViewModel : ConfigWizardViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly List<Host> _addedHosts = new List<Host>();
        private readonly List<Host> _deletedHosts = new List<Host>();
        private readonly List<Host> _editedHosts = new List<Host>();
        private readonly List<Host> _originalHosts = new List<Host>();

        private IG2SEgm _egm;

        private string _macAddress;

        private int _port;
        private bool _activated;
        private bool _registeredHostsEnabled;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostConfigurationViewModel" /> class.
        /// </summary>
        public HostConfigurationViewModel(bool isWizardPage) : base(isWizardPage)
        {
            if (!InDesigner)
            {
                _dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
            }

            Hosts = new ObservableCollection<Host>();

            RegisteredHosts = new CollectionViewSource
            {
                Source = Hosts.OrderBy(h => h.Id)
            };

            RegisteredHosts.Filter += RegisteredFilter;

            ResetEditState();

            NewCommand = new ActionCommand<object>(_ => NewHost());
            EditCommand = new ActionCommand<Host>(EditHost);
            DeleteCommand = new ActionCommand<Host>(DeleteHost);

            _port = PropertiesManager.GetValue(Constants.Port, Constants.DefaultPort);

            _egm = GetEgm();

            LoadHosts();

            WireDesignerData();

#if !(RETAIL)
            if (isWizardPage)
            {
                CreateTempHosts();
            }
#endif
            CopyCurrentHostList();
        }

        public ObservableCollection<Host> Hosts { get; }

        public ICommand NewCommand { get; }

        public ICommand EditCommand { get; }

        public ICommand DeleteCommand { get; }

        public CollectionViewSource RegisteredHosts { get; }

        public bool RegisteredHostsEnabled
        {
            get => _registeredHostsEnabled;
            set
            {
                _registeredHostsEnabled = value;
                RaisePropertyChanged(nameof(RegisteredHostsEnabled));
                RaisePropertyChanged(nameof(ProgressRingIsActive));
                if (_registeredHostsEnabled && WizardNavigator != null)
                {
                    SetupNavigation();
                }
            }
        }

        public bool ProgressRingIsActive => !RegisteredHostsEnabled;

        public string EgmId => _egm?.Id ?? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable);

        public string MacAddress
        {
            get => _macAddress;

            private set
            {
                if (_macAddress != value)
                {
                    _macAddress = value;
                    RaisePropertyChanged(nameof(MacAddress));
                }
            }
        }

        public int Port
        {
            get => _port;
            set
            {
                if (_port != value)
                {
                    ValidatePort(value);
                    _port = value;
                    RaisePropertyChanged(nameof(Port));
                }
            }
        }

        protected override void OnUnloaded()
        {
            // the base calls Save, don't want to. This page saved the hosts already...
        }

        protected override void Loaded()
        {
            _egm = GetEgm();
            RaisePropertyChanged(nameof(EgmId));

            MacAddress = NetworkInterfaceInfo.DefaultPhysicalAddress;

            if (!_activated)
            {
                _activated = true;
            }
            else
            {
                ResetOriginalHosts();
            }
        }

        protected override void OnOperatorCultureChanged(OperatorCultureChangedEvent evt)
        {
            MvvmHelper.ExecuteOnUI(ResetOriginalHosts);
            base.OnOperatorCultureChanged(evt);
        }

        protected override void SaveChanges()
        {
            CommitChanges();
            OnCommitted();
            EventBus.Publish(new OperatorMenuSettingsChangedEvent());
        }

        /// <summary>
        ///     Sets ths properties in the property manager
        /// </summary>
        protected override void OnCommitted()
        {
            if (Committed)
            {
                return;
            }

            var hosts = Hosts.Cast<IHost>().ToList();
            PropertiesManager.SetProperty(Constants.RegisteredHosts, hosts);
            PropertiesManager.SetProperty(Constants.Port, Port);
            var addresses = new { addresses = hosts.Select(x => x.Address).ToList() };
            PropertiesManager.SetProperty(ApplicationConstants.HostAddresses, JsonConvert.SerializeObject(addresses));

            Committed = true;

            base.OnCommitted();
        }

        protected override void ValidateAll()
        {
            base.ValidateAll();

            ValidatePort(_port);
        }

        private static void RegisteredFilter(object sender, FilterEventArgs e)
        {
            if (e.Item is Host host)
            {
                e.Accepted = !host.IsEgm();
            }
        }

        private void ValidatePort(int value)
        {
            ClearErrors(nameof(Port));

            if (value < 0 || value > ushort.MaxValue)
            {
                SetError(nameof(Port), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Port_MustBeInRange));
            }
        }

        private void ResetEditState()
        {
            _deletedHosts.Clear();
            _addedHosts.Clear();
            _editedHosts.Clear();
        }

        private void ResetOriginalHosts()
        {
            // VLT-9125: Retain values during configuration
            if (WizardNavigator != null)
            {
                return;
            }

            Hosts.Clear();

            foreach (var h in _originalHosts)
            {
                var host = new Host
                {
                    Index = h.Index,
                    Id = h.Id,
                    Address = h.Address,
                    Registered = h.Registered,
                    RequiredForPlay = h.RequiredForPlay,
                    RegisteredDisplayText = GetBooleanDisplayText(h.Registered),
                    RequiredForPlayDisplayText = GetBooleanDisplayText(h.RequiredForPlay)
                };

                Hosts.Add(host);
            }

            RefreshHosts();
            ResetEditState();

            Port = PropertiesManager.GetValue(Constants.Port, Constants.DefaultPort);
        }

        private void LoadHosts()
        {
            if (InDesigner)
            {
                return;
            }

            Hosts.Clear();
            _registeredHostsEnabled = false;

            var hosts = PropertiesManager.GetValues<IHost>(Constants.RegisteredHosts);

            foreach (var host in hosts)
            {
                Hosts.Add(
                    new Host
                    {
                        Index = host.Index,
                        Id = host.Id,
                        Address = host.Address,
                        Registered = host.IsEgm() || host.Registered,
                        RequiredForPlay = host.RequiredForPlay,
                        RegisteredDisplayText = GetBooleanDisplayText(host.Registered),
                        RequiredForPlayDisplayText = GetBooleanDisplayText(host.RequiredForPlay)
                    });
            }

            if (!Hosts.Any())
            {
                try
                {
                    Task.Run(() => GetVendorSpecificInformation());
                }
                catch (Exception ex)
                {
                    Logger.Error($"Exception {ex} caught getting vendor specific information");
                }
            }
            else
            {
                _registeredHostsEnabled = true;
                RegisteredHosts.View.Refresh();
            }
        }

        private void CopyCurrentHostList()
        {
            _originalHosts.Clear();

            foreach (var h in Hosts)
            {
                var host = new Host
                {
                    Index = h.Index,
                    Id = h.Id,
                    Address = h.Address,
                    Registered = h.Registered,
                    RequiredForPlay = h.RequiredForPlay
                };

                _originalHosts.Add(host);
            }
        }

        private void NewHost()
        {
            int newHostId = (Hosts.Max(h => (int?)h.Id) ?? 0) + 1;

            var viewModel = new EditHostViewModel(IsWizardPage, newHostId);

            var result = _dialogService.ShowDialog<EditHostView>(
                this,
                viewModel,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NewHost));

            if (result == true)
            {
                var host = new Host
                {
                    Index = newHostId,
                    Id = viewModel.HostId ?? 0,
                    Address = new Uri(viewModel.Address),
                    Registered = viewModel.Registered,
                    RequiredForPlay = viewModel.RequiredForPlay,
                };

                Hosts.Add(host);
                _addedHosts.Add(host);

                RefreshHosts();

                SaveChanges();
            }
        }

        private void EditHost(Host host)
        {
            var viewModel = new EditHostViewModel(
                IsWizardPage,
                host.Id,
                host.Address.ToString(),
                host.Registered,
                host.RequiredForPlay);

            var result = _dialogService.ShowDialog<EditHostView>(
                this,
                viewModel,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EditHost));

            if (result == true)
            {
                if (!viewModel.HasChanges() || !viewModel.HostId.HasValue)
                {
                    //do nothing if nothing changed or we don't have a host id
                    return;
                }

                // Stop the old host and flag it for deletion if the host id changed
                var idChanged = false;
                if (host.Id != viewModel.HostId.Value)
                {
                    idChanged = true;
                    var hostControl = GetEgm()?.GetHostById(host.Id);
                    hostControl?.Stop();
                    _deletedHosts.Add(
                        new Host
                        {
                            Address = host.Address,
                            Id = host.Id,
                            Index = host.Index,
                            Registered = host.Registered,
                            RequiredForPlay = host.RequiredForPlay
                        });
                }

                host.Id = viewModel.HostId.Value;
                host.Address = new Uri(viewModel.Address);
                host.Registered = viewModel.Registered;
                host.RequiredForPlay = viewModel.RequiredForPlay;

                RefreshHosts();

                // if the id changed treat it as an add
                if (idChanged)
                {
                    _addedHosts.Add(host);
                }
                else
                {
                    _editedHosts.Add(host);
                }

                SaveChanges();
            }
        }

        private StartupContext AddHosts(IContainerService containerService)
        {
            StartupContext context = null;

            foreach (var host in _addedHosts)
            {
                if (host.Registered)
                {
                    context = RegisterHost(host, containerService);
                }
            }

            return context;
        }

        private StartupContext EditHosts(IContainerService containerService)
        {
            StartupContext context = null;

            foreach (var host in _editedHosts)
            {
                if (host.Registered)
                {
                    context = RegisterHost(host, containerService);
                }
                else
                {
                    UnregisterHost(host, containerService);
                }
            }

            return context;
        }

        private void DeleteHosts(IContainerService containerService)
        {
            foreach (var host in _deletedHosts)
            {
                Hosts.Remove(host);

                RefreshHosts();

                if (host.Registered)
                {
                    UnregisterHost(host, containerService);
                }
            }
        }

        private void DeleteHost(Host host)
        {
            var result = _dialogService.ShowYesNoDialog(this, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ConfirmDeleteHost));

            if (result == true)
            {
                _deletedHosts.Add(host);

                Hosts.Remove(host);

                RefreshHosts();

                SaveChanges();
            }
        }

        private void CommitChanges()
        {
            var containerService = ServiceManager.GetInstance().TryGetService<IContainerService>();

            if (containerService == null)
            {
                Logger.Error("CommitChanges() - Container service is unavailable. Host changes cannot be saved.");
                return;
            }

            var egm = containerService.Container.GetInstance<IG2SEgm>();

            ShowPopup(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HostConfigurationUpdatingHosts));

            // commit UI changes to properties database
            OnCommitted();
            // apply deletes to HostFactory
            DeleteHosts(containerService);
            var addContext = AddHosts(containerService);
            var editContext = EditHosts(containerService);

            // the edit save will occur last so they may have edited an added host take its context first
            var context = editContext ?? addContext;

            // VLT-9870 : spin a thread to prevent UI from hanging while protocol restarts
            Task.Run(() => egm.Restart(new List<IStartupContext> { context ?? new StartupContext() })).FireAndForget(ex => Logger.Error($"Return: Exception occurred {ex}", ex));

            ResetEditState();

            //after the commit save the new host config
            CopyCurrentHostList();
            RefreshHosts();
        }

        protected override void SetupNavigation()
        {
            var configured = Hosts.Count(h => !h.IsEgm() && h.Registered) > 0;

            WizardNavigator.CanNavigateBackward = true;
            WizardNavigator.CanNavigateForward = configured && !HasErrors && !ProgressRingIsActive;
        }

        private void RefreshHosts()
        {
            RaisePropertyChanged(nameof(Hosts));
            RegisteredHosts.View.Refresh();

            // VLT-9577 : enable Next button on wizard if enter a url in retail (default is empty in retail)
            if (WizardNavigator != null)
            {
                SetupNavigation();
            }
        }

        private StartupContext RegisterHost(IHost host, IContainerService containerService)
        {
            StartupContext context = null;

            var hostFactory = containerService.Container.GetInstance<IHostFactory>();
            var egm = containerService.Container.GetInstance<IG2SEgm>();
            var existing = egm.Hosts.FirstOrDefault(h => h.Index == host.Index);

            if (existing == null)
            {
                host = hostFactory.Create(host);
                context = RegisterHost(host, containerService);
                EventBus.Publish(new CommHostListChangedEvent(new List<int> { host.Index }));

                if (context == null)
                {
                    context = new StartupContext
                    {
                        HostId = host.Id,
                        DeviceChanged = true,
                        DeviceStateChanged = true,
                        DeviceAccessChanged = true,
                        SubscriptionLost = true,
                        MetersReset = true,
                        DeviceReset = true
                    };
                }
            }
            else
            {
                if (existing.Address != host.Address)
                {
                    context = new StartupContext { HostId = host.Id };
                }

                //Spin a new thread to not lock the UI if address does not exist
                Task.Run(() => hostFactory.Update(host)).FireAndForget(ex => Logger.Error($"Return: Exception occurred {ex}", ex));
            }

            return context;
        }

        private void UnregisterHost(IHost host, IContainerService containerService)
        {
            var egm = containerService.Container.GetInstance<IG2SEgm>();

            var egmHost = egm.Hosts.FirstOrDefault(h => h.Index == host.Index);
            if (egmHost?.Registered ?? false)
            {
                var hostFactory = containerService.Container.GetInstance<IHostFactory>();
                hostFactory.Delete(egmHost);
                Logger.Debug("Reset attached devices for deleted host(s).");
                // VLT-8307
                ResetDevicesAfterHostDeletion(containerService, egmHost, egm);
            }
        }

        private void ResetDevicesAfterHostDeletion(IContainerService containerService, IHost host, IG2SEgm egm)
        {
            if (egm == null)
            {
                return;
            }

            var devices = egm.GetDevices<ClientDeviceBase>().OrderBy(d => d.DeviceClass).ThenBy(i => i.Id).ToList();
            var profileService = containerService?.Container.GetInstance<IProfileService>();

            if (profileService != null)
            {
                // spin through all devices attached to this host and set owner to 0 and active to false
                foreach (var device in devices.Where(device => device.IsMember(host.Id)))
                {
                    Logger.Debug($"Host {host.Id} has been deleted resetting device '{device.DeviceClass}' ownership to 0 and deactivate");
                    device.HasOwner(0, device.Active);
                    profileService.Save(device);
                }
            }
            else
            {
                Logger.Warn($"Profile Service unavailable - unable to reset Device ownership for deleted host {host.Id}.");
            }
        }

        private void CreateTempHosts()
        {
            // We're going to forcibly add the RGS host here until we have a proper keyboard and a dialog to capture this data
            for (var id = 1; id < 2; id++)
            {
                var host = Hosts.FirstOrDefault(h => h.Index == id);
                if (host == null)
                {
                    Hosts.Add(
                        new Host
                        {
                            Index = id,
                            Id = id,
                            Address = new Uri(
                                $"http://{Dns.GetHostEntry(string.Empty).HostName}:3110{id}/RGS/api-services/G2SAPI"),
                            Registered = id == 1,
                            RequiredForPlay = false,
                        });
                }
            }
        }

        private void GetVendorSpecificInformation()
        {
            var dhcpClient = ServiceManager.GetInstance().GetService<IDhcpClient>();

            var vendorSpecificString = dhcpClient.GetVendorSpecificInformation();
            if (!string.IsNullOrEmpty(vendorSpecificString))
            {
                Logger.Info($"Vendor specific information data: {vendorSpecificString}");

                var info = VendorSpecificInformation.Create(vendorSpecificString);

                var index = 0;
                foreach (var host in info.CommConfigDefinitions)
                {
                    index++;

                    var requiredForPlay = _egm?.GetDevice<ICommunicationsDevice>(host.HostId)?.RequiredForPlay ?? false;

                    Hosts.Add(
                        new Host
                        {
                            Index = index,
                            Id = host.HostId,
                            Address = host.Address,
                            Registered = true,
                            RequiredForPlay = requiredForPlay,
                        });
                }
            }

            MvvmHelper.ExecuteOnUI(EnableRegisteredHosts);
        }

        private static IG2SEgm GetEgm()
        {
            var containerService = ServiceManager
                .GetInstance()
                .TryGetService<IContainerService>();

            return containerService?.Container.GetInstance<IG2SEgm>();
        }

        private void EnableRegisteredHosts()
        {
            RegisteredHostsEnabled = true;
            RegisteredHosts.View.Refresh();
        }

        protected override void LoadAutoConfiguration()
        {
            if (AutoConfigurator == null || !AutoConfigurator.AutoConfigurationExists)
            {
                return;
            }

            // For now, only support auto-configuration of a single host where the ID will be
            // forced to 1 and registered will be true.
            string hostUriString = null;
            if (!AutoConfigurator.GetValue("G2SHostUri", ref hostUriString))
            {
                return;
            }

            try
            {
                var hostUri = new Uri(hostUriString);
                Hosts.Clear();
                Hosts.Add(
                    new Host
                    {
                        Index = 1,
                        Id = 1,
                        Address = hostUri,
                        Registered = true,
                        RequiredForPlay = false,
                    });

                RefreshHosts();

                PropertiesManager?.SetProperty(ApplicationConstants.DemonstrationMode, false);

                base.LoadAutoConfiguration();
            }
            catch (UriFormatException ex)
            {
                Logger.Warn("G2S host URI is malformed", ex);
            }
        }

        protected override void DisposeInternal()
        {
            if (RegisteredHosts != null)
            {
                RegisteredHosts.Filter -= RegisteredFilter;
            }

            base.DisposeInternal();
        }

    }
}
