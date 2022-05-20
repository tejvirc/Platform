namespace Aristocrat.Monaco.G2S.UI.ViewModels
{
    using System;
    using System.Linq;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.ConfigWizard;
    using Application.UI.OperatorMenu;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Communications;
    using Kernel;
    using Localization.Properties;
    using MVVM;
    using Constants = Constants;

    /// <summary>
    ///     The EditHostViewModel supports adding and editing a G2S host.
    /// </summary>
    public sealed partial class EditHostViewModel : OperatorMenuSaveViewModelBase, IConfigWizardDialog
    {
        private readonly string _originalAddress;
        private readonly int _originalHostId;
        private readonly bool _originalRegistered;
        private readonly bool _originalRequiredForPlay;

        private string _address;
        private int? _hostId;
        private bool _registered;
        private bool _requiredForPlay;

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
            bool requiredForPlay = false)
        {
            IsInWizard = isInWizard;

            _hostId = hostId;
            _originalHostId = hostId;
            _originalAddress = _address = address ?? string.Empty;
            _originalRegistered = _registered = registered;
            _originalRequiredForPlay = _requiredForPlay = requiredForPlay;

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

        public bool IsInWizard { get; set; }

        public override bool HasChanges()
        {
            return !string.IsNullOrWhiteSpace(Address)
                   && (_originalAddress != Address
                       || _originalHostId != HostId
                       || _originalRegistered != Registered
                       || _originalRequiredForPlay != RequiredForPlay);
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
