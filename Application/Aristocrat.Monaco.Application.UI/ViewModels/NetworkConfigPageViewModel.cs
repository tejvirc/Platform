namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.UI.Common.MVVM;
    using Aristocrat.Extensions.CommunityToolkit;
    using ConfigWizard;
    using Contracts;
    using Contracts.Localization;
    using Kernel;
    using Monaco.Localization.Properties;
    using Monaco.UI.Common;
    using log4net;

    /// <summary>
    ///     View model for network configuration
    /// </summary>
    [CLSCompliant(false)]
    public sealed class NetworkConfigPageViewModel : ConfigWizardViewModelBase
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(NetworkConfigPageViewModel));

        private readonly INetworkService _network = ServiceManager.GetInstance().GetService<INetworkService>();

        private bool _canApplyChanges;

        private bool _dhcpEnabled;

        private string _dnsServer1;
        private string _dnsServer2;
        private string _gateway;

        private string _ipAddress;
        private bool _showStatus;
        private bool _staticIp;
        private string _subnetMask;

        private bool _originalDhcpEnabled;
        private bool _originalStaticIp;
        private string _originalIpAddress;
        private string _originalSubnetMask;
        private string _originalGateway;
        private string _originalDnsServer1;
        private string _originalDnsServer2;

        public bool CanApplyChanges
        {
            get => _canApplyChanges;
            set => SetProperty(ref _canApplyChanges, value);
        }

        [CustomValidation(typeof(NetworkConfigPageViewModel), nameof(ValidateDnsServer))]
        public string DnsServer1
        {
            get => _dnsServer1;
            set => SetProperty(ref _dnsServer1, value, IsLoaded && !DhcpEnabled);
        }

        [CustomValidation(typeof(NetworkConfigPageViewModel), nameof(ValidateDnsServer))]
        public string DnsServer2
        {
            get => _dnsServer2;
            set => SetProperty(ref _dnsServer2, value, IsLoaded && !DhcpEnabled);
        }

        [CustomValidation(typeof(NetworkConfigPageViewModel), nameof(ValidateIpAddress))]
        public string IpAddress
        {
            get => _ipAddress;
            set => SetProperty(ref _ipAddress, value, IsLoaded && !DhcpEnabled);
        }

        [CustomValidation(typeof(NetworkConfigPageViewModel), nameof(ValidateSubnetMask))]
        public string SubnetMask
        {
            get => _subnetMask;
            set => SetProperty(ref _subnetMask, value, IsLoaded && !DhcpEnabled);
        }

        [CustomValidation(typeof(NetworkConfigPageViewModel), nameof(ValidateGateway))]
        public string Gateway
        {
            get => _gateway;
            set => SetProperty(ref _gateway, value, IsLoaded && !DhcpEnabled);
        }

        public bool StaticIp
        {
            get => _staticIp;
            set => SetProperty(ref _staticIp, value, nameof(StaticIp));
        }

        public bool DhcpEnabled
        {
            get => _dhcpEnabled;
            set
            {
                if (SetProperty(ref _dhcpEnabled, value, nameof(DhcpEnabled)))
                {
                    RunCustomValidation();
                }
            }
        }

        [IgnoreTracking]
        public bool ShowStatus
        {
            get => _showStatus;
            set => SetProperty(ref _showStatus, value);
        }

        public NetworkConfigPageViewModel(bool isWizardPage) : base(isWizardPage)
        {
            InputStatusText = string.Empty;
            PropertyChanged += NetworkConfigPageViewModel_PropertyChanged;
        }

        ~NetworkConfigPageViewModel()
        {
            Dispose();
        }

        protected override void DisposeInternal()
        {
            base.DisposeInternal();
            PropertyChanged -= NetworkConfigPageViewModel_PropertyChanged;
        }

        private void NetworkConfigPageViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateCanApplyChanges();
        }

        private void RefreshNetworkInfo()
        {
            var info = _network.GetNetworkInfo();

            _originalStaticIp = StaticIp = !info.DhcpEnabled;
            _originalIpAddress = IpAddress = info.IpAddress;
            _originalSubnetMask = SubnetMask = info.SubnetMask;
            _originalGateway = Gateway = info.DefaultGateway;
            _originalDnsServer1 = DnsServer1 = info.PreferredDnsServer;
            _originalDnsServer2 = DnsServer2 = info.AlternateDnsServer;
            _originalDhcpEnabled = DhcpEnabled = info.DhcpEnabled;
        }

        protected override void OnCommitted()
        {
            base.OnCommitted();

            var networkInfo = new NetworkInfo
            {
                DhcpEnabled = _dhcpEnabled,
                IpAddress = _ipAddress,
                SubnetMask = _subnetMask,
                DefaultGateway = _gateway,
                PreferredDnsServer = _dnsServer1,
                AlternateDnsServer = _dnsServer2
            };

            ShowStatus = true;

            Task.Run(() =>
                {
                    _network.SetNetworkInfo(networkInfo);
                })
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Log.Error("Encountered exception while attempting to set network info", task.Exception);
                    }
                    else
                    {
                        Execute.OnUIThread(() =>
                        {
                            RefreshNetworkInfo();
                            RunCustomValidation();
                            ShowStatus = false;
                            IsCommitted = true;
                        });
                    }
                });
        }

        private bool HasChanges()
        {
            return DhcpEnabled != _originalDhcpEnabled ||
                   StaticIp != _originalStaticIp ||
                   IpAddress != _originalIpAddress ||
                   SubnetMask != _originalSubnetMask ||
                   Gateway != _originalGateway ||
                   DnsServer1 != _originalDnsServer1 ||
                   DnsServer2 != _originalDnsServer2;
        }

        protected override void SaveChanges()
        {
        }

        protected override void Loaded()
        {
            RefreshNetworkInfo();
            RunCustomValidation();
            IsCommitted = true;
        }

        protected override void OnInputEnabledChanged()
        {
            UpdateCanApplyChanges();
        }

        protected override void RunCustomValidation()
        {
            if (DhcpEnabled)
            {
                ClearErrors(nameof(IpAddress));
                ClearErrors(nameof(SubnetMask));
                ClearErrors(nameof(Gateway));
                ClearErrors(nameof(DnsServer1));
                ClearErrors(nameof(DnsServer2));
            }
            else
            {
                ValidateProperty(IpAddress, nameof(IpAddress));
                ValidateProperty(SubnetMask, nameof(SubnetMask));
                ValidateProperty(Gateway, nameof(Gateway));
                ValidateProperty(DnsServer1, nameof(DnsServer1));
                ValidateProperty(DnsServer2, nameof(DnsServer2));
            }
            UpdateCanApplyChanges();
        }

        private void UpdateCanApplyChanges()
        {
            CanApplyChanges = !HasErrors && InputEnabled && HasChanges();
        }

        public static ValidationResult ValidateIpAddress(string address, ValidationContext context)
        {
            if (!IpValidation.IsIpV4AddressValid(address))
            {
                return new(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AddressNotValid));
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateSubnetMask(string address, ValidationContext context)
        {
            if (!IpValidation.IsIpV4SubnetMaskValid(address))
            {
                return new(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AddressNotValid));
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateDnsServer(string address, ValidationContext context)
        {
            if (!string.IsNullOrEmpty(address) && !IpValidation.IsIpV4AddressValid(address))
            {
                return new(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AddressNotValid));
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateGateway(string address, ValidationContext context)
        {
            var instance = (NetworkConfigPageViewModel)context.ObjectInstance;
            if (instance.StaticIp)
            {
                if (!string.IsNullOrEmpty(address) && !IpValidation.IsIpV4AddressValid(address))
                {
                    return new(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AddressNotValid));
                }

                return ValidationResult.Success;
            }

            if (!IpValidation.IsIpV4AddressValid(address))
            {
                return new(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AddressNotValid));
            }

            return ValidationResult.Success;
        }
    }
}
