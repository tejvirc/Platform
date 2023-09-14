namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Application.Protocol;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Monaco.Localization.Properties;
    using Mono.Addins;

    [CLSCompliant(false)]
    public class ProtocolSelection : ObservableObject
    {
        private const string ProtocolExtensionPath = "/Protocol/Runnables";

        private bool _selected;
        private bool _enabled;
        private string _protocolName;
        private string _protocolCapabilitiesList;
        private ProtocolCapabilityAttribute _protocolCapabilities;

        public void Initialize(string protocolName)
        {
            ProtocolName = protocolName;
            Selected = false;
            Enabled = true;
        }

        public string ProtocolName
        {
            get => _protocolName;
            set
            {
                if (value == _protocolName)
                {
                    return;
                }

                SetProperty(ref _protocolName, value, nameof(ProtocolName));
            }
        }

        public bool Selected
        {
            get => _selected;
            set
            {
                if (value == _selected)
                {
                    return;
                }

                SetProperty(ref _selected, value, nameof(Selected));
            }
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (value == _enabled)
                {
                    return;
                }

                SetProperty(ref _enabled, value, nameof(Enabled));
            }
        }

        public string ProtocolCapabilitiesList
        {
            get
            {
                if (_protocolCapabilitiesList == null)
                {
                    DiscoverCapabilities();
                }

                return _protocolCapabilitiesList;
            }
            set
            {
                if (value == _protocolCapabilitiesList)
                {
                    return;
                }

                SetProperty(ref _protocolCapabilitiesList, value, nameof(ProtocolCapabilitiesList));
            }
        }

        public ProtocolCapabilityAttribute ProtocolCapabilities
        {
            get
            {
                if (_protocolCapabilities == null)
                {
                    DiscoverCapabilities();
                }

                return _protocolCapabilities;
            }
            set
            {
                if (value == _protocolCapabilities)
                {
                    return;
                }

                SetProperty(ref _protocolCapabilities, value, nameof(ProtocolCapabilities));
            }
        }

        private void DiscoverCapabilities()
        {
            var protocolNode = AddinManager.GetExtensionNodes<ProtocolTypeExtensionNode>(ProtocolExtensionPath)
                .SingleOrDefault(x => x.ProtocolId == ProtocolName);

            if (protocolNode == null)
            {
                return;
            }

            _protocolCapabilities = (ProtocolCapabilityAttribute)Attribute.GetCustomAttribute(
                protocolNode.Type,
                typeof(ProtocolCapabilityAttribute));

            var protocolCapabilityList = new List<string>();

            if (_protocolCapabilities != null)
            {
                if (_protocolCapabilities.IsValidationSupported)
                {
                    protocolCapabilityList.Add(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Validation));
                }

                if (_protocolCapabilities.IsFundTransferSupported)
                {
                    protocolCapabilityList.Add(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.FundTransfer));
                }

                if (_protocolCapabilities.IsProgressivesSupported)
                {
                    protocolCapabilityList.Add(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Progressive));
                }

                if (_protocolCapabilities.IsCentralDeterminationSystemSupported)
                {
                    protocolCapabilityList.Add(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CentralDeterminationSystem));
                }
            }

            ProtocolCapabilitiesList = protocolCapabilityList.Count == 0
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.None)
                : string.Join(", ", protocolCapabilityList);
        }
    }
}
