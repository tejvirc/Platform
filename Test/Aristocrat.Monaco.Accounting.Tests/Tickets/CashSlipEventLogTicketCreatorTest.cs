namespace Aristocrat.Monaco.Accounting.Tests.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Accounting.Tickets;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Application.Contracts.Currency;
    using Contracts.Tickets;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SharedDevice;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Summary description for CashSlipEventLogTicketCreatorTest
    /// </summary>
    [TestClass]
    public class CashSlipEventLogTicketCreatorTest
    {
        /// <summary>
        ///     Time to use for testing.
        /// </summary>
        private static readonly DateTime PrintTimestamp = new DateTime(2012, 7, 22, 19, 45, 0, 0);

        private Mock<IPrinter> _printerMock;

        // Mock Services
        private Mock<IPropertiesManager> _propertiesManager;

        // Test target
        private CashSlipEventLogTicketCreator _target;
        private Mock<ITime> _time;
        private Mock<IIO> _iio;

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _time = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict, true);
            _time.Setup(mock => mock.GetLocationTime(It.IsAny<DateTime>())).Returns(PrintTimestamp);
            _iio = MoqServiceManager.CreateAndAddService<IIO>(MockBehavior.Loose);

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.AuditTicketEventsPerPage, 6))
                .Returns(6)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.AuditTicketLineLimit, 36))
                .Returns(36)
                .Verifiable();

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.LocalizationOperatorTicketDateFormat, ApplicationConstants.DefaultDateFormat))
                .Returns(ApplicationConstants.DefaultDateFormat)
                .Verifiable();

            _target = new CashSlipEventLogTicketCreator();

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
            Assert.AreEqual("Cash Slip Event Log Ticket Creator", _target.Name);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            var expectedType = typeof(ICashSlipEventLogTicketCreator);
            var actualTypes = _target.ServiceTypes;
            Assert.AreEqual(1, actualTypes.Count);
            Assert.AreEqual(expectedType, actualTypes.First());
        }

        [TestMethod]
        public void CreateBillEventLogTicketTest()
        {
            string Retailer = "RETAILER";
            string RetailerAddress = "RETAILER ADDRESS";
            string License = "6000";
            string CabinetSerial = "56789";
            string SystemVersion = "98765";
            uint SystemId = 9;

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

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPageZoneOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);

            _iio.Setup(i => i.DeviceConfiguration).Returns(new Device { Manufacturer = "Manufacturer", Model = "Model" });

            _time = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);
            _time.Setup(t => t.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.Now);

            _printerMock = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);
            _printerMock.Setup(mock => mock.PaperState).Returns(PaperStates.Full);
            _printerMock.Setup(mock => mock.GetCharactersPerLine(false, 0)).Returns(36);

            // Mock more events than will fit on a page
            const int rowsPerPage = 10;
            int numRecordsToAdd = rowsPerPage + 2;
            List<CashSlipEventLogRecord> records = new List<CashSlipEventLogRecord>();
            for (int i = 0; i < numRecordsToAdd; ++i)
            {
                records.Add(
                    new CashSlipEventLogRecord
                    {
                        TimeStamp = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                        Amount = i.FormattedCurrencyString(),
                    });
            }

            // Page argument
            int page = 1;

            // Expected Ticket fields where the field value is checkable.
            Dictionary<string, string> verifiableFields = new Dictionary<string, string>
            {
                { "ticket type", "text" },
                { "title", "CASH SLIP EVENT LOG" },
                { "left", "left" },
                { "center", "center" },
                { "right", "right" }
            };

            // Call target method
            Ticket ticket = _target.Create(page, records);

            // Test fields for which the specific value can be known
            foreach (KeyValuePair<string, string> entry in verifiableFields)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.Value));
            }

            _propertiesManager.Verify();
        }
    }
}