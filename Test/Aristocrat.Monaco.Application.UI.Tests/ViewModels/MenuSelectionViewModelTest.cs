namespace Aristocrat.Monaco.Application.UI.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using Contracts;
    using Contracts.Input;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Events;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Display;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.KeySwitch;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SharedDevice;
    using Hardware.Contracts.Touch;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.UI.Common.Events;
    using Mono.Addins;
    using Moq;
    using OperatorMenu;
    using Test.Common;
    using UI.ViewModels;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     Tests for the MenuSelectionViewModel class
    /// </summary>
    [TestClass]
    public class MenuSelectionViewModelTest
    {
        private dynamic _accessor;
        private Mock<IButtonService> _buttonService;
        private Mock<IEventBus> _eventBus;
        private Mock<IMeter> _meter = new Mock<IMeter>(MockBehavior.Strict);
        private Mock<IMeterManager> _meterManager;
        private Mock<IOperatorMenuLauncher> _operatorMenu;
        private Mock<IPrinter> _printer;
        private Mock<IDeviceService> _printerService;
        private Mock<IPropertiesManager> _propertiesManager;
        private MenuSelectionViewModel _target;
        private Mock<ITime> _time;
        private Mock<IDoorService> _door;
        private Mock<IDialogService> _dialogService;
        private Mock<IOperatorMenuConfiguration> _configuration;
        private Mock<IOperatorMenuGamePlayMonitor> _gamePlayMonitor;
        private Mock<ISerialTouchService> _serialTouchService;
        private Mock<ITouchCalibration> _touchCalibration;

        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _buttonService = MoqServiceManager.CreateAndAddService<IButtonService>(MockBehavior.Strict);
            _buttonService.Setup(m => m.Enable(It.IsAny<Collection<int>>())).Verifiable();

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.RolePropertyKey, string.Empty))
                .Returns(ApplicationConstants.DefaultRole);
            _propertiesManager.Setup(m => m.GetProperty("Mono.SelectedAddinConfigurationHashCode", null)).Returns(null);
            _propertiesManager.Setup(m => m.GetProperty("System.CurrentBalance", It.IsAny<long>())).Returns(0L);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMultiplierKey, It.IsAny<double>())).Returns(1.0);
            _propertiesManager.Setup(m => m.GetProperty("display", It.IsAny<string>())).Returns(string.Empty);
            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.RolePropertyKey, It.IsAny<string>()));

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MenuSelectionViewModel>(), It.IsAny<Action<OpenEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MenuSelectionViewModel>(), It.IsAny<Action<ClosedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MenuSelectionViewModel>(), It.IsAny<Action<UpEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MenuSelectionViewModel>(), It.IsAny<Action<OperatorMenuPrintJobEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MenuSelectionViewModel>(), It.IsAny<Action<OperatorMenuPageLoadedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MenuSelectionViewModel>(), It.IsAny<Action<OperatorMenuPrintJobStartedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MenuSelectionViewModel>(), It.IsAny<Action<OperatorMenuPrintJobCompletedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MenuSelectionViewModel>(), It.IsAny<Action<SystemDownEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MenuSelectionViewModel>(), It.IsAny<Action<TouchDisplayDisconnectedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MenuSelectionViewModel>(), It.IsAny<Action<UpdateOperatorMenuRoleEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MenuSelectionViewModel>(), It.IsAny<Action<OperatorMenuPopupEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MenuSelectionViewModel>(), It.IsAny<Action<ServiceAddedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MenuSelectionViewModel>(), It.IsAny<Action<OffEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<OperatorMenuPrintHandler>(), It.IsAny<Action<Hardware.Contracts.Printer.InspectedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<OperatorMenuPrintHandler>(), It.IsAny<Action<Hardware.Contracts.Printer.EnabledEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<OperatorMenuPrintHandler>(), It.IsAny<Action<Hardware.Contracts.Printer.DisabledEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<OperatorMenuPrintHandler>(), It.IsAny<Action<Hardware.Contracts.Printer.HardwareFaultClearEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<OperatorMenuPrintHandler>(), It.IsAny<Action<PrintCompletedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<OperatorMenuPrintHandler>(), It.IsAny<Action<OperatorMenuPrintJobStartedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<OperatorMenuPrintHandler>(), It.IsAny<Action<OperatorMenuPrintJobCompletedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<OperatorMenuPrintHandler>(), It.IsAny<Action<ServiceAddedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<OperatorMenuPrintHandler>(), It.IsAny<Action<PrintStartedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<OperatorMenuPrintHandler>(), It.IsAny<Action<OperatorCultureChangedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MenuSelectionViewModel>(), It.IsAny<Action<DisplayConnectedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MenuSelectionViewModel>(), It.IsAny<Action<OperatorMenuWarningMessageEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MenuSelectionViewModel>(), It.IsAny<Action<TouchCalibrationCompletedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MenuSelectionViewModel>(), It.IsAny<Action<DialogOpenedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MenuSelectionViewModel>(), It.IsAny<Action<DialogClosedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MenuSelectionViewModel>(), It.IsAny<Action<SerialTouchCalibrationCompletedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MenuSelectionViewModel>(), It.IsAny<Action<OperatorCultureChangedEvent>>()))
                .Verifiable();

            _printerService = new Mock<IDeviceService>(MockBehavior.Strict);
            _printerService.Setup(m => m.LastError).Returns(string.Empty);
            _printerService.SetupGet(m => m.Enabled).Returns(true);
            _printer = _printerService.As<IPrinter>();
            _printer.SetupGet(p => p.CanPrint).Returns(true);
            _printer.SetupGet(m => m.LogicalState).Returns(PrinterLogicalState.Idle);
            MoqServiceManager.AddService(_printer);

            _operatorMenu = new Mock<IOperatorMenuLauncher>(MockBehavior.Strict);
            _operatorMenu.Setup(m => m.IsShowing).Returns(true);
            MoqServiceManager.AddService(_operatorMenu);

            _time = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);
            _time.Setup(m => m.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.MinValue);
            _time.Setup(m => m.FormatDateTimeString(It.IsAny<DateTime>(), ApplicationConstants.DefaultDateTimeFormat))
                .Returns(It.IsAny<DateTime>().ToString());
            _time.Setup(m => m.FormatDateTimeString(It.IsAny<DateTime>(), ApplicationConstants.DefaultDateTimeFormat))
                .Returns(It.IsAny<DateTime>().ToString());
            _time.Setup(m => m.FormatDateTimeString(It.IsAny<DateTime>(), "HH:mm:ss"))
                .Returns(It.IsAny<DateTime>().ToString());

            _door = MoqServiceManager.CreateAndAddService<IDoorService>(MockBehavior.Strict);
            _dialogService = MoqServiceManager.CreateAndAddService<IDialogService>(MockBehavior.Strict);
            _configuration = MoqServiceManager.CreateAndAddService<IOperatorMenuConfiguration>(MockBehavior.Strict);
            _configuration.Setup(m => m.GetSetting(OperatorMenuSetting.ShowExitButton, false)).Returns(true);
            _configuration.Setup(m => m.GetSetting(OperatorMenuSetting.ShowOperatorRole, false)).Returns(false);
            _configuration.Setup(m => m.GetSetting(OperatorMenuSetting.KeySwitchExitOverridesButton, false)).Returns(false);
            _configuration.Setup(m => m.GetSetting(OperatorMenuSetting.ShowToggleLanguageButton, false)).Returns(true);
            _configuration.Setup(m => m.GetAccessRuleSet(It.IsAny<MenuSelectionViewModel>())).Returns("");
            _configuration.Setup(m => m.GetSetting(OperatorMenuSetting.UseOperatorCultureForCurrencyFormatting, false)).Returns(false);
            _gamePlayMonitor = MoqServiceManager.CreateAndAddService<IOperatorMenuGamePlayMonitor>(MockBehavior.Strict);
            _gamePlayMonitor.Setup(m => m.InReplay).Returns(false);
            _serialTouchService = MoqServiceManager.CreateAndAddService<ISerialTouchService>(MockBehavior.Strict);
            _touchCalibration = MoqServiceManager.CreateAndAddService<ITouchCalibration>(MockBehavior.Strict);
            _touchCalibration.Setup(m => m.IsCalibrating).Returns(false);

            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);

            MoqServiceManager.CreateAndAddService<IPrinterMonitor>(MockBehavior.Default);

            var access = MoqServiceManager.CreateAndAddService<IOperatorMenuAccess>(MockBehavior.Default);
            access.SetupGet(a => a.TechnicianMode).Returns(true);

            if (Application.Current == null)
            {
                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            }

            _target = new MenuSelectionViewModel();
            _accessor = new DynamicPrivateObject(_target);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
            try
            {
                AddinManager.Shutdown();
            }
            catch (InvalidOperationException)
            {
                // temporarily swallow exception
            }
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [Ignore]
        [TestMethod]
        public void HandleDoorOpenCloseWhenNotLogicDoorTest()
        {
            string role = ApplicationConstants.DefaultRole;
            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.RolePropertyKey, role)).Verifiable();
            _door.Setup(m => m.LogicalDoors).Returns(
                new Dictionary<int, LogicalDoor>
                {
                    { (int)DoorLogicalId.Main, new LogicalDoor { Closed = false } },
                    { (int)DoorLogicalId.Logic, new LogicalDoor { Closed = true } },
                });
            var closeEvent = new ClosedEvent((int)DoorLogicalId.CashBox, string.Empty);

            _accessor.HandleDoorOpenClose(closeEvent);
            Assert.AreEqual(role, _accessor._role);
        }

        [Ignore]
        [TestMethod]
        public void HandleDoorOpenCloseWhenLogicDoorCloseTest()
        {
            CreateViews();
            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.RolePropertyKey, ApplicationConstants.DefaultRole)).Verifiable();
            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.RolePropertyKey, ApplicationConstants.TechnicianRole)).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuEnteredEvent>())).Verifiable();
            _meterManager.Setup(m => m.IsMeterProvided(ApplicationConstants.DefaultRole + "Access")).Returns(true);
            _meterManager.Setup(m => m.IsMeterProvided(ApplicationConstants.TechnicianRole + "Access")).Returns(true);
            _meter.Setup(m => m.Increment(1)).Verifiable();
            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(_meter.Object);
            _door.Setup(m => m.LogicalDoors).Returns(
                new Dictionary<int, LogicalDoor>
                {
                    { (int)DoorLogicalId.Main, new LogicalDoor { Closed = false } },
                    { (int)DoorLogicalId.Logic, new LogicalDoor { Closed = true } },
                });

            var openEvent = new OpenEvent((int)DoorLogicalId.Logic, string.Empty);
            var closeEvent = new ClosedEvent((int)DoorLogicalId.Logic, string.Empty);

            _accessor.HandleDoorOpenClose(openEvent);
            //Assert.AreEqual(ApplicationConstants.TechnicianRole, _accessor._role);
            _accessor.HandleDoorOpenClose(closeEvent);

            Assert.AreEqual(ApplicationConstants.DefaultRole, _accessor._role);
            _propertiesManager.Verify();
            _eventBus.Verify();
            _meterManager.Verify();
            _meter.Verify();
        }

        [Ignore]
        [TestMethod]
        public void HandleDoorOpenCloseWhenNotLogicDoorOpenTest()
        {
            string role = ApplicationConstants.DefaultRole;
            _door.Setup(m => m.LogicalDoors).Returns(
                new Dictionary<int, LogicalDoor>
                {
                    { (int)DoorLogicalId.Main, new LogicalDoor { Closed = false } },
                    { (int)DoorLogicalId.Logic, new LogicalDoor { Closed = true } },
                });
            var openEvent = new OpenEvent((int)DoorLogicalId.CashBox, string.Empty);

            _accessor.HandleDoorOpenClose(openEvent);

            Assert.AreEqual(role, _accessor._role);
        }

        [Ignore]
        [TestMethod]
        public void HandleDoorOpenCloseWhenLogicDoorOpenTest()
        {
            CreateViews();
            string role = ApplicationConstants.DefaultRole;
            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.RolePropertyKey, role)).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuEnteredEvent>())).Verifiable();
            _meterManager.Setup(m => m.IsMeterProvided(role + "Access")).Returns(true);
            _meter.Setup(m => m.Increment(1)).Verifiable();
            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(_meter.Object);
            _door.Setup(m => m.LogicalDoors).Returns(
                new Dictionary<int, LogicalDoor>
                {
                    { (int)DoorLogicalId.Main, new LogicalDoor { Closed = false } },
                    { (int)DoorLogicalId.Logic, new LogicalDoor { Closed = false } },
                });
            var openEvent = new OpenEvent((int)DoorLogicalId.Logic, string.Empty);

            _accessor.HandleDoorOpenClose(openEvent);

            Assert.AreEqual(role, _accessor._role);
            _propertiesManager.Verify();
            _eventBus.Verify();
            _meterManager.Verify();
            _meter.Verify();
        }

        [Ignore]
        [TestMethod]
        public void HandleDoorOpenCloseWhenLogicDoorOpenSelectionNotEnabledTest()
        {
            CreateViews();
            _target.SelectedItem.IsEnabled = false;
            string role = ApplicationConstants.DefaultRole;
            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.RolePropertyKey, role)).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuEnteredEvent>())).Verifiable();
            _meterManager.Setup(m => m.IsMeterProvided(role + "Access")).Returns(true);
            _meter.Setup(m => m.Increment(1)).Verifiable();
            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(_meter.Object);
            _door.Setup(m => m.LogicalDoors).Returns(
                new Dictionary<int, LogicalDoor>
                {
                    { (int)DoorLogicalId.Main, new LogicalDoor { Closed = false } },
                    { (int)DoorLogicalId.Logic, new LogicalDoor { Closed = false } },
                });
            var openEvent = new OpenEvent((int)DoorLogicalId.Logic, string.Empty);

            _accessor.HandleDoorOpenClose(openEvent);

            Assert.AreEqual(role, _accessor._role);
            _propertiesManager.Verify();
            _eventBus.Verify();
            _meterManager.Verify();
            _meter.Verify();
        }

        [TestMethod]
        public void PrintButtonVisibleTest()
        {
            _target.PrintButtonEnabled = true;
            Assert.IsTrue(_target.PrintButtonEnabled);
        }

        [TestMethod]
        public void OperatorMenuLabelContentTest()
        {
            _target.OperatorMenuLabelContent = "Test";
            Assert.AreEqual("Test", _target.OperatorMenuLabelContent);
        }

        [TestMethod]
        public void PrintStatusTextTest()
        {
            _target.PrintStatusText = "Test";
            Assert.AreEqual("Test", _target.PrintStatusText);
        }

        [TestMethod]
        public void CreditBalanceVisibleTest()
        {
            _target.CreditBalanceVisible = true;
            Assert.IsTrue(_target.CreditBalanceVisible);
        }

        [TestMethod]
        public void ExitButtonFocusedTest()
        {
            _target.ExitButtonFocused = true;
            Assert.IsTrue(_target.ExitButtonFocused);
        }

        [TestMethod]
        public void PageTitleContentTest()
        {
            _target.PageTitleContent = "Test";
            Assert.AreEqual("Test", _target.PageTitleContent);
        }

        //[TestMethod]
        //public void PrintButtonContentTest()
        //{
        //    _target.PrintButtonContent = "Test";
        //    Assert.AreEqual("Test", _target.PrintButtonContent);
        //}

        [TestMethod]
        public void CreditBalanceContentTest()
        {
            _target.CreditBalanceContent = "Test";
            Assert.AreEqual("Test", _target.CreditBalanceContent);
        }

        [TestMethod]
        public void SoftwareVersionTest()
        {
            _accessor.SoftwareVersion = "Test";
            Assert.AreEqual("Test", _target.SoftwareVersion);
        }

        [TestMethod]
        public void DemoModeTextTest()
        {
            _accessor.DemoModeText = "Test";
            Assert.AreEqual("Test", _target.DemoModeText);
        }

        [TestMethod]
        public void TestPrintButtonStatusWithDisabledPrinter()
        {
            _printer.SetupGet(m => m.LogicalState).Returns(PrinterLogicalState.Disabled);
            _printerService.SetupGet(m => m.Enabled).Returns(false);

            _accessor.OnPrintButtonStatusUpdated(PrintButtonStatus.Print);
            _eventBus.Setup(m => m.Publish(It.IsAny<PrintButtonStatusEvent>())).Verifiable();

            Assert.IsFalse(_target.PrintButtonEnabled);
            Assert.IsFalse(_target.ShowCancelPrintButton);
        }

        [TestMethod]
        public void TestPrintButtonStatusWithEnabledPrinter()
        {
            _printer.SetupGet(m => m.LogicalState).Returns(PrinterLogicalState.Idle);
            _printer.SetupGet(m => m.CanPrint).Returns(true);
            _printerService.SetupGet(m => m.Enabled).Returns(true);
            _eventBus.Setup(m => m.Publish(It.IsAny<PrintButtonStatusEvent>())).Verifiable();
            _accessor.DataEmpty = false;
            _accessor.IsLoadingData = false;
            _accessor.MainPrintButtonEnabled = true;

            _accessor.OnPrintButtonStatusUpdated(PrintButtonStatus.Print);

            Assert.IsTrue(_target.PrintButtonEnabled);
            Assert.IsFalse(_target.ShowCancelPrintButton);
        }

        [TestMethod]
        public void TestToggleLanguageButton()
        {
            _accessor._secondaryCulture = new CultureInfo("fr-CA");
            _accessor.ShowToggleLanguageButton = true;
            _accessor.UpdateToggleLanguageButton();

            Assert.IsTrue(_target.ShowToggleLanguageButton);
            Assert.AreEqual("Français", _target.ToggleLanguageButtonText);
            Assert.AreEqual(MockLocalization.Localizer.Object.CurrentCulture, new CultureInfo("en-US"));
        }

        private void CreateViews()
        {
            var view = new TestLoader { IsEnabled = true };
            _target.MenuItems.Add(view);
            _target.SelectedItem = view;
        }
    }
}
