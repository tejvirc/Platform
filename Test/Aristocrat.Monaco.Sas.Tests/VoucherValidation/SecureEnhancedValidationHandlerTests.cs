namespace Aristocrat.Monaco.Sas.Tests.VoucherValidation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
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

    [TestClass]
    public class SecureEnhancedValidationHandlerTests
    {
        private const uint MachineNumber = 0x654321;
        private const uint SequenceNumber = 0;
        private const int WaitTime = 1000;

        private static readonly VoucherOutTransaction ValidVoucherTransaction = new VoucherOutTransaction(
            0,
            new DateTime(2019, 12, 21, 15, 31, 44),
            300,
            AccountType.NonCash,
            "006429188185446104",
            SasConstants.MaxTicketExpirationDays,
            4,
            "")
        {
            VoucherSequence = 3,
            HostAcknowledged = false,
            HostSequence = 62,
            TransactionId = 1000
        };

        private static readonly HandpayTransaction ValidHandpayTransaction = new HandpayTransaction(
            0,
            ValidVoucherTransaction.TransactionDateTime.AddSeconds(1.0),
            100,
            200,
            300,
            400,
            HandpayType.GameWin,
            false,
            Guid.NewGuid())
        {
            LogSequence = 4,
            State = HandpayState.Committed,
            Barcode = "006429188185446104",
            HostSequence = ValidVoucherTransaction.HostSequence + 1,
            Printed = false,
            TransactionId = ValidVoucherTransaction.TransactionId + 1
        };

        private static IEnumerable<object[]> InitializationTestData => new List<object[]>
        {
            new object[]
            {
                new List<ITransaction>
                {
                    ValidVoucherTransaction
                },
                SequenceNumber + 1
            },
            new object[]
            {
                new List<ITransaction>
                {
                    ValidHandpayTransaction
                },
                SequenceNumber + 1
            },
            new object[]
            {
                new List<ITransaction>(),
                SequenceNumber
            }
        };

        private SecureEnhancedValidationHandler _target;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ITransactionHistory> _transactionHistory;
        private Mock<ISasDisableProvider> _sasDisableProvider;
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private Mock<ITicketingCoordinator> _ticketingCoordinator;
        private Mock<IStorageDataProvider<ValidationInformation>> _validationDataProvider;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _transactionHistory = new Mock<ITransactionHistory>(MockBehavior.Default);
            _sasDisableProvider = new Mock<ISasDisableProvider>(MockBehavior.Default);
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Default);
            _ticketingCoordinator = new Mock<ITicketingCoordinator>(MockBehavior.Default);
            _validationDataProvider = new Mock<IStorageDataProvider<ValidationInformation>>(MockBehavior.Default);

            _validationDataProvider.Setup(x => x.GetData()).Returns(new ValidationInformation
            {
                MachineValidationId = MachineNumber,
                SequenceNumber = SequenceNumber,
                ValidationConfigured = true
            });

            _transactionHistory.Setup(t => t.RecallTransactions(It.IsAny<bool>()))
                .Returns(new List<ITransaction> { new VoucherOutTransaction { HostAcknowledged = true } }
                    .OrderBy(t => t.TransactionId));

            _target = CreateTarget();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [DataTestMethod]
        [DataRow(true, false, false, false, false, false)]
        [DataRow(false, true, false, false, false, false)]
        [DataRow(false, false, true, false, false, false)]
        [DataRow(false, false, false, true, false, false)]
        [DataRow(false, false, false, false, true, false)]
        [DataRow(false, false, false, false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorArgumentTest(
            bool nullProperties,
            bool nullTransactionHistory,
            bool nullSasDisable,
            bool nullTicketingCoordinator,
            bool nullExceptionHandler,
            bool nullValidationDataProvider)
        {
            _target = CreateTarget(
                nullProperties,
                nullTransactionHistory,
                nullSasDisable,
                nullTicketingCoordinator,
                nullExceptionHandler,
                nullValidationDataProvider);
        }

        [TestMethod]
        public void ValidationTypeTest()
        {
            Assert.AreEqual(SasValidationType.SecureEnhanced, _target.ValidationType);
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
            DisplayName = "HadPay validated validation test")]
        [DataTestMethod]
        public void CashableTicketsSuccessfulValidation(TicketType ticketType, TicketValidationType validationType)
        {
            const ulong amount = 100;
            const ulong cashableExpirationDays = 150;

            var expectedResults = new TicketOutInfo
            {
                ValidationType = validationType,
                TicketExpiration = (uint)cashableExpirationDays,
                Amount = amount,
                Barcode = "006429188185446104",
                Pool = 0
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

        [DataRow(
            TicketType.CashOutReceipt,
            TicketValidationType.HandPayFromCashOutReceiptPrinted,
            true,
            DisplayName = "Cash out with receipt print enabled validation test")]
        [DataRow(
            TicketType.JackpotReceipt,
            TicketValidationType.HandPayFromWinReceiptPrinted,
            true,
            DisplayName = "Jackpot with receipt print enabled validation test")]
        [DataRow(
            TicketType.CashOutReceipt,
            TicketValidationType.HandPayFromCashOutNoReceipt,
            false,
            DisplayName = "Cash out with receipt print disabled validation test")]
        [DataRow(
            TicketType.JackpotReceipt,
            TicketValidationType.HandPayFromWinNoReceipt,
            false,
            DisplayName = "Jackpot with receipt print disabled validation test")]
        [DataTestMethod]
        public void ReceiptTicketsSuccessfulValidation(
            TicketType ticketType,
            TicketValidationType validationType,
            bool printReceipts)
        {
            const ulong amount = 100;
            const ulong cashableExpirationDays = 150;

            var expectedResults = new TicketOutInfo
            {
                ValidationType = validationType,
                TicketExpiration = (uint)cashableExpirationDays,
                Amount = amount,
                Barcode = "006429188185446104",
                Pool = 0
            };

            _transactionHistory.Setup(x => x.RecallTransactions<VoucherOutTransaction>())
                .Returns(new List<VoucherOutTransaction>());
            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>())
                .Returns(new List<HandpayTransaction>());
            _ticketingCoordinator.SetupGet(t => t.TicketExpirationCashable).Returns(cashableExpirationDays);
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.ValidateHandpays, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.EnableReceipts, It.IsAny<bool>()))
                .Returns(printReceipts);
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.VoucherOut, It.IsAny<bool>()))
                .Returns(true);

            var ticketOutValidation = _target.HandleTicketOutValidation(amount, ticketType);
            Assert.IsTrue(ticketOutValidation.Wait(WaitTime));
            TicketOutMatched(expectedResults, ticketOutValidation.Result);
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

        [TestMethod]
        public void IncrementValidationNumberOnTicketCompleteTest()
        {
            _validationDataProvider.Setup(x => x.Save(It.Is<ValidationInformation>(v => v.SequenceNumber == (SequenceNumber + 1))))
                .Verifiable();
            _target.HandleTicketOutCompleted(
                new VoucherOutTransaction(
                    0,
                    DateTime.Now,
                    100,
                    AccountType.Cashable,
                    "006429188185446104",
                    0,
                    string.Empty));

            _validationDataProvider.Verify();
        }

        [DataRow("006429188185446104", MachineNumber, SequenceNumber, SequenceNumber + 1, DisplayName = "Sas Secure Enhance Section 15.14 Example")]
        [DataRow("004580945669109504", MachineNumber, (uint)0xFFFFFF, (uint)0, DisplayName = "Validation Sequence Number Roller")]
        [DataRow("007101753535986872", MachineNumber, (uint)0xFFFFFE, (uint)0xFFFFFF, DisplayName = "Max Sequence is a valid sequence number")]
        [DataTestMethod]
        public void IncrementValidationNumberOnHandPayCompleteTest(
            string barcode,
            uint machineNumber,
            uint currentSequence,
            uint expectedSequence)
        {
            _validationDataProvider.Setup(x => x.GetData()).Returns(new ValidationInformation
            {
                ValidationConfigured = true,
                MachineValidationId = machineNumber,
                SequenceNumber = currentSequence
            });

            _validationDataProvider.Setup(x => x.Save(It.Is<ValidationInformation>(v => v.SequenceNumber == expectedSequence)))
                .Verifiable();
            _target.HandleHandpayCompleted(new HandpayTransaction { Barcode = barcode });

            _validationDataProvider.Verify();
        }

        [DataRow((ulong)13302019, DisplayName = "Invalid date fails validation")]
        [DataRow((ulong)01121950, DisplayName = "Dates in the past fail validation")]
        [DataTestMethod]
        public void BadExpirationDateForRestrictedTicketsFailsValidation(ulong restrictedExpirationDays)
        {
            const ulong amount = 100;

            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.VoucherOutNonCash, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.VoucherOut, It.IsAny<bool>()))
                .Returns(true);
            _ticketingCoordinator.SetupGet(t => t.TicketExpirationRestricted).Returns(restrictedExpirationDays);

            var ticketOutValidation = _target.HandleTicketOutValidation(amount, TicketType.Restricted);
            Assert.IsTrue(ticketOutValidation.Wait(WaitTime));
            Assert.IsNull(ticketOutValidation.Result);
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
                Barcode = "006429188185446104",
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
            _ticketingCoordinator.SetupGet(t => t.TicketExpirationRestricted).Returns(restrictedExpiration);
            _ticketingCoordinator.Setup(x => x.GetData()).Returns(new TicketStorageData { PoolId = poolId });

            var ticketOutValidation = _target.HandleTicketOutValidation(amount, TicketType.Restricted);
            Assert.IsTrue(ticketOutValidation.Wait(WaitTime));
            TicketOutMatched(expectedResults, ticketOutValidation.Result);
            _propertiesManager.VerifyAll();
        }

        [TestMethod]
        public void UnConfiguredValidationFails()
        {
            const ulong amount = 100;

            _validationDataProvider.Setup(x => x.GetData()).Returns(new ValidationInformation { ValidationConfigured = false });

            var ticketOutValidation = _target.HandleTicketOutValidation(amount, TicketType.CashOut);
            Assert.IsTrue(ticketOutValidation.Wait(WaitTime));
            Assert.IsNull(ticketOutValidation.Result);
        }

        [DynamicData(nameof(InitializationTestData))]
        [DataTestMethod]
        public void InitializationTest(IList<ITransaction> transactions, uint expectedSequenceId)
        {
            const int timeOut = 1000;
            var waiter = new AutoResetEvent(false);

            _validationDataProvider.Setup(x => x.GetData()).Returns(new ValidationInformation
            {
                ValidationConfigured = false,
                MachineValidationId = 0
            });

            _propertiesManager.Setup(
                c => c.GetProperty(
                    It.Is<string>(prop => prop == SasProperties.SasShutdownCommandReceivedKey),
                    It.IsAny<bool>())).Returns(false);
            _exceptionHandler.Setup(
                    x => x.ReportException(
                        It.Is<ISasExceptionCollection>(
                            ex => ex.ExceptionCode == GeneralExceptionCode.ValidationIdNotConfigured)))
                .Callback(() => waiter.Set());
            _sasDisableProvider.Setup(x => x.Disable(SystemDisablePriority.Normal, DisableState.ValidationIdNeeded))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _transactionHistory.Setup(c => c.RecallTransactions()).Returns(transactions.OrderBy(x => x.TransactionId));
            if (expectedSequenceId != SequenceNumber)
            {
                _validationDataProvider.Setup(x => x.Save(It.Is<ValidationInformation>(v => v.SequenceNumber == expectedSequenceId)));
            }

            _target.Initialize();
            Assert.IsTrue(waiter.WaitOne(timeOut));
            _sasDisableProvider.Verify();
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
                Barcode = "006429188185446104",
                Pool = 0
            };

            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.ValidateHandpays, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.EnableReceipts, It.IsAny<bool>()))
                .Returns(printReceipts);

            var handpayValidation = _target.HandleHandPayValidation(amount, handPayType);
            Assert.IsTrue(handpayValidation.Wait(WaitTime));
            TicketOutMatched(expectedResults, handpayValidation.Result);
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
        [DataTestMethod]
        public void CanValidateTicketOutTest(
            bool canValidate,
            bool validationConfigured,
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
            _validationDataProvider.Setup(x => x.GetData()).Returns(new ValidationInformation { ValidationConfigured = validationConfigured });

            Assert.AreEqual(canValidate, _target.CanValidateTicketOutRequest(amount, ticketType));
        }

        private SecureEnhancedValidationHandler CreateTarget(
            bool nullProperties = false,
            bool nullTransactionHistory = false,
            bool nullSasDisable = false,
            bool nullTicketingCoordinator = false,
            bool nullExceptionHandler = false,
            bool nullValidationDataProvider = false)
        {
            return new SecureEnhancedValidationHandler(
                nullProperties ? null : _propertiesManager.Object,
                nullTransactionHistory ? null : _transactionHistory.Object,
                nullSasDisable ? null : _sasDisableProvider.Object,
                nullTicketingCoordinator ? null : _ticketingCoordinator.Object,
                nullExceptionHandler ? null : _exceptionHandler.Object,
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