namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using ConfigWizard;
    using Contracts;
    using Contracts.Localization;
    using Contracts.Protocol;
    using Events;
    using Kernel;
    using Models;
    using Monaco.Common;
    using Monaco.Localization.Properties;
    using Mono.Addins;
    using Newtonsoft.Json;
    using Protocol;
    using GlobalProtocol = global::Protocol;

    [CLSCompliant(false)]
    public class ProtocolSetupPageViewModel : ConfigWizardViewModelBase
    {
        private const string ProtocolExtensionPath = "/Protocol/Runnables";
        private readonly List<FunctionalityType> _requiredFunctionality;
        private readonly IMultiProtocolConfigurationProvider _protocolConfigurationProvider;

        private bool _warningMessageVisible;
        private string _popupProtocol;
        private string _popupProtocolInfo;

        public ProtocolSetupPageViewModel(IMultiProtocolConfigurationProvider protocolConfigurationProvider, IConfigurationUtilitiesProvider configurationUtilitiesProvider) : base(true)
        {
            _protocolConfigurationProvider = protocolConfigurationProvider ?? throw new ArgumentNullException(nameof(protocolConfigurationProvider));
            if (configurationUtilitiesProvider == null) throw new ArgumentNullException(nameof(configurationUtilitiesProvider));

            var protocolsConfiguration = configurationUtilitiesProvider.GetConfigWizardConfiguration(
                () => new ConfigWizardConfiguration
                {
                    ProtocolConfiguration = new ConfigWizardConfigurationProtocolConfiguration
                    {
                        ProtocolsAllowed = ProtocolNames.All.Select(s => new GlobalProtocol { Name = EnumParser.ParseOrThrow<CommsProtocol>(s) }).ToArray(),
                        ExclusiveProtocols = Array.Empty<ExclusiveProtocol>(),
                        RequiredFunctionality = Array.Empty<FunctionalityType>()
                    }
                });

            ProtocolSelections = new List<ProtocolSelection>();

            if (PropertiesManager.GetValue(ApplicationConstants.ShowMode, false))
            {
                var protocolsToMerge = protocolsConfiguration.ProtocolConfiguration.ProtocolsAllowed;
                protocolsConfiguration.ProtocolConfiguration.ProtocolsAllowed =
                    new GlobalProtocol[protocolsToMerge.Length + 1];
                protocolsConfiguration.ProtocolConfiguration.ProtocolsAllowed[0] =
                    new GlobalProtocol { Name = CommsProtocol.DemonstrationMode, IsMandatory = true };
                Array.Copy(protocolsToMerge, 0, protocolsConfiguration.ProtocolConfiguration.ProtocolsAllowed, 1, protocolsToMerge.Length);

                var demo = new ProtocolSelection()
                {
                    Enabled = true,
                    ProtocolName = CommsProtocol.DemonstrationMode.ToString(),
                    Selected = true
                };

                demo.PropertyChanged += ProtocolSelection_PropertyChanged;
                ProtocolSelections.Add(demo);
            }

            _requiredFunctionality = protocolsConfiguration.ProtocolConfiguration.RequiredFunctionality?.ToList() ?? new List<FunctionalityType>();

            var protocolsAllowed = protocolsConfiguration.ProtocolConfiguration.ProtocolsAllowed ?? Array.Empty<GlobalProtocol>();
            var requiredProtocols = protocolsAllowed
                .Where(s => s.IsMandatory)
                .Select(s => s.Name)
                .Distinct()
                .ToList();

            var savedProtocols = _protocolConfigurationProvider.MultiProtocolConfiguration.ToList();

            var protocols = MonoAddinsHelper.GetSelectableConfigurationAddins(ApplicationConstants.Protocol);
            foreach (var protocolName in protocols)
            {
                if (protocolsAllowed.All(x => Enum.GetName(typeof(CommsProtocol), x.Name) != protocolName))
                {
                    continue;
                }

                var protocolSelection = new ProtocolSelection();
                protocolSelection.Initialize(protocolName);
                var protocolType = EnumParser.ParseOrThrow<CommsProtocol>(protocolName);
                if (requiredProtocols.Contains(protocolType) || protocolType == protocolsAllowed.FirstOrDefault(x => x.IsMandatory)?.Name)
                {
                    protocolSelection.Selected = true;
                    if (protocolName != null)
                    {
                        protocolSelection.Enabled = false;
                    }

                    //For retail build, make the mandatory protocol un-selectable.
#if (RETAIL)
                    protocolSelection.Enabled = false;
#endif
                }
                else
                {
                    protocolSelection.Selected = savedProtocols.Any(p => p.Protocol == EnumParser.ParseOrThrow<CommsProtocol>(protocolName));
                }

                protocolSelection.PropertyChanged += ProtocolSelection_PropertyChanged;
                if (!protocolSelection.Enabled)
                {
                    // mandatory one goes first
                    ProtocolSelections.Insert(0, protocolSelection);
                }
                else
                {
                    ProtocolSelections.Add(protocolSelection);
                }
            }
        }

        protected override void Loaded()
        {
            DisableExclusiveProtocols();
            CheckRequiredFunctionalityProtocolSelected();
            ServiceManager.GetInstance().GetService<IEventBus>().Subscribe<OperatorMenuPopupEvent>(this, OnShowPopup);
        }

        private void OnShowPopup(OperatorMenuPopupEvent evt)
        {
            PopupProtocol = evt.PopupText;
        }

        public string PopupProtocol
        {
            get => _popupProtocol;
            set
            {
                if (value == _popupProtocol)
                {
                    return;
                }

                var protocolCapabilityAttribute = GetProtocolCapabilityAttribute(value);
                var protocolCapabilityList = new List<string>();

                if (protocolCapabilityAttribute != null)
                {
                    if (protocolCapabilityAttribute.IsValidationSupported)
                    {
                        protocolCapabilityList.Add(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Validation));
                    }

                    if (protocolCapabilityAttribute.IsFundTransferSupported)
                    {
                        protocolCapabilityList.Add(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.FundTransfer));
                    }

                    if (protocolCapabilityAttribute.IsProgressivesSupported)
                    {
                        protocolCapabilityList.Add(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Progressive));
                    }

                    if (protocolCapabilityAttribute.IsCentralDeterminationSystemSupported)
                    {
                        protocolCapabilityList.Add(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CentralDeterminationSystem));
                    }
                }

                PopupProtocolInfo = protocolCapabilityList.Count == 0
                    ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.None)
                    : string.Join(", ", protocolCapabilityList);

                SetProperty(ref _popupProtocol, value, nameof(PopupProtocol));
            }
        }

        public string PopupProtocolInfo
        {
            get => _popupProtocolInfo;
            set
            {
                if (value == _popupProtocolInfo)
                {
                    return;
                }

                SetProperty(ref _popupProtocolInfo, value, nameof(PopupProtocolInfo));
            }
        }

        public bool IsDisplayRequiredFunctionalityProtocolSelectionMessage => CanDisplayMandatoryProtocolSelectionMessage();

        public string RequiredFunctionalityProtocolSelectionMessage => GetMandatoryProtocolSelectionMessage();

        private ProtocolCapabilityAttribute GetProtocolCapabilityAttribute(string protocolName)
        {
            var protocolNode = AddinManager.GetExtensionNodes<ProtocolTypeExtensionNode>(ProtocolExtensionPath)
                .SingleOrDefault(x => x.ProtocolId == protocolName);

            if (protocolNode == null)
            {
                return null;
            }

            var protocolCapabilityAttribute = (ProtocolCapabilityAttribute)Attribute.GetCustomAttribute(
                protocolNode.Type,
                typeof(ProtocolCapabilityAttribute));

            return protocolCapabilityAttribute;
        }

        private void ProtocolSelection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ProtocolSelection.Selected))
            {
                return;
            }

            SetupNavigation();

            DisableExclusiveProtocols();

            CheckRequiredFunctionalityProtocolSelected();
        }

        private void DisableExclusiveProtocols()
        {
            // Only enable unselected protocols to avoid enabling a mandatory protocol
            // For retail build, make the mandatory protocol un-selectable.
#if (RETAIL)
            foreach (var protocol in ProtocolSelections.Where(x => !x.Selected))
#else
            foreach (var protocol in ProtocolSelections)
#endif
            {
                if (protocol.Selected && !protocol.Enabled)
                {
                    continue;
                }

                protocol.Enabled = true;
            }

            WarningMessageVisible = false;
        }

        public List<ProtocolSelection> ProtocolSelections { get; }

        public bool WarningMessageVisible
        {
            get => _warningMessageVisible;
            set
            {
                if (value == _warningMessageVisible)
                {
                    return;
                }

                SetProperty(ref _warningMessageVisible, value, nameof(WarningMessageVisible));
            }
        }

        protected override void SaveChanges()
        {
            var selectedProtocols = ProtocolSelections.Where(p => p.Selected).Select(p => p.ProtocolName);
            SetAddinConfigProperty(ApplicationConstants.Protocol, JsonConvert.SerializeObject(selectedProtocols, Formatting.None));

            var multiProtocolConfiguration = ProtocolSelections
                .Where(x => x.Selected)
                .Select(s => EnumParser.Parse<CommsProtocol>(s.ProtocolName))
                .Where(w => w.IsValid && w.Result.HasValue)
                .Select(x => new ProtocolConfiguration(x.Result.Value))
                .ToList();

            if (multiProtocolConfiguration.Count == 1)
            {
                var protocolNode = AddinManager.GetExtensionNodes<ProtocolTypeExtensionNode>(ProtocolExtensionPath)
                    .SingleOrDefault(x => x.ProtocolId == EnumParser.ToName(multiProtocolConfiguration.First().Protocol));

                if (protocolNode != null)
                {
                    var protocolCapabilityAttribute = (ProtocolCapabilityAttribute)(Attribute.GetCustomAttribute(
                        protocolNode.Type,
                        typeof(ProtocolCapabilityAttribute)));

                    multiProtocolConfiguration[0].IsValidationHandled = protocolCapabilityAttribute.IsValidationSupported;
                    multiProtocolConfiguration[0].IsFundTransferHandled = protocolCapabilityAttribute.IsFundTransferSupported;
                    multiProtocolConfiguration[0].IsProgressiveHandled = protocolCapabilityAttribute.IsProgressivesSupported;
                    multiProtocolConfiguration[0].IsCentralDeterminationHandled = protocolCapabilityAttribute.IsCentralDeterminationSystemSupported;
                }
            }

            _protocolConfigurationProvider.IsValidationRequired = _requiredFunctionality.Any(r => r.Type == Functionality.Validation);
            _protocolConfigurationProvider.IsFundsTransferRequired = _requiredFunctionality.Any(r => r.Type == Functionality.FundsTransfer);
            _protocolConfigurationProvider.IsProgressiveRequired = _requiredFunctionality.Any(r => r.Type == Functionality.Progressive);
            _protocolConfigurationProvider.IsCentralDeterminationSystemRequired = _requiredFunctionality.Any(r => r.Type == Functionality.CentralDeterminationSystem);

            _protocolConfigurationProvider.MultiProtocolConfiguration = multiProtocolConfiguration;
        }

        protected override void SetupNavigation()
        {
            WizardNavigator.CanNavigateBackward = false;
            WizardNavigator.CanNavigateForward = ProtocolSelections.Exists(x => x.Selected) && IsRequiredFunctionalityProtocolSelected();
        }

        protected override void LoadAutoConfiguration()
        {
            var protocol = string.Empty;
            AutoConfigurator.GetValue(ApplicationConstants.Protocol, ref protocol);
            foreach (var p in ProtocolSelections.Where(p => p.ProtocolName == protocol))
            {
                p.Selected = true;
            }

            base.LoadAutoConfiguration();
        }

        private bool IsRequiredFunctionalityProtocolSelected()
        {

            if (!_requiredFunctionality.Any())
            {
                return true;
            }

            foreach (var functionality in _requiredFunctionality)
            {
                switch (functionality.Type)
                {
                    case Functionality.Validation when !ProtocolSelections.Any(p => p.Selected && (GetProtocolCapabilityAttribute(p.ProtocolName)?.IsValidationSupported ?? false)):
                    case Functionality.FundsTransfer when !ProtocolSelections.Any(p => p.Selected && (GetProtocolCapabilityAttribute(p.ProtocolName)?.IsFundTransferSupported ?? false)):
                    case Functionality.Progressive when !ProtocolSelections.Any(p => p.Selected && (GetProtocolCapabilityAttribute(p.ProtocolName)?.IsProgressivesSupported ?? false)):
                    case Functionality.CentralDeterminationSystem when !ProtocolSelections.Any(p => p.Selected && (GetProtocolCapabilityAttribute(p.ProtocolName)?.IsCentralDeterminationSystemSupported ?? false)):
                        return false;
                }
            }

            return true;
        }

        private bool CanDisplayMandatoryProtocolSelectionMessage()
        {
            return (_requiredFunctionality?.Any() ?? false)
                   && ProtocolSelections.Any(p => p.Selected)
                   && !IsRequiredFunctionalityProtocolSelected();
        }

        private string GetMandatoryProtocolSelectionMessage()
        {
            if (!_requiredFunctionality?.Any() ?? true)
            {
                return string.Empty;
            }

            var missingFunctionality = new List<string>();
            foreach (var functionality in _requiredFunctionality)
            {
                switch (functionality.Type)
                {
                    case Functionality.Validation when !ProtocolSelections.Any(p => p.Selected && GetProtocolCapabilityAttribute(p.ProtocolName).IsValidationSupported):
                        missingFunctionality.Add(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Validation));
                        break;
                    case Functionality.FundsTransfer when !ProtocolSelections.Any(p => p.Selected && GetProtocolCapabilityAttribute(p.ProtocolName).IsFundTransferSupported):
                        missingFunctionality.Add(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.FundTransfer));
                        break;
                    case Functionality.Progressive when !ProtocolSelections.Any(p => p.Selected && GetProtocolCapabilityAttribute(p.ProtocolName).IsProgressivesSupported):
                        missingFunctionality.Add(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Progressive));
                        break;
                    case Functionality.CentralDeterminationSystem when !ProtocolSelections.Any(p => p.Selected && GetProtocolCapabilityAttribute(p.ProtocolName).IsCentralDeterminationSystemSupported):
                        missingFunctionality.Add(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CentralDeterminationSystem));
                        break;
                }
            }

            return !missingFunctionality.Any()
                ? string.Empty
                : string.Format(
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.RequiredFunctionalityProtocolSelectionMessage),
                    string.Join(", ", missingFunctionality));
        }

        private void CheckRequiredFunctionalityProtocolSelected()
        {
            RaisePropertyChanged(nameof(IsDisplayRequiredFunctionalityProtocolSelectionMessage));
            RaisePropertyChanged(nameof(RequiredFunctionalityProtocolSelectionMessage));
        }
    }
}
