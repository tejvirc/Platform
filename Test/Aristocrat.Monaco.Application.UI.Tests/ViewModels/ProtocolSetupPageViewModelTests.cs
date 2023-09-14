namespace Aristocrat.Monaco.Application.UI.Tests.ViewModels
{
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Application.Contracts.ConfigWizard;
    using Aristocrat.Monaco.Application.Contracts.Protocol;
    using Aristocrat.Monaco.Application.UI.ViewModels;
    using Aristocrat.Monaco.Common;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Protocol = global::Protocol;

    [TestClass]
    public class ProtocolSetupPageViewModelTests
    {
        private ProtocolSetupPageViewModel _target;
        private dynamic _accessor;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IConfigWizardNavigator> _configWizardNavigator;
        private Mock<IMultiProtocolConfigurationProvider> _multiProtocolConfigurationProvider;
        private Mock<IConfigurationUtilitiesProvider> _configurationUtilitiesProvider;

        [TestInitialize]
        public void Initialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();
            MoqServiceManager.CreateInstance(MockBehavior.Default);

            _configWizardNavigator = MoqServiceManager.CreateAndAddService<IConfigWizardNavigator>(MockBehavior.Strict);
            _configWizardNavigator.SetupSet(x => x.CanNavigateBackward = It.IsAny<bool>());
            _configWizardNavigator.SetupSet(x => x.CanNavigateForward = It.IsAny<bool>()).Verifiable();

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(mock => mock.GetProperty("Mono.SelectedAddinConfigurationHashCode", It.IsAny<object>())).Returns(new Dictionary<string, string>());
            _propertiesManager.Setup(mock => mock.SetProperty(ApplicationConstants.SelectedConfigurationKey, It.IsAny<object>()));
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.ShowMode, false)).Returns(false);
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.ConfigWizardLocalizationCurrency, Culture.Default)).Returns(Culture.Default);

            _multiProtocolConfigurationProvider = MoqServiceManager.CreateAndAddService<IMultiProtocolConfigurationProvider>(MockBehavior.Strict);
            _multiProtocolConfigurationProvider.SetupSet(x => x.MultiProtocolConfiguration = It.IsAny<List<ProtocolConfiguration>>());
            _multiProtocolConfigurationProvider.SetupSet(x => x.IsFundsTransferRequired = It.IsAny<bool>());
            _multiProtocolConfigurationProvider.SetupSet(x => x.IsValidationRequired = It.IsAny<bool>());
            _multiProtocolConfigurationProvider.SetupSet(x => x.IsProgressiveRequired = It.IsAny<bool>());
            _multiProtocolConfigurationProvider.SetupSet(x => x.IsCentralDeterminationSystemRequired = It.IsAny<bool>());
            _multiProtocolConfigurationProvider.SetupGet(x => x.MultiProtocolConfiguration).Returns(new List<ProtocolConfiguration>());

            _configurationUtilitiesProvider = MoqServiceManager.CreateAndAddService<IConfigurationUtilitiesProvider>(MockBehavior.Strict);
            _configurationUtilitiesProvider.Setup(s => s.GetConfigWizardConfiguration(It.IsAny<Func<ConfigWizardConfiguration>>())).Returns(() => new ConfigWizardConfiguration
            {
                ProtocolConfiguration = new ConfigWizardConfigurationProtocolConfiguration
                {
                    ProtocolsAllowed = new[]
                        {
                            new Protocol { Name = CommsProtocol.SAS },
                            new Protocol { Name = CommsProtocol.G2S },
                            new Protocol { Name = CommsProtocol.MGAM },
                            new Protocol { Name = CommsProtocol.HHR },
                            new Protocol { Name = CommsProtocol.Bingo },
                            new Protocol { Name = CommsProtocol.DACOM }
                        },
                    ExclusiveProtocols = Array.Empty<ExclusiveProtocol>(),
                    RequiredFunctionality = Array.Empty<FunctionalityType>(),
                }
            });

            _target = new ProtocolSetupPageViewModel(_multiProtocolConfigurationProvider.Object, _configurationUtilitiesProvider.Object);
            _accessor = new DynamicPrivateObject(_target);
        }

        [TestMethod]
        public void ConstructorTest()
        {
            CreateTarget();
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void NextButton_WhenNoProtocolsSelected_IsDisabled()
        {
            CreateTarget();
            Assert.AreEqual(0, _target.ProtocolSelections.Where(x => x.Selected).Count());
            _configWizardNavigator.VerifySet(x => x.CanNavigateForward = true, Times.Never());
        }

        [TestMethod]
        public void NextButton_WhenProtocolsSelected_IsEnabled()
        {
            CreateTarget();
            _target.ProtocolSelections[0].Selected = true;
            Assert.AreEqual(1, _target.ProtocolSelections.Where(x => x.Selected).Count());
            _configWizardNavigator.VerifySet(x => x.CanNavigateForward = true);
        }

        [TestMethod]
        public void MultiProtocolConfigurationProvider_WhenProtocolSaved_IsUsed()
        {
            CreateTarget();
            _target.ProtocolSelections[0].Selected = true;

            _accessor.SaveChanges();

            _multiProtocolConfigurationProvider.VerifySet(x => x.MultiProtocolConfiguration = It.IsAny<List<ProtocolConfiguration>>());
        }

        [TestMethod]
        public void NextButton_WhenProtocolWithExpectedFunctionalityNotSelected_IsDisabled()
        {
            CreateTarget();
            _target.ProtocolSelections[0].Selected = true;
            _accessor._requiredFunctionality = new List<FunctionalityType> { new FunctionalityType { Type = Functionality.Validation } };
            _accessor.SetupNavigation();

            _configWizardNavigator.VerifySet(x => x.CanNavigateForward = false);

        }

        [TestMethod]
        public void MultiProtocolConfiguration_WhenFunctionalityRequired_IsUsed()
        {
            CreateTarget();
            _target.ProtocolSelections[0].Selected = true;
            _accessor._requiredFunctionality = new List<FunctionalityType> { new FunctionalityType { Type = Functionality.Validation } };
            _accessor.SaveChanges();

            _multiProtocolConfigurationProvider.VerifySet(x => x.IsValidationRequired = true);

        }

        [TestMethod]
        public void MultiProtocolConfiguration_WhenFunctionalityNotRequired_IsNotUsed()
        {
            CreateTarget();
            _target.ProtocolSelections[0].Selected = true;
            _accessor._requiredFunctionality = new List<FunctionalityType>();
            _accessor.SaveChanges();

            _multiProtocolConfigurationProvider.VerifySet(x => x.IsValidationRequired = false);

        }

        [TestMethod]
        public void RequiredFunctionalityWarningMessage_WhenFunctionalityNotExpected_IsNotVisible()
        {
            CreateTarget();
            _target.ProtocolSelections[0].Selected = true;

            Assert.IsFalse(_target.IsDisplayRequiredFunctionalityProtocolSelectionMessage);

        }

        [TestMethod]
        public void RequiredFunctionalityWarningMessage_WhenFunctionalityNotSelectedButExpected_IsVisible()
        {
            CreateTarget();
            _target.ProtocolSelections[0].Selected = true;
            _accessor._requiredFunctionality = new List<FunctionalityType> { new FunctionalityType { Type = Functionality.Validation } };

            Assert.IsTrue(_target.IsDisplayRequiredFunctionalityProtocolSelectionMessage);

        }

        [TestMethod]
        public void RequiredFunctionalityWarningMessage_WhenNoProtocolSelected_IsNotVisible()
        {
            CreateTarget();

            _accessor._requiredFunctionality = new List<FunctionalityType> { new FunctionalityType { Type = Functionality.Validation } };

            Assert.IsFalse(_target.IsDisplayRequiredFunctionalityProtocolSelectionMessage);

        }

        [TestMethod]
        public void MultiProtocolConfiguration_WhenImportedWithSingleProtocol_IsAppliedAndLoaded()
        {
            _multiProtocolConfigurationProvider.SetupGet(x => x.MultiProtocolConfiguration).Returns(new List<ProtocolConfiguration>
            {
                new ProtocolConfiguration(CommsProtocol.SAS, true, true, false, false)
            });

            CreateTarget();

            Assert.IsTrue(_target.ProtocolSelections.Any(p => p.ProtocolName == ProtocolNames.SAS && p.Enabled));
        }

        [TestMethod]
        public void MultiProtocolConfigurationWithMultiProtocol_WhenImported_IsAppliedAndLoaded()
        {
            var dummyProtocols = new List<ProtocolConfiguration>
            {
                new ProtocolConfiguration(CommsProtocol.SAS, true, true, false, false),
                new ProtocolConfiguration(CommsProtocol.HHR, true, true, false, false)
            };

            _multiProtocolConfigurationProvider.SetupGet(x => x.MultiProtocolConfiguration).Returns(dummyProtocols);

            CreateTarget();

            foreach (var protocolConfig in dummyProtocols)
            {
                Assert.IsTrue(_target.ProtocolSelections.Any(p => p.ProtocolName == EnumParser.ToName(protocolConfig.Protocol) && p.Enabled));
            }
        }
        private void CreateTarget()
        {
            _target = new ProtocolSetupPageViewModel(_multiProtocolConfigurationProvider.Object, _configurationUtilitiesProvider.Object);
            _accessor = new DynamicPrivateObject(_target);
        }
    }
}
