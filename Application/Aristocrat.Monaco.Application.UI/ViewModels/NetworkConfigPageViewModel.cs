namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using ConfigWizard;
    using Contracts;
    using Contracts.Localization;
    using Kernel;
    using Monaco.Localization.Properties;
    using Monaco.UI.Common;
    using MVVM;

    /// <summary>
    ///     View model for network configuration
    /// </summary>
    [CLSCompliant(false)]
    public sealed class NetworkConfigPageViewModel : ConfigWizardViewModelBase
    {
        private readonly INetworkService _network;

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

        public NetworkConfigPageViewModel(bool isWizardPage) : base(isWizardPage)
        {
            if (!isWizardPage)
            {
                ValidateAll();
                // Set whether the operator can change the time zone with credits on the machine.
            }

            _network = ServiceManager.GetInstance().GetService<INetworkService>();

            InputStatusText = string.Empty;
            IgnorePropertyForCommitted(nameof(ShowStatus));
        }

        public bool CanApplyChanges => InputEnabled && HasChanges();

        public string DnsServer1
        {
            get => _dnsServer1;

            set
            {
                if (value != _dnsServer1)
                {
                    ValidateNetworkAddressAllowEmpty(value, nameof(DnsServer1));
                    _dnsServer1 = value;
                    RaisePropertyChanged(nameof(DnsServer1), nameof(CanApplyChanges));
                }
            }
        }

        public string DnsServer2
        {
            get => _dnsServer2;

            set
            {
                if (value != _dnsServer2)
                {
                    ValidateNetworkAddressAllowEmpty(value, nameof(DnsServer2));
                    _dnsServer2 = value;
                    RaisePropertyChanged(nameof(DnsServer2), nameof(CanApplyChanges));
                }
            }
        }

        public string IpAddress
        {
            get => _ipAddress;

            set
            {
                if (value != _ipAddress)
                {
                    ValidateNetworkAddress(value, nameof(IpAddress));
                    _ipAddress = value;
                    RaisePropertyChanged(nameof(IpAddress), nameof(CanApplyChanges));
                }
            }
        }

        public string SubnetMask
        {
            get => _subnetMask;

            set
            {
                if (value != _subnetMask)
                {
                    ValidateSubnetAddress(value, nameof(SubnetMask));
                    _subnetMask = value;
                    RaisePropertyChanged(nameof(SubnetMask), nameof(CanApplyChanges));
                }
            }
        }

        public string Gateway
        {
            get => _gateway;

            set
            {
                if (value != _gateway)
                {
                    if (StaticIp)
                    {
                        ValidateNetworkAddressAllowEmpty(value, nameof(Gateway));
                    }
                    else
                    {
                        ValidateNetworkAddress(value, nameof(Gateway));
                    }
                    _gateway = value;
                    RaisePropertyChanged(nameof(Gateway), nameof(CanApplyChanges));
                }
            }
        }

        public bool StaticIp
        {
            get => _staticIp;

            set
            {
                if (_staticIp != value)
                {
                    _staticIp = value;
                    RaisePropertyChanged(nameof(StaticIp), nameof(CanApplyChanges));
                }
            }
        }

        public bool DhcpEnabled
        {
            get => _dhcpEnabled;

            set
            {
                if (_dhcpEnabled == value)
                {
                    return;
                }

                _dhcpEnabled = value;
                RaisePropertyChanged(nameof(DhcpEnabled), nameof(CanApplyChanges));

                if (_dhcpEnabled)
                {
                    // RefreshScreen();
                    ClearErrors(nameof(IpAddress));
                    ClearErrors(nameof(SubnetMask));
                    ClearErrors(nameof(Gateway));
                    ClearErrors(nameof(DnsServer1));
                    ClearErrors(nameof(DnsServer2));
                }
                else
                {
                    ValidateAll();

                    RaisePropertyChanged(nameof(IpAddress));
                    RaisePropertyChanged(nameof(SubnetMask));
                    RaisePropertyChanged(nameof(Gateway));
                    RaisePropertyChanged(nameof(DnsServer1));
                    RaisePropertyChanged(nameof(DnsServer2));
                }

            }
        }

        public bool ShowStatus
        {
            get => _showStatus;

            set
            {
                if (_showStatus != value)
                {
                    _showStatus = value;
                    RaisePropertyChanged(nameof(ShowStatus));
                }
            }
        }

        ~NetworkConfigPageViewModel()
        {
            Dispose();
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

            Task.Run(
                () =>
                {
                    _network.SetNetworkInfo(networkInfo);
                    MvvmHelper.ExecuteOnUI(
                        () =>
                        {
                            RefreshScreen();
                            ShowStatus = false;
                            Committed = true;
                        });
                });
        }

        protected override void ValidateAll()
        {
            base.ValidateAll();

            if (!DhcpEnabled)
            {
                ValidateNetworkAddress(IpAddress, nameof(IpAddress));
                ValidateSubnetAddress(SubnetMask, nameof(SubnetMask));
                if (StaticIp)
                {
                    ValidateNetworkAddressAllowEmpty(Gateway, nameof(Gateway));
                }
                else
                {
                    ValidateNetworkAddress(Gateway, nameof(Gateway));
                }
                ValidateNetworkAddressAllowEmpty(DnsServer1, nameof(DnsServer1));
                ValidateNetworkAddressAllowEmpty(DnsServer2, nameof(DnsServer2));
            }
        }

        private bool HasChanges()
        {
            return _originalDhcpEnabled != DhcpEnabled ||
                   _originalStaticIp != StaticIp ||
                   _originalIpAddress != IpAddress ||
                   _originalSubnetMask != SubnetMask ||
                   _originalGateway != Gateway ||
                   _originalDnsServer1 != DnsServer1 ||
                   _originalDnsServer2 != DnsServer2;
        }

        protected override void SaveChanges()
        {

        }

        protected override void Loaded()
        {
            RefreshScreen();
            Committed = true;
        }

        protected override void OnInputEnabledChanged()
        {
            RaisePropertyChanged(nameof(CanApplyChanges));
        }

        private void RefreshScreen()
        {
            var info = _network.GetNetworkInfo();

            _originalStaticIp = StaticIp = !info.DhcpEnabled;
            _originalIpAddress = IpAddress = info.IpAddress;
            _originalSubnetMask = SubnetMask = info.SubnetMask;
            _originalGateway = Gateway = info.DefaultGateway;
            _originalDnsServer1 = DnsServer1 = info.PreferredDnsServer;
            _originalDnsServer2 = DnsServer2 = info.AlternateDnsServer;

            // keep this property assignment at last so the validation logic inside the property setter
            // can check and clear the errors on other properties accordingly
            _originalDhcpEnabled = DhcpEnabled = info.DhcpEnabled;
        }

        private void ValidateSubnetAddress(string address, string propertyName)
        {
            ClearErrors(propertyName);

            if (!IpValidation.IsIpV4SubnetMaskValid(address))
            {
                SetError(propertyName, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AddressNotValid));
            }
        }

        private void ValidateNetworkAddress(string address, string propertyName)
        {
            ClearErrors(propertyName);

            if (!IpValidation.IsIpV4AddressValid(address))
            {
                SetError(propertyName, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AddressNotValid));
            }
        }

        private void ValidateNetworkAddressAllowEmpty(string address, string propertyName)
        {
            ClearErrors(propertyName);

            if (!string.IsNullOrEmpty(address) && !IpValidation.IsIpV4AddressValid(address))
            {
                SetError(propertyName, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AddressNotValid));
            }
        }
    }
}
