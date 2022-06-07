namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.Globalization;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Handpay;
    using Handpay;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     This is a test class for HandpayTicketProxy and is intended
    ///     to contain all HandpayTicketProxy Unit Tests
    /// </summary>
    [TestClass]
    public sealed class HandpayTicketProxyTest
    {
        private const string LocaleDateFormat = "yyyy-MM-dd";

        private static readonly DateTime TransactionTimestamp = new DateTime(2012, 7, 22, 19, 45, 0);

        private Mock<IPrinter> _printer;
        private Mock<IPropertiesManager> _propertiesManager;
        private HandpayTicketProxy _target;
        private Mock<ITime> _time;

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _time = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict, true);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict, true);
            _printer = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict, true);
            CurrencyExtensions.SetCultureInfo(CultureInfo.CurrentCulture, null, null, true, true, "c");
            _target = new HandpayTicketProxy();

            TicketCurrencyExtensions.PlayerTicketLocale = "en-US";
            TicketCurrencyExtensions.SetCultureInfo(
                "en-US",
                new CultureInfo("en-US"),
                new CultureInfo("en-US"),
                "Cent",
                "Cents",
                true,
                true,
                "c"
            );
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
            _propertiesManager = null;
            _time = null;
            _printer = null;
        }

        /// <summary>A test for TransactionTypes</summary>
        [TestMethod]
        public void TransactionTypesTest()
        {
            var actual = _target.TransactionTypes;
            Assert.IsTrue(actual.Contains(typeof(HandpayTransaction)));
            Assert.AreEqual(1, actual.Count);
        }

        /// <summary>A test for CreateTicket where the transaction type is valid</summary>
        [TestMethod]
        public void CreateTicketTest()
        {
            // Set up a fake printer (needed for CanceledCreditsTicketsCreator).
            SetupMockPropertiesManager();
            _time.Setup(mock => mock.GetLocationTime(TransactionTimestamp)).Returns(TransactionTimestamp).Verifiable();

            // This is needed by CurrencyToWords - no need to verify.
            _printer.Setup(mock => mock.GetCharactersPerLine(true, 0)).Returns(80).Verifiable();

            // create a random transaction for the test
            var transaction = new HandpayTransaction(
                0,
                TransactionTimestamp,
                1045000,
                2144000,
                0,
                123,
                HandpayType.CancelCredit,
                true,
                Guid.NewGuid()) { LogSequence = 987 };
            transaction.HostOnline = true;
            var actual = _target.CreateTicket(transaction);

            var expected = CreateTestReprintTicket();
            expected["title"] = $"{expected["title"]} REPRINT";
            CheckTickets(expected, actual);

            _time.Verify();
            _propertiesManager.Verify();
            _printer.Verify();
        }

        /// <summary>A test for CreateTicket where the transaction type is not valid</summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateTicketInvalidTransactionTest()
        {
            _target.CreateTicket(new BillTransaction());
        }

        private static void CheckTickets(Ticket expected, Ticket actual)
        {
            Assert.AreEqual(expected.Data.Count, actual.Data.Count);

            foreach (var key in actual.Data.Keys)
            {
                Assert.AreEqual(expected.Data[key], actual.Data[key]);
            }
        }

        /// <summary>
        ///     Create the a reprint ticket.
        /// </summary>
        /// <returns>a ticket that is a reprint ticket</returns>
        private static Ticket CreateTestReprintTicket()
        {
            var ticket = new Ticket
            {
                ["ticket type"] = "handpay offset",
                ["title"] = "HANDPAY TICKET",
                ["serial id"] = "5498",
                ["machine id"] = "4387",
                ["machine id 2"] = "Machine #: 4387",
                ["machine id 3"] = "MACHINE # 4387",
                ["zone"] = "Calzone",
                ["bank"] = "33",
                ["position"] = "44",
                ["datetime"] = TransactionTimestamp.ToString(),
                ["locale datetime"] = TransactionTimestamp.ToString($"{LocaleDateFormat} {ApplicationConstants.DefaultTimeFormat}"),
                ["establishment name"] = "My Casino",
                ["location address"] = "Line 3",
                ["location name"] = "Line 2",
                ["validation label"] = "Validation:",
                ["validation label 2"] = "VALIDATION",
                ["value"] = "$31.89",
                ["value 2"] = "$31.89",
                ["value in words 1"] = "THIRTY-ONE DOLLARS AND EIGHTY-NINE CENTS",
                ["value in words 2"] = string.Empty,
                ["value in words with newline"] = "THIRTY-ONE DOLLARS AND EIGHTY-NINE CENTS",
                ["sequence number"] = "987",
                ["validation number"] = "----------------------",
                ["validation number alt"] = "",
                ["asset id 2"] = "Asset# 4387",
            };
            return ticket;
        }

        /// <summary>
        ///     Sets up a mock IPropertiesManager with strict behavior for verification.
        /// </summary>
        private void SetupMockPropertiesManager()
        {
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.SerialNumber", It.IsAny<string>()))
                .Returns("5498")
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.MachineId", It.IsAny<uint>()))
                .Returns((uint)4387)
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.ZoneName", It.IsAny<object>()))
                .Returns("Calzone")
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.BankId", It.IsAny<object>()))
                .Returns("33")
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.EgmPosition", It.IsAny<object>()))
                .Returns("44")
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("TicketProperty.TicketTextLine1", It.IsAny<object>()))
                .Returns("My Casino")
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("TicketProperty.TicketTextLine2", It.IsAny<object>()))
                .Returns("Line 2")
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("TicketProperty.TicketTextLine3", It.IsAny<object>()))
                .Returns("Line 3")
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.CurrencyMultiplierKey, It.IsAny<object>()))
                .Returns(100000d)
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("System.VoucherOfflineNotify", It.IsAny<object>()))
                .Returns(true)
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.LocalizationPlayerTicketDateFormat, It.IsAny<string>()))
                .Returns(LocaleDateFormat)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPageZoneOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPageBankOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPagePositionOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);
        }
    }
}