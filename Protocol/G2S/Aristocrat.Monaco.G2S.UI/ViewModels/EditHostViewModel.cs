namespace Aristocrat.Monaco.G2S.UI.ViewModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.ConfigWizard;
    using Application.UI.OperatorMenu;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Communications;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using Kernel;
    using Localization.Properties;
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
        [CustomValidation(typeof(EditHostViewModel), nameof(ValidateHostId))]
        public int? HostId
        {
            get => _hostId;
            set
            {
                SetProperty(ref _hostId, value, nameof(HostId), nameof(CanSave));
            }
        }

        /// <summary>
        ///     Gets or sets the host address
        /// </summary>
        [CustomValidation(typeof(EditHostViewModel), nameof(ValidateAddress))]
        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value, nameof(Address), nameof(CanSave));
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
                Execute.OnUIThread(Cancel);
            }
        }

        public static ValidationResult ValidateHostId(int? hostId, ValidationContext context)
        {
            EditHostViewModel instance = (EditHostViewModel)context.ObjectInstance;
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
            EditHostViewModel instance = (EditHostViewModel)context.ObjectInstance;
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
    }
}
