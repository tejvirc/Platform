namespace Aristocrat.Monaco.Application.UI.Tests.ViewModels
{
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Application.Contracts.ConfigWizard;
    using Aristocrat.Monaco.Application.Contracts.Protocol;
    using Aristocrat.Monaco.Application.UI.ViewModels;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    [TestClass]
    public class MultiProtocolConfigPageViewModelTests
    {
        private dynamic _accessor;
        private MultiProtocolConfigPageViewModel _target;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IConfigWizardNavigator> _configWizardNavigator;
        private Mock<IProtocolCapabilityAttributeProvider> _protocolCapabilityAttributeProvider;
        private Mock<IMultiProtocolConfigurationProvider> _multiProtocolConfigurationProvider;
        private Mock<IConfigurationUtilitiesProvider> _configurationUtilitiesProvider;

        private List<ProtocolConfiguration> _protocols = new List<ProtocolConfiguration>
        {
            new ProtocolConfiguration(CommsProtocol.DACOM, false, false, true, false),
            new ProtocolConfiguration(CommsProtocol.SAS, true, true, true, false),
            new ProtocolConfiguration(CommsProtocol.G2S, true, false, false, true),
        };

        private Dictionary<CommsProtocol, ProtocolCapabilityAttribute> attributes = new Dictionary<CommsProtocol, ProtocolCapabilityAttribute>
        {
            { CommsProtocol.DACOM, new ProtocolCapabilityAttribute(CommsProtocol.DACOM, false, false, true, false) },
            { CommsProtocol.SAS, new ProtocolCapabilityAttribute(CommsProtocol.SAS, true, true, true, false) },
            { CommsProtocol.G2S, new ProtocolCapabilityAttribute(CommsProtocol.G2S, true, false, false, true) },
        };

        [TestInitialize]
        public void Initialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();
            MoqServiceManager.CreateInstance(MockBehavior.Default);

            _configurationUtilitiesProvider = new Mock<IConfigurationUtilitiesProvider>(MockBehavior.Strict);
            _configurationUtilitiesProvider.Setup(s => s.GetConfigWizardConfiguration(It.IsAny<Func<ConfigWizardConfiguration>>())).Returns(() => new ConfigWizardConfiguration { ProtocolConfiguration = new ConfigWizardConfigurationProtocolConfiguration() });

            _protocolCapabilityAttributeProvider = new Mock<IProtocolCapabilityAttributeProvider>(MockBehavior.Strict);
            _protocolCapabilityAttributeProvider.Setup(s => s.GetAttribute(It.Is<string>(i => i == ProtocolNames.DACOM))).Returns(attributes[CommsProtocol.DACOM]);
            _protocolCapabilityAttributeProvider.Setup(s => s.GetAttribute(It.Is<string>(i => i == ProtocolNames.SAS))).Returns(attributes[CommsProtocol.SAS]);
            _protocolCapabilityAttributeProvider.Setup(s => s.GetAttribute(It.Is<string>(i => i == ProtocolNames.G2S))).Returns(attributes[CommsProtocol.G2S]);

            _configWizardNavigator = MoqServiceManager.CreateAndAddService<IConfigWizardNavigator>(MockBehavior.Strict);
            _configWizardNavigator.SetupSet(x => x.CanNavigateBackward = It.IsAny<bool>());
            _configWizardNavigator.SetupSet(x => x.CanNavigateForward = It.IsAny<bool>()).Verifiable();

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(mock => mock.GetProperty("Mono.SelectedAddinConfigurationHashCode", It.IsAny<object>())).Returns(new Dictionary<string, string>());
            _propertiesManager.Setup(mock => mock.SetProperty(ApplicationConstants.SelectedConfigurationKey, It.IsAny<object>()));
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.ShowMode, It.IsAny<object>())).Returns(false);

            _multiProtocolConfigurationProvider = MoqServiceManager.CreateAndAddService<IMultiProtocolConfigurationProvider>(MockBehavior.Strict);
            _multiProtocolConfigurationProvider.SetupSet(x => x.MultiProtocolConfiguration = It.IsAny<List<ProtocolConfiguration>>());
            _multiProtocolConfigurationProvider.SetupSet(x => x.IsFundsTransferRequired = It.IsAny<bool>());
            _multiProtocolConfigurationProvider.SetupSet(x => x.IsValidationRequired = It.IsAny<bool>());
            _multiProtocolConfigurationProvider.SetupSet(x => x.IsCentralDeterminationSystemRequired = It.IsAny<bool>());

            _multiProtocolConfigurationProvider.SetupGet(x => x.IsCentralDeterminationSystemRequired).Returns(() => true);
            _multiProtocolConfigurationProvider.SetupGet(x => x.MultiProtocolConfiguration).Returns(() => _protocols);

            CreateTarget();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            CreateTarget();
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullDependency_MultiProtocolConfigurationProvider_ShouldThrow()
        {
            new MultiProtocolConfigPageViewModel(null, _protocolCapabilityAttributeProvider.Object, _configurationUtilitiesProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullDependency_ProtocolCapabilityAttributeProvider_ShouldThrow()
        {
            new MultiProtocolConfigPageViewModel(_multiProtocolConfigurationProvider.Object, null, _configurationUtilitiesProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullDependency_ConfigurationUtilitiesProvider_ShouldThrow()
        {
            new MultiProtocolConfigPageViewModel(_multiProtocolConfigurationProvider.Object, _protocolCapabilityAttributeProvider.Object, null);
        }

        [TestMethod]
        public void LoadedMethodLoadsProtocols_NoExclusiveProtocols_NoProtocolsSelected()
        {
            var changedProperties = new List<string>();

            var config = new ConfigWizardConfiguration
            {
                ProtocolConfiguration = new ConfigWizardConfigurationProtocolConfiguration
                {
                    ProtocolsAllowed = new global::Protocol[]
                    {
                        new global::Protocol { Name = CommsProtocol.None },
                        new global::Protocol { Name = CommsProtocol.SAS },
                        new global::Protocol { Name = CommsProtocol.G2S },
                        new global::Protocol { Name = CommsProtocol.DACOM },
                    },

                    ExclusiveProtocols = Array.Empty<ExclusiveProtocol>(),
                    RequiredFunctionality = Array.Empty<FunctionalityType>(),
                }
            };

            //Operator can select any available protocols
            _configurationUtilitiesProvider.Setup(s => s.GetConfigWizardConfiguration(It.IsAny<Func<ConfigWizardConfiguration>>())).Returns(() => config);

            _target.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);

            _accessor.Loaded();

            CollectionAssert.AreEquivalent(_target.ValidationProtocols.ToList(), new[] { CommsProtocol.None, CommsProtocol.SAS, CommsProtocol.G2S });
            CollectionAssert.AreEquivalent(_target.FundTransferProtocols.ToList(), new[] { CommsProtocol.None, CommsProtocol.SAS });
            CollectionAssert.AreEquivalent(_target.ProgressiveProtocols.ToList(), new[] { CommsProtocol.None, CommsProtocol.SAS, CommsProtocol.DACOM });
            CollectionAssert.AreEquivalent(_target.CentralDeterminationSystemProtocols.ToList(), new[] { CommsProtocol.None, CommsProtocol.G2S });

            Assert.IsTrue(_target.ValidationProtocol == CommsProtocol.SAS);
            Assert.IsTrue(_target.FundTransferProtocol == CommsProtocol.SAS);
            Assert.IsTrue(_target.ProgressiveProtocol == CommsProtocol.DACOM);
            Assert.IsTrue(_target.CentralDeterminationSystemProtocol == CommsProtocol.G2S);

            Assert.IsTrue(_target.IsValidationComboBoxEnabled);
            Assert.IsTrue(_target.IsFundTransferComboBoxEnabled);
            Assert.IsTrue(_target.IsProgressiveComboBoxEnabled);
            Assert.IsTrue(_target.IsCentralDeterminationSystemComboBoxEnabled);

            CollectionAssert.IsSubsetOf(new[] { "IsValidationProtocolsEmpty", "IsFundTransferProtocolsEmpty", "IsProgressiveProtocolsEmpty", "IsCentralDeterminationSystemsEmpty" }, changedProperties);
        }

        [TestMethod]
        public void LoadedMethodLoadsProtocols_ExclusiveProtocols_LockedToProtocolWhenProtocolSelected()
        {
            var changedProperties = new List<string>();

            //Operator can select any available protocol for each function
            _configurationUtilitiesProvider.Setup(s => s.GetConfigWizardConfiguration(It.IsAny<Func<ConfigWizardConfiguration>>())).Returns(() => new ConfigWizardConfiguration
            {
                ProtocolConfiguration = new ConfigWizardConfigurationProtocolConfiguration
                {
                    ProtocolsAllowed = new global::Protocol[]
                    {
                        new global::Protocol { Name = CommsProtocol.None },
                        new global::Protocol { Name = CommsProtocol.SAS },
                        new global::Protocol { Name = CommsProtocol.G2S },
                        new global::Protocol { Name = CommsProtocol.DACOM },
                        new global::Protocol { Name = CommsProtocol.Test },
                    },

                    ExclusiveProtocols = new ExclusiveProtocol[]
                    {
                        new ExclusiveProtocol { Name = CommsProtocol.G2S, Function = Functionality.CentralDeterminationSystem },
                        new ExclusiveProtocol { Name = CommsProtocol.SAS, Function = Functionality.FundsTransfer },
                        new ExclusiveProtocol { Name = CommsProtocol.DACOM, Function = Functionality.Progressive },
                        new ExclusiveProtocol { Name = CommsProtocol.G2S, Function = Functionality.Validation },
                    },
                    RequiredFunctionality = new FunctionalityType[0]
                }
            });

            _target.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);

            _accessor.Loaded();

            CollectionAssert.AreEquivalent(_target.ValidationProtocols.ToList(), new[] { CommsProtocol.None, CommsProtocol.SAS, CommsProtocol.G2S });
            CollectionAssert.AreEquivalent(_target.FundTransferProtocols.ToList(), new[] { CommsProtocol.None, CommsProtocol.SAS });
            CollectionAssert.AreEquivalent(_target.ProgressiveProtocols.ToList(), new[] { CommsProtocol.None, CommsProtocol.SAS, CommsProtocol.DACOM });
            CollectionAssert.AreEquivalent(_target.CentralDeterminationSystemProtocols.ToList(), new[] { CommsProtocol.None, CommsProtocol.G2S });

            Assert.IsTrue(_target.ValidationProtocol == CommsProtocol.G2S);
            Assert.IsTrue(_target.FundTransferProtocol == CommsProtocol.SAS);
            Assert.IsTrue(_target.ProgressiveProtocol == CommsProtocol.DACOM);
            Assert.IsTrue(_target.CentralDeterminationSystemProtocol == CommsProtocol.G2S);

            Assert.IsFalse(_target.IsValidationComboBoxEnabled);
            Assert.IsFalse(_target.IsFundTransferComboBoxEnabled);
            Assert.IsFalse(_target.IsProgressiveComboBoxEnabled);
            Assert.IsFalse(_target.IsCentralDeterminationSystemComboBoxEnabled);

            CollectionAssert.IsSubsetOf(new[] { "IsValidationProtocolsEmpty", "IsFundTransferProtocolsEmpty", "IsProgressiveProtocolsEmpty", "IsCentralDeterminationSystemsEmpty" }, changedProperties);
        }

        [TestMethod]
        public void SaveChangesUpdatesMultiProtocolProvider()
        {
            var changedProperties = new List<string>();

            var config = new ConfigWizardConfiguration
            {
                ProtocolConfiguration = new ConfigWizardConfigurationProtocolConfiguration
                {
                    ProtocolsAllowed = Array.Empty<global::Protocol>(),
                    ExclusiveProtocols = Array.Empty<ExclusiveProtocol>(),
                    RequiredFunctionality = Array.Empty<FunctionalityType>(),
                }
            };

            _configurationUtilitiesProvider.Setup(s => s.GetConfigWizardConfiguration(It.IsAny<Func<ConfigWizardConfiguration>>())).Returns(() => config);

            _target.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);

            _accessor.Loaded();

            _accessor.SaveChanges();

            _multiProtocolConfigurationProvider.VerifySet(v => v.MultiProtocolConfiguration = It.IsAny<IEnumerable<ProtocolConfiguration>>(), Times.Once);
        }

        private void CreateTarget()
        {
            _target = new MultiProtocolConfigPageViewModel(_multiProtocolConfigurationProvider.Object, _protocolCapabilityAttributeProvider.Object, _configurationUtilitiesProvider.Object);
            _accessor = new DynamicPrivateObject(_target);
        }
    }
}
