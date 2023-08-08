namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Hardware.Contracts.Gds.NoteAcceptor;
    using Aristocrat.Monaco.UI.Common.MVVM;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using ConfigWizard;
    using Contracts;
    using Contracts.Localization;
    using Kernel;
    using Monaco.Localization.Properties;
    using Monaco.UI.Common;

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
                RunCustomValidation();
            }

            _network = ServiceManager.GetInstance().GetService<INetworkService>();

            InputStatusText = string.Empty;
        }

        public bool CanApplyChanges => !HasErrors && InputEnabled && HasChanges();

        [CustomValidation(typeof(NetworkConfigPageViewModel), nameof(ValidateIpAddressAllowEmpty))]
        public string DnsServer1
        {
            get => _dnsServer1;
            set
            {
                if (SetProperty(ref _dnsServer1, value, true))
                {
                    OnPropertyChanged(nameof(CanApplyChanges));
                }
            }
        }

        [CustomValidation(typeof(NetworkConfigPageViewModel), nameof(ValidateIpAddressAllowEmpty))]
        public string DnsServer2
        {
            get => _dnsServer2;
            set
            {
                if (SetProperty(ref _dnsServer2, value, true))
                {
                    OnPropertyChanged(nameof(CanApplyChanges));
                }
            }
        }

        [CustomValidation(typeof(NetworkConfigPageViewModel), nameof(ValidateIpAddress))]
        public string IpAddress
        {
            get => _ipAddress;
            set
            {
                if (SetProperty(ref _ipAddress, value, true))
                {
                    OnPropertyChanged(nameof(CanApplyChanges));
                }
            }
        }

        [CustomValidation(typeof(NetworkConfigPageViewModel), nameof(ValidateSubnetMask))]
        public string SubnetMask
        {
            get => _subnetMask;
            set
            {
                if (SetProperty(ref _subnetMask, value, true))
                {
                    OnPropertyChanged(nameof(CanApplyChanges));
                }
            }
        }

        [CustomValidation(typeof(NetworkConfigPageViewModel), nameof(ValidateIpAddressAllowEmptyIfStatic))]
        public string Gateway
        {
            get => _gateway;
            set
            {
                if (SetProperty(ref _gateway, value, true))
                {
                    OnPropertyChanged(nameof(CanApplyChanges));
                }
            }
        }

        public bool StaticIp
        {
            get => _staticIp;
            set
            {
                if (SetProperty(ref _staticIp, value, nameof(StaticIp)))
                {
                    OnPropertyChanged(nameof(CanApplyChanges));
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

                if (SetProperty(ref _dhcpEnabled, value, nameof(DhcpEnabled)))
                {
                    OnPropertyChanged(nameof(CanApplyChanges));
                }

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
                    RunCustomValidation();

                    OnPropertyChanged(nameof(IpAddress));
                    OnPropertyChanged(nameof(SubnetMask));
                    OnPropertyChanged(nameof(Gateway));
                    OnPropertyChanged(nameof(DnsServer1));
                    OnPropertyChanged(nameof(DnsServer2));
                }

            }
        }

        [IgnoreTracking]
        public bool ShowStatus
        {
            get => _showStatus;
            set => SetProperty(ref _showStatus, value);
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
                    Execute.OnUIThread(
                        () =>
                        {
                            RefreshScreen();
                            ShowStatus = false;
                            IsCommitted = true;
                        });
                });
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
            IsCommitted = true;
        }

        protected override void OnInputEnabledChanged()
        {
            OnPropertyChanged(nameof(CanApplyChanges));
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

        protected override void RunCustomValidation()
        {
            if (!DhcpEnabled)
            {
                ValidateProperty(IpAddress, nameof(IpAddress));
                ValidateProperty(SubnetMask, nameof(SubnetMask));
                ValidateProperty(Gateway, nameof(Gateway));
                ValidateProperty(DnsServer1, nameof(DnsServer1));
                ValidateProperty(DnsServer2, nameof(DnsServer2));
            }
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

        public static ValidationResult ValidateIpAddressAllowEmpty(string address, ValidationContext context)
        {
            if (!string.IsNullOrEmpty(address) && !IpValidation.IsIpV4AddressValid(address))
            {
                return new(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AddressNotValid));
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateIpAddressAllowEmptyIfStatic(string address, ValidationContext context)
        {
            NetworkConfigPageViewModel instance = (NetworkConfigPageViewModel)context.ObjectInstance;
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
