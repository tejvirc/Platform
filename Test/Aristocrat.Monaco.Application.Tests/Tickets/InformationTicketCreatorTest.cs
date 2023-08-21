namespace Aristocrat.Monaco.Application.Tests.Tickets
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Tickets;
    using Contracts;
    using Contracts.Localization;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    #endregion

    /// <summary>
    ///     Summary description for InformationTicketCreatorTest
    /// </summary>
    [TestClass]
    public class InformationTicketCreatorTest
    {
        // Mock Services
        private Mock<IPropertiesManager> _propertiesManager;

        // Test target
        private InformationTicketCreator _target;
        private Mock<ITime> _time;

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
            _time = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);
            _time.Setup(t => t.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.Now);

            _target = new InformationTicketCreator();
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
            Assert.AreEqual("Information Ticket Creator", _target.Name);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            var expectedType = typeof(IInformationTicketCreator);
            var actualTypes = _target.ServiceTypes;
            Assert.AreEqual(1, actualTypes.Count);
            Assert.AreEqual(expectedType, actualTypes.First());
        }

        [TestMethod]
        public void CreateInformationTicketTest()
        {
            // Mock properties
            string serialNumber = "123";
            uint machineId = 99;
            string zone = "Zone1";
            string bank = "Bank22";
            string position = "P7";

            _propertiesManager.Setup(m => m.GetProperty("Cabinet.SerialNumber", It.IsAny<string>()))
                .Returns(serialNumber);
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.MachineId", It.IsAny<object>())).Returns(machineId);
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.ZoneName", It.IsAny<object>())).Returns(zone);
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.BankId", It.IsAny<object>())).Returns(bank);
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.EgmPosition", It.IsAny<object>())).Returns(position);
            _propertiesManager.Setup(m => m.GetProperty("ConfigWizard.IdentityPage.ZoneOverride", It.IsAny<object>())).Returns(null);
            _propertiesManager.Setup(m => m.GetProperty("ConfigWizard.IdentityPage.BankOverride", It.IsAny<object>())).Returns(null);
            _propertiesManager.Setup(m => m.GetProperty("ConfigWizard.IdentityPage.PositionOverride", It.IsAny<object>())).Returns(null);

            // Arguments
            string titleText = "a test title";
            string bodyText = "some test text for the ticket body";

            // Expected Ticket fields where the field value is checkable.
            Dictionary<string, string> verifiableFields = new Dictionary<string, string>
            {
                { "ticket type", "text" },
                { "title", titleText }
            };

            // Call target method
            Ticket ticket = _target.CreateInformationTicket(titleText, bodyText);

            // Test fields for which the specific value can be known
            foreach (KeyValuePair<string, string> entry in verifiableFields)
            {
                Assert.AreEqual(entry.Value, ticket[entry.Key]);
            }

            // Test remaining fields
            string copyright = ticket["copyright"];
            Assert.IsNotNull(copyright);
            Assert.IsTrue(copyright.StartsWith("COPYRIGHT " + (DateTime.UtcNow).Year + " Aristocrat, INC."));
            Assert.IsTrue(copyright.EndsWith(" INC."));
        }
    }
}