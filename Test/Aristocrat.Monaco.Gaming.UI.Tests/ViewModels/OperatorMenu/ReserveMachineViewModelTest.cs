namespace Aristocrat.Monaco.Gaming.UI.Tests.ViewModels.OperatorMenu
{
    using System;
    using System.IO;
    using Application.Contracts;
    using Application.Contracts.OperatorMenu;
    using Contracts;
    using Kernel;
    using Loaders;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using UI.ViewModels.OperatorMenu;

    [TestClass]
    public class ReserveMachineViewModelTests
    {
        private Mock<IOperatorMenuConfiguration> _operatorMenuConfiguration;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IEventBus> _eventBus;
        private ReserveMachineViewModel _target;
        private Mock<IReserveService> _reserveService;
        private dynamic _accessor;
        private Action<PropertyChangedEvent> _propertyChangedHandler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);

            _operatorMenuConfiguration =
                MoqServiceManager.CreateAndAddService<IOperatorMenuConfiguration>(MockBehavior.Strict);

            _operatorMenuConfiguration.Setup(o => o.GetPageName(It.IsAny<IOperatorMenuConfigObject>()))
                .Returns(string.Empty);

            _operatorMenuConfiguration.Setup(o => o.GetSetting(OperatorMenuSetting.UseOperatorCultureForCurrencyFormatting, false))
                .Returns(false);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);

            MoqServiceManager.CreateAndAddService<IContainerService>(MockBehavior.Loose);

            _reserveService = MoqServiceManager.CreateAndAddService<IReserveService>(MockBehavior.Strict);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            AddinManager.Shutdown();
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ReserveMachinePageLoaderTest()
        {
            const string expected = "Reserve Machine";

            var target = new ReserveMachinePageLoader();

            Assert.IsNotNull(target);
            Assert.AreEqual(expected, target.PageName);
        }

        [TestMethod]
        public void ReserveMachineViewModelConstructorTest()
        {
            CreateTarget();

            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void ReserveMachineViewModelOnLoadedTest()
        {
            OnLoadedTest();
        }

        [TestMethod]
        public void ReserveMachineViewModelOnLoadedWithOptionNotEnabledAndExistingLockup()
        {
            CreateTarget();

            Assert.IsNotNull(_target);

            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.ReserveServiceEnabled, true))
                .Returns(false);
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.ReserveServiceLockupPresent, false))
                .Returns(true);
            _propertiesManager
                .Setup(x => x.GetProperty(ApplicationConstants.ReserveServiceTimeoutInSeconds, It.IsAny<int>()))
                .Returns(180);
            _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.ReserveServiceEnabled, false));
            _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.ReserveServiceTimeoutInSeconds, 180));

            _eventBus.Setup(
                    x => x.Subscribe(
                        _target,
                        It.IsAny<Action<PropertyChangedEvent>>()))
                .Callback<object, Action<PropertyChangedEvent>>((y, x) => _propertyChangedHandler = x);

            _reserveService.Setup(x => x.ExitReserveMachine()).Returns(true);

            _accessor.OnLoaded();

            Assert.AreEqual(_target.ReserveMachineDurationSelection, 3);
            Assert.IsFalse(_target.IsReserveMachineOptionEnabled);
            Assert.IsFalse(_target.AllowPlayerToReserveMachine);
            Assert.IsFalse(_target.IsReserveMachineDurationEnabled);
        }

        [TestMethod]
        public void HandlingReserveServiceLockupPresentEvent()
        {
            OnLoadedTest();

            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.ReserveServiceLockupPresent, false))
                .Returns(true);

            _propertyChangedHandler?.Invoke(
                new PropertyChangedEvent { PropertyName = ApplicationConstants.ReserveServiceLockupPresent });

            Assert.AreEqual(_target.IsReserveMachineOptionEnabled, false);
        }

        [TestMethod]
        public void ReserveMachineViewModelOnUnloadedTest()
        {
            OnLoadedTest();

            _eventBus.Setup(x => x.Unsubscribe<PropertyChangedEvent>(It.IsAny<ReserveMachineViewModel>()));

            _accessor.OnUnloaded();
        }

        private void OnLoadedTest()
        {
            CreateTarget();

            Assert.IsNotNull(_target);

            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.ReserveServiceEnabled, true))
                .Returns(true);
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.ReserveServiceLockupPresent, false))
                .Returns(false);
            _propertiesManager
                .Setup(x => x.GetProperty(ApplicationConstants.ReserveServiceTimeoutInSeconds, It.IsAny<int>()))
                .Returns(180);
            _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.ReserveServiceEnabled, true));
            _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.ReserveServiceTimeoutInSeconds, 180));

            _eventBus.Setup(
                    x => x.Subscribe(
                        _target,
                        It.IsAny<Action<PropertyChangedEvent>>()))
                .Callback<object, Action<PropertyChangedEvent>>((y, x) => _propertyChangedHandler = x);

            _accessor.OnLoaded();

            Assert.AreEqual(_target.ReserveMachineDurationSelection, 3);
            Assert.IsTrue(_target.IsReserveMachineOptionEnabled);
            Assert.IsTrue(_target.AllowPlayerToReserveMachine);
            Assert.IsTrue(_target.IsReserveMachineDurationEnabled);
        }

        private void CreateTarget()
        {
            _target = new ReserveMachineViewModel();
            _accessor = new DynamicPrivateObject(_target);
        }
    }
}