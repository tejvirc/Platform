namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;
    using Sas.Ticketing;
    using Sas.VoucherValidation;
    using Sas.Contracts;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Monaco.Protocol.Common.Storage.Repositories;
    using Aristocrat.Monaco.Protocol.Common.Storage.Entity;
    using System.Linq;

    [TestClass]
    public class LP71RedeemTicketHandlerTests
    {
        private const int WaitTime = 1000;

        private TicketingCoordinator _ticketingCoordinator;
        private LP71RedeemTicketHandler _target;
        private Mock<IPropertiesManager> _propertiesManger;
        private Mock<IBank> _bank;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactory;
        private Mock<IUnitOfWork> _unitOfWork;
        private Mock<ITransactionHistory> _transactionHistory;
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private Mock<IEventBus> _bus;
        private Mock<ITicketingCoordinator> _ticketCoordinator;
        private Mock<IRepository<TicketStorageData>> _ticketRepository;
        private SasVoucherInProvider _sasVoucherInProvider;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Default);
            _ticketCoordinator = new Mock<ITicketingCoordinator>(MockBehavior.Default);
            _bank = new Mock<IBank>(MockBehavior.Default);
            _propertiesManger = new Mock<IPropertiesManager>(MockBehavior.Default);
            _unitOfWorkFactory = new Mock<IUnitOfWorkFactory>(MockBehavior.Default);
            _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Default);
            _transactionHistory = new Mock<ITransactionHistory>(MockBehavior.Default);
            _ticketRepository = new Mock<IRepository<TicketStorageData>>(MockBehavior.Default);
            _bus = new Mock<IEventBus>(MockBehavior.Default);

            var transaction = new Mock<IPersistentStorageTransaction>();
            IReadOnlyCollection < VoucherInTransaction > transactions = new List<VoucherInTransaction>
            {
                new VoucherInTransaction()
            };

            _ticketCoordinator.Setup(x => x.GetData()).Returns(new TicketStorageData());
            _unitOfWorkFactory.Setup(x => x.Create()).Returns(_unitOfWork.Object);
            _unitOfWork.Setup(x => x.Repository<TicketStorageData>()).Returns(_ticketRepository.Object);
            _ticketRepository.Setup(x => x.Queryable()).Returns(Enumerable.Empty<TicketStorageData>().AsQueryable());
            _transactionHistory.Setup(x => x.RecallTransactions<VoucherInTransaction>()).Returns(transactions);
            _propertiesManger.Setup(c => c.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ValidationType = SasValidationType.SecureEnhanced });
            _propertiesManger.Setup(x => x.GetProperty(PropertyKey.VoucherIn, It.IsAny<bool>()))
                .Returns(true);

            _ticketingCoordinator = new TicketingCoordinator(_unitOfWorkFactory.Object, _propertiesManger.Object);
            _sasVoucherInProvider = new SasVoucherInProvider(_exceptionHandler.Object, _transactionHistory.Object, _propertiesManger.Object, _bus.Object, _ticketCoordinator.Object, _bank.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTicketCoordinatorTest()
        {
            _target = new LP71RedeemTicketHandler(null, _bank.Object, _propertiesManger.Object, _sasVoucherInProvider);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullBankTest()
        {
            _target = new LP71RedeemTicketHandler(_ticketingCoordinator, null, _propertiesManger.Object, _sasVoucherInProvider);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPropertiesManagerTest()
        {
            _target = new LP71RedeemTicketHandler(_ticketingCoordinator, _bank.Object, null, _sasVoucherInProvider);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullSasVoucherInProviderTest()
        {
            _target = new LP71RedeemTicketHandler(_ticketingCoordinator, _bank.Object, _propertiesManger.Object, null);
        }

        [DataRow(
            SasValidationType.SecureEnhanced,
            "1234567890",
            100L,
            -1,
            0,
            TicketTransferCode.ValidCashableTicket,
            RedemptionStatusCode.CashableTicketRedeemed,
            DisplayName = "Valid Cashable Ticket Redeemed")]
        [DataRow(
            SasValidationType.SecureEnhanced,
            "1234567890",
            100L,
            -1,
            30,
            TicketTransferCode.ValidRestrictedPromotionalTicket,
            RedemptionStatusCode.RestrictedPromotionalTicketRedeemed,
            DisplayName = "Valid Restricted Promotional Ticket Redeemed")]
        [DataRow(
            SasValidationType.SecureEnhanced,
            "1234567890",
            100L,
            0,
            0,
            TicketTransferCode.ValidNonRestrictedPromotionalTicket,
            RedemptionStatusCode.NonRestrictedPromotionalTicketRedeemed,
            DisplayName = "Valid Non Restricted Promotional Ticket Redeemed")]
        [DataRow(
            SasValidationType.System,
            "1234567890",
            100L,
            -1,
            0,
            TicketTransferCode.ValidCashableTicket,
            RedemptionStatusCode.CashableTicketRedeemed,
            DisplayName = "System validation test")]
        [DataTestMethod]
        public void SuccessfulTicketRedeemed(
            SasValidationType validation,
            string barcode,
            long ticketAmount,
            int poolId,
            long expiration,
            TicketTransferCode transferCode,
            RedemptionStatusCode statusCode)
        {
            _bank.Setup(x => x.QueryBalance()).Returns(0);

            _propertiesManger.Setup(c => c.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ValidationType = validation });
            _propertiesManger.Setup(
                x => x.GetProperty(AccountingConstants.VoucherOutNonCashExpirationDays, 0))
                .Returns(0);
            _target = new LP71RedeemTicketHandler(_ticketingCoordinator, _bank.Object, _propertiesManger.Object, _sasVoucherInProvider);

            var ticketInResult = ConfigureValidTicketIn(barcode);
            var response = _target.Handle(
                new RedeemTicketData
                {
                    Barcode = barcode,
                    ParsingCode = ParsingCode.Bcd,
                    PoolId = poolId,
                    RestrictedExpiration = expiration,
                    TransferCode = transferCode,
                    TransferAmount = (ulong)ticketAmount
                });

            Assert.IsTrue(ticketInResult.Wait(WaitTime));
            var ticketInInfo = ticketInResult.Result;
            Assert.AreEqual(ticketInInfo.Barcode, barcode);
            Assert.AreEqual(ticketInInfo.Amount, (ulong)ticketAmount);
            Assert.AreEqual(_sasVoucherInProvider.CurrentState, SasVoucherInState.RequestPending);
            Assert.AreEqual(_sasVoucherInProvider.CurrentTicketInfo.RedemptionStatusCode, RedemptionStatusCode.TicketRedemptionPending);
            Assert.AreEqual(response.Barcode, barcode);
            Assert.AreEqual(response.MachineStatus, RedemptionStatusCode.TicketRedemptionPending);
            Assert.AreEqual(response.ParsingCode, ParsingCode.Bcd);
            Assert.AreEqual(response.TransferAmount, (ulong)ticketAmount);
        }

        [DataRow(
            "1234567890",
            100L,
            0L,
            3,
            10,
            20,
            5,
            5,
            TicketTransferCode.ValidRestrictedPromotionalTicket,
            RedemptionStatusCode.RestrictedPromotionalTicketRedeemed,
            DisplayName = "Zero restricted balance ignores set pool ID")]
        [DataRow(
            "1234567890",
            100L,
            200L,
            10,
            10,
            30,
            0,
            30,
            TicketTransferCode.ValidRestrictedPromotionalTicket,
            RedemptionStatusCode.RestrictedPromotionalTicketRedeemed,
            DisplayName = "Matching pool IDs when there are restricted credits on the machine")]
        [DataRow(
            "1234567890",
            100L,
            200L,
            10,
            10,
            30,
            09052520,
            30,
            TicketTransferCode.ValidRestrictedPromotionalTicket,
            RedemptionStatusCode.RestrictedPromotionalTicketRedeemed,
            DisplayName = "Expiration provided as a date can't override an already set days expiration when we have restricted credits")]
        [DataRow(
            "1234567890",
            100L,
            200L,
            10,
            10,
            09052019,
            09052020,
            09052020,
            TicketTransferCode.ValidRestrictedPromotionalTicket,
            RedemptionStatusCode.RestrictedPromotionalTicketRedeemed,
            DisplayName = "Expiration provided as a date can override an older date")]
        [DataRow(
            "1234567890",
            100L,
            200L,
            10,
            10,
            30,
            31,
            31,
            TicketTransferCode.ValidRestrictedPromotionalTicket,
            RedemptionStatusCode.RestrictedPromotionalTicketRedeemed,
            DisplayName = "Expiration in days overrides if newer")]
        [DataRow(
            "1234567890",
            100L,
            200L,
            10,
            10,
            30,
            5,
            30,
            TicketTransferCode.ValidRestrictedPromotionalTicket,
            RedemptionStatusCode.RestrictedPromotionalTicketRedeemed,
            DisplayName = "Expiration in days doesn't overrides if older")]
        [DataRow(
            "1234567890",
            100L,
            200L,
            10,
            10,
            30,
            0,
            30,
            TicketTransferCode.ValidRestrictedPromotionalTicket,
            RedemptionStatusCode.RestrictedPromotionalTicketRedeemed,
            DisplayName = "Zero Expiration date use the default expiration")]
        [DataTestMethod]
        public void ValidRestrictedCreditTest(
            string barcode,
            long ticketAmount,
            long currentRestrictedBalance,
            int poolId,
            int currentPoolId,
            int currentExpiration,
            long expiration,
            long expectedExpiration,
            TicketTransferCode transferCode,
            RedemptionStatusCode statusCode)
        {
            _bank.Setup(x => x.QueryBalance(AccountType.NonCash)).Returns(currentRestrictedBalance);
            _bank.Setup(x => x.QueryBalance()).Returns(currentRestrictedBalance);
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, TicketStorageData>>()))
                .Returns(new TicketStorageData { PoolId = currentPoolId });
            _ticketRepository.Setup(x => x.Update(It.Is<TicketStorageData>(t => t.PoolId == poolId)));

            _propertiesManger.Setup(
                    x => x.GetProperty(AccountingConstants.VoucherOutNonCashExpirationDays, It.IsAny<int>()))
                .Returns(currentExpiration);
            _ticketingCoordinator = new TicketingCoordinator(_unitOfWorkFactory.Object, _propertiesManger.Object);
            _target = new LP71RedeemTicketHandler(_ticketingCoordinator, _bank.Object, _propertiesManger.Object, _sasVoucherInProvider);

            var ticketInResult = ConfigureValidTicketIn(barcode);
            var response = _target.Handle(
                new RedeemTicketData
                {
                    Barcode = barcode,
                    ParsingCode = ParsingCode.Bcd,
                    PoolId = poolId,
                    RestrictedExpiration = expiration,
                    TransferCode = transferCode,
                    TransferAmount = (ulong)ticketAmount
                });

            Assert.IsTrue(ticketInResult.Wait(WaitTime));
            var ticketInInfo = ticketInResult.Result;
            Assert.AreEqual(ticketInInfo.Barcode, barcode);
            Assert.AreEqual(ticketInInfo.Amount, (ulong)ticketAmount);
            Assert.AreEqual(_sasVoucherInProvider.CurrentState, SasVoucherInState.RequestPending);
            Assert.AreEqual(_sasVoucherInProvider.CurrentTicketInfo.RedemptionStatusCode, RedemptionStatusCode.TicketRedemptionPending);
            Assert.AreEqual(response.Barcode, barcode);
            Assert.AreEqual(response.MachineStatus, RedemptionStatusCode.TicketRedemptionPending);
            Assert.AreEqual(response.ParsingCode, ParsingCode.Bcd);
            Assert.AreEqual(response.TransferAmount, (ulong)ticketAmount);
            Assert.AreEqual(_ticketingCoordinator.TicketExpirationRestricted, (ulong)expectedExpiration);
        }

        [DataRow(
            "1234567890",
            100L,
            100L,
            3,
            10,
            0,
            TicketTransferCode.ValidRestrictedPromotionalTicket,
            RedemptionStatusCode.GamingMachineUnableToAcceptTransfer,
            DisplayName = "Mismatch Pool ID")]
        [DataRow(
            "1234567890",
            100L,
            0L,
            3,
            3,
            0,
            TicketTransferCode.TicketAlreadyRedeemed,
            RedemptionStatusCode.TicketRejectedByHost,
            DisplayName = "Ticket already redeemed reject transfer")]
        [DataRow(
            "1234567890",
            100L,
            0L,
            3,
            3,
            0,
            TicketTransferCode.NotAValidValidationNumber,
            RedemptionStatusCode.TicketRejectedByHost,
            DisplayName = "Not a valid validation number reject transfer")]
        [DataRow(
            "1234567890",
            100L,
            0L,
            3,
            3,
            0,
            TicketTransferCode.TicketAmountDoesNotMatchSystem,
            RedemptionStatusCode.TicketRejectedByHost,
            DisplayName = "Ticket amount does not match system reject transfer")]
        [DataRow(
            "1234567890",
            100L,
            0L,
            3,
            3,
            0,
            TicketTransferCode.TicketExpired,
            RedemptionStatusCode.TicketRejectedByHost,
            DisplayName = "Ticket Expired reject transfer")]
        [DataRow(
            "1234567890",
            100L,
            0L,
            3,
            3,
            0,
            TicketTransferCode.TicketAmountExceedsAutoRedemptionLimit,
            RedemptionStatusCode.TicketRejectedByHost,
            DisplayName = "Ticket Amount Exceeds Auto Redemption Limit reject transfer")]
        [DataRow(
            "1234567890",
            100L,
            0L,
            3,
            3,
            0,
            TicketTransferCode.ValidationInfoNotAvailable,
            RedemptionStatusCode.TicketRejectedByHost,
            DisplayName = "Validation info not available reject transfer")]
        [DataRow(
            "1234567890",
            100L,
            0L,
            3,
            3,
            0,
            TicketTransferCode.UnableToValidate,
            RedemptionStatusCode.TicketRejectedByHost,
            DisplayName = "Unable to validate reject transfer")]
        [DataRow(
            "1234567890",
            100L,
            0L,
            3,
            3,
            0,
            TicketTransferCode.ValidationNumberNotInSystem,
            RedemptionStatusCode.TicketRejectedByHost,
            DisplayName = "Validation number not in system reject transfer")]
        [DataRow(
            "1234567890",
            100L,
            0L,
            3,
            3,
            0,
            TicketTransferCode.TicketMarkedPendingInSystem,
            RedemptionStatusCode.TicketRejectedByHost,
            DisplayName = "Ticket marked pending in system reject transfer")]
        [DataTestMethod]
        public void FailedRedeemTest(
            string barcode,
            long ticketAmount,
            long currentRestrictedBalance,
            int poolId,
            int currentPoolId,
            long expiration,
            TicketTransferCode transferCode,
            RedemptionStatusCode statusCode)
        {
            _bank.Setup(x => x.QueryBalance(AccountType.NonCash)).Returns(currentRestrictedBalance);
            _bank.Setup(x => x.QueryBalance()).Returns(currentRestrictedBalance);
            _propertiesManger.Setup(
                x => x.GetProperty(AccountingConstants.VoucherOutNonCashExpirationDays, 0))
                .Returns(0);
            _ticketRepository.Setup(x => x.Queryable()).Returns(new List<TicketStorageData> { new TicketStorageData { PoolId = currentPoolId } }.AsQueryable());
            _ticketRepository.Setup(x => x.Update(It.Is<TicketStorageData>(t => t.PoolId == poolId)));

            _target = new LP71RedeemTicketHandler(_ticketingCoordinator, _bank.Object, _propertiesManger.Object, _sasVoucherInProvider);

            var ticketInResult = ConfigureValidTicketIn(barcode);
            var response = _target.Handle(
                new RedeemTicketData
                {
                    Barcode = barcode,
                    ParsingCode = ParsingCode.Bcd,
                    PoolId = poolId,
                    RestrictedExpiration = expiration,
                    TransferCode = transferCode,
                    TransferAmount = (ulong)ticketAmount
                });

            Assert.IsTrue(ticketInResult.Wait(WaitTime));
            var ticketInInfo = ticketInResult.Result;
            Assert.AreEqual(ticketInInfo.Barcode, barcode);
            Assert.AreEqual(ticketInInfo.Amount, (ulong)0);
            Assert.AreEqual(response.Barcode, barcode);
            Assert.AreEqual(response.MachineStatus, statusCode);
            Assert.AreEqual(response.ParsingCode, ParsingCode.Bcd);
            Assert.AreEqual(response.TransferAmount, (ulong)0);
        }

        [TestMethod]
        public void RequestForCurrentTicketOnlyTest()
        {
            const string barcode = "1234567";
            const long ticketAmount = 1000;
            const long currentBankBalance = 100;
            const TicketTransferCode transferCode = TicketTransferCode.RequestForCurrentTicketStatus;

            _bank.Setup(x => x.QueryBalance()).Returns(currentBankBalance.CentsToMillicents());

            _target = new LP71RedeemTicketHandler(_ticketingCoordinator, _bank.Object, _propertiesManger.Object, _sasVoucherInProvider);

            var ticketInResult = ConfigureValidTicketIn(barcode);
            var response = _target.Handle(
                new RedeemTicketData
                {
                    Barcode = barcode,
                    ParsingCode = ParsingCode.Bcd,
                    TransferCode = transferCode,
                    TransferAmount = ticketAmount
                });

            Assert.IsFalse(ticketInResult.Wait(WaitTime));
        }

        [TestMethod]
        public void RedemptionDisabledTest()
        {
            const string barcode = "1234567";
            const long ticketAmount = 1000;
            const long currentBankBalance = 100;
            const TicketTransferCode transferCode = TicketTransferCode.ValidCashableTicket;

            _bank.Setup(x => x.QueryBalance()).Returns(currentBankBalance.CentsToMillicents());

            _propertiesManger.Setup(x => x.GetProperty(PropertyKey.VoucherIn, It.IsAny<bool>()))
                .Returns(false).Verifiable();

            _target = new LP71RedeemTicketHandler(_ticketingCoordinator, _bank.Object, _propertiesManger.Object, _sasVoucherInProvider);

            var ticketInResult = ConfigureValidTicketIn(barcode);
            var response = _target.Handle(
                new RedeemTicketData
                {
                    Barcode = barcode,
                    ParsingCode = ParsingCode.Bcd,
                    TransferCode = transferCode,
                    TransferAmount = ticketAmount
                });

            _propertiesManger.Verify();
            Assert.AreEqual(response.MachineStatus, RedemptionStatusCode.TicketRedemptionDisabled);
            Assert.IsTrue(ticketInResult.Wait(WaitTime));
        }

        private Task<TicketInInfo> ConfigureValidTicketIn(string barcode)
        {
            _exceptionHandler.Setup(
                x => x.ReportException(
                    It.Is<ISasExceptionCollection>(
                        ex => ex.ExceptionCode == GeneralExceptionCode.TicketHasBeenInserted)));
            var transaction = new VoucherInTransaction(0, new DateTime(), barcode);
            var validateTicketIn = _sasVoucherInProvider.ValidationTicket(transaction);
            _sasVoucherInProvider.RequestValidationData();
            return validateTicketIn;
        }
    }
}