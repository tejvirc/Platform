namespace Aristocrat.Monaco.Application.Tests.Tickets
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Application.Tickets;
    using Contracts.Currency;
    using Hardware.Contracts.Printer;
    using Contracts;
    using Contracts.Extensions;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    #endregion

    /// <summary>
    ///     Summary description for MetersTicketCreatorTest
    /// </summary>
    [TestClass]
    public class MetersTicketCreatorTest
    {
        private Mock<IMeterManager> _meterManager;

        // Mock Services
        private Mock<IPropertiesManager> _propertiesManager;

        // Test target
        private MetersTicketCreator _target;
        private Mock<ITime> _time;
        private Mock<IPrinter> _printerMock;

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
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.LocalizationOperatorTicketDateFormat, ApplicationConstants.DefaultDateFormat))
                .Returns(ApplicationConstants.DefaultDateFormat)
                .Verifiable();

            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);
            _time = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);
            _time.Setup(t => t.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.Now);

            // set up currency
            string minorUnitSymbol = "c";
            string cultureName = "en-US";
            CultureInfo culture = new CultureInfo(cultureName);

            RegionInfo region = new RegionInfo(cultureName);
            CurrencyExtensions.Currency = new Currency(region.ISOCurrencySymbol, region, culture, minorUnitSymbol);
            CurrencyExtensions.SetCultureInfo(region.ISOCurrencySymbol, culture, null, null, true, true, minorUnitSymbol);

            _printerMock = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);
            _printerMock.Setup(mock => mock.PaperState).Returns(PaperStates.Full);
            _printerMock.Setup(mock => mock.GetCharactersPerLine(false, 0)).Returns(36);

            _target = new MetersTicketCreator();
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
            Assert.AreEqual("Meters Ticket Creator", _target.Name);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            var expectedType = typeof(IMetersTicketCreator);
            var actualTypes = _target.ServiceTypes;
            Assert.AreEqual(1, actualTypes.Count);
            Assert.AreEqual(expectedType, actualTypes.First());
        }

        [TestMethod]
        public void CreateEgmMetersTicketTestForMasterMeters()
        {
            // Mock properties
            string retailerName = "Test Retailer";
            string retailerId = "Test Retailer ID";
            string jurisdiction = "Test Jurisdiction";
            string serialNumber = "555";
            string version = "5.4.3.2";
            double multiplier = 1.0;

            _propertiesManager.Setup(m => m.GetProperty(PropertyKey.TicketTextLine1, It.IsAny<object>()))
                .Returns(retailerName);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.Zone, It.IsAny<object>())).Returns(retailerId);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.JurisdictionKey, It.IsAny<object>()))
                .Returns(jurisdiction);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<string>()))
                .Returns(serialNumber);
            _propertiesManager.Setup(m => m.GetProperty(KernelConstants.SystemVersion, It.IsAny<object>()))
                .Returns(version);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMultiplierKey, It.IsAny<object>())).Returns(multiplier);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OccurrenceMeterRolloverText, It.IsAny<int>()))
                .Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);

            // Mock meter manager
            DateTime lastClear = DateTime.UtcNow - TimeSpan.FromSeconds(10.0);
            _meterManager.SetupGet(m => m.LastMasterClear).Returns(lastClear);

            // Mock meters
            List<Tuple<IMeter, string>> meters = new List<Tuple<IMeter, string>>();
            meters.Add(new Tuple<IMeter, string>(CreateMockMeter("TestMeter1", 10, 1, true).Object, "Print Name 1"));
            meters.Add(new Tuple<IMeter, string>(CreateMockMeter("TestMeter2", 20, 2, true).Object, "Print Name 2"));
            meters.Add(new Tuple<IMeter, string>(CreateMockMeter("TestMeter3", 30, 3, false).Object, "Print Name 3"));
            meters.Add(new Tuple<IMeter, string>(CreateMockMeter("TestMeter4", 40, 4, false).Object, "Print Name 4"));

            // Expected Ticket fields where the field value is checkable.
            Dictionary<string, string> verifiableFields = new Dictionary<string, string>
            {
                { "ticket type", "text" },
                { "title", "MASTER METER LOG" },
                { "left", "left" },
                { "center", "center" },
                { "right", "right" }
            };

            Ticket ticket = _target.CreateEgmMetersTicket(meters, true);

            // Test fields for which the specific value can be known
            foreach (KeyValuePair<string, string> entry in verifiableFields)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.Value));
            }
        }

        [TestMethod]
        public void CreateEgmMetersTicketTestForPeriodMeters()
        {
            // Mock properties
            string retailerName = "Test Retailer";
            string retailerId = "Test Retailer ID";
            string jurisdiction = "Test Jurisdiction";
            string serialNumber = "555";
            string version = "5.4.3.2";
            double multiplier = 1.0;

            _propertiesManager.Setup(m => m.GetProperty(PropertyKey.TicketTextLine1, It.IsAny<object>()))
                .Returns(retailerName);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.Zone, It.IsAny<object>())).Returns(retailerId);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.JurisdictionKey, It.IsAny<object>()))
                .Returns(jurisdiction);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<string>()))
                .Returns(serialNumber);
            _propertiesManager.Setup(m => m.GetProperty(KernelConstants.SystemVersion, It.IsAny<object>()))
                .Returns(version);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMultiplierKey, It.IsAny<object>())).Returns(multiplier);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OccurrenceMeterRolloverText, It.IsAny<int>()))
                .Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);

            // Mock meter manager
            DateTime lastClear = DateTime.UtcNow - TimeSpan.FromSeconds(10.0);
            _meterManager.SetupGet(m => m.LastPeriodClear).Returns(lastClear);

            // Mock meters
            List<Tuple<IMeter, string>> meters = new List<Tuple<IMeter, string>>();
            meters.Add(new Tuple<IMeter, string>(CreateMockMeter("TestMeter1", 10, 1, true).Object, "Print Name 1"));
            meters.Add(new Tuple<IMeter, string>(CreateMockMeter("TestMeter2", 20, 2, true).Object, "Print Name 2"));
            meters.Add(new Tuple<IMeter, string>(CreateMockMeter("TestMeter3", 30, 3, false).Object, "Print Name 3"));
            meters.Add(new Tuple<IMeter, string>(CreateMockMeter("TestMeter4", 40, 4, false).Object, "Print Name 4"));

            // Expected Ticket fields where the field value is checkable.
            Dictionary<string, string> verifiableFields = new Dictionary<string, string>
            {
                { "ticket type", "text" },
                { "title", "PERIOD METER LOG" },
                { "left", "left" },
                { "center", "center" },
                { "right", "right" }
            };

            // Call target method
            Ticket ticket = _target.CreateEgmMetersTicket(meters, false);

            // Test fields for which the specific value can be known
            foreach (KeyValuePair<string, string> entry in verifiableFields)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.Value));
            }
        }

        private Mock<IMeter> CreateMockMeter(string name, long masterValue, long periodValue, bool isCurrency)
        {
            Mock<IMeter> meter = new Mock<IMeter>(MockBehavior.Strict);
            meter.SetupGet(m => m.Name).Returns(name);
            meter.SetupGet(m => m.Lifetime).Returns(masterValue);
            meter.SetupGet(m => m.Period).Returns(periodValue);
            if (isCurrency)
            {
                meter.SetupGet(m => m.Classification).Returns(new CurrencyMeterClassification());
            }
            else
            {
                meter.SetupGet(m => m.Classification).Returns(new OccurrenceMeterClassification());
            }

            return meter;
        }
    }
}