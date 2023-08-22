namespace Aristocrat.Monaco.Gaming.Tests.Tickets
{
    #region Using

    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Tickets;
    using Contracts;
    using Gaming.Tickets;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Test.Common;
    using Application.Contracts.Currency;

    #endregion

    /// <summary>
    ///     Summary description for AuditTicketCreatorTest
    /// </summary>
    [TestClass]
    public class AuditTicketCreatorTest
    {
        // Meter names
        private const string DollarsInMeter = "TotalIn";
        private const string DollarsOutMeter = "TotalOut";
        private const string CurrentCreditBalanceMeter = "System.CurrentBalance";
        private const string CurrentWagerMeter = "BetAmount";

        // The persisted field name for audit slip number
        private const string AuditSlipNumberFieldName = "AuditSlipNumber";
        private Mock<IBank> _bank;
        private Mock<IMeterManager> _meterManager;

        // Mock Services
        private Mock<IPersistentStorageManager> _persistenceManager;
        private Mock<IPropertiesManager> _propertiesManager;

        // Test target
        private AuditTicketCreator _target;
        private Mock<ITime> _time;
        private Mock<IGamePlayState> _gameState;
        private Mock<IGameHistory> _gameHistory;

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

            _persistenceManager = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);
            _bank = MoqServiceManager.CreateAndAddService<IBank>(MockBehavior.Strict);
            _time = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);
            _gameState = MoqServiceManager.CreateAndAddService<IGamePlayState>(MockBehavior.Strict);
            _gameHistory = MoqServiceManager.CreateAndAddService<IGameHistory>(MockBehavior.Strict);

            _target = new AuditTicketCreator();
            MockLocalization.Setup(MockBehavior.Strict);
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
        public void InitializeTestWithoutExistingBlock()
        {
            var accessor = new Mock<IPersistentStorageAccessor>(MockBehavior.Strict);
            var transaction = new Mock<IPersistentStorageTransaction>(MockBehavior.Strict);
            accessor.Setup(m => m.StartTransaction()).Returns(transaction.Object);
            transaction.SetupSet(mock => mock[AuditSlipNumberFieldName] = 1).Verifiable();
            transaction.Setup(m => m.Commit()).Verifiable();
            transaction.Setup(m => m.Dispose()).Verifiable();

            var blockName = typeof(AuditTicketCreator).ToString();
            _persistenceManager.Setup(m => m.BlockExists(blockName)).Returns(false);
            _persistenceManager.Setup(m => m.CreateBlock(PersistenceLevel.Critical, blockName, 1))
                .Returns(accessor.Object);

            _target.Initialize();
        }

        [TestMethod]
        public void InitializeTestWithExistingBlock()
        {
            Mock<IPersistentStorageAccessor> accessor = new Mock<IPersistentStorageAccessor>(MockBehavior.Strict);
            accessor.SetupGet(mock => mock[AuditSlipNumberFieldName]).Returns(10).Verifiable();

            string blockName = typeof(AuditTicketCreator).ToString();
            _persistenceManager.Setup(m => m.BlockExists(blockName)).Returns(true);
            _persistenceManager.Setup(m => m.GetBlock(blockName)).Returns(accessor.Object);

            _target.Initialize();

            accessor.VerifyAll();
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual("Audit Ticket Creator", _target.Name);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            var expectedType = typeof(IAuditTicketCreator);
            var actualTypes = _target.ServiceTypes;
            Assert.AreEqual(1, actualTypes.Count);
            Assert.AreEqual(expectedType, actualTypes.First());
        }

        [TestMethod]
        public void CreateAuditTicketTest()
        {
            string minorUnitSymbol = "c";
            RegionInfo region = new RegionInfo(CultureInfo.CurrentCulture.Name);
            CurrencyExtensions.Currency = new Currency(region.ISOCurrencySymbol, region, CultureInfo.CurrentCulture, minorUnitSymbol);
            CurrencyExtensions.SetCultureInfo(null, null, true, true, minorUnitSymbol);

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
            _propertiesManager.Setup(m => m.GetProperty(PropertyKey.CurrentBalance, It.IsAny<long>()))
                .Returns(1000L);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPageZoneOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);

            // Mock meters
            DateTime lastMasterClear = DateTime.UtcNow - TimeSpan.FromSeconds(10.0);
            DateTime lastPeriodClear = DateTime.UtcNow - TimeSpan.FromSeconds(5.0);
            long dollarsInMasterValue = 10000;
            long dollarsInPeriodValue = 100;
            long dollarsOutMasterValue = 4000;
            long dollarsOutPeriodValue = 40;
            long dollarsPlayedMasterValue = 20000;
            long dollarsPlayedPeriodValue = 200;
            long dollarsWonMasterValue = 15000;
            long dollarsWonPeriodValue = 150;
            _gameState.Setup(m => m.InGameRound).Returns(false);
            _gameHistory.Setup(m => m.CurrentLog.FinalWager).Returns(0L);
            _gameHistory.SetupGet(m => m.IsRecoveryNeeded).Returns(false);
            _meterManager.SetupGet(m => m.LastMasterClear).Returns(lastMasterClear);
            _meterManager.SetupGet(m => m.LastPeriodClear).Returns(lastPeriodClear);
            _meterManager.Setup(m => m.IsMeterProvided(DollarsInMeter)).Returns(true);
            _meterManager.Setup(m => m.IsMeterProvided(DollarsOutMeter)).Returns(true);
            _meterManager.Setup(m => m.IsMeterProvided(GamingMeters.WageredAmount)).Returns(true);
            _meterManager.Setup(m => m.IsMeterProvided(GamingMeters.TotalEgmPaidGameWonAmount)).Returns(true);
            _meterManager.Setup(m => m.IsMeterProvided(CurrentCreditBalanceMeter)).Returns(true);
            _meterManager.Setup(m => m.IsMeterProvided(CurrentWagerMeter)).Returns(false);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);

            MockMeter(DollarsInMeter, dollarsInMasterValue, dollarsInPeriodValue, true);
            MockMeter(DollarsOutMeter, dollarsOutMasterValue, dollarsOutPeriodValue, true);
            MockMeter(GamingMeters.WageredAmount, dollarsPlayedMasterValue, dollarsPlayedPeriodValue, true);
            MockMeter(GamingMeters.TotalEgmPaidGameWonAmount, dollarsWonMasterValue, dollarsWonPeriodValue, true);

            // Mock bank
            long currentBalanceValue = 1000;
            _bank.Setup(m => m.QueryBalance()).Returns(currentBalanceValue);
            MockMeter(CurrentCreditBalanceMeter, currentBalanceValue, currentBalanceValue, true);

            _time.Setup(t => t.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.Now);

            // Mock persistence
            Mock<IPersistentStorageAccessor> accessor = new Mock<IPersistentStorageAccessor>(MockBehavior.Strict);
            accessor.Setup(m => m.StartUpdate(true)).Returns(true).Verifiable();
            accessor.SetupSet(mock => mock[AuditSlipNumberFieldName] = 1).Verifiable();
            accessor.Setup(m => m.Commit()).Verifiable();
            string blockName = typeof(AuditTicketCreator).ToString();
            _persistenceManager.Setup(m => m.GetBlock(blockName)).Returns(accessor.Object);

            // Expected Ticket fields where the field value is checkable.
            CurrencyMeterClassification currencyClassification = new CurrencyMeterClassification();
            string door = "LOGIC DOOR";
            string expectedTitle = door + " " + "ACCESS";
            Dictionary<string, string> verifiableFields = new Dictionary<string, string>
            {
                { "ticket type", "text" },
                { "title", expectedTitle },
                { "left", "left" },
                { "center", "center" },
                { "right", "right" }
            };

            // Call target method
            Ticket ticket = _target.CreateAuditTicket(door);

            // Test fields for which the specific value can be known
            foreach (KeyValuePair<string, string> entry in verifiableFields)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.Value));
            }
        }

        private void MockMeter(string name, long masterValue, long periodValue, bool isCurrency)
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

            _meterManager.Setup(m => m.GetMeter(name)).Returns(meter.Object);
        }
    }
}