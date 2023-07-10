namespace Aristocrat.Monaco.Accounting.Tests.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Accounting.Tickets;
    using Application.Contracts;
    using Application.Contracts.Currency;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Tickets;
    using Hardware.Contracts;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SerialPorts;
    using Hardware.Contracts.SharedDevice;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Summary description for BillEventLogTicketCreatorTest
    /// </summary>
    [TestClass]
    public class BillEventLogTicketCreatorTest
    {
        private const string Retailer = "RETAILER";
        private const string RetailerAddress = "RETAILER ADDRESS";
        private const string License = "6000";
        private const string CabinetSerial = "56789";
        private const string SystemVersion = "98765";
        private const uint SystemId = 9;

        //private Mock<ILocaleProvider> _localizer;
        private Mock<IPrinter> _printerMock;

        // Mock Services
        private Mock<IPropertiesManager> _propertiesManager;

        // Test target
        private BillEventLogTicketCreator _target;
        private Mock<ITime> _time;
        private Mock<IIO> _iio;
        private Mock<IOSService> _os;

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(mock => mock.GetProperty(PropertyKey.TicketTextLine1, It.IsAny<string>()))
                .Returns(Retailer)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(PropertyKey.TicketTextLine2, It.IsAny<string>()))
                .Returns("Test Casino city")
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(PropertyKey.TicketTextLine3, It.IsAny<string>()))
                .Returns(RetailerAddress)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.Zone, It.IsAny<string>()))
                .Returns(License)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<string>()))
                .Returns(CabinetSerial)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.MachineId, It.IsAny<uint>()))
                .Returns(SystemId)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(KernelConstants.SystemVersion, It.IsAny<string>()))
                .Returns(SystemVersion)
                .Verifiable();

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.AuditTicketEventsPerPage, 6))
                .Returns(6)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.AuditTicketLineLimit, 36))
                .Returns(36)
                .Verifiable();

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.LocalizationOperatorTicketDateFormat, ApplicationConstants.DefaultDateFormat))
                .Returns(ApplicationConstants.DefaultDateFormat)
                .Verifiable();

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPageZoneOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.LocalizationOperatorTicketLanguageSettingOperatorOverride, It.IsAny<object>()))
                .Returns(false);

            _iio = MoqServiceManager.CreateAndAddService<IIO>(MockBehavior.Loose);
            var serialService = new Mock<ISerialPortsService>();
            _iio.Setup(i => i.DeviceConfiguration).Returns(new Device(serialService.Object) { Manufacturer = "Manufacturer", Model = "Model" });

            _time = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);
            _time.Setup(t => t.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.Now);

            _os = MoqServiceManager.CreateAndAddService<IOSService>(MockBehavior.Strict, true);
            _os.Setup(mock => mock.OsImageVersion).Returns(new Version());

            _printerMock = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);
            _printerMock.Setup(mock => mock.PaperState).Returns(PaperStates.Full);
            _printerMock.Setup(mock => mock.GetCharactersPerLine(false, 0)).Returns(36);

            _target = new BillEventLogTicketCreator();

            // set up currency
            string minorUnitSymbol = "c";
            string cultureName = "en-US";
            CultureInfo culture = new CultureInfo(cultureName);

            RegionInfo region = new RegionInfo(cultureName);
            CurrencyExtensions.Currency = new Currency(region.ISOCurrencySymbol, region, culture, minorUnitSymbol);
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.AreNotEqual(null, _target);
        }

        [TestMethod]
        public void InitializeTest()
        {
            _target.Initialize();
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual("Bill Event Log Ticket Creator", _target.Name);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            var expectedType = typeof(IBillEventLogTicketCreator);
            var actualTypes = _target.ServiceTypes;
            Assert.AreEqual(1, actualTypes.Count);
            Assert.AreEqual(expectedType, actualTypes.First());
        }

        [TestMethod]
        public void CreateBillEventLogTicketTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMultiplierKey, 1d)).Returns(1d);

            // Mock more bill events than will fit on a page
            const int rowsPerPage = 7;
            int numBillEventsToAdd = rowsPerPage + 2;
            List<BillTransaction> billEvents = new List<BillTransaction>();
            for (int i = 0; i < numBillEventsToAdd; ++i)
            {
                billEvents.Add(
                    new BillTransaction("USD".ToCharArray(0, 3), 1, DateTime.UtcNow, i + 1)
                    {
                        Accepted = DateTime.MinValue + TimeSpan.FromSeconds(i)
                    });
            }

            // Expected Ticket fields where the field value is checkable.
            Dictionary<string, string> verifiableFields = new Dictionary<string, string>
            {
                { "ticket type", "text" },
                { "title", "BILL EVENT LOG" },
                { "left", "left" },
                { "center", "center" },
                { "right", "right" }
            };

            // Call target method
            Ticket ticket = _target.Create(billEvents);

            // Test fields for which the specific value can be known
            foreach (KeyValuePair<string, string> entry in verifiableFields)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.Value));
            }

            _propertiesManager.Verify();
        }
    }
}