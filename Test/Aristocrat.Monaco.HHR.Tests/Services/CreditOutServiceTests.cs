namespace Aristocrat.Monaco.Hhr.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Wat;
    using Client.Messages;
    using Client.WorkFlow;
    using Aristocrat.Monaco.Hhr.Services;
    using Application.Contracts;
    using Gaming.Contracts;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using HandpayType = Accounting.Contracts.Handpay.HandpayType;
    using Aristocrat.Monaco.Test.Common;

    [TestClass]
    public class CreditOutServiceTests
    {
        private readonly Mock<IBank> _bank = new Mock<IBank>(MockBehavior.Default);
        private readonly Mock<ICentralManager> _centralManager = new Mock<ICentralManager>(MockBehavior.Default);
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Default);
        private readonly Mock<IGameProvider> _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);
        private readonly Mock<ITransactionIdProvider> _idProvider = new Mock<ITransactionIdProvider>(MockBehavior.Default);
        private readonly Mock<IPlayerSessionService> _playerSession = new Mock<IPlayerSessionService>(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _properties = new Mock<IPropertiesManager>(MockBehavior.Default);
        private readonly Mock<IGameDataService> _gameDataService = new Mock<IGameDataService>(MockBehavior.Default);

        private Action<WatTransferCommittedEvent> _aftOutHandler;

        private Action<HandpayKeyedOffEvent> _handPayKeyedOffHandler;

        private bool _requestSent;

        private CreditOutService _sut;
        private Action<VoucherIssuedEvent> _voucherOutHandler;

        [TestInitialize]
        public void Initialize()
        {
            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<CreditOutService>(),
                        It.IsAny<Action<HandpayKeyedOffEvent>>()))
                .Callback<object, Action<HandpayKeyedOffEvent>
                >((y, x) => _handPayKeyedOffHandler = x);

            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<CreditOutService>(),
                        It.IsAny<Action<VoucherIssuedEvent>>()))
                .Callback<object, Action<VoucherIssuedEvent>
                >((y, x) => _voucherOutHandler = x);

            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<CreditOutService>(),
                        It.IsAny<Action<WatTransferCommittedEvent>>()))
                .Callback<object, Action<WatTransferCommittedEvent>
                >((y, x) => _aftOutHandler = x);

            _bank.Setup(b => b.QueryBalance(AccountType.Cashable)).Returns(It.IsAny<long>());
            _bank.Setup(b => b.QueryBalance(AccountType.NonCash)).Returns(It.IsAny<long>());
            _bank.Setup(b => b.QueryBalance(AccountType.Promo)).Returns(It.IsAny<long>());
            _idProvider.Setup(i => i.GetNextTransactionId()).Returns(It.IsAny<uint>());

            _properties.Setup(p =>
                p.GetProperty(GamingConstants.SelectedGameId, It.IsAny<int>())).Returns(1);
            _properties.Setup(p =>
                    p.GetProperty(HHRPropertyNames.LastGamePlayTime, It.IsAny<uint>()))
                .Returns(12345u);
            _properties.Setup(p =>
                p.GetProperty(GamingConstants.IsGameRunning, It.IsAny<bool>())).Returns(true);
            _properties.Setup(p =>
                p.GetProperty(GamingConstants.SelectedDenom, It.IsAny<long>())).Returns(10L);

            var denomination = new Mock<IDenomination>();
            denomination.SetupGet(x => x.Value).Returns(10L);

            var gameInfoList = new List<IGameDetail>
            {
                new MockGameInfo {Denominations = new List<IDenomination> {denomination.Object}, Id = 1},
                new MockGameInfo {Denominations = new List<IDenomination> {denomination.Object}, Id = 2}
            };

            _properties.Setup(p => p.GetProperty(GamingConstants.Games, null))
                .Returns(gameInfoList);

            _gameProvider.Setup(m => m.GetGame(It.IsAny<int>())).Returns(gameInfoList[0]);

            CreateCreditOutService();

            _requestSent = false;
        }

        [DataRow(true, false, false, false, false, false, false, DisplayName = "Null EventBus throws exception")]
        [DataRow(false, true, false, false, false, false, false, DisplayName = "Null CentralManager throws exception")]
        [DataRow(false, false, true, false, false, false, false, DisplayName = "Null PlayerSessionService throws exception")]
        [DataRow(false, false, false, true, false, false, false, DisplayName = "Null IIdProvider throws exception")]
        [DataRow(false, false, false, false, true, false, false, DisplayName = "Null Properties throws exception")]
        [DataRow(false, false, false, false, false, true, false, DisplayName = "Null Bank throws exception")]
        [DataRow(false, false, false, false, false, false, true, DisplayName = "Null game data service throws exception")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_InvalidParam_ThrowsException(
            bool nullEventBus,
            bool nullCentralManager,
            bool nullPlayerService,
            bool nullIdProvider,
            bool nullProperties,
            bool nullBank,
            bool nullGameDataService)
        {
            _ = CreateCreditOutService(
                nullEventBus, nullCentralManager, nullPlayerService, nullProperties, nullIdProvider, 
                nullBank, nullGameDataService);
        }

        [DataTestMethod]
        [DataRow(HandpayType.BonusPay)]
        [DataRow(HandpayType.GameWin)]
        public void HandPay_NotCancelledCredits_TransactionRequestNotSent(HandpayType type)
        {
            SetupTransactionRequestSend(false);

            Assert.IsNotNull(_handPayKeyedOffHandler);

            _handPayKeyedOffHandler.Invoke(new HandpayKeyedOffEvent(SetupDummyTransaction(type)));

            Assert.IsFalse(_requestSent);
        }

        [TestMethod]
        public void HandPay_CancelledCredits_TransactionRequestNotSent()
        {
            SetupTransactionRequestSend();

            Assert.IsNotNull(_handPayKeyedOffHandler);

            _handPayKeyedOffHandler.Invoke(new
                HandpayKeyedOffEvent(SetupDummyTransaction(HandpayType.CancelCredit)));

            Assert.IsFalse(_requestSent);
        }

        [DataTestMethod]
        [DataRow(AccountType.Cashable, 100, DisplayName = "Cashable Type with correct amount")]
        [DataRow(AccountType.Cashable, 0, DisplayName = "Cashable Type with zero amount")]
        [DataRow(AccountType.NonCash, 100, DisplayName = "NonCash Type with correct amount")]
        [DataRow(AccountType.Promo, 100, DisplayName = "Promo Type with correct amount")]
        public void VoucherOut_ValidAmount_TransactionRequestIsSent(AccountType accountType, int amount)
        {
            var transaction =
                new VoucherOutTransaction(1, DateTime.Now, amount,
                    accountType, "123", 5000, "12345")
                {
                    TransactionId = 123
                };

            if (amount > 0)
            {
                SetupTransactionRequestSend(false);
            }
            else
            {
                SetupTransactionRequestSend();
            }

            Assert.IsNotNull(_voucherOutHandler);

            _voucherOutHandler.Invoke(new VoucherIssuedEvent(transaction, new Ticket()));

            if (amount <= 0)
            {
                Assert.IsFalse(_requestSent);
            }
            else
            {
                _centralManager.Verify();
            }
        }

        [DataTestMethod]
        [DataRow(AccountType.Cashable, 100, DisplayName = "Cashable Type with correct amount")]
        [DataRow(AccountType.Cashable, 0, DisplayName = "Cashable Type with zero amount")]
        [DataRow(AccountType.NonCash, 100, DisplayName = "NonCash Type with correct amount")]
        [DataRow(AccountType.Promo, 100, DisplayName = "Promo Type with correct amount")]
        public void AftOut_ValidAmount_TransactionRequestIsSent(AccountType type, int amount)
        {
            var watTransaction = new WatTransaction();

            switch (type)
            {
                case AccountType.Cashable:
                    watTransaction.TransferredCashableAmount = amount;
                    break;

                case AccountType.Promo:
                    watTransaction.TransferredPromoAmount = amount;
                    break;

                case AccountType.NonCash:
                    watTransaction.TransferredNonCashAmount = amount;
                    break;

                default:
                    watTransaction.TransferredCashableAmount = amount;
                    break;
            }

            if (amount > 0)
            {
                SetupTransactionRequestSend(false);
            }
            else
            {
                SetupTransactionRequestSend();
            }

            Assert.IsNotNull(_aftOutHandler);

            _aftOutHandler.Invoke(new WatTransferCommittedEvent(watTransaction));

            if (amount <= 0)
            {
                Assert.IsFalse(_requestSent);
            }
            else
            {
                _centralManager.Verify();
            }
        }

        private void SetupTransactionRequestSend(bool expectSend = true)
        {
            if (expectSend)
            {
                _centralManager.Setup(c =>
                    c.Send<TransactionRequest, CloseTranResponse>(It.IsAny<TransactionRequest>(),
                        It.IsAny<CancellationToken>())).Verifiable();
            }
            else
            {
                _centralManager.Setup(c =>
                        c.Send<TransactionRequest, CloseTranResponse>(It.IsAny<TransactionRequest>(),
                            It.IsAny<CancellationToken>()))
                    .Callback((Request request, CancellationToken token) => _requestSent = true);
            }
        }

        private void SetupTransactionRequestError()
        {
            _centralManager.Setup(c =>
                    c.Send<TransactionRequest, CloseTranResponse>(It.IsAny<TransactionRequest>(),
                        It.IsAny<CancellationToken>()))
                .Throws(new UnexpectedResponseException(CreateErrorResponse()))
                .Verifiable();
        }

        private CreditOutService CreateCreditOutService(
            bool nullEventBus = false,
            bool nullCentralManager = false,
            bool nullPlayerService = false,
            bool nullProperties = false,
            bool nullIdProvider = false,
            bool nullBank = false,
            bool nullGameDataService = false)
        {
            _sut = new CreditOutService(
                nullEventBus ? null : _eventBus.Object,
                nullCentralManager ? null : _centralManager.Object,
                nullPlayerService ? null : _playerSession.Object,
                nullIdProvider ? null : _idProvider.Object,
                nullProperties ? null : _properties.Object,
                nullBank ? null : _bank.Object,
                nullGameDataService ? null : _gameDataService.Object);

            return _sut;
        }

        private static CloseTranErrorResponse CreateErrorResponse()
        {
            return new CloseTranErrorResponse
            {
                Status = Status.Error
            };
        }

        /// <summary>
        ///     Create a dummy HandpayTransaction transaction for use in the test methods.
        /// </summary>
        /// <returns>A HandpayTransaction transaction matching the values in the base expected ticket.</returns>
        private static HandpayTransaction SetupDummyTransaction(HandpayType handPayType)
        {
            // create a fake transaction with deviceId = 3, time = now transactionId = 1234,
            // amount = $10.45 log sequence = 987, barcode = 123459876, and expiration from constant Expiration
            var transaction = new HandpayTransaction(
                3,
                DateTime.Now,
                1045000,
                0,
                0,
                handPayType,
                true,
                Guid.NewGuid()) {LogSequence = 987, Barcode = "123459876", HostOnline = true};

            return transaction;
        }
    }
}