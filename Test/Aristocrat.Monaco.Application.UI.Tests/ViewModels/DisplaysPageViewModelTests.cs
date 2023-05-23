namespace Aristocrat.Monaco.Application.UI.Tests.ViewModels
{
    #region Using

    using System.Collections.Generic;
    using Aristocrat.Cabinet;
    using Aristocrat.Cabinet.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Input;
    using Aristocrat.Monaco.Hardware.Contracts.Cabinet;
    using Aristocrat.Monaco.Hardware.Contracts.Touch;
    using Contracts;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using UI.ViewModels;

    #endregion

    [TestClass]
    public class DisplaysPageViewModelTests
    {
        private dynamic _accessor;
        private DisplaysPageViewModel _target;

        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ICabinetDetectionService> _cabinetService;
        private Mock<ISerialTouchService> _serialTouchService;
        private Mock<ISerialTouchCalibration> _serialTouchCalibration;
        private Mock<ITouchCalibration> _touchCalibrationService;
        private Mock<IEventBus> _eventBus;
        private Mock<IServiceManager> _serviceManager;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Default);

            _cabinetService = MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Default);
            _serialTouchService = MoqServiceManager.CreateAndAddService<ISerialTouchService>(MockBehavior.Default);
            _serialTouchCalibration = MoqServiceManager.CreateAndAddService<ISerialTouchCalibration>(MockBehavior.Default);
            _touchCalibrationService = MoqServiceManager.CreateAndAddService<ITouchCalibration>(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _serviceManager = MoqServiceManager.CreateAndAddService<IServiceManager>(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _serviceManager.Setup(m => m.GetService<IPropertiesManager>()).Returns(_propertiesManager.Object);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CabinetBrightnessControlEnabled, false))
                .Returns(false);

            _target = new DisplaysPageViewModel(true);
            _accessor = new DynamicPrivateObject(_target);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void OnLoadedInInspectionModeTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(KernelConstants.IsInspectionOnly, false))
                .Returns(true);

            _accessor.OnLoaded();
            Assert.IsTrue(_target.CalibrateTouchScreenVisible);
            Assert.IsTrue(_target.TestTouchScreenVisible);
        }

        [TestMethod]
        public void OnLoadedNoTouchDevicesAndNonInspectionModeTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(KernelConstants.IsInspectionOnly, false))
                .Returns(false);

            _accessor.OnLoaded();

            Assert.IsFalse(_target.CalibrateTouchScreenVisible);
            Assert.IsFalse(_target.TestTouchScreenVisible);
        }

        [TestMethod]
        public void OnLoadedWithTouchDevicesAndNonInspectionModeTest()
        {
            var displayDevices = new List<IDisplayDevice>() { new DisplayDevice() };
            var touchDevices = new List<ITouchDevice>() { new Aristocrat.Cabinet.TouchDevice() };

            _cabinetService.Setup(c => c.ExpectedDisplayDevices).Returns(displayDevices);
            _cabinetService.Setup(c => c.ExpectedTouchDevices).Returns(touchDevices);
            _propertiesManager.Setup(m => m.GetProperty(KernelConstants.IsInspectionOnly, false))
                .Returns(false);

            _accessor.OnLoaded();

            Assert.IsFalse(_target.CalibrateTouchScreenVisible);
            Assert.IsTrue(_target.TestTouchScreenVisible);
        }

        [TestMethod]
        public void OnLoadedWithSerialTouchDevicesAndNonInspectionModeTest()
        {
            var displayDevices = new List<IDisplayDevice>() { new DisplayDevice() };

            _cabinetService.Setup(c => c.ExpectedDisplayDevicesWithSerialTouch).Returns(displayDevices);
            _propertiesManager.Setup(m => m.GetProperty(KernelConstants.IsInspectionOnly, false)).Returns(false);

            _accessor.OnLoaded();

            Assert.IsTrue(_target.CalibrateTouchScreenVisible);
            Assert.IsTrue(_target.TestTouchScreenVisible);
        }
    }
}
