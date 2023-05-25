namespace Aristocrat.Monaco.G2S.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
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
    using Kernel;
    using Localization.Properties;
    using MVVM;
    using Constants = Constants;

    /// <summary>
    ///     The EditHostViewModel supports adding and editing a G2S host.
    /// </summary>
    public sealed partial class EditHostViewModel : OperatorMenuSaveViewModelBase, IConfigWizardDialog
    {

        private readonly string[] _commonVertexHosts = new string[] { };
        private readonly string _originalAddress;
        private readonly int _originalHostId;
        private readonly TimeSpan _originalOfflineTimerInterval;
        private readonly TimeSpan _recommendedOfflineTimerInterval;
        private readonly bool _originalRegistered;
        private readonly bool _originalRequiredForPlay;
        private readonly bool _originalIsProgressiveHost;

        private string _address;
        private int? _hostId;
        private TimeSpan _offlineTimerInterval;
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
            double offlineTimerInterval = 30)
        {
            IsInWizard = isInWizard;

            _hostId = hostId;
            _originalHostId = hostId;
            _originalAddress = _address = address ?? string.Empty;
            _originalRegistered = _registered = registered;
            _originalRequiredForPlay = _requiredForPlay = requiredForPlay;
            _originalIsProgressiveHost = _isProgressiveHost = isProgressiveHost;
            _originalOfflineTimerInterval = TimeSpan.FromSeconds(offlineTimerInterval);
            _offlineTimerInterval = TimeSpan.FromSeconds(offlineTimerInterval);
            _recommendedOfflineTimerInterval = TimeSpan.FromSeconds(30);


            IMultiProtocolConfigurationProvider MPCProvider = ServiceManager.GetInstance().TryGetService<IMultiProtocolConfigurationProvider>();
            var G2SConfig = MPCProvider.MultiProtocolConfiguration.FirstOrDefault(c => c.Protocol == CommsProtocol.G2S);
            if(G2SConfig != null && G2SConfig.IsProgressiveHandled)
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
        public int? HostId
        {
            get => _hostId;
            set
            {
                if (SetProperty(ref _hostId, value, nameof(HostId)))
                {
                    ValidateHostId(_hostId);
                    RaisePropertyChanged(nameof(CanSave));
                }
            }
        }

        /// <summary>
        ///     Gets or Sets the Progressive Host Offline Check Frequency
        /// </summary>
        public double OfflineTimerInterval
        {
            get => _offlineTimerInterval.TotalSeconds;
            set
            {
                if (SetProperty(ref _offlineTimerInterval, TimeSpan.FromSeconds(value), nameof(OfflineTimerInterval)))
                {
                    RaisePropertyChanged(nameof(CanSave));
                }
                RaisePropertyChanged(nameof(IsOfflineTimerIntervalUnderRecommended));
            }
        }

        /// <summary>
        ///     Gets whether the current offline timer interval is under the default recommended 
        /// </summary>
        public bool IsOfflineTimerIntervalUnderRecommended => _offlineTimerInterval.TotalSeconds < _recommendedOfflineTimerInterval.TotalSeconds;

        /// <summary>
        ///     Gets or sets the host address
        /// </summary>
        public string Address
        {
            get => _address;
            set
            {
                if (SetProperty(ref _address, value, nameof(Address)))
                {
                    ValidateAddress(_address);
                    RaisePropertyChanged(nameof(CanSave));
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
                RaisePropertyChanged(nameof(CommonAddresses));
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
                RaisePropertyChanged(nameof(SelectedCommonAddress));
            }
        }

        public Visibility AddressComboBoxVisibility
        {
            get => _addressComboBoxVisibility;
            set
            {
                _addressComboBoxVisibility = value;
                RaisePropertyChanged(nameof(AddressComboBoxVisibility));
            }
        }

        public Visibility SpecificProgressiveHostCheckboxVisibility
        {
            get => _specificProgressiveHostCheckboxVisibility;
            set
            {
                _specificProgressiveHostCheckboxVisibility = value;
                RaisePropertyChanged(nameof(SpecificProgressiveHostCheckboxVisibility));
            }
        }

        public int AddressTextBoxColumnSpan
        {
            get => _addressTextBoxColumnSpan;
            set
            {
                _addressTextBoxColumnSpan = value;
                RaisePropertyChanged(nameof(AddressTextBoxColumnSpan));
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the host is registered
        /// </summary>
        public bool Registered
        {
            get => _registered;
            set => SetProperty(ref _registered, value, nameof(Registered), nameof(CanSave));
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the device MUST be functioning and enabled before the
        ///     egm can be played.
        /// </summary>
        public bool RequiredForPlay
        {
            get => _requiredForPlay;
            set => SetProperty(ref _requiredForPlay, value, nameof(RequiredForPlay), nameof(CanSave));
        }

        /// <summary>
        ///     Gets or sets the value indicating whether or not the specific host will be the progressive host
        /// </summary>
        public bool IsProgressiveHost
        {
            get => _isProgressiveHost;
            set
            {
                SetProperty(ref _isProgressiveHost, value, nameof(IsProgressiveHost), nameof(CanSave));
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
                       || _originalOfflineTimerInterval.TotalSeconds != OfflineTimerInterval);
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
                MvvmHelper.ExecuteOnUI(Cancel);
            }
        }

        private void ValidateHostId(int? hostId)
        {
            ClearErrors(nameof(HostId));

            if (hostId.HasValue && _originalHostId == hostId.Value)
            {
                return;
            }

            if (!hostId.HasValue || hostId.Value <= 0)
            {
                SetError(nameof(HostId), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HostIdGreaterThanZero));
            }
            else if (HostIdExists(hostId.Value))
            {
                SetError(nameof(HostId), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HostExists));
            }
        }

        private bool HostIdExists(int hostId)
        {
            return PropertiesManager
                .GetValues<IHost>(Constants.RegisteredHosts)
                .Any(host => host.Id == hostId);
        }

        private void ValidateAddress(string address)
        {
            ClearErrors(nameof(Address));

            if (!IsAddressValid(address))
            {
                SetError(nameof(Address), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HostAddressNotValid));
            }
        }

        private static bool IsAddressValid(string address)
        {
            return !string.IsNullOrWhiteSpace(address)
                   && Uri.TryCreate(address, UriKind.Absolute, out var uri)
                   && EndpointUtilities.IsSchemeValid(uri);
        }
    }
}
