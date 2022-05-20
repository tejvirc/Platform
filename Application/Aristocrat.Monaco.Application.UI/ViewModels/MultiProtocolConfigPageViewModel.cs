namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ConfigWizard;
    using Contracts;
    using Contracts.Protocol;
    using Monaco.Common;
    using Protocol = global::Protocol;

    [CLSCompliant(false)]
    public class MultiProtocolConfigPageViewModel : ConfigWizardViewModelBase
    {
        private const int MinProtocols = 1;

        private readonly IMultiProtocolConfigurationProvider _multiProtocolConfigurationProvider;
        private readonly IProtocolCapabilityAttributeProvider _protocolCapabilityAttributeProvider;
        private readonly IConfigurationUtilitiesProvider _configurationUtilitiesProvider;
        private IReadOnlyCollection<ProtocolConfiguration> _multiProtocolConfiguration;

        private IReadOnlyCollection<CommsProtocol> _validationProtocols;
        private IReadOnlyCollection<CommsProtocol> _fundTransferProtocols;
        private IReadOnlyCollection<CommsProtocol> _progressiveProtocols;
        private IReadOnlyCollection<CommsProtocol> _centralDeterminationSystemProtocols;

        private CommsProtocol _validationProtocol;
        private CommsProtocol _fundTransferProtocol;
        private CommsProtocol _progressiveProtocol;
        private CommsProtocol _centralDeterminationSystemProtocol;

        private bool _isProgressiveComboBoxEnabled;
        private bool _isValidationComboBoxEnabled;
        private bool _isFundTransferComboBoxEnabled;
        private bool _isCentralDeterminationSystemComboBoxEnabled;

        private ConfigWizardConfigurationProtocolConfiguration _protocolConfiguration;

        public IReadOnlyCollection<CommsProtocol> ValidationProtocols
        {
            get => _validationProtocols;
            set => SetProperty(ref _validationProtocols, value, nameof(ValidationProtocols));
        }

        public IReadOnlyCollection<CommsProtocol> FundTransferProtocols
        {
            get => _fundTransferProtocols;
            set => SetProperty(ref _fundTransferProtocols, value, nameof(FundTransferProtocols));
        }

        public IReadOnlyCollection<CommsProtocol> ProgressiveProtocols
        {
            get => _progressiveProtocols;
            set => SetProperty(ref _progressiveProtocols, value, nameof(ProgressiveProtocols));
        }

        public IReadOnlyCollection<CommsProtocol> CentralDeterminationSystemProtocols
        {
            get => _centralDeterminationSystemProtocols;
            set => SetProperty(ref _centralDeterminationSystemProtocols, value, nameof(CentralDeterminationSystemProtocols));
        }

        public bool IsValidationProtocolsEmpty => IsProtocolCollectionEmpty(ValidationProtocols);
        public bool IsFundTransferProtocolsEmpty => IsProtocolCollectionEmpty(FundTransferProtocols);
        public bool IsProgressiveProtocolsEmpty => IsProtocolCollectionEmpty(ProgressiveProtocols);
        public bool IsCentralDeterminationSystemsEmpty => IsProtocolCollectionEmpty(CentralDeterminationSystemProtocols);

        public CommsProtocol ValidationProtocol
        {
            get => _validationProtocol;
            set
            {
                if (_validationProtocol == value) return;
                if (IsValidationComboBoxEnabled) SetProperty(ref _validationProtocol, value, nameof(ValidationProtocol));
            }
        }

        public CommsProtocol FundTransferProtocol
        {
            get => _fundTransferProtocol;

            set
            {
                if (_fundTransferProtocol == value) return;
                if (IsFundTransferComboBoxEnabled) SetProperty(ref _fundTransferProtocol, value, nameof(FundTransferProtocol));
            }
        }

        public CommsProtocol ProgressiveProtocol
        {
            get => _progressiveProtocol;

            set
            {
                if (_progressiveProtocol == value) return;
                if (IsProgressiveComboBoxEnabled) SetProperty(ref _progressiveProtocol, value, nameof(ProgressiveProtocol));
            }
        }

        public CommsProtocol CentralDeterminationSystemProtocol
        {
            get => _centralDeterminationSystemProtocol;

            set
            {
                if (value == _centralDeterminationSystemProtocol) return;
                SetProperty(ref _centralDeterminationSystemProtocol, value, nameof(CentralDeterminationSystemProtocol));
            }
        }

        public bool IsProgressiveComboBoxEnabled
        {
            get => _isProgressiveComboBoxEnabled;
            set
            {
                if (value == _isProgressiveComboBoxEnabled) return;
                SetProperty(ref _isProgressiveComboBoxEnabled, value, nameof(IsProgressiveComboBoxEnabled));
            }
        }

        public bool IsValidationComboBoxEnabled
        {
            get => _isValidationComboBoxEnabled;
            set
            {
                if (value == _isValidationComboBoxEnabled) return;
                SetProperty(ref _isValidationComboBoxEnabled, value, nameof(IsValidationComboBoxEnabled));
            }
        }

        public bool IsFundTransferComboBoxEnabled
        {
            get => _isFundTransferComboBoxEnabled;
            set
            {
                if (value == _isFundTransferComboBoxEnabled) return;
                SetProperty(ref _isFundTransferComboBoxEnabled, value, nameof(IsFundTransferComboBoxEnabled));
            }
        }

        public bool IsCentralDeterminationSystemComboBoxEnabled
        {
            get => _isCentralDeterminationSystemComboBoxEnabled;
            set
            {
                if (value == _isCentralDeterminationSystemComboBoxEnabled) return;
                SetProperty(ref _isCentralDeterminationSystemComboBoxEnabled, value, nameof(IsCentralDeterminationSystemComboBoxEnabled));
            }
        }

        public MultiProtocolConfigPageViewModel(
            IMultiProtocolConfigurationProvider multiProtocolConfigurationProvider,
            IProtocolCapabilityAttributeProvider protocolCapabilityAttributeProvider,
            IConfigurationUtilitiesProvider configurationUtilitiesProvider)
            : base(true)
        {
            _multiProtocolConfigurationProvider = multiProtocolConfigurationProvider ?? throw new ArgumentNullException(nameof(multiProtocolConfigurationProvider));
            _protocolCapabilityAttributeProvider = protocolCapabilityAttributeProvider ?? throw new ArgumentNullException(nameof(protocolCapabilityAttributeProvider));
            _configurationUtilitiesProvider = configurationUtilitiesProvider ?? throw new ArgumentNullException(nameof(configurationUtilitiesProvider));
        }

        protected override void Loaded()
        {
            _multiProtocolConfiguration = _multiProtocolConfigurationProvider.MultiProtocolConfiguration.ToList();

            var protocolsConfiguration = _configurationUtilitiesProvider
                .GetConfigWizardConfiguration(() => new ConfigWizardConfiguration
                {
                    ProtocolConfiguration = new ConfigWizardConfigurationProtocolConfiguration
                    {
                        ProtocolsAllowed = ProtocolNames.All.Select(s => new Protocol { Name = EnumParser.ParseOrThrow<CommsProtocol>(s) }).ToArray(),
                        ExclusiveProtocols = Array.Empty<ExclusiveProtocol>(),
                        RequiredFunctionality = Array.Empty<FunctionalityType>()
                    }
                });

            _protocolConfiguration = protocolsConfiguration.ProtocolConfiguration;

            IsProgressiveComboBoxEnabled = true;
            IsValidationComboBoxEnabled = true;
            IsFundTransferComboBoxEnabled = true;
            IsCentralDeterminationSystemComboBoxEnabled = true;

            LoadProtocols();
        }

        private void LoadProtocols()
        {
            var protocolDetails = GetProtocolDetails().AsReadOnly();

            ValidationProtocols = GetProtocols(protocolDetails, Functionality.Validation, s => s.IsValidationSupported);
            FundTransferProtocols = GetProtocols(protocolDetails, Functionality.FundsTransfer, s => s.IsFundTransferSupported);
            ProgressiveProtocols = GetProtocols(protocolDetails, Functionality.Progressive, s => s.IsProgressivesSupported);
            CentralDeterminationSystemProtocols = _multiProtocolConfigurationProvider.IsCentralDeterminationSystemRequired ?
                    GetProtocols(protocolDetails, Functionality.CentralDeterminationSystem, s => s.IsCentralDeterminationSystemSupported) :
                    new List<CommsProtocol>();

            SelectProtocol(
                Functionality.Validation,
                ValidationProtocols,
                protocolDetails,
                (protocol, isEnabled) =>
                {
                    ValidationProtocol = protocol;
                    IsValidationComboBoxEnabled = isEnabled;
                });

            SelectProtocol(
                Functionality.FundsTransfer,
                FundTransferProtocols,
                protocolDetails,
                (protocol, isEnabled) =>
                {
                    FundTransferProtocol = protocol;
                    IsFundTransferComboBoxEnabled = isEnabled;
                });

            SelectProtocol(
                Functionality.Progressive,
                ProgressiveProtocols,
                protocolDetails,
                (protocol, isEnabled) =>
                {
                    ProgressiveProtocol = protocol;
                    IsProgressiveComboBoxEnabled = isEnabled;
                });

            if (_multiProtocolConfigurationProvider.IsCentralDeterminationSystemRequired)
            {
                SelectProtocol(
                    Functionality.CentralDeterminationSystem,
                    CentralDeterminationSystemProtocols,
                    protocolDetails,
                    (protocol, isEnabled) =>
                    {
                        CentralDeterminationSystemProtocol = protocol;
                        IsCentralDeterminationSystemComboBoxEnabled = isEnabled;
                    });
            }

            RaisePropertyChanged(nameof(IsValidationProtocolsEmpty), nameof(IsFundTransferProtocolsEmpty), nameof(IsProgressiveProtocolsEmpty), nameof(IsCentralDeterminationSystemsEmpty));
        }

        private List<(CommsProtocol Protocol, ProtocolCapabilityAttribute Attribute)> GetProtocolDetails()
        {
            var protocolDetails = _multiProtocolConfiguration
                .Select(s =>
                    {
                        var protocolId = s.Protocol;

                        return (
                            protocolId,
                            _protocolCapabilityAttributeProvider.GetAttribute(EnumParser.ToName(protocolId))
                        );
                    })
                .ToList();

            return protocolDetails;
        }

        /// <summary>
        /// Selects the initial protocol for specified functionality
        /// </summary>
        /// <param name="functionality">The functionality we're setting the protocol for.</param>
        /// <param name="availableProtocols">The available protocols that can provide the specified functionality.</param>
        /// <param name="protocolDetails">List of functionality each protocol supports.</param>
        /// <param name="onSelectedCallback">Callback invoked when protocol selection has completed</param>
        private void SelectProtocol(
            Functionality functionality,
            IReadOnlyCollection<CommsProtocol> availableProtocols,
            IEnumerable<(CommsProtocol Protocol, ProtocolCapabilityAttribute Attribute)> protocolDetails,
            Action<CommsProtocol, bool> onSelectedCallback)
        {
            //The protocol specified as exclusive will always be selected
            var exclusive = _protocolConfiguration.ExclusiveProtocols?
                                                                .SingleOrDefault(s => s.Function == functionality)
                                                                ?.Name;

            var (isValid, result) = EnumParser.Parse<CommsProtocol>(exclusive);
            if (isValid && result.HasValue && availableProtocols.Contains(result.Value))
            {
                onSelectedCallback(result.Value, false);
                return;
            }

            //Otherwise, if this functionality is required, select the first protocol providing the required functionality
            if (_protocolConfiguration.RequiredFunctionality?.Any(a => a.Type == functionality) ?? false)
            {
                var requiredFunctionalityProtocol = protocolDetails
                    .FirstOrDefault(w => w.Protocol != CommsProtocol.None && w.Attribute.ProvidedFunctions.Contains(functionality));

                if (requiredFunctionalityProtocol != default)
                {
                    onSelectedCallback(requiredFunctionalityProtocol.Protocol, availableProtocols.Count > 1);
                    return;
                }
            }

            //If we haven't found a good selection yet then default to first non-None option
            onSelectedCallback(availableProtocols.FirstOrDefault(p => p != CommsProtocol.None), availableProtocols.Count > 1);
        }

        private List<CommsProtocol> GetProtocols(IEnumerable<(CommsProtocol Protocol, ProtocolCapabilityAttribute Attribute)> protocols, Functionality functionality, Func<ProtocolCapabilityAttribute, bool> selector)
        {
            var emptyList = new List<CommsProtocol> { CommsProtocol.None };

            var required = _protocolConfiguration.RequiredFunctionality?.Any(a => a.Type == functionality) ?? false;

            var availableProtocols = protocols
                                                        .Where(w => w.Attribute != null && selector(w.Attribute))
                                                        .Select(s => s.Protocol)
                                                        .ToList();

            return required ? availableProtocols : emptyList.Union(availableProtocols).ToList();
        }

        protected override void SaveChanges()
        {
            foreach (var protocolConfiguration in _multiProtocolConfiguration)
            {
                protocolConfiguration.IsValidationHandled = protocolConfiguration.Protocol == ValidationProtocol;
                protocolConfiguration.IsFundTransferHandled = protocolConfiguration.Protocol == FundTransferProtocol;
                protocolConfiguration.IsProgressiveHandled = protocolConfiguration.Protocol == ProgressiveProtocol;
                protocolConfiguration.IsCentralDeterminationHandled = protocolConfiguration.Protocol == CentralDeterminationSystemProtocol;
            }

            _multiProtocolConfigurationProvider.MultiProtocolConfiguration = _multiProtocolConfiguration;
        }

        private static bool IsProtocolCollectionEmpty(IReadOnlyCollection<CommsProtocol> protocolCollection)
        {
            if (protocolCollection == null || !protocolCollection.Any()) return true;
            return protocolCollection.Count == MinProtocols && protocolCollection.First() == CommsProtocol.None;
        }
    }
}