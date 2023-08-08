namespace Aristocrat.Monaco.G2S.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.ConfigWizard;
    using Application.UI.OperatorMenu;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Communications;
    using Aristocrat.Monaco.Application.Contracts.Protocol;
    using Aristocrat.Monaco.G2S.Services;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using Kernel;
    using Localization.Properties;
    using Constants = Constants;

    /// <summary>
    ///     The EditHostViewModel supports adding and editing a G2S host.
    /// </summary>
    public sealed partial class EditHostViewModel : OperatorMenuSaveViewModelBase, IConfigWizardDialog
    {

        private readonly string[] _commonVertexHosts = new string[] { };
        private readonly string _originalAddress;
        private readonly int _originalHostId;
        private readonly int _originalOfflineTimerIntervalSeconds;
        private readonly int _recommendedOfflineTimerIntervalSeconds;
        private readonly bool _originalRegistered;
        private readonly bool _originalRequiredForPlay;
        private readonly bool _originalIsProgressiveHost;

        private string _address;
        private int? _hostId;
        private int _offlineTimerIntervalSeconds;
        private bool _registered;
        private bool _requiredForPlay;
        private bool _isProgressiveHost;

        private List<string> _commonAddresses = new List<string>();
        private object _selectedCommonAddress;
        private Visibility _addressComboBoxVisibility = Visibility.Hidden;
        private Visibility _specificProgressiveHostCheckboxVisibility = Visibility.Hidden;
        private int _addressTextBoxColumnSpan = 2;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EditHostViewModel" /> class.
        /// </summary>
        /// <param name="isInWizard">Is the page in a wizard</param>
        /// <param name="hostId">The host id</param>
        /// <param name="address">The host address</param>
        /// <param name="registered">Is the device registered</param>
        /// <param name="requiredForPlay">Is the device required for play</param>
        public EditHostViewModel(
            bool isInWizard,
            int hostId,
            string address = null,
            bool registered = false,
            bool requiredForPlay = false,
            bool isProgressiveHost = false,
            int offlineTimerIntervalSeconds = 30)
        {
            IsInWizard = isInWizard;

            _hostId = hostId;
            _originalHostId = hostId;
            _originalAddress = _address = address ?? string.Empty;
            _originalRegistered = _registered = registered;
            _originalRequiredForPlay = _requiredForPlay = requiredForPlay;
            _originalIsProgressiveHost = _isProgressiveHost = isProgressiveHost;
            _originalOfflineTimerIntervalSeconds = offlineTimerIntervalSeconds;
            _offlineTimerIntervalSeconds = offlineTimerIntervalSeconds;
            _recommendedOfflineTimerIntervalSeconds = 30;


            IMultiProtocolConfigurationProvider MPCProvider = ServiceManager.GetInstance().TryGetService<IMultiProtocolConfigurationProvider>();
            var G2SConfig = MPCProvider.MultiProtocolConfiguration.FirstOrDefault(c => c.Protocol == CommsProtocol.G2S);
            if (G2SConfig != null && G2SConfig.IsProgressiveHandled)
            {
                _addressComboBoxVisibility = Visibility.Visible;
                _specificProgressiveHostCheckboxVisibility = Visibility.Visible;
                _addressTextBoxColumnSpan = 1;

                for (int i = 0; i < _commonVertexHosts.Length; i++)
                {
                    _commonAddresses.Add(_commonVertexHosts[i]);
                }
            }


            EventBus.Subscribe<OperatorMenuEnteredEvent>(this, HandleOperatorMenuEntered);
            WireDesignerData();
        }

        /// <summary>
        ///     Gets or sets the host identifier
        /// </summary>
        [CustomValidation(typeof(EditHostViewModel), nameof(ValidateHostId))]
        public int? HostId
        {
            get => _hostId;
            set
            {
                if (SetProperty(ref _hostId, value, true))
                {
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        /// <summary>
        ///     Gets or Sets the Progressive Host Offline Check Frequency
        /// </summary>
        [CustomValidation(typeof(EditHostViewModel), nameof(ValidateOfflineTimerInterval))]
        public int OfflineTimerIntervalSeconds
        {
            get => _offlineTimerIntervalSeconds;
            set
            {
                if (SetProperty(ref _offlineTimerIntervalSeconds, value, true))
                {
                    OnPropertyChanged(nameof(CanSave));
                }
                OnPropertyChanged(nameof(IsOfflineTimerIntervalUnderRecommended));
            }
        }

        /// <summary>
        ///     Gets whether the current offline timer interval is under the default recommended 
        /// </summary>
        public bool IsOfflineTimerIntervalUnderRecommended => _offlineTimerIntervalSeconds < _recommendedOfflineTimerIntervalSeconds;

        /// <summary>
        ///     Gets or sets the host address
        /// </summary>
        [CustomValidation(typeof(EditHostViewModel), nameof(ValidateAddress))]
        public string Address
        {
            get => _address;
            set
            {
                if (SetProperty(ref _address, value, true))
                {
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        /// <summary>
        ///     List of commonly used Vertex G2S addresses, will only be used while the Vertex G2S is enabled
        /// </summary>
        public List<string> CommonAddresses
        {
            get => _commonAddresses;
            set
            {
                _commonAddresses = value;
                OnPropertyChanged(nameof(CommonAddresses));
            }
        }

        /// <summary>
        ///     The current selected object, used for updating the value stored within the 'Address' text field.
        /// </summary>
        public object SelectedCommonAddress
        {
            get => _selectedCommonAddress;
            set
            {
                _selectedCommonAddress = value;
                Address = value.ToString();
                OnPropertyChanged(nameof(SelectedCommonAddress));
            }
        }

        public Visibility AddressComboBoxVisibility
        {
            get => _addressComboBoxVisibility;
            set
            {
                _addressComboBoxVisibility = value;
                OnPropertyChanged(nameof(AddressComboBoxVisibility));
            }
        }

        public Visibility SpecificProgressiveHostCheckboxVisibility
        {
            get => _specificProgressiveHostCheckboxVisibility;
            set
            {
                _specificProgressiveHostCheckboxVisibility = value;
                OnPropertyChanged(nameof(SpecificProgressiveHostCheckboxVisibility));
            }
        }

        public int AddressTextBoxColumnSpan
        {
            get => _addressTextBoxColumnSpan;
            set
            {
                _addressTextBoxColumnSpan = value;
                OnPropertyChanged(nameof(AddressTextBoxColumnSpan));
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the host is registered
        /// </summary>
        public bool Registered
        {
            get => _registered;
            set
            {
                if (SetProperty(ref _registered, value, nameof(Registered)))
                {
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the device MUST be functioning and enabled before the
        ///     egm can be played.
        /// </summary>
        public bool RequiredForPlay
        {
            get => _requiredForPlay;
            set
            {
                if (SetProperty(ref _requiredForPlay, value, nameof(RequiredForPlay)))
                {
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the value indicating whether or not the specific host will be the progressive host
        /// </summary>
        public bool IsProgressiveHost
        {
            get => _isProgressiveHost;
            set
            {
                if (SetProperty(ref _isProgressiveHost, value, nameof(IsProgressiveHost)))
                {
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public bool IsInWizard { get; set; }

        public override bool HasChanges()
        {
            return !string.IsNullOrWhiteSpace(Address)
                   && (_originalAddress != Address
                       || _originalHostId != HostId
                       || _originalRegistered != Registered
                       || _originalRequiredForPlay != RequiredForPlay
                       || _originalIsProgressiveHost != IsProgressiveHost
                       || _originalOfflineTimerIntervalSeconds != OfflineTimerIntervalSeconds);
        }

        /// <summary>
        ///     This property is used to determine whether or not the progressive host field has been changed and is toggled on.
        /// </summary>
        public bool IsProgressiveHostChangedAndToggled
        {
            get { return _originalIsProgressiveHost != IsProgressiveHost && IsProgressiveHost; }
        }

        private void HandleOperatorMenuEntered(OperatorMenuEnteredEvent operatorMenuEvent)
        {
            if (!operatorMenuEvent.IsTechnicianRole)
            {
                Execute.OnUIThread(Cancel);
            }
        }

        public static ValidationResult ValidateHostId(int? hostId, ValidationContext context)
        {
            var instance = (EditHostViewModel)context.ObjectInstance;
            instance.ClearErrors(nameof(HostId));
            var errors = "";
            if (hostId.HasValue && instance._originalHostId == hostId.Value)
            {
                return ValidationResult.Success;
            }

            if (!hostId.HasValue || hostId.Value <= 0)
            {
                errors = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HostIdGreaterThanZero);
            }
            else if (instance.HostIdExists(hostId.Value))
            {
                errors = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HostExists);
            }
            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }
            return new(errors);
        }

        private bool HostIdExists(int hostId)
        {
            return PropertiesManager
                .GetValues<IHost>(Constants.RegisteredHosts)
                .Any(host => host.Id == hostId);
        }

        public static ValidationResult ValidateAddress(string address, ValidationContext context)
        {
            var instance = (EditHostViewModel)context.ObjectInstance;
            var errors = "";
            instance.ClearErrors(nameof(Address));

            if (!IsAddressValid(address))
            {
                errors = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HostAddressNotValid);
            }
            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }
            return new(errors);
        }

        private static bool IsAddressValid(string address)
        {
            return !string.IsNullOrWhiteSpace(address)
                   && Uri.TryCreate(address, UriKind.Absolute, out var uri)
                   && EndpointUtilities.IsSchemeValid(uri);
        }

        public static ValidationResult ValidateOfflineTimerInterval(double seconds, ValidationContext context)
        {
            var instance = (EditHostViewModel)context.ObjectInstance;
            instance.ClearErrors(nameof(OfflineTimerIntervalSeconds));

            if (seconds <= 0)
            {
                return new(string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GreaterThanErrorMessage), 0));
            }

            return ValidationResult.Success;
        }
    }
}
