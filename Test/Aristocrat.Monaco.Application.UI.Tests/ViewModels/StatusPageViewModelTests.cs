namespace Aristocrat.Monaco.Application.UI.Tests.ViewModels
{
    #region Using

    using System;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Application.UI.Events;
    using Contracts;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.Battery;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.KeySwitch;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using UI.ViewModels;

    #endregion

    [TestClass]
    public class StatusPageViewModelTests
    {
        private dynamic _accessor;
        private Mock<IDisableByOperatorManager> _disabledByOperatorManager;
        private Mock<ISystemDisableManager> _disableManager;
        private Mock<IEventBus> _eventBus;
        private Mock<IOperatorMenuGamePlayMonitor> _operatorMenu;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IMessageDisplay> _messageDisplay;
        private Mock<IBattery> _batteryTestService;
        private Mock<IBank> _bank;
        private Mock<IOperatorMenuConfiguration> _operatorMenuConfiguration;
        private StatusPageViewModel _target;
        private Action<PropertyChangedEvent> _propertyChangedHandler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _disableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Strict);
            _disableManager.Setup(m => m.IsDisabled).Returns(false);

            _batteryTestService = MoqServiceManager.CreateAndAddService<IBattery>(MockBehavior.Strict);
            _bank = MoqServiceManager.CreateAndAddService<IBank>(MockBehavior.Strict);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            //_propertiesManager
            //    .Setup(m => m.GetProperty(ApplicationConstants.OperatorMenuStatusPageOperatorDisableWithCredits, true))
            //    .Returns(false);
            //_propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OperatorMenuPrintButtonOptionsPrintLast15, true))
            //    .Returns(true);
            //_propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OperatorMenuTechnicianModeRestrictions, false))
            //    .Returns(false);
            //_propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OperatorMenuPrintButtonOptionsPrintCurrentPage, true))
            //    .Returns(true);
            //_propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OperatorMenuPrintButtonOptionsPrintSelected, true))
            //    .Returns(true);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _eventBus.Setup(m => m.Publish(It.IsAny<PrintButtonStatusEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuWarningMessageEvent>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<PrintButtonClickedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<PrintCompletedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<ClosedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<OnEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<OperatorCultureChangedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<SystemEnabledByOperatorEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<SystemDisabledByOperatorEvent>>()));
            _disabledByOperatorManager =
                MoqServiceManager.CreateAndAddService<IDisableByOperatorManager>(MockBehavior.Strict);

            _messageDisplay = MoqServiceManager.CreateAndAddService<IMessageDisplay>(MockBehavior.Strict);

            _operatorMenu = new Mock<IOperatorMenuGamePlayMonitor>(MockBehavior.Strict);
            _operatorMenu.Setup(m => m.InGameRound).Returns(true);
            _operatorMenu.Setup(m => m.IsRecoveryNeeded).Returns(false);
            MoqServiceManager.AddService(_operatorMenu);

            _operatorMenuConfiguration = MoqServiceManager.CreateAndAddService<IOperatorMenuConfiguration>(MockBehavior.Strict);
            _operatorMenuConfiguration.Setup(o => o.GetSetting(OperatorMenuSetting.UseOperatorCultureForCurrencyFormatting, false))
                .Returns(false);

            _target = new StatusPageViewModel();
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
        public void OutOfServiceModeButtonActiveTest()
        {
            _target.OutOfServiceViewModel.OutOfServiceModeButtonIsEnabled = true;
            Assert.IsTrue(_target.OutOfServiceViewModel.OutOfServiceModeButtonIsEnabled);
        }

        [TestMethod]
        public void StatusTextTest()
        {
            _target.InputStatusText = "Test";
            Assert.AreEqual("Test", _target.InputStatusText);
        }

        [TestMethod]
        public void LoadedCommandTest()
        {
            Assert.IsNotNull(_target.LoadedCommand);
        }

        [TestMethod]
        public void UnloadedCommandTest()
        {
            Assert.IsNotNull(_target.UnloadedCommand);
        }

        [TestMethod]
        public void OutOfServiceModeButtonCommandTest()
        {
            Assert.IsNotNull(_target.OutOfServiceViewModel.OutOfServiceModeButtonCommand);
        }

        [TestMethod]
        public void OnLoadedWhenDisabledByOperatorTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<PageTitleEvent>())).Verifiable();
            _disabledByOperatorManager.Setup(m => m.DisabledByOperator).Returns(true);
            _messageDisplay.Setup(m => m.AddMessageDisplayHandler(It.IsAny<StatusPageViewModel>(), true));
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEnabled, false)).Returns(true);
            _bank.Setup(m => m.QueryBalance()).Returns(0);
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.ReserveServiceLockupPresent, false))
                .Returns(false);
            _eventBus.Setup(
                    x => x.Subscribe(
                        _target,
                        It.IsAny<Action<PropertyChangedEvent>>()))
                .Callback<object, Action<PropertyChangedEvent>>((y, x) => _propertyChangedHandler = x);

            _accessor.OnLoaded();

            _messageDisplay.Verify(m => m.AddMessageDisplayHandler(It.IsAny<StatusPageViewModel>(), true), Times.Once());
        }

        [TestMethod]
        public void OnLoadedWhenReserveLockupPresent()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<PageTitleEvent>())).Verifiable();
            _disabledByOperatorManager.Setup(m => m.DisabledByOperator).Returns(true);
            _messageDisplay.Setup(m => m.AddMessageDisplayHandler(It.IsAny<StatusPageViewModel>(), true));
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEnabled, false)).Returns(true);
            _bank.Setup(m => m.QueryBalance()).Returns(0);
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.ReserveServiceLockupPresent, false))
                .Returns(true);
            _eventBus.Setup(
                    x => x.Subscribe(
                        _target,
                        It.IsAny<Action<PropertyChangedEvent>>()))
                .Callback<object, Action<PropertyChangedEvent>>((y, x) => _propertyChangedHandler = x);

            _accessor.OnLoaded();

            Assert.IsTrue(_accessor.IsExitReserveButtonVisible);

            _messageDisplay.Verify(m => m.AddMessageDisplayHandler(It.IsAny<StatusPageViewModel>(), true), Times.Once());
        }

        [TestMethod]
        public void HandlingPropertyChangedForReserveServiceLockupEventToFalse()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<PageTitleEvent>())).Verifiable();
            _disabledByOperatorManager.Setup(m => m.DisabledByOperator).Returns(true);
            _messageDisplay.Setup(m => m.AddMessageDisplayHandler(It.IsAny<StatusPageViewModel>(), true));
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEnabled, false)).Returns(true);
            _bank.Setup(m => m.QueryBalance()).Returns(0);
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.ReserveServiceLockupPresent, false))
                .Returns(true);
            _eventBus.Setup(
                    x => x.Subscribe(
                        _target,
                        It.IsAny<Action<PropertyChangedEvent>>()))
                .Callback<object, Action<PropertyChangedEvent>>((y, x) => _propertyChangedHandler = x);

            _accessor.OnLoaded();

            Assert.IsTrue(_accessor.IsExitReserveButtonVisible);

            _messageDisplay.Verify(m => m.AddMessageDisplayHandler(It.IsAny<StatusPageViewModel>(), true), Times.Once());

            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.ReserveServiceLockupPresent, false))
                .Returns(false);

            _propertyChangedHandler?.Invoke(new PropertyChangedEvent { PropertyName = ApplicationConstants.ReserveServiceLockupPresent });

            Assert.IsFalse(_accessor.IsExitReserveButtonVisible);
        }

        [TestMethod]
        public void OnLoadedWhenNotDisabledByOperatorTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<PageTitleEvent>())).Verifiable();
            _disabledByOperatorManager.Setup(m => m.DisabledByOperator).Returns(false);
            _propertiesManager.Setup(m => m.GetProperty(PropertyKey.CurrentBalance, 0L)).Returns(1L);
            _messageDisplay.Setup(m => m.AddMessageDisplayHandler(It.IsAny<StatusPageViewModel>(), true));
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEnabled, false)).Returns(true);
            _bank.Setup(m => m.QueryBalance()).Returns(0);
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.ReserveServiceLockupPresent, false))
                .Returns(false);
            _eventBus.Setup(
                    x => x.Subscribe(
                        _target,
                        It.IsAny<Action<PropertyChangedEvent>>()))
                .Callback<object, Action<PropertyChangedEvent>>((y, x) => _propertyChangedHandler = x);

            _accessor.OnLoaded();

            _messageDisplay.Verify(m => m.AddMessageDisplayHandler(It.IsAny<StatusPageViewModel>(), true), Times.Once());
        }

        [TestMethod]
        public void OnUnloadedTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<PageTitleEvent>())).Verifiable();
            _eventBus.Setup(m => m.Unsubscribe<ClosedEvent>(_target));

            _disabledByOperatorManager.Setup(m => m.DisabledByOperator).Returns(true);
            _messageDisplay.Setup(m => m.AddMessageDisplayHandler(It.IsAny<StatusPageViewModel>(), true));
            _messageDisplay.Setup(m => m.RemoveMessageDisplayHandler(It.IsAny<StatusPageViewModel>()));
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEnabled, false)).Returns(true);
            _bank.Setup(m => m.QueryBalance()).Returns(0);
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.ReserveServiceLockupPresent, false))
                .Returns(false);
            _eventBus.Setup(
                    x => x.Subscribe(
                        _target,
                        It.IsAny<Action<PropertyChangedEvent>>()))
                .Callback<object, Action<PropertyChangedEvent>>((y, x) => _propertyChangedHandler = x);

            _accessor.OnLoaded();

            _eventBus.Setup(x => x.Unsubscribe<PropertyChangedEvent>(It.IsAny<StatusPageViewModel>()));

            _accessor.OnUnloaded();

            _messageDisplay.Verify(m => m.AddMessageDisplayHandler(It.IsAny<StatusPageViewModel>(), true), Times.Once());
            _messageDisplay.Verify(m => m.RemoveMessageDisplayHandler(It.IsAny<StatusPageViewModel>()), Times.Once());
        }
    }
}