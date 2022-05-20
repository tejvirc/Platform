namespace Aristocrat.Monaco.Application.Tests.Tickets
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Tickets;
    using Contracts;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    #endregion

    /// <summary>
    ///     Summary description for IdentityTicketCreatorTest
    /// </summary>
    [TestClass]
    public class IdentityTicketCreatorTest
    {
        private Mock<IPropertiesManager> _propertiesManager;

        // Test target
        private IdentityTicketCreator _target;

        // Mock Services
        private Mock<ITime> _timeService;

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
            _timeService = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);

            _target = new IdentityTicketCreator();
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
            Assert.AreEqual("Identity Ticket Creator", _target.Name);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            var expectedType = typeof(IIdentityTicketCreator);
            var actualTypes = _target.ServiceTypes;
            Assert.AreEqual(1, actualTypes.Count);
            Assert.AreEqual(expectedType, actualTypes.First());
        }

        [TestMethod]
        public void CreateIdentityTicketTestWithBarcodes()
        {
            // Mock properties
            string ticketTextLine1 = "ticket line 1 text";
            string ticketTextLine2 = "ticket line 1 text";
            string ticketTextLine3 = "ticket line 1 text";
            string ticketTextLine4 = "ticket line 1 text";
            string partitionSignatureKey = NetworkInterfaceInfo.DefaultPhysicalAddress;
            string serialNumber = "123";
            uint machineId = 99;
            string zone = "Zone1";
            string bank = "Bank22";
            string position = "P7";
            string systemSpecification = "System Spec";
            string cabinetStyle = "H";
            string clientVersion = "12.3.2.1";
            string gameServerVersion = "5.4.3.1";
            double currencyMultiplier = 1.0;

            _propertiesManager.Setup(m => m.GetProperty("TicketProperty.TicketTextLine1", It.IsAny<object>()))
                .Returns(ticketTextLine1);
            _propertiesManager.Setup(m => m.GetProperty("TicketProperty.TicketTextLine2", It.IsAny<object>()))
                .Returns(ticketTextLine2);
            _propertiesManager.Setup(m => m.GetProperty("TicketProperty.TicketTextLine3", It.IsAny<object>()))
                .Returns(ticketTextLine3);
            _propertiesManager.Setup(m => m.GetProperty("TicketProperty.TicketTextLine4", It.IsAny<object>()))
                .Returns(ticketTextLine4);
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.SerialNumber", It.IsAny<string>()))
                .Returns(serialNumber);
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.MachineId", It.IsAny<uint>())).Returns(machineId);
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.ZoneName", It.IsAny<object>())).Returns(zone);
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.BankId", It.IsAny<object>())).Returns(bank);
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.EgmPosition", It.IsAny<object>())).Returns(position);
            _propertiesManager.Setup(m => m.GetProperty("GamePlay.GameSystemString", It.IsAny<object>()))
                .Returns(systemSpecification);
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.CabinetStyleString", It.IsAny<object>()))
                .Returns(cabinetStyle);
            _propertiesManager.Setup(m => m.GetProperty("System.Version", It.IsAny<object>())).Returns(clientVersion);
            _propertiesManager.Setup(m => m.GetProperty("GameServer.Version", It.IsAny<object>()))
                .Returns(gameServerVersion);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMultiplierKey, It.IsAny<object>()))
                .Returns(currencyMultiplier);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPageZoneOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPageBankOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPagePositionOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);

            // Mock time
            DateTime testTime = DateTime.UtcNow;
            _timeService.Setup(m => m.GetLocationTime(It.IsAny<DateTime>())).Returns(testTime);

            // Expected Ticket fields where the field value is checkable.
            Dictionary<string, string> verifiableFields = new Dictionary<string, string>
            {
                { "ticket type", "text" },
                { "title", "IDENTITY TICKET" },
                { "establishment name", ticketTextLine1 },
                { "header 1", ticketTextLine2 },
                { "header 2", ticketTextLine3 },
                { "header 3", ticketTextLine4 },
                { "datetime", testTime.ToString("ddd MMM dd\nHH:mm:ss yyyy") },
                { "datetime numbers", testTime.ToString() },
                { "sequence number", "00000000" },
                { "validation", partitionSignatureKey },
                { "serial id", serialNumber.ToString() },
                { "machine id", machineId.ToString() },
                { "zone", zone },
                { "bank", bank },
                { "position", position },
                { "client version", clientVersion },
                { "version", systemSpecification + " v " + gameServerVersion + cabinetStyle }
            };

            // Call target method
            Ticket ticket = _target.CreateIdentityTicket();

            // Test fields for which the specific value can be known
            foreach (KeyValuePair<string, string> entry in verifiableFields)
            {
                Assert.AreEqual(entry.Value, ticket[entry.Key]);
            }

            // Test remaining fields
            string legacyValidation = ticket["legacy validation"];
            Assert.IsNotNull(legacyValidation);
            long longValue = 0;
            Assert.IsTrue(long.TryParse(legacyValidation, out longValue));

            string copywrite = ticket["copyright"];
            Assert.IsNotNull(copywrite);
            Assert.IsTrue(copywrite.StartsWith("COPYRIGHT 1996 - "));
            Assert.IsTrue(copywrite.EndsWith(" VGT, INC."));
        }

        [TestMethod]
        public void CreateIdentityTicketTestWithoutBarcodes()
        {
            // Mock properties
            string ticketTextLine1 = "ticket line 1 text";
            string ticketTextLine2 = "ticket line 1 text";
            string ticketTextLine3 = "ticket line 1 text";
            string ticketTextLine4 = "ticket line 1 text";
            string partitionSignatureKey = NetworkInterfaceInfo.DefaultPhysicalAddress;
            string serialNumber = "123";
            uint machineId = 99;
            string zone = "Zone1";
            string bank = "Bank22";
            string position = "P7";
            string systemSpecification = "System Spec";
            string cabinetStyle = "H";
            string clientVersion = "12.3.2.1";
            string gameServerVersion = "5.4.3.1";
            double currencyMultiplier = 1.0;

            _propertiesManager.Setup(m => m.GetProperty("TicketProperty.TicketTextLine1", It.IsAny<object>()))
                .Returns(ticketTextLine1);
            _propertiesManager.Setup(m => m.GetProperty("TicketProperty.TicketTextLine2", It.IsAny<object>()))
                .Returns(ticketTextLine2);
            _propertiesManager.Setup(m => m.GetProperty("TicketProperty.TicketTextLine3", It.IsAny<object>()))
                .Returns(ticketTextLine3);
            _propertiesManager.Setup(m => m.GetProperty("TicketProperty.TicketTextLine4", It.IsAny<object>()))
                .Returns(ticketTextLine4);
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.SerialNumber", It.IsAny<string>()))
                .Returns(serialNumber);
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.MachineId", It.IsAny<uint>())).Returns(machineId);
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.ZoneName", It.IsAny<object>())).Returns(zone);
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.BankId", It.IsAny<object>())).Returns(bank);
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.EgmPosition", It.IsAny<object>())).Returns(position);
            _propertiesManager.Setup(m => m.GetProperty("GamePlay.GameSystemString", It.IsAny<object>()))
                .Returns(systemSpecification);
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.CabinetStyleString", It.IsAny<object>()))
                .Returns(cabinetStyle);
            _propertiesManager.Setup(m => m.GetProperty("System.Version", It.IsAny<object>())).Returns(clientVersion);
            _propertiesManager.Setup(m => m.GetProperty("GameServer.Version", It.IsAny<object>()))
                .Returns(gameServerVersion);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMultiplierKey, It.IsAny<object>()))
                .Returns(currencyMultiplier);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPageZoneOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPageBankOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPagePositionOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);

            // Mock time
            DateTime testTime = DateTime.UtcNow;
            _timeService.Setup(m => m.GetLocationTime(It.IsAny<DateTime>())).Returns(testTime);

            // Expected Ticket fields where the field value is checkable.
            Dictionary<string, string> verifiableFields = new Dictionary<string, string>
            {
                { "ticket type", "text" },
                { "title", "IDENTITY TICKET" },
                { "establishment name", ticketTextLine1 },
                { "header 1", ticketTextLine2 },
                { "header 2", ticketTextLine3 },
                { "header 3", ticketTextLine4 },
                { "datetime", testTime.ToString("ddd MMM dd\nHH:mm:ss yyyy") },
                { "datetime numbers", testTime.ToString() },
                { "sequence number", "00000000" },
                { "validation", partitionSignatureKey },
                { "serial id", serialNumber.ToString() },
                { "machine id", machineId.ToString() },
                { "zone", zone },
                { "bank", bank },
                { "position", position },
                { "client version", clientVersion },
                { "version", systemSpecification + " v " + gameServerVersion + cabinetStyle }
            };

            // Call target method
            Ticket ticket = _target.CreateIdentityTicket();

            // Test fields for which the specific value can be known
            foreach (KeyValuePair<string, string> entry in verifiableFields)
            {
                Assert.AreEqual(entry.Value, ticket[entry.Key]);
            }

            // Test remaining fields
            string legacyValidation = ticket["legacy validation"];
            Assert.IsNotNull(legacyValidation);
            long longValue = 0;
            Assert.IsTrue(long.TryParse(legacyValidation, out longValue));

            Assert.IsNull(ticket["human readable barcode 1"]);
            Assert.IsNull(ticket["human readable barcode 2"]);
            Assert.IsNull(ticket["legacy barcode 1"]);
            Assert.IsNull(ticket["legacy barcode 2"]);

            string copywrite = ticket["copyright"];
            Assert.IsNotNull(copywrite);
            Assert.IsTrue(copywrite.StartsWith("COPYRIGHT 1996 - "));
            Assert.IsTrue(copywrite.EndsWith(" VGT, INC."));
        }
    }
}