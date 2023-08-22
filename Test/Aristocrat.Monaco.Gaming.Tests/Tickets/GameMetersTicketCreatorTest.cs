namespace Aristocrat.Monaco.Gaming.Tests.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.MeterPage;
    using Application.Contracts.Currency;
    using Contracts;
    using Contracts.Meters;
    using Contracts.Tickets;
    using Gaming.Tickets;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class GameMetersTicketCreatorTest
    {
        private Mock<IPropertiesManager> _propertiesManager;

        private GameMetersTicketCreator _target;
        private Mock<ITime> _time;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _time = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);
            _time.Setup(t => t.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.Now);

            _target = new GameMetersTicketCreator();

            MockLocalization.Setup(MockBehavior.Strict);
            string minorUnitSymbol = "c";
            string cultureName = "en-US";
            CultureInfo culture = new CultureInfo(cultureName);

            RegionInfo region = new RegionInfo(cultureName);
            CurrencyExtensions.Currency = new Currency(region.ISOCurrencySymbol, region, culture, minorUnitSymbol);
            CurrencyExtensions.SetCultureInfo(null, null, true, true, minorUnitSymbol);
        }

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
            Assert.AreEqual("Game Meters Ticket Creator", _target.Name);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            var expectedType = typeof(IGameMetersTicketCreator);
            var actualTypes = _target.ServiceTypes;
            Assert.AreEqual(1, actualTypes.Count);
            Assert.AreEqual(expectedType, actualTypes.First());
        }

        [TestMethod]
        public void CreateGameMetersTicketTestForMasterMeters()
        {
            // Mock properties
            var retailerName = "Test Retailer";
            var retailerId = "Test Retailer ID";
            var jurisdiction = "Test Jurisdiction";
            var serialNumber = "555";
            var version = "5.4.3.2";
            var multiplier = 1.0;
            var gameName = "A Test Game";
            var gameId = 3;

            var games = new List<IGameDetail>
            {
                CreateMockGameDetail(gameId + 1, "A Second Game").Object,
                CreateMockGameDetail(gameId, gameName).Object,
                CreateMockGameDetail(gameId + 2, "A Third Game").Object
            };

            MoqServiceManager.CreateAndAddService<IGameMeterManager>(MockBehavior.Strict);

            _propertiesManager.Setup(m => m.GetProperty(PropertyKey.TicketTextLine1, It.IsAny<object>()))
                .Returns(retailerName);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.Zone, It.IsAny<object>())).Returns(retailerId);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.JurisdictionKey, It.IsAny<object>()))
                .Returns(jurisdiction);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<object>()))
                .Returns(serialNumber);
            _propertiesManager.Setup(m => m.GetProperty(KernelConstants.SystemVersion, It.IsAny<object>()))
                .Returns(version);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMultiplierKey, It.IsAny<object>())).Returns(multiplier);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.AllGames, It.IsAny<object>())).Returns(games);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OccurrenceMeterRolloverText, It.IsAny<int>()))
                .Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.TicketModeAuditKey, It.IsAny<TicketModeAuditBehavior>()))
                .Returns(TicketModeAuditBehavior.Audit);

            // Mock meters
            var meterNodes = new List<MeterNode>();

            // Expected Ticket fields where the field value is checkable.
            var verifiableFields = new Dictionary<string, string>
            {
                { "ticket type", "text" },
                { "title", "MASTER GAME METERS" },
                { "left", "left" },
                { "center", "center" },
                { "right", "right" }
            };

            _target.Initialize();

            var ticket = _target.CreateGameMetersTicket(gameId, meterNodes, true, false);

            // Test fields for which the specific value can be known
            foreach (var entry in verifiableFields)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.Value));
            }
        }

        [TestMethod]
        public void CreateGameMetersTicketTestForPeriodMeters()
        {
            // Mock properties
            var retailerName = "Test Retailer";
            var retailerId = "Test Retailer ID";
            var jurisdiction = "Test Jurisdiction";
            var serialNumber = "555";
            var version = "5.4.3.2";
            var multiplier = 1.0;
            var gameName = "A Test Game";
            var gameId = 3;

            var games = new List<IGameDetail>
            {
                CreateMockGameDetail(gameId + 1, "A Second Game").Object,
                CreateMockGameDetail(gameId, gameName).Object,
                CreateMockGameDetail(gameId + 2, "A Third Game").Object
            };

            MoqServiceManager.CreateAndAddService<IGameMeterManager>(MockBehavior.Strict);

            _propertiesManager.Setup(m => m.GetProperty(PropertyKey.TicketTextLine1, It.IsAny<object>()))
                .Returns(retailerName);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.Zone, It.IsAny<object>())).Returns(retailerId);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.JurisdictionKey, It.IsAny<object>()))
                .Returns(jurisdiction);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<object>()))
                .Returns(serialNumber);
            _propertiesManager.Setup(m => m.GetProperty(KernelConstants.SystemVersion, It.IsAny<object>()))
                .Returns(version);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMultiplierKey, It.IsAny<object>())).Returns(multiplier);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.AllGames, It.IsAny<object>())).Returns(games);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OccurrenceMeterRolloverText, It.IsAny<int>()))
                .Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.TicketModeAuditKey, It.IsAny<TicketModeAuditBehavior>()))
                .Returns(TicketModeAuditBehavior.Audit);

            // Mock meters
            var meterNodes = new List<MeterNode>();

            // Expected Ticket fields where the field value is checkable.
            var verifiableFields = new Dictionary<string, string>
            {
                { "ticket type", "text" },
                { "title", "PERIOD GAME METERS" },
                { "left", "left" },
                { "center", "center" },
                { "right", "right" }
            };

            _target.Initialize();

            var ticket = _target.CreateGameMetersTicket(gameId, meterNodes, false, false);

            // Test fields for which the specific value can be known
            foreach (var entry in verifiableFields)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.Value));
            }
        }

        private Mock<IGameDetail> CreateMockGameDetail(int id, string name)
        {
            var gameDetail = new Mock<IGameDetail>(MockBehavior.Strict);
            gameDetail.SetupGet(m => m.Id).Returns(id);
            gameDetail.SetupGet(m => m.ThemeName).Returns(name);

            return gameDetail;
        }

        private Mock<IMeter> CreateMockMeter(string name, long masterValue, long periodValue, bool isCurrency)
        {
            var meter = new Mock<IMeter>(MockBehavior.Strict);
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
