namespace Aristocrat.Monaco.Sas.Tests.VoucherValidation
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.VoucherValidation;
    using Storage.Models;
    using Storage.Repository;
    using Ticketing;

    [TestClass]
    public class NoValidationHandlerTests
    {
        private const int WaitTime = 1000;
        private Mock<IPropertiesManager> _propertiesManager;

        private NoValidationHandler _target;
        private Mock<ITicketingCoordinator> _ticketingCoordinator;
        private Mock<ITransactionHistory> _transactionHistory;
        private Mock<IStorageDataProvider<ValidationInformation>> _validationDataProvider;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _ticketingCoordinator = new Mock<ITicketingCoordinator>(MockBehavior.Default);
            _transactionHistory = new Mock<ITransactionHistory>(MockBehavior.Default);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _validationDataProvider = new Mock<IStorageDataProvider<ValidationInformation>>(MockBehavior.Default);

            _target = new NoValidationHandler(
                _ticketingCoordinator.Object,
                _transactionHistory.Object,
                _propertiesManager.Object,
                _validationDataProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTicketingCoordinatorTest()
        {
            _target = new NoValidationHandler(null, _transactionHistory.Object, _propertiesManager.Object, _validationDataProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTransactionHistoryTest()
        {
            _target = new NoValidationHandler(_ticketingCoordinator.Object, null, _propertiesManager.Object, _validationDataProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPropertiesManagerTest()
        {
            _target = new NoValidationHandler(_ticketingCoordinator.Object, _transactionHistory.Object, null, _validationDataProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullValidationDataProviderTest()
        {
            _target = new NoValidationHandler(_ticketingCoordinator.Object, _transactionHistory.Object, _propertiesManager.Object, null);
        }

        [DataRow(
            true,
            TicketType.Restricted,
            (ulong)100,
            true,
            true,
            100,
            DisplayName = "Able to validate valid restricted amount")]
        [DataRow(
            false,
            TicketType.Restricted,
            (ulong)100,
            true,
            false,
            100,
            DisplayName = "Can't print restricted ticket test")]
        [DataRow(
            false,
            TicketType.CashOut,
            (ulong)100,
            false,
            true,
            100,
            DisplayName = "Can't use the print as a cashout device")]
        [DataRow(
            false,
            TicketType.Restricted,
            (ulong)100,
            true,
            true,
            01011981,
            DisplayName = "Past date fail test")]
        [DataRow(
            true,
            TicketType.CashOut,
            (ulong)100,
            true,
            true,
            01011981,
            DisplayName = "Date is ignored for cashable credits")]
        [DataTestMethod]
        public void CanValidateTicketOutTest(
            bool canValidate,
            TicketType ticketType,
            ulong amount,
            bool printTicket,
            bool printRestrictedTickets,
            int ticketExpirationDate)
        {
            _ticketingCoordinator.Setup(x => x.TicketExpirationCashable).Returns((ulong)ticketExpirationDate);
            _ticketingCoordinator.Setup(x => x.TicketExpirationRestricted).Returns((ulong)ticketExpirationDate);
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.VoucherOutNonCash, It.IsAny<bool>()))
                .Returns(printRestrictedTickets);
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.VoucherOut, It.IsAny<bool>()))
                .Returns(printTicket);

            Assert.AreEqual(canValidate, _target.CanValidateTicketOutRequest(amount, ticketType));
        }

        [TestMethod]
        public void ValidationTypeTest()
        {
            Assert.AreEqual(SasValidationType.None, _target.ValidationType);
        }

        [DataRow(
            TicketType.CashOut,
            TicketValidationType.CashableTicketFromCashOutOrWin,
            DisplayName = "Cash out validation test")]
        [DataRow(
            TicketType.CashOutOffline,
            TicketValidationType.CashableTicketFromCashOutOrWin,
            DisplayName = "Cash out offline validation test")]
        [DataRow(
            TicketType.Jackpot,
            TicketValidationType.HandPayFromWinNoReceipt,
            DisplayName = "Jackpot validation test")]
        [DataRow(
            TicketType.JackpotOffline,
            TicketValidationType.HandPayFromWinNoReceipt,
            DisplayName = "Jackpot offline validation test")]
        [DataRow(
            TicketType.HandPayValidated,
            TicketValidationType.HandPayFromCashOutNoReceipt,
            DisplayName = "HandPay validated validation test")]
        [DataTestMethod]
        public void CashableTicketsSuccessfulValidation(TicketType ticketType, TicketValidationType validationType)
        {
            const ulong amount = 801251000;
            const ulong cashableExpirationDays = 150;

            var expectedResults = new TicketOutInfo
            {
                ValidationType = validationType,
                TicketExpiration = (uint)cashableExpirationDays,
                Amount = amount
            };

            _transactionHistory.Setup(x => x.RecallTransactions<VoucherOutTransaction>())
                .Returns(new List<VoucherOutTransaction>());
            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>())
                .Returns(new List<HandpayTransaction>());
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.VoucherOut, It.IsAny<bool>()))
                .Returns(true);

            _ticketingCoordinator.SetupGet(t => t.TicketExpirationCashable).Returns(cashableExpirationDays);

            var ticketOutValidation = _target.HandleTicketOutValidation(amount, ticketType);
            Assert.IsTrue(ticketOutValidation.Wait(WaitTime));
            TicketOutMatched(expectedResults, ticketOutValidation.Result);
            _propertiesManager.VerifyAll();
        }

        [TestMethod]
        public void GenerateBarcodeTest()
        {
            // Sample values from SAS 5.02 15.11.  Amount converted to millicents.
            const ulong amount = 801251000;
            var transactionDateTime =
                new DateTime(1900, 1, 1, 23, 15, 51);
            const string expectedBarcode = "33761282";

            var generatedBarcode = NoValidationHandler.GenerateBarcode(amount, transactionDateTime);

            Assert.AreEqual(expectedBarcode, generatedBarcode);
        }

        [DataRow(
            HandPayType.CanceledCredit,
            TicketType.CashOutReceipt,
            TicketValidationType.HandPayFromCashOutReceiptPrinted,
            true,
            DisplayName = "Canceled credits with printing receipts")]
        [DataRow(
            HandPayType.CanceledCredit,
            TicketType.HandPayValidated,
            TicketValidationType.HandPayFromCashOutNoReceipt,
            false,
            DisplayName = "Canceled credits without printing receipts")]
        [DataRow(
            HandPayType.NonProgressive,
            TicketType.JackpotReceipt,
            TicketValidationType.HandPayFromWinReceiptPrinted,
            true,
            DisplayName = "Non Progressive with printing receipts")]
        [DataRow(
            HandPayType.NonProgressive,
            TicketType.JackpotReceipt,
            TicketValidationType.HandPayFromWinNoReceipt,
            false,
            DisplayName = "Non Progressive without printing receipts")]
        [DataRow(
            HandPayType.NonProgressiveNoReceipt,
            TicketType.Jackpot,
            TicketValidationType.HandPayFromWinNoReceipt,
            true,
            DisplayName = "Non Progressive no receipt with printing receipts settings")]
        [DataRow(
            HandPayType.NonProgressiveNoReceipt,
            TicketType.Jackpot,
            TicketValidationType.HandPayFromWinNoReceipt,
            false,
            DisplayName = "Non Progressive no receipt without printing receipts settings")]
        [DataRow(
            HandPayType.NonProgressiveTopAward,
            TicketType.JackpotReceipt,
            TicketValidationType.HandPayFromWinReceiptPrinted,
            true,
            DisplayName = "Non Progress Top Award with printing receipts")]
        [DataRow(
            HandPayType.NonProgressiveTopAward,
            TicketType.JackpotReceipt,
            TicketValidationType.HandPayFromWinNoReceipt,
            false,
            DisplayName = "Non Progress Top Award without printing receipts")]
        [DataRow(
            HandPayType.Progressive,
            TicketType.JackpotReceipt,
            TicketValidationType.HandPayFromWinReceiptPrinted,
            true,
            DisplayName = "Progressive with printing receipts")]
        [DataRow(
            HandPayType.Progressive,
            TicketType.JackpotReceipt,
            TicketValidationType.HandPayFromWinNoReceipt,
            false,
            DisplayName = "Progressive without printing receipts")]
        [DataTestMethod]
        public void HandPayPendingTest(
            HandPayType handPayType,
            TicketType ticketType,
            TicketValidationType validationType,
            bool printReceipts)
        {
            const ulong amount = 100;

            var expectedResults = new TicketOutInfo
            {
                ValidationType = validationType,
                TicketExpiration = 0,
                Amount = amount,
                Pool = 0
            };

            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.ValidateHandpays, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.EnableReceipts, It.IsAny<bool>()))
                .Returns(printReceipts);

            var handpayValidation = _target.HandleHandPayValidation(amount, handPayType);
            Assert.IsTrue(handpayValidation.Wait(WaitTime));
            var validationResult = handpayValidation.Result;
            Assert.IsNotNull(validationResult);
            Assert.AreEqual(expectedResults.ValidationType, validationResult.ValidationType);
            Assert.AreEqual(expectedResults.Amount, validationResult.Amount);
            Assert.AreEqual(expectedResults.TicketExpiration, validationResult.TicketExpiration);
            Assert.AreEqual(expectedResults.Pool, validationResult.Pool);
        }

        private static void TicketOutMatched(TicketOutInfo expected, TicketOutInfo actual)
        {
            Assert.AreEqual(expected.ValidationType, actual.ValidationType);
            Assert.AreEqual(expected.Amount, actual.Amount);
            Assert.AreEqual(expected.TicketExpiration, actual.TicketExpiration);
        }
    }
}