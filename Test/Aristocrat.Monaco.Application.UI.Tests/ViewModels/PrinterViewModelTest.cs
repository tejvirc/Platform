namespace Aristocrat.Monaco.Application.UI.Tests.ViewModels
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Contracts;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using UI.ViewModels;

    /// <summary>
    ///     Contains the unit tests for the PrinterViewModel class
    /// </summary>
    [TestClass]
    public class PrinterViewModelTest
    {
        private PrinterViewModel _target;
        private TestPrinterViewModel _target2;
        private Mock<IPropertiesManager> _propertiesManager;

        private class TestPrinterViewModel : PrinterViewModel
        {
            public void TestEventHandlerStop(bool stop)
            {
                base.EventHandlerStopped = stop;
            }

            public void TestUpdateScreen()
            {
                base.UpdateScreen();
            }
        }

        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.LocalizationPlayerAvailable, new[] { CultureInfo.CurrentCulture.Name })).Returns(new string[] { CultureInfo.CurrentCulture.Name })
                .Verifiable();

            var playerTicketSelectionArrayEntry = new PlayerTicketSelectionArrayEntry[]
            {
                new PlayerTicketSelectionArrayEntry
                {
                    Locale=CultureInfo.CurrentCulture.Name,
                    CurrencyValueLocale=CultureInfo.CurrentCulture.Name,
                    CurrencyWordsLocale=CultureInfo.CurrentCulture.Name
                }
            };

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.LocalizationPlayerTicketSelectable, It.IsAny<PlayerTicketSelectionArrayEntry[]>()))
                .Returns(playerTicketSelectionArrayEntry)
                .Verifiable();
            
                _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.LocalizationPlayerTicketOverride, true)).Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.LocalizationPlayerTicketOverride, false))
                .Returns(false).Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.LocalizationPlayerTicketDefault, String.Empty))
                .Returns(string.Empty).Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.LocalizationPlayerTicketLocale, String.Empty))
                .Returns(CultureInfo.CurrentCulture.Name).Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.LocalizationPlayerTicketLanguageSettingVisible, false))
                .Returns(false).Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.LocalizationPlayerTicketLanguageSettingShowCheckBox, false))
                .Returns(false).Verifiable();

            TicketCurrencyExtensions.PlayerTicketLocale = "en-US";

            _target2 = new TestPrinterViewModel();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();

            if (AddinManager.IsInitialized)
            {
                AddinManager.Shutdown();
            }
        }

        [TestMethod]
        public void ConstructorTest()
        {
            _target = new PrinterViewModel();
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void UpdateScreenEventHandlerStoppedTest()
        {
            _target2.TestEventHandlerStop(true);
            _target2.TestUpdateScreen();

            // verify we didn't get to the line checking for a printer
            MoqServiceManager.Instance.Verify(m => m.IsServiceAvailable<IPrinter>(), Times.Never);
        }

        [TestMethod]
        public void UpdateScreenPrinterServiceAvailableTest()
        {
            MoqServiceManager.Instance.Setup(m => m.IsServiceAvailable<IPrinter>()).Returns(false);
            _target2.TestEventHandlerStop(false);
            _target2.TestUpdateScreen();

            // verify we only tested once
            MoqServiceManager.Instance.Verify(m => m.IsServiceAvailable<IPrinter>(), Times.Once);
        }

        [TestMethod]
        public void UpdateScreenPrinterDeviceConfigurationNullTest()
        {
            var printer = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);
            printer.Setup(m => m.DeviceConfiguration).Returns((IDevice)null).Verifiable();

            _target2.TestEventHandlerStop(false);
            _target2.TestUpdateScreen();

            // verify we only tested once
            printer.Verify(m => m.DeviceConfiguration, Times.Once);
            printer.Verify(m => m.DeviceConfiguration.Protocol, Times.Never);
        }

        [TestMethod]
        public void UpdateScreenPrinterDeviceConfigurationProtocolNullTest()
        {
            var printer = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);
            var device = new Mock<IDevice>(MockBehavior.Strict);
            device.Setup(m => m.Protocol).Returns((string)null).Verifiable();
            printer.Setup(m => m.DeviceConfiguration).Returns(device.Object).Verifiable();

            _target2.TestEventHandlerStop(false);
            _target2.TestUpdateScreen();

            // verify we only tested once
            printer.Verify(m => m.DeviceConfiguration, Times.AtMost(2));
            device.Verify(m => m.Protocol, Times.Once);
        }

        [TestMethod]
        public void UpdateScreenShowDiagnosticsTest()
        {
            var eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            var printer = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);
            var device = new Mock<IDevice>(MockBehavior.Strict);
            device.Setup(m => m.Protocol).Returns("GDS").Verifiable();
            printer.Setup(m => m.DeviceConfiguration).Returns(device.Object).Verifiable();
            printer.Setup(m => m.LogicalState).Returns(PrinterLogicalState.Idle);
            eventBus.Setup(m => m.Publish(It.IsAny<BaseEvent>()));

            _target2.TestEventHandlerStop(false);
            _target2.DiagnosticsEnabled = true;
            _target2.TestUpdateScreen();

            Assert.IsTrue(_target2.ShowDiagnostics);
        }
    }
}
