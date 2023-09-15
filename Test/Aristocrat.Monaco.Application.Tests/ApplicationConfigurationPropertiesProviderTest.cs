namespace Aristocrat.Monaco.Application.Tests
{
    using System.Collections.Generic;
    using Cabinet.Contracts;
    using Contracts;
	using Hardware.Contracts;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class ApplicationConfigurationPropertiesProviderTest
    {
        private const long DefaultBellValueInMillicents = 1000000;
        private Mock<IPersistentStorageManager> _storageManager;
        private Mock<IPersistentStorageAccessor> _accessor;
        private Mock<IPropertiesManager> _propertiesManager;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Strict);
            _storageManager = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Default);
            _accessor = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);

            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None))
                .Returns(ImportMachineSettings.None);
            _propertiesManager.Setup(p => p.GetProperty(It.IsAny<string>(), It.IsAny<object>()))
                .Returns<string, object>((s, o) => o);
            _propertiesManager.Setup(
                m => m.AddPropertyProvider(It.IsAny<ApplicationConfigurationPropertiesProvider>()));

            var cabinetDetection = MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Strict);
            cabinetDetection.Setup(c => c.Type).Returns(It.IsAny<CabinetType>());

            var storageName = typeof(ApplicationConfigurationPropertiesProvider).ToString();
            _storageManager.Setup(m => m.BlockExists(It.Is<string>(s => s == storageName))).Returns(true);
            _storageManager.Setup(m => m.GetBlock(It.Is<string>(s => s == storageName))).Returns(_accessor.Object);
            _accessor.Setup(a => a[It.IsAny<string>()]).Returns(It.IsAny<object>());
            _accessor.Setup(a => a[PropertyKey.DefaultVolumeLevel]).Returns((byte)100);
            _accessor.Setup(a => a[ApplicationConstants.BottomEdgeLightingOnKey]).Returns(false);
            _accessor.Setup(a => a[ApplicationConstants.CabinetTypeKey]).Returns(CabinetType.Bartop);
            _accessor.Setup(a => a[ApplicationConstants.BarCodeType]).Returns(BarcodeTypeOptions.Interleave2of5);
            _accessor.Setup(a => a[HardwareConstants.AlertVolumeKey]).Returns((byte)100);
            _accessor.Setup(a => a[HardwareConstants.LobbyVolumeScalarKey]).Returns((byte)5);
            _accessor.Setup(a => a[HardwareConstants.PlayerVolumeScalarKey]).Returns((byte)5);
            _accessor.Setup(a => a[ApplicationConstants.LayoutType]).Returns(LayoutTypeOptions.ExtendedLayout);
            _accessor.Setup(a => a[ApplicationConstants.ValidationLength]).Returns(ValidationLengthOptions.System);
            _accessor.Setup(a => a[ApplicationConstants.ReserveServiceLockupPresent]).Returns(false);
            _accessor.Setup(a => a[ApplicationConstants.ReserveServiceEnabled]).Returns(true);
            _accessor.Setup(a => a[ApplicationConstants.ReserveServicePin]).Returns(string.Empty);
            _accessor.Setup(a => a[ApplicationConstants.ReserveServiceTimeoutInSeconds]).Returns(10);
            _accessor.Setup(a => a[ApplicationConstants.ReserveServiceLockupRemainingSeconds]).Returns(10);
            _accessor.Setup(a => a[ApplicationConstants.InitialBellRing]).Returns(DefaultBellValueInMillicents);
            _accessor.Setup(a => a[ApplicationConstants.IntervalBellRing]).Returns(DefaultBellValueInMillicents);
            _accessor.Setup(a => a[HardwareConstants.VolumeControlLocationKey]).Returns(2);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            var target = new ApplicationConfigurationPropertiesProvider();
            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void VerifyBarCodeTypePropertyIsAdded()
        {
            var target = new ApplicationConfigurationPropertiesProvider();
            Assert.IsTrue(
                target.GetCollection.Contains(new KeyValuePair<string, object>(ApplicationConstants.BarCodeType, It.IsAny<int>())));
        }

        [TestMethod]
        public void VerifyBarCodeTypePropertyIsSet()
        {
            _accessor.Setup(a => a[ApplicationConstants.BarCodeType]).Returns(BarcodeTypeOptions.Interleave2of5);
            var target = new ApplicationConfigurationPropertiesProvider();
            var barcodeType = (BarcodeTypeOptions)target.GetProperty(ApplicationConstants.BarCodeType);
            Assert.AreEqual(barcodeType, BarcodeTypeOptions.Interleave2of5);
        }
    }
}
