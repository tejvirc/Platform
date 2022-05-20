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
    ///     This is a test class for CanceledCreditsTicketsCreator and is intended
    ///     to contain all CanceledCreditsTicketsCreator Unit Tests.
    /// </summary>
    [TestClass]
    public sealed class CanceledCreditsTicketsCreatorTest
    {
        private const int Expiration = 23;
        private const string ExpectedJackpotTitle = "Jackpot Receipt";
        private const string LocaleDateFormat = "yyyy-MM-dd";

        private static readonly DateTime TransactionTimestamp = new DateTime(2012, 7, 22, 19, 45, 0);

        private Mock<IPropertiesManager> _propertiesManager;

        private Mock<ITime> _time;

        private HandpayTransaction _transaction;

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            _time = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict, true);
            _propertiesManager = SetupMockPropertiesManager();
            SetupFakePrinter();

            CurrencyExtensions.SetCultureInfo(CultureInfo.CurrentCulture, null, null, true, true, "c");
            _transaction = SetupDummyTransaction();
            _time.Setup(mock => mock.GetLocationTime(TransactionTimestamp)).Returns(TransactionTimestamp).Verifiable();

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

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPageZoneOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPageBankOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPagePositionOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        /// <summary>
        ///     A test for CreateCanceledCreditsTicket
        /// </summary>
        [TestMethod]
        public void CreateCanceledCreditsTicketTest()
        {
            var expected = SetupBaseExpectedTicket();
            var actual = HandpayTicketsCreator.CreateCanceledCreditsTicket(_transaction);
            VerifyTicket(expected, _transaction, actual);
            _propertiesManager.Verify();
            _time.Verify();
        }

        /// <summary>
        ///     A test for CreateCanceledCreditsReprintTicket
        /// </summary>
        [TestMethod]
        public void CreateCanceledCreditsReprintTicket()
        {
            var expected = SetupBaseExpectedTicket();
            var actual = HandpayTicketsCreator.CreateCanceledCreditsReprintTicket(_transaction);
            expected["title"] = $"{expected["title"]} REPRINT";
            VerifyTicket(expected, _transaction, actual);
            _propertiesManager.Verify();
            _time.Verify();
        }

        /// <summary>
        ///     A test for CreateCanceledCreditsReceiptTicket
        /// </summary>
        [TestMethod]
        public void CreateCanceledCreditsReceiptTicketTest()
        {
            var expected = SetupBaseExpectedReceiptTicket();
            expected["ticket type"] = "handpay receipt";
            expected["validation number"] = "----------------------";
            expected["barcode"] = null;
            var actual = HandpayTicketsCreator.CreateCanceledCreditsReceiptTicket(_transaction);
            VerifyTicket(expected, _transaction, actual);
            _propertiesManager.Verify();
            _time.Verify();
        }

        /// <summary>
        ///     A test for CreateCanceledCreditsReceiptReprintTicket
        /// </summary>
        [TestMethod]
        public void CreateCanceledCreditsReceiptReprintTicketTest()
        {
            var expected = SetupBaseExpectedReceiptTicket();
            expected["title"] = $"{expected["title"]} REPRINT";
            expected["ticket type"] = "handpay receipt";
            expected["validation number"] = "----------------------";
            expected["barcode"] = null;
            var actual = HandpayTicketsCreator.CreateCanceledCreditsReceiptReprintTicket(_transaction);
            VerifyTicket(expected, _transaction, actual);
            _propertiesManager.Verify();
            _time.Verify();
        }


        /// <summary>
        ///     A test for CreateBonusPayTicket
        /// </summary>
        [TestMethod]
        public void CreateBonusPayTicketTest()
        {
            var expected = SetupBaseExpectedReceiptTicket();
            expected["ticket type"] = "jackpot";
            expected["title"] = ExpectedJackpotTitle;
            expected["validation number"] = "----------------------";
            expected["barcode"] = null;
            var actual = HandpayTicketsCreator.CreateBonusPayTicket(_transaction);
            VerifyTicket(expected, _transaction, actual);
            _propertiesManager.Verify();
            _time.Verify();
        }

        /// <summary>
        ///     A test for CreateBonusPayTicket
        /// </summary>
        [TestMethod]
        public void CreateBonusPayReprintTicketTest()
        {
            var expected = SetupBaseExpectedReceiptTicket();
            expected["ticket type"] = "jackpot";
            expected["title"] = "JACKPOT HANDPAY TICKET REPRINT";
            expected["validation number"] = "----------------------";
            expected["barcode"] = null;
            var actual = HandpayTicketsCreator.CreateBonusPayReprintTicket(_transaction);
            VerifyTicket(expected, _transaction, actual);
            _propertiesManager.Verify();
            _time.Verify();
        }

        /// <summary>
        ///     Creates and sets up a mock IPropertiesManager with strict behavior for verification.
        /// </summary>
        /// <returns>The mock IPropertiesManager which can be used for verification.</returns>
        private Mock<IPropertiesManager> SetupMockPropertiesManager()
        {
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict, true);
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.SerialNumber", It.IsAny<string>())).Returns("5498")
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.MachineId", It.IsAny<uint>())).Returns((uint)4387)
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.ZoneName", It.IsAny<object>()))
                .Returns("Calzone")
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.BankId", It.IsAny<object>())).Returns("33")
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.EgmPosition", It.IsAny<object>())).Returns("44")
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("TicketProperty.TicketTextLine1", It.IsAny<object>()))
                .Returns("My Casino").Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("TicketProperty.TicketTextLine2", It.IsAny<object>()))
                .Returns("Line 2").Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("TicketProperty.TicketTextLine3", It.IsAny<object>()))
                .Returns("Line 3").Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.CurrencyMultiplierKey, It.IsAny<object>()))
                .Returns(100000d);
            _propertiesManager.Setup(mock => mock.GetProperty("Handpay.TitleCancelReceipt", It.IsAny<object>()))
                .Returns("HANDPAY RECEIPT");
            var defaultTicketBarcodeLength = 18;
            _propertiesManager.Setup(mock => mock.GetProperty("TicketProperty.BarcodeLength", It.IsAny<object>()))
                .Returns(defaultTicketBarcodeLength);
            _propertiesManager.Setup(mock => mock.GetProperty("System.VoucherOfflineNotify", It.IsAny<object>()))
                .Returns(true);            
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.TitleJackpotReceipt, It.IsAny<object>()))
                .Returns(ExpectedJackpotTitle);
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.LocalizationPlayerTicketDateFormat, It.IsAny<string>()))
                .Returns(LocaleDateFormat)
                .Verifiable();
            return _propertiesManager;
        }

        /// <summary>
        ///     Creates and sets up a fake IPrinter.
        /// </summary>
        private static void SetupFakePrinter()
        {
            // Set up the fake IPrinter service. This is needed by CurrencyToWords - no need to verify.
            var printer = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict, true);

            // canceled credit tickets are printed in landscape orientation
            const int charactersPerLineInLandscapeMode = 80;
            const bool isLandscape = true;
            printer.Setup(mock => mock.GetCharactersPerLine(isLandscape, 0)).Returns(charactersPerLineInLandscapeMode);
        }

        /// <summary>
        ///     Creates and sets up the base expected ticket.
        ///     Each test method may override some values unique to the test.
        /// </summary>
        /// <returns>A ticket matching the values in the dummy transaction.</returns>
        private static Ticket SetupBaseExpectedTicket()
        {
            var ticket = new Ticket
            {
                ["ticket type"] = "handpay offset",
                ["title"] = "HANDPAY TICKET",
                ["serial id"] = "5498",
                ["machine id"] = "4387",
                ["zone"] = "Calzone",
                ["bank"] = "33",
                ["position"] = "44",
                ["datetime"] = "VERIFY IT IS NOT NULL, EMPTY, OR TRANSACTION DATE/TIME",
                ["establishment name"] = "My Casino",
                ["location address"] = "Line 3",
                ["location name"] = "Line 2",
                ["redemption text"] = "VOID AFTER " + Expiration + " DAYS",
                ["redemption text 2"] = Expiration + " DAYS",
                ["validation label"] = "Validation:",
                ["value"] = "$10.45",
                ["value in words 1"] = "TEN DOLLARS AND FORTY-FIVE CENTS",
                ["value in words 2"] = string.Empty,                
                ["value in words with newline"] = "TEN DOLLARS AND FORTY-FIVE CENTS",

                ["sequence number"] = "987",
                ["validation number"] = "00-0000-0001-2345-9876",
                //["barcode"] = "123459876",
                ["asset id"] = "12345"
            };
            return ticket;
        }

        /// <summary>
        ///     Creates and sets up the base expected ticket.
        ///     Each test method may override some values unique to the test.
        /// </summary>
        /// <returns>A ticket matching the values in the dummy transaction.</returns>
        private static Ticket SetupBaseExpectedReceiptTicket()
        {
            var ticket = new Ticket
            {
                ["ticket type"] = "handpay receipt",
                ["title"] = "HANDPAY RECEIPT",
                ["serial id"] = "5498",
                ["machine id"] = "4387",
                ["zone"] = "Calzone",
                ["bank"] = "33",
                ["position"] = "44",
                ["datetime"] = "VERIFY IT IS NOT NULL, EMPTY, OR TRANSACTION DATE/TIME",
                ["establishment name"] = "My Casino",
                ["location address"] = "Line 3",
                ["location name"] = "Line 2",
                ["redemption text"] = "VOID AFTER " + Expiration + " DAYS",
                ["redemption text 2"] = Expiration + " DAYS",
                ["validation label"] = "Validation:",
                ["value"] = "$10.45",
                ["value in words 1"] = "TEN DOLLARS AND FORTY-FIVE CENTS",
                ["value in words 2"] = " *** NO CASH VALUE ***",                
                ["value in words with newline"] = "TEN DOLLARS AND FORTY-FIVE CENTS",
                ["sequence number"] = "987",
                ["validation number"] = "00-0000-0001-2345-9876",
                ["barcode"] = "123459876",
                ["asset id"] = "Asset: 12345"
            };
            return ticket;
        }

        /// <summary>
        ///     Create a dummy canceled-credits transaction for use in the test methods.
        /// </summary>
        /// <returns>A canceled-credits transaction matching the values in the base expected ticket.</returns>
        private static HandpayTransaction SetupDummyTransaction()
        {
            // create a fake transaction with deviceId = 3, time = 5/22/2010 19:45:00 transactionId = 1234, amount = $10.45 log sequence = 987, barcode = 123459876, and expiration from constant Expiration
            var transaction = new HandpayTransaction(
                3,
                TransactionTimestamp,
                1045000,
                0,
                0,
                HandpayType.CancelCredit,
                true,
                Guid.NewGuid()) { LogSequence = 987, Barcode = "123459876" };
            transaction.HostOnline = true;
            return transaction;
        }

        /// <summary>
        ///     Verifies that the actual ticket values match the expected ticket values.
        /// </summary>
        /// <param name="expected">The expected values for the ticket</param>
        /// <param name="transaction">The transaction used to create the actual ticket</param>
        /// <param name="actual">The actual values for the ticket</param>
        private static void VerifyTicket(Ticket expected, HandpayTransaction transaction, Ticket actual)
        {
            Assert.AreEqual(expected["ticket type"], actual["ticket type"]);
            Assert.AreEqual(expected["title"], actual["title"]);
            Assert.AreEqual(expected["serial id"], actual["serial id"]);
            Assert.AreEqual(expected["machine id"], actual["machine id"]);
            Assert.AreEqual(expected["zone"], actual["zone"]);
            Assert.AreEqual(expected["bank"], actual["bank"]);
            Assert.AreEqual(expected["position"], actual["position"]);
            Assert.IsNotNull(actual["datetime"], "The ticket date/time must not be null.");
            Assert.AreNotEqual(string.Empty, actual["datetime"], "The ticket date/time must not be empty.");
            Assert.AreEqual(
                transaction.TransactionDateTime,
                DateTime.Parse(actual["datetime"]),
                "The ticket date/time must be the same as the _transaction date/time.");
            Assert.AreEqual(expected["establishment name"], actual["establishment name"]);
            Assert.AreEqual(expected["location address"], actual["location address"]);
            Assert.AreEqual(expected["location name"], actual["location name"]);
            Assert.AreEqual(expected["validation label"], actual["validation label"]);
            Assert.AreEqual(expected["value"], actual["value"]);
            Assert.AreEqual(expected["value in words 1"], actual["value in words 1"]);
            Assert.AreEqual(expected["value in words 2"], actual["value in words 2"]);
            //Assert.AreEqual(expected["sequence number"], actual["sequence number"]);
            //Assert.AreEqual(expected["asset id"], actual["asset id"]);
        }
    }
}
