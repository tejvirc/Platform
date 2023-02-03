namespace Aristocrat.Monaco.Hhr.UI.ViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Application.Contracts.Localization;
    using Application.UI.ConfigWizard;
    using Application.Contracts.OperatorMenu;
    using Client.Messages;
    using CommunityToolkit.Mvvm.Input;
    using Kernel;
    using Kernel.Contracts;
    using Localization.Properties;
    using Monaco.UI.Common;
    using Vgt.Client12.Application.OperatorMenu;
    using System.ComponentModel.DataAnnotations;

    //using Aristocrat.Monaco.Common.Validation;

    public class ServerConfigurationPageViewModel : ConfigWizardViewModelBase
    {
        private string _ipAddress;
        private string _tcpPortNumber;
        private string _udpPortNumber;
        private string _encryptionKey;
        private string _manualHandicapMode;
        private readonly string _initialIpAddress;
        private readonly string _initialTcpPortNumber;
        private readonly string _initialUdpPortNumber;
        private readonly string _initialEncryptionKey;
        private readonly string _initialManualHandicapMode;
        private bool _isConfigChanged;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IOperatorMenuLauncher _operatorMenuLauncher;

        private const int MaxPortNumber = ushort.MaxValue;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServerConfigurationPageViewModel" /> class.
        /// </summary>
        public ServerConfigurationPageViewModel(bool isWizardPage)
            : base(isWizardPage)
        {
            _propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _operatorMenuLauncher = isWizardPage ? null : ServiceManager.GetInstance().GetService<IOperatorMenuLauncher>();

            IpAddress = _initialIpAddress = _propertiesManager.GetValue(HHRPropertyNames.ServerTcpIp, HhrConstants.DefaultServerTcpIp);
            TcpPortNumber = _initialTcpPortNumber = _propertiesManager.GetValue(HHRPropertyNames.ServerTcpPort, HhrConstants.DefaultServerTcpPort).ToString();
            UdpPortNumber = _initialUdpPortNumber = _propertiesManager.GetValue(HHRPropertyNames.ServerUdpPort, HhrConstants.DefaultServerUdpPort).ToString();
            EncryptionKey = _initialEncryptionKey = _propertiesManager.GetValue(HHRPropertyNames.EncryptionKey, HhrConstants.DefaultEncryptionKey);
            ManualHandicapMode = _initialManualHandicapMode = _propertiesManager.GetValue(HHRPropertyNames.ManualHandicapMode, HhrConstants.DetectPickMode);

            PropertyChanged += ServerConfigurationPageViewModel_PropertyChanged;

            ApplyServerConfigurationCommand = new RelayCommand<object>(Apply);
        }

        protected override void Loaded()
        {
            ValidateProperty(IpAddress, nameof(IpAddress));
            ValidateProperty(TcpPortNumber, nameof(TcpPortNumber));
            ValidateProperty(UdpPortNumber, nameof(UdpPortNumber));
        }

        public RelayCommand<object> ApplyServerConfigurationCommand { get; set; }

        private void ServerConfigurationPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IpAddress) && e.PropertyName != nameof(TcpPortNumber) &&
                e.PropertyName != nameof(UdpPortNumber) && e.PropertyName != nameof(EncryptionKey)
                && e.PropertyName != nameof(ManualHandicapMode))
            {
                return;
            }

            if (_initialIpAddress == IpAddress &&
                _initialTcpPortNumber == TcpPortNumber &&
                _initialUdpPortNumber == UdpPortNumber &&
                _initialEncryptionKey == EncryptionKey &&
                _initialManualHandicapMode == ManualHandicapMode)
            {
                IsConfigChanged = false;
            }
            else
            {
                IsConfigChanged = !HasErrors;
            }
        }

        [CustomValidation(typeof(ServerConfigurationPageViewModel), nameof(NetworkAddressValidate))]
        public string IpAddress
        {
            get => _ipAddress;

            set
            {
                SetProperty(ref _ipAddress, value, true, nameof(IpAddress));
            }
        }

        [CustomValidation(typeof(ServerConfigurationPageViewModel), nameof(TcpPortValidate))]
        public string TcpPortNumber
        {
            get => _tcpPortNumber;

            set => SetProperty(ref _tcpPortNumber, value, true, nameof(TcpPortNumber));
        }


        [CustomValidation(typeof(ServerConfigurationPageViewModel), nameof(TcpPortValidate))]
        public string UdpPortNumber
        {
            get => _udpPortNumber;

            set => SetProperty(ref _udpPortNumber, value, true, nameof(UdpPortNumber));
            
        }

        public string EncryptionKey
        {
            get => _encryptionKey;

            set
            {
                if (value == _encryptionKey)
                {
                    return;
                }

                SetProperty(ref _encryptionKey, value, nameof(EncryptionKey));
            }
        }

        public string ManualHandicapMode
        {
            get => _manualHandicapMode;

            set
            {
                if (value == _manualHandicapMode)
                {
                    return;
                }

                SetProperty(ref _manualHandicapMode, value, nameof(ManualHandicapMode));
            }
        }

        public bool IsConfigChanged
        {
            get => _isConfigChanged;

            set
            {
                if (value == _isConfigChanged)
                {
                    return;
                }

                SetProperty(ref _isConfigChanged, value, nameof(IsConfigChanged));
            }
        }

        private bool IsValueInvalidPortNumber(string value)
        {
            if (int.TryParse(value, out var parsedValue))
            {
                return parsedValue > MaxPortNumber || parsedValue < 1;
            }

            return true;
        }

        public static ValidationResult NetworkAddressValidate(string address, ValidationContext context)
        {
           var errors = "";

            if (!IpValidation.IsIpV4AddressValid(address))
            {
                errors = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AddressNotValid);
            }

            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }
            return new(errors);
        }

        public static ValidationResult TcpPortValidate(string value, ValidationContext context)
        {
            ServerConfigurationPageViewModel instance = (ServerConfigurationPageViewModel)context.ObjectInstance;
            var errors = "";
            if (string.IsNullOrEmpty(value))
            {
                errors = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CannotBeLeftBlankErrorMessage);
            }
            else if (instance.IsValueInvalidPortNumber(value))
            {
                errors = string.Format(
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LessThanOrEqualErrorMessage),
                    MaxPortNumber);
            }
            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }
            return new(errors);
        }

        private void Apply(object parameter)
        {
            EventBus.Publish(new OperatorMenuSettingsChangedEvent());
            SetHhrProperties();
            _operatorMenuLauncher?.Close();
            EventBus.Publish(new ExitRequestedEvent(ExitAction.Restart));
        }

        protected override void SaveChanges()
        {
            if (IsWizardPage)
            {
                SetHhrProperties();
            }
        }

        private void SetHhrProperties()
        {
            if (IpValidation.IsIpV4AddressValid(IpAddress))
            {
                _propertiesManager.SetProperty(HHRPropertyNames.ServerTcpIp, IpAddress);
            }

            if (!string.IsNullOrEmpty(TcpPortNumber) && !IsValueInvalidPortNumber(TcpPortNumber))
            {
                _propertiesManager.SetProperty(HHRPropertyNames.ServerTcpPort, int.Parse(TcpPortNumber));
            }

            if (!string.IsNullOrEmpty(UdpPortNumber) && !IsValueInvalidPortNumber(UdpPortNumber))
            {
                _propertiesManager.SetProperty(HHRPropertyNames.ServerUdpPort, int.Parse(UdpPortNumber));
            }

            _propertiesManager.SetProperty(HHRPropertyNames.EncryptionKey, EncryptionKey);

            _propertiesManager.SetProperty(HHRPropertyNames.ManualHandicapMode, ManualHandicapMode);
        }

        private void CheckNavigation()
        {
            if (!IsWizardPage) return;

            WizardNavigator.CanNavigateForward = !HasErrors;
        }

        public List<string> ManualHandicapModes => new List<string>
        {
            HhrConstants.DetectPickMode,
            HhrConstants.QuickPickMode,
            HhrConstants.AutoPickMode
        };
    }
}
