namespace Aristocrat.Monaco.Hhr.Tests.Services
{
    using System;
    using System.Threading;
    using Accounting.Contracts;
    using Hhr.Client.Messages;
    using Hhr.Client.WorkFlow;
    using Hhr.Services;
    using Application.Contracts;
    using Gaming.Contracts;
    using Gaming.Contracts.Bonus;
    using Gaming.Contracts.Payment;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System.IO;
    using Mono.Addins;
    using Aristocrat.Monaco.Test.Common;

    [TestClass]
    public class BonusServiceTests
    {
        private readonly Mock<IBank> _bank = new Mock<IBank>(MockBehavior.Default);
        private readonly Mock<ICentralManager> _centralManager = new Mock<ICentralManager>(MockBehavior.Default);
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Default);
        private readonly Mock<ITransactionIdProvider> _idProvider = new Mock<ITransactionIdProvider>(MockBehavior.Default);
        private readonly Mock<IPaymentDeterminationProvider> _paymentDeterminationProvider =
            new Mock<IPaymentDeterminationProvider>(MockBehavior.Default);
        private readonly Mock<IPlayerSessionService> _playerSession =
            new Mock<IPlayerSessionService>(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _properties = new Mock<IPropertiesManager>(MockBehavior.Default);
        private readonly Mock<IGameDataService> _gameDataService = new Mock<IGameDataService>(MockBehavior.Default);

        private Action<BonusAwardedEvent> _bonusAwardedHandler;
        private TransactionRequest _lastRequestSent;

        private bool _requestSent;

        private BonusService _target;

        [TestInitialize]
        public void Initialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            _eventBus.Setup(bus => bus.Subscribe(
                    It.IsAny<BonusService>(),
                    It.IsAny<Action<BonusAwardedEvent>>()))
                .Callback<object, Action<BonusAwardedEvent>>((subscriber, callback) =>
                    _bonusAwardedHandler = callback);

            _bank.Setup(b => b.QueryBalance(AccountType.Cashable)).Returns(It.IsAny<long>());
            _bank.Setup(b => b.QueryBalance(AccountType.Promo)).Returns(It.IsAny<long>());

            _idProvider.Setup(i => i.GetNextTransactionId()).Returns(It.IsAny<uint>());

            _properties.Setup(p => p.GetProperty(GamingConstants.SelectedGameId, It.IsAny<int>())).Returns(1);
            _properties.Setup(p => p.GetProperty(HHRPropertyNames.LastGamePlayTime, It.IsAny<uint>()))
                .Returns(12345u);

            CreateBonusAwardService();

            _requestSent = false;
            _lastRequestSent = null;
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
            try
            {
                AddinManager.Shutdown();
            }
            catch (InvalidOperationException)
            {
                // temporarily swallow exception
            }
        }

