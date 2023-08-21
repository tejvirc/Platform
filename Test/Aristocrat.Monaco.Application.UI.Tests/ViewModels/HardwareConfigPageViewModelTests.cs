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

    [TestClass]
    public class HardwareConfigPageViewModelTests
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
            new HardwareConfigPageViewModel().Dispose();
        }

        [TestMethod]
        [DataRow(TowerLightTierTypes.TwoTier, TowerLightTierTypes.FourTier)]
        [DataRow(TowerLightTierTypes.FourTier, TowerLightTierTypes.TwoTier)]
        public void GivenFistAndSecondWhenInitialiseThenFirstIsDefault(TowerLightTierTypes first, TowerLightTierTypes second)
        {
            _configWizardConfiguration.TowerLightTierType = new ConfigWizardConfigurationTowerLightTierType()
            {
                AvailableTowerLightTierType = new []
                {
                    new AvailableTowerLightTierType() { Type = first, IsDefault = true, },
                    new AvailableTowerLightTierType() { Type = second, }
                },
                Visible = true,
                Configurable = true,
                CanReconfigure = true,
            };
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.TowerLightTierTypeSelection, first.ToString()))
                .Returns(first.ToString());

            _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.TowerLightTierTypeSelection, first.ToString()));

            using (var underTest = new HardwareConfigPageViewModel())
            {
                Assert.IsTrue(underTest.VisibleTowerLightTierTypes);
                Assert.IsTrue(underTest.ConfigurableTowerLightTierTypes);
                Assert.AreEqual(first.ToString(), underTest.TowerLightConfigSelection.ToString());
            }
        }

        [TestMethod]
        [DataRow(TowerLightTierTypes.TwoTier, TowerLightTierTypes.FourTier)]
        [DataRow(TowerLightTierTypes.FourTier, TowerLightTierTypes.TwoTier)]
        public void GivenFistAndSecondWhenInitialiseNoDefaultDefinedThenFirstIsDefault(TowerLightTierTypes first, TowerLightTierTypes second)
        {
            _configWizardConfiguration.TowerLightTierType = new ConfigWizardConfigurationTowerLightTierType()
            {
                AvailableTowerLightTierType = new []
                {
                    new AvailableTowerLightTierType() { Type = first, },
                    new AvailableTowerLightTierType() { Type = second, }
                },
                Visible = true,
                Configurable = true,
                CanReconfigure = true,
            };
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.TowerLightTierTypeSelection, first.ToString()))
                .Returns(first.ToString());

            _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.TowerLightTierTypeSelection, first.ToString()));

            using (var underTest = new HardwareConfigPageViewModel())
            {
                Assert.IsTrue(underTest.VisibleTowerLightTierTypes);
                Assert.IsTrue(underTest.ConfigurableTowerLightTierTypes);
                Assert.AreEqual(first.ToString(), underTest.TowerLightConfigSelection.ToString());
            }
        }

        [TestMethod]
        [DataRow(TowerLightTierTypes.TwoTier, TowerLightTierTypes.FourTier, TowerLightTierTypes.FourTier)]
        [DataRow(TowerLightTierTypes.FourTier, TowerLightTierTypes.TwoTier, TowerLightTierTypes.TwoTier)]
        public void GivenFistAndSecondWhenInitialiseDefaultDefinedThenFirstIsDefault(TowerLightTierTypes first, TowerLightTierTypes second, TowerLightTierTypes @default)
        {
            _configWizardConfiguration.TowerLightTierType = new ConfigWizardConfigurationTowerLightTierType()
            {
                AvailableTowerLightTierType = new []
                {
                    new AvailableTowerLightTierType() { Type = first, IsDefault = first == @default },
                    new AvailableTowerLightTierType() { Type = second, IsDefault = second == @default }
                },
                Visible = true,
                Configurable = true,
                CanReconfigure = true,
            };
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.TowerLightTierTypeSelection, @default.ToString()))
                .Returns(Contracts.TowerLight.TowerLightTierTypes.Undefined.ToString());

            _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.TowerLightTierTypeSelection, @default.ToString())).Verifiable();

            using (var underTest = new HardwareConfigPageViewModel())
            {
                Assert.IsTrue(underTest.VisibleTowerLightTierTypes);
                Assert.IsTrue(underTest.ConfigurableTowerLightTierTypes);
                Assert.AreEqual(@default.ToString(), underTest.TowerLightConfigSelection.ToString());
            }
            _propertiesManager.Verify();
        }

        [TestMethod]
        [DataRow(TowerLightTierTypes.TwoTier, TowerLightTierTypes.FourTier)]
        [DataRow(TowerLightTierTypes.FourTier, TowerLightTierTypes.TwoTier)]
        public void GivenFistAndSecondWhenInitialiseThenStoredSelected(TowerLightTierTypes first, TowerLightTierTypes stored)
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
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.TowerLightTierTypeSelection, first.ToString()))
                .Returns(stored.ToString());

            _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.TowerLightTierTypeSelection, stored.ToString()));

            using (var underTest = new HardwareConfigPageViewModel())
            {
                Assert.IsTrue(underTest.VisibleTowerLightTierTypes);
                Assert.IsTrue(underTest.ConfigurableTowerLightTierTypes);
                Assert.AreEqual(stored.ToString(), underTest.TowerLightConfigSelection.ToString());
            }
        }

        [TestMethod]
        public void GivenTowerLightConfigSelectionWhenSelectNewThenStoreSelected()
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
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.TowerLightTierTypeSelection, TowerLightTierTypes.TwoTier.ToString()))
                .Returns(stored.ToString());

            _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.TowerLightTierTypeSelection, stored.ToString()));

            using (var underTest = new HardwareConfigPageViewModel())
            {
                Assert.AreEqual(stored.ToString(), underTest.TowerLightConfigSelection.ToString());
                _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.TowerLightTierTypeSelection, newSelected.ToString()));
                underTest.TowerLightConfigSelection = newSelected;
                _propertiesManager.Verify(
                    x => x.SetProperty(ApplicationConstants.TowerLightTierTypeSelection, newSelected.ToString()), Times.Once);
                Assert.AreEqual(newSelected, underTest.TowerLightConfigSelection);
            }
        }

    }
}