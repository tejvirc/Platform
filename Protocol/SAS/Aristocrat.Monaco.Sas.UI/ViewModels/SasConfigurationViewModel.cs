namespace Aristocrat.Monaco.Sas.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.Contracts.Protocol;
    using Application.UI.ConfigWizard;
    using Contracts;
    using Contracts.Events;
    using Contracts.SASProperties;
    using Kernel;
    using Localization.Properties;
    using Storage.Models;

    /// <summary>
    /// A view model through which the settings for the SAS settings can be configured.
    /// </summary>
    public sealed class SasConfigurationViewModel : ConfigWizardViewModelBase
    {
        private const sbyte DefaultAddress = 1;
        private const sbyte MaxAddress = 127;
        private const int MaxHostCount = 2;

        // TODO: get the comm port numbers from a cabinet property provider
        private const int Host1ComPort = 5;
        private const int Host2ComPort = 11;

        private sbyte _communicationAddress1 = DefaultAddress;
        private string _communicationAddress1ErrorText = string.Empty;
        private int _communicationAddress1MaxLength = 3;

        private sbyte _communicationAddress2 = DefaultAddress;
        private string _communicationAddress2ErrorText = string.Empty;
        private int _communicationAddress2MaxLength = 3;

        private bool _host1AftEnabled = true;
        private bool _host1LegacyBonusEnabled = true;
        private bool _host1ValidationEnabled;
        private bool _host1ProgressiveEnabled;
        private bool _host1GeneralControlEnabled = true;
        private bool _host1GameStartEndEnabled = true;
        private bool _host1NonSasProgressiveHitReporting;

        private bool _host2AftEnabled;
        private bool _host2LegacyBonusEnabled;
        private bool _host2ValidationEnabled;
        private bool _host2ProgressiveEnabled;
        private bool _host2GeneralControlEnabled;
        private bool _host2GameStartEndEnabled;
        private bool _host2NonSasProgressiveHitReporting;

        private readonly bool _gameStartEndHostAutoSelection;

        private bool _dualHostSetup;
        private decimal _accountingDenom1 = 0.01M;
        private decimal _accountingDenom2 = 0.01M;
        private int _progressiveGroupId = 1;
        private string _progressiveGroupIdErrorText = string.Empty;
        private int _progressiveGroupIdMaxLength = 10;

        private readonly bool _isWizardPage;
        private int _accountingDenom1Index;
        private int _accountingDenom2Index;

        private ProtocolConfiguration _sasProtocolConfiguration;

        /// <summary>
        ///     ctor
        /// </summary>
        public SasConfigurationViewModel(bool isWizardPage) : base(isWizardPage)
        {
            _isWizardPage = isWizardPage;
            var multiProtocolConfigurationProvider = ServiceManager.GetInstance().GetService<IMultiProtocolConfigurationProvider>();
            SasProtocolConfiguration = multiProtocolConfigurationProvider.MultiProtocolConfiguration
                .FirstOrDefault(x => x.Protocol == CommsProtocol.SAS);
            if (_isWizardPage)
            {
                _gameStartEndHostAutoSelection = true;
                var portConfiguration = PropertiesManager.GetValue(
                    SasProperties.SasPortAssignments,
                    new PortAssignment());
                var sasSettings = PropertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures());
                DualHostSetup = portConfiguration.IsDualHost;
                GeneralControlEditable = sasSettings.GeneralControlEditable;
                ValidationOnHost1 = portConfiguration.ValidationPort == HostId.Host1;
                ValidationOnHost2 = portConfiguration.ValidationPort == HostId.Host2;
                AftOnHost1 = portConfiguration.AftPort == HostId.Host1;
                AftOnHost2 = portConfiguration.AftPort == HostId.Host2;

                var host1 = new List<GameStartEndHost> { GameStartEndHost.Host1, GameStartEndHost.Both };
                var host2 = new List<GameStartEndHost> { GameStartEndHost.Host2, GameStartEndHost.Both };
                var gameStartEndHost = portConfiguration.GameStartEndHosts;
                GameStartEndOnHost1 = host1.Any(h => h == gameStartEndHost);
                GameStartEndOnHost2 = host2.Any(h => h == gameStartEndHost);
                _gameStartEndHostAutoSelection = GameStartEndOnHost1 || GameStartEndOnHost2;

                NonSasProgressiveHitReportingHost1 = portConfiguration.Host1NonSasProgressiveHitReporting;
                NonSasProgressiveHitReportingHost2 = portConfiguration.Host2NonSasProgressiveHitReporting;
                CheckNavigation();

                if (PropertiesManager.GetValue(
                    ApplicationConstants.MachineSettingsImported,
                    ImportMachineSettings.None) == ImportMachineSettings.None)
                {
                    SaveChanges();
                }
            }
        }

        /// <summary>
        ///     Gets and sets the SAS Protocol Configuration
        /// </summary>
        public ProtocolConfiguration SasProtocolConfiguration
        {
            get => _sasProtocolConfiguration;

            private set
            {
                if (value == null)
                {
                    _sasProtocolConfiguration = new ProtocolConfiguration(CommsProtocol.SAS);
                }

                _sasProtocolConfiguration = value;
            }
        }

        /// <summary>
        ///     Dual Host Setup on or off
        /// </summary>
        public bool DualHostSetup
        {
            get => _dualHostSetup;
            set
            {
                _dualHostSetup = value;

                // make sure we always have one host handling game start/end exceptions
                if (!_dualHostSetup && _gameStartEndHostAutoSelection)
                {
                    GameStartEndOnHost1 = true;
                    GameStartEndOnHost2 = false;
                }

                RaisePropertyChanged(nameof(DualHostSetup));
                CheckNavigation();
            }
        }

        /// <summary>
        ///     Gets or sets the value indicating if AFT on host1 is enabled.
        /// </summary>
        public bool AftOnHost1
        {
            get => _host1AftEnabled;
            set
            {
                _host1AftEnabled = value;
                RaisePropertyChanged(nameof(AftOnHost1));
            }
        }

        /// <summary>
        ///     Gets or sets the value indicating if AFT on host2 is enabled.
        /// </summary>
        public bool AftOnHost2
        {
            get => _host2AftEnabled;
            set
            {
                _host2AftEnabled = value;
                RaisePropertyChanged(nameof(AftOnHost2));
            }
        }

        /// <summary>
        ///     Gets or sets the value indicating if LegacyBonus on host1 is enabled.
        /// </summary>
        public bool LegacyBonusOnHost1
        {
            get => _host1LegacyBonusEnabled;
            set
            {
                _host1LegacyBonusEnabled = value;
                RaisePropertyChanged(nameof(LegacyBonusOnHost1));
            }
        }

        /// <summary>
        ///     Gets or sets the value indicating if LegacyBonus on host2 is enabled.
        /// </summary>
        public bool LegacyBonusOnHost2
        {
            get => _host2LegacyBonusEnabled;
            set
            {
                _host2LegacyBonusEnabled = value;
                RaisePropertyChanged(nameof(LegacyBonusOnHost2));
            }
        }

        /// <summary>
        ///     Gets or sets the value indicating if Validation on host1 is enabled.
        /// </summary>
        public bool ValidationOnHost1
        {
            get => _host1ValidationEnabled;
            set
            {
                _host1ValidationEnabled = value;
                RaisePropertyChanged(nameof(ValidationOnHost1));
            }
        }

        /// <summary>
        ///     Gets or sets the value indicating if Validation on host2 is enabled.
        /// </summary>
        public bool ValidationOnHost2
        {
            get => _host2ValidationEnabled;
            set
            {
                _host2ValidationEnabled = value;
                RaisePropertyChanged(nameof(ValidationOnHost2));
            }
        }

        /// <summary>
        ///     Gets or sets the value indicating if Progressive on host1 is enabled.
        /// </summary>
        public bool ProgressiveOnHost1
        {
            get => _host1ProgressiveEnabled;
            set
            {
                _host1ProgressiveEnabled = value;
                RaisePropertyChanged(nameof(ProgressiveOnHost1));
            }
        }

        /// <summary>
        ///     Gets or sets the value indicating if Progressive on host2 is enabled.
        /// </summary>
        public bool ProgressiveOnHost2
        {
            get => _host2ProgressiveEnabled;
            set
            {
                _host2ProgressiveEnabled = value;
                RaisePropertyChanged(nameof(ProgressiveOnHost2));
            }
        }

        /// <summary>
        ///     Gets or sets the value indicating if GeneralControl on host1 is enabled.
        /// </summary>
        public bool GeneralControlOnHost1
        {
            get => _host1GeneralControlEnabled;
            set
            {
                _host1GeneralControlEnabled = value;
                RaisePropertyChanged(nameof(GeneralControlOnHost1));
            }
        }

        /// <summary>
        ///     Gets or sets the value indicating if GeneralControl on host2 is enabled.
        /// </summary>
        public bool GeneralControlOnHost2
        {
            get => _host2GeneralControlEnabled;
            set
            {
                _host2GeneralControlEnabled = value;
                RaisePropertyChanged(nameof(GeneralControlOnHost2));
            }
        }

        /// <summary>
        ///     Gets or sets the value indicating if Game Start/End on host1 is enabled.
        /// </summary>
        public bool GameStartEndOnHost1
        {
            get => _host1GameStartEndEnabled;
            set
            {
                _host1GameStartEndEnabled = value;
                RaisePropertyChanged(nameof(GameStartEndOnHost1));
            }
        }

        /// <summary>
        ///     Gets or sets the value indicating if Game Start/End on host2 is enabled.
        /// </summary>
        public bool GameStartEndOnHost2
        {
            get => _host2GameStartEndEnabled;
            set
            {
                _host2GameStartEndEnabled = value;
                RaisePropertyChanged(nameof(GameStartEndOnHost2));
            }
        }

        /// <summary>
        ///     Gets or sets the value indicating if Non Sas Progressive Hit Reporting on host1 is enabled.
        /// </summary>
        public bool NonSasProgressiveHitReportingHost1
        {
            get => _host1NonSasProgressiveHitReporting;
            set
            {
                _host1NonSasProgressiveHitReporting = value;
                RaisePropertyChanged(nameof(NonSasProgressiveHitReportingHost1));
            }
        }

        /// <summary>
        ///     Gets or sets the value indicating if Non Sas Progressive Hit Reporting on host2 is enabled.
        /// </summary>
        public bool NonSasProgressiveHitReportingHost2
        {
            get => _host2NonSasProgressiveHitReporting;
            set
            {
                _host2NonSasProgressiveHitReporting = value;
                RaisePropertyChanged(nameof(NonSasProgressiveHitReportingHost2));
            }
        }

        /// <summary>
        ///     Gets the value indicating if Address of Host1 is editable
        /// </summary>
        public bool AddressHost1Editable { get; private set; } = true;

        /// <summary>
        ///     Gets the value indicating if Address of Host2 is editable
        /// </summary>
        public bool AddressHost2Editable { get; private set; } = true;

        /// <summary>
        ///     Gets the value indicating if GeneralControl is editable or not.
        /// </summary>
        public bool GeneralControlEditable { get; }

        /// <summary>
        ///     Gets or sets Communication Address for the first host
        /// </summary>
        public sbyte CommunicationAddress1
        {
            get => _communicationAddress1;
            set => SetProperty(ref _communicationAddress1, value, nameof(CommunicationAddress1));
        }

        /// <summary>
        ///     Gets or sets Communication Address Error Text for the first host
        /// </summary>
        public string CommunicationAddress1ErrorText
        {
            get => _communicationAddress1ErrorText;
            set
            {
                if (SetProperty(ref _communicationAddress1ErrorText, value, nameof(CommunicationAddress1ErrorText)))
                {
                    SetError(nameof(CommunicationAddress1), _communicationAddress1ErrorText);
                }
            }
        }

        /// <summary>
        ///     Gets or sets Communication Address Max Length for the first host
        /// </summary>
        public int CommunicationAddress1MaxLength
        {
            get => _communicationAddress1MaxLength;
            set => SetProperty(ref _communicationAddress1MaxLength, value, nameof(CommunicationAddress1MaxLength));
        }

        /// <summary>
        ///     Gets or sets Communication Address for the second host
        /// </summary>
        public sbyte CommunicationAddress2
        {
            get => _communicationAddress2;
            set => SetProperty(ref _communicationAddress2, value, nameof(CommunicationAddress2));
        }

        /// <summary>
        ///     Gets or sets Communication Address Error Text for the second host
        /// </summary>
        public string CommunicationAddress2ErrorText
        {
            get => _communicationAddress2ErrorText;
            set
            {
                if (SetProperty(ref _communicationAddress2ErrorText, value, nameof(CommunicationAddress2ErrorText)))
                {
                    SetError(nameof(CommunicationAddress2), _communicationAddress2ErrorText);
                }
            }
        }

        /// <summary>
        ///     Gets or sets Communication Address Max Length for the second host
        /// </summary>
        public int CommunicationAddress2MaxLength
        {
            get => _communicationAddress2MaxLength;
            set => SetProperty(ref _communicationAddress2MaxLength, value, nameof(CommunicationAddress2MaxLength));
        }

        /// <summary>
        ///     List of accounting denoms to choose from
        /// </summary>
        public IList<decimal> AccountingDenoms => new List<decimal>
        {
            1L.CentsToDollars(),
            25L.CentsToDollars(),
            100L.CentsToDollars(),
            500L.CentsToDollars(),
            1000L.CentsToDollars(),
            2000L.CentsToDollars(),
            5000L.CentsToDollars(),
            10000L.CentsToDollars()
        };

        /// <summary>
        ///     Host 1 accounting denomination
        /// </summary>
        public decimal AccountingDenom1
        {
            get => _accountingDenom1;
            set
            {
                _accountingDenom1 = value;
                RaisePropertyChanged(nameof(AccountingDenom1));
            }
        }

        /// <summary>
        ///     Host 1 accounting denomination index
        /// </summary>
        public int AccountingDenom1Index
        {
            get => _accountingDenom1Index;
            set
            {
                _accountingDenom1Index = value;
                RaisePropertyChanged(nameof(AccountingDenom1Index));
            }
        }

        /// <summary>
        ///     Host 2 accounting denomination
        /// </summary>
        public decimal AccountingDenom2
        {
            get => _accountingDenom2;
            set
            {
                _accountingDenom2 = value;
                RaisePropertyChanged(nameof(AccountingDenom2));
            }
        }

        /// <summary>
        ///     Host 2 accounting denomination index
        /// </summary>
        public int AccountingDenom2Index
        {
            get => _accountingDenom2Index;
            set
            {
                _accountingDenom2Index = value;
                RaisePropertyChanged(nameof(AccountingDenom2Index));
            }
        }

        /// <summary>
        ///     Gets or sets Progressive Group Id
        /// </summary>
        public int ProgressiveGroupId
        {
            get => _progressiveGroupId;
            set => SetProperty(ref _progressiveGroupId, value, nameof(ProgressiveGroupId));
        }

        /// <summary>
        ///     Gets or sets Progressive Group Id Error Text
        /// </summary>
        public string ProgressiveGroupIdErrorText
        {
            get => _progressiveGroupIdErrorText;
            set
            {
                if (SetProperty(ref _progressiveGroupIdErrorText, value, nameof(ProgressiveGroupIdErrorText)))
                {
                    SetError(nameof(ProgressiveGroupId), _progressiveGroupIdErrorText);
                }
            }
        }

        /// <summary>
        ///     Gets or sets Progressive Group Id Max Length
        /// </summary>
        public int ProgressiveGroupIdMaxLength
        {
            get => _progressiveGroupIdMaxLength;
            set => SetProperty(ref _progressiveGroupIdMaxLength, value, nameof(ProgressiveGroupIdMaxLength));
        }

        /// <inheritdoc />
        protected override void Loaded()
        {
            base.Loaded();
            GetHostConfigurations();
        }

        /// <inheritdoc />
        protected override void SaveChanges()
        {
            OnCommitted();
        }

        /// <summary>
        ///     Occurs each time page is loaded
        /// </summary>
        protected override void InitializeData()
        {
            Committed = false;
        }

        /// <summary>
        ///     Sets ths properties in the property manager
        /// </summary>
        protected override void OnCommitted()
        {
            if (Committed || HasErrors || !CanNavigate())
            {
                return;
            }

            Commit();
        }

        /// <inheritdoc />
        protected override void LoadAutoConfiguration()
        {
            const byte host1Id = 1;
            const byte host2Id = 2;
            bool ValidHost(byte hostId)
            {
                return hostId == 0 || hostId == host1Id || hostId == host2Id;
            }

            var boolValue = false;
            var stringValue = string.Empty;
            var autoConfigured = true;

            if (AutoConfigurator.GetValue("SasDualHost", ref boolValue))
            {
                DualHostSetup = boolValue;
            }

            if (AutoConfigurator.GetValue("SasAddressHost1", ref stringValue) && AddressHost1Editable)
            {
                autoConfigured &= sbyte.TryParse(stringValue, out var address) && (address == 0 || !string.IsNullOrEmpty(CheckError(address, MaxAddress)));
                if (autoConfigured)
                {
                    CommunicationAddress1 = address;
                }
            }

            if (AutoConfigurator.GetValue("SasAddressHost2", ref stringValue) && AddressHost2Editable)
            {
                autoConfigured &= sbyte.TryParse(stringValue, out var address) && (address == 0 || !string.IsNullOrEmpty(CheckError(address, MaxAddress)));
                if (autoConfigured)
                {
                    CommunicationAddress2 = address;
                }
            }

            if (AutoConfigurator.GetValue("SasValidationHost", ref stringValue))
            {
                autoConfigured &= byte.TryParse(stringValue, out var hostId) && ValidHost(hostId);
                if (autoConfigured)
                {
                    ValidationOnHost1 = hostId == host1Id;
                    ValidationOnHost2 = hostId == host2Id;
                }
            }

            if (AutoConfigurator.GetValue("SasAftHost", ref stringValue))
            {
                autoConfigured &= byte.TryParse(stringValue, out var hostId) && ValidHost(hostId);
                if (autoConfigured)
                {
                    AftOnHost1 = hostId == host1Id;
                    AftOnHost2 = hostId == host2Id;
                }
            }

            if (AutoConfigurator.GetValue("SasLegacyBonusHost", ref stringValue))
            {
                autoConfigured &= byte.TryParse(stringValue, out var hostId) && ValidHost(hostId);
                if (autoConfigured)
                {
                    LegacyBonusOnHost1 = hostId == host1Id;
                    LegacyBonusOnHost2 = hostId == host2Id;
                }
            }

            if (AutoConfigurator.GetValue("SasGeneralControlHost", ref stringValue) && GeneralControlEditable)
            {
                autoConfigured &= byte.TryParse(stringValue, out var hostId) && ValidHost(hostId);
                if (autoConfigured)
                {
                    GeneralControlOnHost1 = hostId == host1Id;
                    GeneralControlOnHost2 = hostId == host2Id;
                }
            }

            if (AutoConfigurator.GetValue("SasProgressiveHost", ref stringValue))
            {
                autoConfigured &= byte.TryParse(stringValue, out var hostId) && ValidHost(hostId);
                if (autoConfigured)
                {
                    ProgressiveOnHost1 = hostId == host1Id;
                    ProgressiveOnHost2 = hostId == host2Id;
                }
            }

            if (AutoConfigurator.GetValue("SasGameStartEndHost", ref stringValue))
            {
                autoConfigured &= Enum.TryParse(stringValue, out GameStartEndHost host);
                if (autoConfigured)
                {
                    GameStartEndOnHost1 = host == GameStartEndHost.Host1 || host == GameStartEndHost.Both;
                    GameStartEndOnHost2 = host == GameStartEndHost.Host2 || host == GameStartEndHost.Both;
                }
            }

            if (AutoConfigurator.GetValue("Host1NonSasProgressiveHitReporting", ref stringValue))
            {
                autoConfigured &= bool.TryParse(stringValue, out var nonSasProgressiveHit);
                if (autoConfigured)
                {
                    NonSasProgressiveHitReportingHost1 = nonSasProgressiveHit;
                }
            }

            if (AutoConfigurator.GetValue("Host2NonSasProgressiveHitReporting", ref stringValue))
            {
                autoConfigured &= bool.TryParse(stringValue, out var nonSasProgressiveHit);
                if (autoConfigured)
                {
                    NonSasProgressiveHitReportingHost2 = nonSasProgressiveHit;
                }
            }

            if (autoConfigured)
            {
                base.LoadAutoConfiguration();
            }
        }

        private void Commit()
        {
            var restartProtocol = IsWizardPage;
            var aftComPort = AftOnHost1 ? Host1ComPort : (AftOnHost2 && DualHostSetup) ? Host2ComPort : 0;
            var legacyBonusComPort = LegacyBonusOnHost1 ? Host1ComPort : (LegacyBonusOnHost2 && DualHostSetup) ? Host2ComPort : 0;
            var validationComPort = ValidationOnHost1 ? Host1ComPort : (ValidationOnHost2 && DualHostSetup) ? Host2ComPort : 0;
            var progressiveComPort = ProgressiveOnHost1 ? Host1ComPort : (ProgressiveOnHost2 && DualHostSetup) ? Host2ComPort : 0;
            var generalComPort = GeneralControlOnHost1 ? Host1ComPort : (GeneralControlOnHost2 && DualHostSetup) ? Host2ComPort : 0;
            var gameStartEndHosts = GameStartEndHost.None;
            if (GameStartEndOnHost1 && GameStartEndOnHost2)
            {
                gameStartEndHosts = GameStartEndHost.Both;
            }
            else if (GameStartEndOnHost1)
            {
                gameStartEndHosts = GameStartEndHost.Host1;
            }
            else if (GameStartEndOnHost2)
            {
                gameStartEndHosts = GameStartEndHost.Host2;
            }

            var ports = new PortAssignment();

            var host1 =  new Host { ComPort = Host1ComPort };
            var host2 = new Host { ComPort = Host2ComPort };
            host1.AccountingDenom = AccountingDenom1.DollarsToCents();
            host1.SasAddress = (byte)CommunicationAddress1;
            host2.AccountingDenom = AccountingDenom2.DollarsToCents();
            host2.SasAddress = (byte)(DualHostSetup ? CommunicationAddress2 : 0);
            var hosts = new List<Host> { host1, host2 };

            restartProtocol |= PropertiesManager.UpdateProperty(
                SasProperties.SasHosts,
                hosts,
                new HostEqualityComparer());

            ports.AftPort = GetHostId(aftComPort);
            ports.ProgressivePort = GetHostId(progressiveComPort);
            ports.LegacyBonusPort = GetHostId(legacyBonusComPort);
            ports.GameStartEndHosts = gameStartEndHosts;
            ports.GeneralControlPort = GetHostId(generalComPort);
            ports.ValidationPort = GetHostId(validationComPort);
            ports.IsDualHost = DualHostSetup;
            ports.Host1NonSasProgressiveHitReporting = NonSasProgressiveHitReportingHost1;
            ports.Host2NonSasProgressiveHitReporting = NonSasProgressiveHitReportingHost2;
            restartProtocol |= PropertiesManager.UpdateProperty(
                SasProperties.SasPortAssignments,
                ports,
                new PortAssignmentEqualityComparer());

            var progHostId = ProgressiveOnHost1 ? CommunicationAddress1 : (ProgressiveOnHost2 && DualHostSetup) ? CommunicationAddress2 : 0;
            var settings = (SasFeatures)PropertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).Clone();
            settings.ProgressiveGroupId = progHostId;
            settings.NonSasProgressiveHitReporting = NonSasProgressiveHitReportingHost1 || NonSasProgressiveHitReportingHost2;
            restartProtocol |= PropertiesManager.UpdateProperty(
                SasProperties.SasFeatureSettings,
                settings,
                new SasFeaturesEqualityComparer());

            if (settings.AddressConfigurableOnlyOnce)
            {
                AddressHost1Editable = string.IsNullOrEmpty(CheckError(CommunicationAddress1, MaxAddress));
                AddressHost2Editable = string.IsNullOrEmpty(CheckError(CommunicationAddress2, MaxAddress));
                RaisePropertyChanged(nameof(AddressHost1Editable));
                RaisePropertyChanged(nameof(AddressHost2Editable));
            }

            Committed = true;

            base.OnCommitted();

            if (restartProtocol)
            {
                if (settings.ConfigNotification == ConfigNotificationTypes.Always)
                {
                    EventBus.Publish(new OperatorMenuSettingsChangedEvent());
                }

                EventBus.Publish(new RestartProtocolEvent());
            }

            HostId GetHostId(int portNumber)
            {
                switch (portNumber)
                {
                    case Host1ComPort:
                        return HostId.Host1;
                    case Host2ComPort:
                        return HostId.Host2;
                    default:
                        return HostId.None;
                }
            }
        }

        private void GetHostConfigurations()
        {
            var portAssignments = PropertiesManager.GetValue(SasProperties.SasPortAssignments, new PortAssignment());
            var hosts = PropertiesManager.GetValue(SasProperties.SasHosts, Enumerable.Empty<Host>()).ToList();
            RaisePropertyChanged(nameof(AccountingDenoms));

            AccountingDenom1Index = -1; // Set the index to an invalid one so it can be refreshed
            AccountingDenom2Index = -1; // Set the index to an invalid one so it can be refreshed
            if (hosts.Count == MaxHostCount)
            {
                var denomIndex = AccountingDenoms.IndexOf(hosts[0].AccountingDenom.CentsToDollars());
                AccountingDenom1Index = denomIndex < 0 ? 0 : denomIndex;
                denomIndex = AccountingDenoms.IndexOf(hosts[1].AccountingDenom.CentsToDollars());
                AccountingDenom2Index = denomIndex < 0 ? 0 : denomIndex;
            }
            else
            {
                AccountingDenom1Index = 0;
                AccountingDenom2Index = 0;
            }

            AccountingDenom1 = AccountingDenoms[AccountingDenom1Index];
            AccountingDenom2 = AccountingDenoms[AccountingDenom2Index];
            DualHostSetup = portAssignments.IsDualHost;

            AftOnHost1 = portAssignments.AftPort == HostId.Host1;
            AftOnHost2 = portAssignments.AftPort == HostId.Host2;
            LegacyBonusOnHost1 = portAssignments.LegacyBonusPort == HostId.Host1;
            LegacyBonusOnHost2 = portAssignments.LegacyBonusPort == HostId.Host2;
            ValidationOnHost1 = portAssignments.ValidationPort == HostId.Host1;
            ValidationOnHost2 = portAssignments.ValidationPort == HostId.Host2;
            ProgressiveOnHost1 = portAssignments.ProgressivePort == HostId.Host1;
            ProgressiveOnHost2 = portAssignments.ProgressivePort == HostId.Host2;
            GeneralControlOnHost1 = portAssignments.GeneralControlPort == HostId.Host1;
            GeneralControlOnHost2 = portAssignments.GeneralControlPort == HostId.Host2;
            GameStartEndOnHost1 = portAssignments.GameStartEndHosts == GameStartEndHost.Host1 ||
                                  portAssignments.GameStartEndHosts == GameStartEndHost.Both;
            GameStartEndOnHost2 = portAssignments.GameStartEndHosts == GameStartEndHost.Host2 ||
                                  portAssignments.GameStartEndHosts == GameStartEndHost.Both;
            NonSasProgressiveHitReportingHost1 = portAssignments.Host1NonSasProgressiveHitReporting;
            NonSasProgressiveHitReportingHost2 = portAssignments.Host2NonSasProgressiveHitReporting;

            CommunicationAddress1 = DefaultAddress;
            CommunicationAddress2 = DefaultAddress;
            if (hosts.Count == MaxHostCount)
            {
                var address1 = hosts[0].SasAddress > 0 ? hosts[0].SasAddress : ((byte)DefaultAddress);
                var address2 = hosts[1].SasAddress > 0 ? hosts[1].SasAddress : ((byte)DefaultAddress);
                // We are converting byte to sbyte, which might be problematic in normal circumstances
                // but in this case, the value that is saved is sbyte sized, so the conversion is valid.
                CommunicationAddress1 = ((sbyte)address1);
                CommunicationAddress2 = ((sbyte)address2);
            }

            ProgressiveGroupId = 0;
            var settings = PropertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures());
            ProgressiveGroupId = settings.ProgressiveGroupId;
            if (settings.AddressConfigurableOnlyOnce)
            {
                AddressHost1Editable = string.IsNullOrEmpty(CheckError(CommunicationAddress1, MaxAddress));
                AddressHost2Editable = string.IsNullOrEmpty(CheckError(CommunicationAddress2, MaxAddress));
                RaisePropertyChanged(nameof(AddressHost1Editable));
                RaisePropertyChanged(nameof(AddressHost2Editable));
            }
        }

        private string CheckError(int value, int maximum)
        {
            if (value <= 0)
            {
                return string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GreaterThanErrorMessage), 0);
            }

            if (value > maximum)
            {
                return string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LessThanOrEqualErrorMessage), maximum);
            }

            return string.Empty;
        }

        private bool CanNavigate()
        {
            var communicationAddress1 = $"{CommunicationAddress1}";
            var communicationAddress2 = $"{CommunicationAddress2}";
            var progressiveGroupId = $"{ProgressiveGroupId}";

            var canNavigate = string.IsNullOrEmpty(CommunicationAddress1ErrorText) && !string.IsNullOrEmpty(communicationAddress1) &&
                              (!DualHostSetup || string.IsNullOrEmpty(CommunicationAddress2ErrorText) && !string.IsNullOrEmpty(communicationAddress2)) &&
                              string.IsNullOrEmpty(ProgressiveGroupIdErrorText) && !string.IsNullOrEmpty(progressiveGroupId);

            return canNavigate;
        }

        private void CheckNavigation()
        {
            if (_isWizardPage)
            {
                WizardNavigator.CanNavigateForward = CanNavigate();
            }
        }

        /// <inheritdoc />
        protected override void SetError(string propertyName, string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                ClearErrors(propertyName);
            }
            else
            {
                base.SetError(propertyName, error);
            }

            CheckNavigation();
        }
    }
}