        [DataRow(true, false, false, false, false, false, false, false, DisplayName = "Null EventBus test")]
        [DataRow(false, true, false, false, false, false, false, false, DisplayName = "Null CentralManager test")]
        [DataRow(false, false, true, false, false, false, false, false, DisplayName = "Null PlayerSessionService test")]
        [DataRow(false, false, false, true, false, false, false, false, DisplayName = "Null Properties test")]
        [DataRow(false, false, false, false, true, false, false, false, DisplayName = "Null IdProvider test")]
        [DataRow(false, false, false, false, false, true, false, false, DisplayName = "Null Bank test")]
        [DataRow(false, false, false, false, false, false, true, false, DisplayName = "Null payment determination service test")]
        [DataRow(false, false, false, false, false, false, false, true, DisplayName = "Null game data service test")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_InvalidParams_ThrowsException(
            bool nullEventBus,
            bool nullCentralManager,
            bool nullPlayerService,
            bool nullProperties,
            bool nullIdProvider,
            bool nullBank,
            bool nullPaymentDeterminationProvider,
            bool nullGameDataService)
        {
            _ = CreateBonusAwardService(nullEventBus,
                nullCentralManager,
                nullPlayerService,
                nullProperties,
                nullIdProvider, nullBank,
                nullPaymentDeterminationProvider,
                nullGameDataService);
        }

        [TestMethod]
        public void HandleBonusAwardedEvent_AmountIsZero_TransactionMessageNotSent()
        {
            SetupTransactionRequestSend(false);

            Assert.IsNotNull(_bonusAwardedHandler);
            Assert.IsFalse(_requestSent);

            _bonusAwardedHandler.Invoke(new BonusAwardedEvent(CreateCommittedBonusTransaction()));

            Assert.IsFalse(_requestSent);
        }

        [TestMethod]
        public void HandleBonusAwardedEvent_CashBonusToCreditMeter_TransactionMessageSent()
        {
            SetupTransactionRequestSend();

            Assert.IsNotNull(_bonusAwardedHandler);
            Assert.IsFalse(_requestSent);

            _bonusAwardedHandler.Invoke(new BonusAwardedEvent(CreateCommittedBonusTransaction(PayMethod.Credit, 2000)));

            Assert.IsTrue(_requestSent);
            Assert.IsNotNull(_lastRequestSent);
            Assert.AreEqual(2u, _lastRequestSent.Credit);
            Assert.AreEqual(CommandTransactionType.BonusWinToCreditMeter, _lastRequestSent.TransactionType);
        }

        [TestMethod]
        public void HandleBonusAwardedEvent_NonCashBonusToCreditMeter_TransactionMessageSent()
        {
            SetupTransactionRequestSend();

            Assert.IsNotNull(_bonusAwardedHandler);
            Assert.IsFalse(_requestSent);

            _bonusAwardedHandler.Invoke(
                new BonusAwardedEvent(CreateCommittedBonusTransaction(PayMethod.Credit, 0, 1000)));

            Assert.IsTrue(_requestSent);
            Assert.IsNotNull(_lastRequestSent);
            Assert.AreEqual(1u, _lastRequestSent.Credit);
            Assert.AreEqual(CommandTransactionType.BonusWinToCreditMeter, _lastRequestSent.TransactionType);
        }

        [TestMethod]
        public void HandleBonusAwardedEvent_BonusToCashoutVoucher_TransactionMessageSent()
        {
            SetupTransactionRequestSend();

            Assert.IsNotNull(_bonusAwardedHandler);
            Assert.IsFalse(_requestSent);

            _bonusAwardedHandler.Invoke(new BonusAwardedEvent(CreateCommittedBonusTransaction(PayMethod.Voucher, 3000)));

            Assert.IsTrue(_requestSent);
            Assert.IsNotNull(_lastRequestSent);
            Assert.AreEqual(3u, _lastRequestSent.Credit);
            Assert.AreEqual(CommandTransactionType.BonusWinToCashableOutTicket, _lastRequestSent.TransactionType);
        }

        [TestMethod]
        public void HandleBonusAwardedEvent_BonusToHandpay_TransactionMessageSent()
        {
            SetupTransactionRequestSend();

            Assert.IsNotNull(_bonusAwardedHandler);
            Assert.IsFalse(_requestSent);

            _bonusAwardedHandler.Invoke(new BonusAwardedEvent(CreateCommittedBonusTransaction(PayMethod.Handpay, 4000)));

            Assert.IsTrue(_requestSent);
            Assert.IsNotNull(_lastRequestSent);
            Assert.AreEqual(4u, _lastRequestSent.Credit);
            Assert.AreEqual(CommandTransactionType.BonusWinToHandpayNoReceipt, _lastRequestSent.TransactionType);
        }

        [TestMethod]
        public void WhenNoGamePlayed_WhenBonusExceedsIrsLimitWithMaxWager_ExpectPayTypeHandPay()
        {
            const int bonusWinInMillicents = 121000000;

            _properties.Setup(p => p.GetProperty(GamingConstants.IsGameRunning, It.IsAny<bool>())).Returns(true);
            _properties.Setup(p => p.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(1000000L);
            _properties.Setup(p => p.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>()))
                .Returns(120000000L);

            var bonusTrans = CreateCommittedBonusTransaction(PayMethod.Any, 10, 20, 30);
            var actual = _target.GetBonusPayMethod(bonusTrans, bonusWinInMillicents);
            Assert.AreEqual(PayMethod.Handpay, actual);
        }

        [TestMethod]
        public void
            WhenNoGamePlayed_WhenBonusNotExceedsIrsLimitWithMaxWagerAndCreditLimitIsExceeded_ExpectPayTypeVoucher()
        {
            const int bonusWinInMillicents = 40000000;

            _properties.Setup(p => p.GetProperty(GamingConstants.IsGameRunning, It.IsAny<bool>())).Returns(true);
            _properties.Setup(p => p.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(1000000L);
            _properties.Setup(p => p.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>()))
                .Returns(120000000L);

            var bonusTrans = CreateCommittedBonusTransaction(PayMethod.Any, 10, 20, 30);
            var test = _target.GetBonusPayMethod(bonusTrans, bonusWinInMillicents);
            Assert.AreEqual(PayMethod.Voucher, test);
        }

        [TestMethod]
        public void WhenLastGamePlayed_BonusExceedsIrsLimitWithMaxWager_ExpectPayTypeHandPay()
        {
            const int bonusWinInMillicents = 121000000;

            _properties.Setup(p => p.GetProperty(GamingConstants.IsGameRunning, It.IsAny<bool>())).Returns(true);
            _properties.Setup(p => p.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(1000000L);
            _properties.Setup(p => p.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>()))
                .Returns(120000000L);

            var bonusTrans = CreateCommittedBonusTransaction(PayMethod.Any, 10, 20, 30);
            var actual = _target.GetBonusPayMethod(bonusTrans, bonusWinInMillicents);

            Assert.AreEqual(PayMethod.Handpay, actual);
        }

        [TestMethod]
        public void
            WhenLastGamePlayed_WhenBonusNotExceedsIrsLimitWithMaxWagerAndCreditLimitIsExceeded_ExpectPayTypeVoucher()
        {
            const int bonusWinInMillicents = 61000000;

            _properties.Setup(p => p.GetProperty(GamingConstants.IsGameRunning, It.IsAny<bool>())).Returns(true);
                        _properties.Setup(p => p.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(1000000L);
            _properties.Setup(p => p.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>()))
                .Returns(120000000L);

            var bonusTrans = CreateCommittedBonusTransaction(PayMethod.Any, 10, 20, 30);
            var test = _target.GetBonusPayMethod(bonusTrans, bonusWinInMillicents);
            Assert.AreEqual(PayMethod.Voucher, test);
        }

        private void SetupTransactionRequestSend(bool expectSend = true)
        {
            if (expectSend)
                _centralManager.Setup(c =>
                    c.Send<TransactionRequest, CloseTranResponse>(It.IsAny<TransactionRequest>(),
                        It.IsAny<CancellationToken>())).Callback((Request request, CancellationToken token) =>
                {
                    _requestSent = true;
                    _lastRequestSent = request as TransactionRequest;
                });
            else
                _centralManager.Setup(c =>
                        c.Send<TransactionRequest, CloseTranResponse>(It.IsAny<TransactionRequest>(),
                            It.IsAny<CancellationToken>()))
                    .Callback((Request request, CancellationToken token) => _requestSent = true);
        }

        private BonusTransaction CreateCommittedBonusTransaction(PayMethod method = PayMethod.Any, long cashAmount = 0,
            long nonCashAmount = 0, long promoAmount = 0)
        {
            var bonus = new BonusTransaction(1,
                DateTime.UtcNow,
                "BonusId",
                cashAmount,
                nonCashAmount,
                promoAmount,
                1,
                1,
                method)
            {
                PaidCashableAmount = cashAmount,
                PaidNonCashAmount = nonCashAmount,
                PaidPromoAmount = promoAmount
            };

            return bonus;
        }

        private BonusService CreateBonusAwardService(
            bool nullEventBus = false,
            bool nullCentralManager = false,
            bool nullPlayerSessionService = false,
            bool nullBank = false,
            bool nullIdProvider = false,
            bool nullPropertiesManager = false,
            bool nullPaymentDeterminationProvider = false,
            bool nullGameDataService = false)
        {
            _target = new BonusService(
                nullEventBus ? null : _eventBus.Object,
                nullCentralManager ? null : _centralManager.Object,
                nullPlayerSessionService ? null : _playerSession.Object,
                nullBank ? null : _bank.Object,
                nullIdProvider ? null : _idProvider.Object,
                nullPropertiesManager ? null : _properties.Object,
                nullPaymentDeterminationProvider ? null : _paymentDeterminationProvider.Object,
                nullGameDataService ? null : _gameDataService.Object
            );

            return _target;
        }
    }
}