namespace Aristocrat.Monaco.Application.UI.Tests.ViewModels
{
    using System;
    using System.IO;
    using Contracts;
    using Contracts.OperatorMenu;
    using Hardware.Contracts;
    using Hardware.Contracts.HardMeter;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using UI.ViewModels;
    using Vgt.Client12.Application.OperatorMenu;

    [TestClass]
    public class HardwareManagerPageViewModelTests
    {
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IHardwareConfiguration> _hardwareConfiguration;
        private Mock<IConfigurationUtilitiesProvider> _configurationUtilitiesProvider;
        private ConfigWizardConfiguration _configWizardConfiguration;
        private Mock<IHardMeterMappingConfigurationProvider> _hardMeterMappingConfigurationProvider;
        private HardMeterMappingConfiguration _hardMeterMappingConfiguration;

        [TestInitialize]
        public void Initialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            var service = MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);


            service.Setup(x => x.TryGetService<IEventBus>()).Returns(Mock.Of<IEventBus>());
            MoqServiceManager.CreateAndAddService<IDialogService>(MockBehavior.Strict);
            MoqServiceManager.CreateAndAddService<IOperatorMenuLauncher>(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);

            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.HardMeterTickValue, 100L))
                .Returns(100L);
            _propertiesManager.Setup(x => x.GetProperty(
                ApplicationConstants.HardMeterMapSelectionValue,
                ApplicationConstants.HardMeterDefaultMeterMappingName))
                .Returns(Contracts.TowerLight.TowerLightTierTypes.Undefined.ToString());

            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.TowerLightTierTypeSelection, Contracts.TowerLight.TowerLightTierTypes.Undefined.ToString()))
                .Returns(Contracts.TowerLight.TowerLightTierTypes.Undefined.ToString());

            _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.TowerLightTierTypeSelection, Contracts.TowerLight.TowerLightTierTypes.Undefined.ToString()));

            MoqServiceManager.CreateAndAddService<IHardMeter>(MockBehavior.Strict);
            
            _hardwareConfiguration = MoqServiceManager.CreateAndAddService<IHardwareConfiguration>(MockBehavior.Strict);
            _hardwareConfiguration.SetupAllProperties();
            _hardwareConfiguration.Setup(x => x.GetCurrent()).Returns(Array.Empty<ConfigurationData>());



            _configurationUtilitiesProvider = MoqServiceManager.CreateAndAddService<IConfigurationUtilitiesProvider>(MockBehavior.Strict);

            _hardMeterMappingConfigurationProvider = MoqServiceManager.CreateAndAddService<IHardMeterMappingConfigurationProvider>(MockBehavior.Strict);

            _hardMeterMappingConfiguration = new HardMeterMappingConfiguration()
            {
                HardMeterMapping = Array.Empty<HardMeterMappingConfigurationHardMeterMapping>()
            };
            _hardMeterMappingConfigurationProvider.Setup(
                    x => x.GetHardMeterMappingConfiguration(It.IsAny<Func<HardMeterMappingConfiguration>>()))
                .Returns(_hardMeterMappingConfiguration);

            _configWizardConfiguration = new ConfigWizardConfiguration()
            {
                TowerLightTierType = new ConfigWizardConfigurationTowerLightTierType()
                {
                    AvailableTowerLightTierType = Array.Empty<AvailableTowerLightTierType>()
                }
            };
            _configurationUtilitiesProvider.Setup(
                x => x.GetConfigWizardConfiguration(It.IsAny<Func<ConfigWizardConfiguration>>()))
                .Returns(_configWizardConfiguration);
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod]
        public void DefaultConstructor()
        {
            new HardwareManagerPageViewModel().Dispose();
        }

        [TestMethod]
        [DataRow(TowerLightTierTypes.TwoTier, TowerLightTierTypes.FourTier)]
        [DataRow(TowerLightTierTypes.FourTier, TowerLightTierTypes.TwoTier)]
        public void GivenStoredAndFirstWhenInitialiseThenStoredSelected(TowerLightTierTypes stored, TowerLightTierTypes first)
        {
            _configWizardConfiguration.TowerLightTierType = new ConfigWizardConfigurationTowerLightTierType()
            {
                AvailableTowerLightTierType = new []
                {
                    new AvailableTowerLightTierType() { Type = first, },
                },
                Visible = true,
                Configurable = true,
                CanReconfigure = true,
            };
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.TowerLightTierTypeSelection, Contracts.TowerLight.TowerLightTierTypes.Undefined.ToString()))
                .Returns(stored.ToString());

            using (var underTest = new HardwareManagerPageViewModel())
            {
                _propertiesManager.ResetCalls();
                Assert.IsTrue(underTest.VisibleTowerLightTierTypes);
                Assert.IsTrue(underTest.ConfigurableTowerLightTierTypes);
                Assert.AreEqual(stored.ToString(), underTest.TowerLightConfigSelection.ToString());
                _propertiesManager.Verify(x => x.SetProperty(ApplicationConstants.TowerLightTierTypeSelection, stored.ToString()), Times.Never);
            }
        }

        [TestMethod]
        public void GivenTowerLightConfigSelectionWhenConfigurableFalseThenNoChangesAllowed()
        {
            var stored = TowerLightTierTypes.TwoTier;
            var newSelected = Contracts.TowerLight.TowerLightTierTypes.FourTier;
            _configWizardConfiguration.TowerLightTierType = new ConfigWizardConfigurationTowerLightTierType()
            {
                AvailableTowerLightTierType = new []
                {
                    new AvailableTowerLightTierType() { Type = stored, },
                },
                Visible = true,
                Configurable = true,
                CanReconfigure = true,
            };
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.TowerLightTierTypeSelection, Contracts.TowerLight.TowerLightTierTypes.Undefined.ToString()))
                .Returns(stored.ToString());

            _configWizardConfiguration.TowerLightTierType.Configurable = false;
            using (var underTest = new HardwareManagerPageViewModel())
            {
                _propertiesManager.ResetCalls();
                Assert.AreEqual(stored.ToString(), underTest.TowerLightConfigSelection.ToString());
                underTest.TowerLightConfigSelection = newSelected;
                Assert.AreEqual(stored.ToString(), underTest.TowerLightConfigSelection.ToString());
            }
            _propertiesManager.Verify(x => x.SetProperty(ApplicationConstants.TowerLightTierTypeSelection, It.IsAny<object>()), Times.Never);
        }

    }
}