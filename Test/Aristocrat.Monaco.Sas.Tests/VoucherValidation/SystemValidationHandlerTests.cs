namespace Aristocrat.Monaco.Sas.Tests.VoucherValidation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Monaco.Sas.Storage.Repository;
    using Aristocrat.Monaco.Sas.Ticketing;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.VoucherValidation;

    [DoNotParallelize]
    [TestClass]
    public class SystemValidationHandlerTests
    {
        private const int WaitTime = 1000;
        private const byte ValidationId = 0x01;
        private const string ValidationNumber = "6429188185446104";

        private SystemValidationHandler _target;
        private Mock<ISasHost> _sasHost;
        private Mock<ITicketingCoordinator> _ticketingCoordinator;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ITransactionHistory> _transactionHistory;
        private Mock<IHostValidationProvider> _hostValidationProvider;
        private Mock<IStorageDataProvider<ValidationInformation>> _validationDataProvider;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _sasHost = new Mock<ISasHost>(MockBehavior.Default);
            _ticketingCoordinator = new Mock<ITicketingCoordinator>(MockBehavior.Default);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _transactionHistory = new Mock<ITransactionHistory>(MockBehavior.Default);
            _hostValidationProvider = new Mock<IHostValidationProvider>(MockBehavior.Default);
            _validationDataProvider = new Mock<IStorageDataProvider<ValidationInformation>>(MockBehavior.Default);
            _sasHost.Setup(x => x.IsHostOnline(SasGroup.Validation)).Returns(true);
            _transactionHistory.Setup(t => t.RecallTransactions(It.IsAny<bool>()))
                .Returns(new List<ITransaction> { new VoucherOutTransaction { HostAcknowledged = true } }
                    .OrderBy(t => t.TransactionId));

            _target = CreateTarget();
        }

        [DataTestMethod]
        [DataRow(true, false, false, false, false, false)]
        [DataRow(false, true, false, false, false, false)]
        [DataRow(false, false, true, false, false, false)]
        [DataRow(false, false, false, true, false, false)]
        [DataRow(false, false, false, false, true, false)]
        [DataRow(false, false, false, false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorAgrumentsTest(
            bool nullHost,
            bool nullTicketingCoordinator,
            bool nullProperties,
            bool nullTransactionHistory,
            bool nullHostValidationProvider,
            bool nullValidationDataProvider)
        {
            _target = CreateTarget(nullHost, nullTicketingCoordinator, nullProperties, nullTransactionHistory, nullHostValidationProvider, nullValidationDataProvider);
        }

        [DataRow(
            true,
            true,
            TicketType.Restricted,
            (ulong)100,
            true,
            true,
            100,
            DisplayName = "Able to validate valid restricted amount")]
        [DataRow(
            false,
            true,
            TicketType.Restricted,
            (ulong)100,
            true,
            false,
            100,
            DisplayName = "Can't print restricted ticket test")]
        [DataRow(
            false,
            true,
            TicketType.CashOut,
            (ulong)100,
            false,
            true,
            100,
            DisplayName = "Can't use the print as a cashout device")]
        [DataRow(
            false,
            true,
            TicketType.Restricted,
            (ulong)100,
            true,
            true,
            01011981,
            DisplayName = "Past date fail test")]
        [DataRow(
            true,
            true,
            TicketType.CashOut,
            (ulong)100,
            true,
            true,
            01011981,
            DisplayName = "Date is ignored for cashable credits")]
        [DataRow(
            false,
            false,
            TicketType.Restricted,
            (ulong)100,
            true,
            true,
            100,
            DisplayName = "Unable to validate when the host is offline")]
        [DataTestMethod]
        public void CanValidateTicketOutTest(
            bool canValidate,
            bool hostOnline,
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
            _sasHost.Setup(x => x.IsHostOnline(SasGroup.Validation)).Returns(hostOnline);

            Assert.AreEqual(canValidate, _target.CanValidateTicketOutRequest(amount, ticketType));
        }

        [TestMethod]
        public void ValidationTypeTest()
        {
            Assert.AreEqual(SasValidationType.System, _target.ValidationType);
        }

        [TestMethod]
        public void AwaitingAcknowledgeFromHostFailsValidation()
        {
            const ulong amount = 100;
            const int validationFailedTime = 2000; // This is double the fail time

            _transactionHistory.Setup(x => x.RecallTransactions(It.IsAny<bool>()))
                .Returns(new List<ITransaction> { new VoucherOutTransaction { HostAcknowledged = false } }.OrderBy(x => x.TransactionId));
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.VoucherOutNonCash, It.IsAny<bool>()))
                .Returns(false);
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.VoucherOut, It.IsAny<bool>()))
                .Returns(true);

            var ticketOutValidation = _target.HandleTicketOutValidation(amount, TicketType.CashOut);
            Assert.IsTrue(ticketOutValidation.Wait(validationFailedTime));
            Assert.IsNull(ticketOutValidation.Result);
        }

        [DataRow((ulong)13302019, DisplayName = "Invalid date fails validation")]
        [DataRow((ulong)01121950, DisplayName = "Dates in the past fail validation")]
        [DataTestMethod]
        public void BadExpirationDateForRestrictedTicketsFailsValidation(ulong restrictedExpirationDays)
        {
            const ulong amount = 100;

            _transactionHistory.Setup(x => x.RecallTransactions<VoucherOutTransaction>())
                .Returns(new List<VoucherOutTransaction>());
            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>())
                .Returns(new List<HandpayTransaction>());
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.VoucherOutNonCash, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.VoucherOut, It.IsAny<bool>()))
                .Returns(true);
            _ticketingCoordinator.SetupGet(t => t.TicketExpirationRestricted).Returns(restrictedExpirationDays);

            var ticketOutValidation = _target.HandleTicketOutValidation(amount, TicketType.Restricted);
            Assert.IsTrue(ticketOutValidation.Wait(WaitTime));
            Assert.IsNull(ticketOutValidation.Result);
        }

        [TestMethod]
        public void CashableTicketsSuccessfulValidation()
        {
            const ulong amount = 100;
            const ulong cashableExpirationDays = 150;

            var expectedResults = new TicketOutInfo
            {
                ValidationType = TicketValidationType.CashableTicketFromCashOutOrWin,
                TicketExpiration = (uint)cashableExpirationDays,
                Amount = amount,
                Barcode = $"{ValidationId:D2}{ValidationNumber}",
                Pool = 0
            };

            _transactionHistory.Setup(x => x.RecallTransactions<VoucherOutTransaction>())
                .Returns(new List<VoucherOutTransaction>());
            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>())
                .Returns(new List<HandpayTransaction>());
            _hostValidationProvider.Setup(x => x.GetValidationResults(amount, TicketType.CashOut))
                .Returns(Task.FromResult(new HostValidationResults(ValidationId, ValidationNumber)));
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.VoucherOut, It.IsAny<bool>()))
                .Returns(true);
            _ticketingCoordinator.SetupGet(t => t.TicketExpirationCashable).Returns(cashableExpirationDays);

            var ticketOutValidation = _target.HandleTicketOutValidation(amount, TicketType.CashOut);
            Assert.IsTrue(ticketOutValidation.Wait(WaitTime));
            TicketOutMatched(expectedResults, ticketOutValidation.Result);
        }

        [DataRow((ulong)12319999, DisplayName = "Valid date passes verification (uses max date Dec 31, 9999)")]
        [DataRow((ulong)150, DisplayName = "Number of days passes verification")]
        [DataTestMethod]
        public void RestrictedTicketsSuccessfulValidation(ulong restrictedExpiration)
        {
            const ulong amount = 100;
            const ushort poolId = 10;

            var expectedResults = new TicketOutInfo
            {
                ValidationType = TicketValidationType.RestrictedPromotionalTicketFromCashOut,
                TicketExpiration = (uint)restrictedExpiration,
                Amount = amount,
                Barcode = $"{ValidationId:D2}{ValidationNumber}",
                Pool = poolId
            };

            _transactionHistory.Setup(x => x.RecallTransactions<VoucherOutTransaction>())
                .Returns(new List<VoucherOutTransaction>());
            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>())
                .Returns(new List<HandpayTransaction>());
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.VoucherOutNonCash, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.VoucherOut, It.IsAny<bool>()))
                .Returns(true);
            _hostValidationProvider.Setup(x => x.GetValidationResults(amount, TicketType.Restricted))
                .Returns(Task.FromResult(new HostValidationResults(ValidationId, ValidationNumber)));
            _ticketingCoordinator.SetupGet(t => t.TicketExpirationRestricted).Returns(restrictedExpiration);
            _ticketingCoordinator.Setup(x => x.GetData()).Returns(new TicketStorageData { PoolId = poolId });

            var ticketOutValidation = _target.HandleTicketOutValidation(amount, TicketType.Restricted);
            Assert.IsTrue(ticketOutValidation.Wait(WaitTime));
            TicketOutMatched(expectedResults, ticketOutValidation.Result);
        }

        [TestMethod]
        public void HostValidationFailedWithNoResults()
        {
            const ulong amount = 100;
            const ulong cashableExpirationDays = 150;

            _transactionHistory.Setup(x => x.RecallTransactions<VoucherOutTransaction>())
                .Returns(new List<VoucherOutTransaction>());
            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>())
                .Returns(new List<HandpayTransaction>());
            _hostValidationProvider.Setup(x => x.GetValidationResults(amount, TicketType.CashOut))
                .Returns(Task.FromResult((HostValidationResults)null));
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.VoucherOut, It.IsAny<bool>()))
                .Returns(true);
            _ticketingCoordinator.SetupGet(t => t.TicketExpirationCashable).Returns(cashableExpirationDays);

            var ticketOutValidation = _target.HandleTicketOutValidation(amount, TicketType.CashOut);
            Assert.IsTrue(ticketOutValidation.Wait(WaitTime));
            Assert.IsNull(ticketOutValidation.Result);
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
                Barcode = string.Empty,
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
            Assert.AreEqual(expectedResults.Barcode, validationResult.Barcode);
            Assert.AreEqual(expectedResults.TicketExpiration, validationResult.TicketExpiration);
            Assert.AreEqual(expectedResults.Pool, validationResult.Pool);
        }

        private SystemValidationHandler CreateTarget(
            bool nullHost = false,
            bool nullTicketingCoordinator = false,
            bool nullProperties = false,
            bool nullTransactionHistory = false,
            bool nullHostValidationProvider = false,
            bool nullValidationDataProvider = false)
        {
            return new SystemValidationHandler(
                nullHost ? null : _sasHost.Object,
                nullTicketingCoordinator ? null : _ticketingCoordinator.Object,
                nullProperties ? null : _propertiesManager.Object,
                nullTransactionHistory ? null : _transactionHistory.Object,
                nullHostValidationProvider ? null : _hostValidationProvider.Object,
                nullValidationDataProvider ? null : _validationDataProvider.Object);
        }

        private static void TicketOutMatched(TicketOutInfo expected, TicketOutInfo actual)
        {
            Assert.AreEqual(expected.ValidationType, actual.ValidationType);
            Assert.AreEqual(expected.Amount, actual.Amount);
            Assert.AreEqual(expected.Barcode, actual.Barcode);
            Assert.AreEqual(expected.TicketExpiration, actual.TicketExpiration);
            Assert.AreEqual(expected.Pool, actual.Pool);
        }
    }
}