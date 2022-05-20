namespace Aristocrat.Monaco.Hhr.Tests.Services
{
    using System;
    using System.Threading;
    using Accounting.Contracts;
    using Client.Messages;
    using Client.WorkFlow;
    using Aristocrat.Monaco.Hhr.Services;
    using Application.Contracts;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Test.Common;
    using Aristocrat.Monaco.Application.Contracts.Localization;

    [TestClass]
    public class CreditInServiceTests
    {
        private readonly Mock<IBank> _bank = new Mock<IBank>(MockBehavior.Default);
        private readonly Mock<ICentralManager> _centralManager = new Mock<ICentralManager>(MockBehavior.Default);
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Default);
        private readonly Mock<IGameProvider> _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);
        private readonly Mock<ITransactionIdProvider> _idProvider = new Mock<ITransactionIdProvider>(MockBehavior.Default);
        private readonly Mock<IPlayerSessionService> _playerSession = new Mock<IPlayerSessionService>(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _properties = new Mock<IPropertiesManager>(MockBehavior.Default);
        private readonly Mock<IGameDataService> _gameDataService = new Mock<IGameDataService>(MockBehavior.Default);

        private Action<WatOnCompleteEvent> _aftInHandler;

        private Action<CurrencyInCompletedEvent> _currencyInHandler;

        private bool _requestSent;

        private CreditInService _sut;
        private Action<VoucherRedeemedEvent> _voucherInHandler;


        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            var locale = MoqServiceManager.CreateAndAddService<ILocalizerFactory>(MockBehavior.Strict);
            locale.Setup(x => x.For(It.IsAny<string>())).Returns(new Mock<ILocalizer>().Object);

            _eventBus.Setup(bus => bus.Subscribe(
                    It.IsAny<CreditInService>(),
                    It.IsAny<Action<CurrencyInCompletedEvent>>()))
                .Callback<object, Action<CurrencyInCompletedEvent>>((subsriber, callback) =>
                    _currencyInHandler = callback);

            _eventBus.Setup(bus => bus.Subscribe(It.IsAny<CreditInService>(), It.IsAny<Action<VoucherRedeemedEvent>>()))
                .Callback<object, Action<VoucherRedeemedEvent>>((subsriber, callback) => _voucherInHandler = callback);

            _eventBus.Setup(bus => bus.Subscribe(It.IsAny<CreditInService>(), It.IsAny<Action<WatOnCompleteEvent>>()))
                .Callback<object, Action<WatOnCompleteEvent>>((subsribre, callback) => _aftInHandler = callback);

            _bank.Setup(b => b.QueryBalance(AccountType.Cashable)).Returns(It.IsAny<long>());
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

            CreateCreditInService();

            _requestSent = false;
        }

        [DataRow(true, false, false, false, false, false, false, DisplayName = "Null EventBus throws exception")]
        [DataRow(false, true, false, false, false, false, false, DisplayName = "Null CentralManager throws exception")]
        [DataRow(false, false, true, false, false, false, false, DisplayName = "Null PlayerSessionService throws exception")]
        [DataRow(false, false, false, true, false, false, false, DisplayName = "Null Properties throws exception")]
        [DataRow(false, false, false, false, true, false, false, DisplayName = "Null IdProvier throws exception")]
        [DataRow(false, false, false, false, false, true, false, DisplayName = "Null Bank throws exception")]
        [DataRow(false, false, false, false, false, false, true, DisplayName = "Null game data service throws exception")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_InvalidParamas_ThrowsException(
            bool nullEventBus,
            bool nullCentralManager,
            bool nullPlayerService,
            bool nullProperties,
            bool nullIdProvider,
            bool nullbank,
            bool nullGameDataService)
        {
            _ = CreateCreditInService(
                nullEventBus, nullCentralManager, nullPlayerService, nullProperties, nullIdProvider,
                nullbank, nullGameDataService);
        }


        [TestMethod]
        public void CreditIn_ZeroAmount_TransactionRequestNotSent()
        {
            SetupTransactionRequestSend(false);

            Assert.IsNotNull(_currencyInHandler);

            _currencyInHandler.Invoke(new CurrencyInCompletedEvent(0, null, new BillTransaction()));

            Assert.IsFalse(_requestSent);
        }

        [TestMethod]
        public void CreditIn_IrregularAmount_TransactionRequestNotSent()
        {
            SetupTransactionRequestSend(false);

            Assert.IsNotNull(_currencyInHandler);

            _currencyInHandler.Invoke(new CurrencyInCompletedEvent(99999, null, new BillTransaction()));

            Assert.IsFalse(_requestSent);
        }

        [TestMethod]
        public void CreditIn_ValidAmount_TransactionRequestIsSent()
        {
            SetupTransactionRequestSend();

            Assert.IsNotNull(_currencyInHandler);

            _currencyInHandler.Invoke(new CurrencyInCompletedEvent(100000, null, new BillTransaction()));

            _centralManager.Verify();
        }

        [TestMethod]
        public void CreditIn_WhenException_DisablesSystem()
        {
            SetupTransactionRequestError();

            Assert.IsNotNull(_currencyInHandler);

            _currencyInHandler.Invoke(new CurrencyInCompletedEvent(100000, null, new BillTransaction()));

            _centralManager.Verify();

            //_disableManager.Verify();
        }

        [TestMethod]
        public void VoucherIn_ZeroAmount_TransactionRequestNotSent()
        {
            SetupTransactionRequestSend(false);

            Assert.IsNotNull(_voucherInHandler);

            _voucherInHandler.Invoke(new VoucherRedeemedEvent(new VoucherInTransaction {Amount = 0}));

            Assert.IsFalse(_requestSent);
        }

        [TestMethod]
        public void VoucherIn_ValidAmount_TransactionRequestIsSent()
        {
            SetupTransactionRequestSend();

            Assert.IsNotNull(_voucherInHandler);

            _voucherInHandler.Invoke(new VoucherRedeemedEvent(new VoucherInTransaction {Amount = 100000}));

            _centralManager.Verify();
        }

        [TestMethod]
        public void VoucherIn_WhenException_DisablesSystem()
        {
            SetupTransactionRequestError();

            Assert.IsNotNull(_voucherInHandler);

            _voucherInHandler.Invoke(new VoucherRedeemedEvent(new VoucherInTransaction {Amount = 100000}));

            _centralManager.Verify();

            //_disableManager.Verify();
        }

        [TestMethod]
        public void AftIn_ZeroAmount_TransactionRequestNotSent()
        {
            SetupTransactionRequestSend(false);

            Assert.IsNotNull(_aftInHandler);

            _aftInHandler.Invoke(new WatOnCompleteEvent(new WatOnTransaction {TransferredCashableAmount = 0}));

            Assert.IsFalse(_requestSent);
        }

        [TestMethod]
        public void AftIn_ValidAmount_TransactionRequestIsSent()
        {
            SetupTransactionRequestSend();

            Assert.IsNotNull(_voucherInHandler);

            _aftInHandler.Invoke(new WatOnCompleteEvent(new WatOnTransaction {TransferredCashableAmount = 100000}));

            _centralManager.Verify();
        }

        [TestMethod]
        public void AftIn_WhenException_DisablesSystem()
        {
            SetupTransactionRequestError();

            Assert.IsNotNull(_aftInHandler);

            _aftInHandler.Invoke(new WatOnCompleteEvent(new WatOnTransaction {TransferredCashableAmount = 100000}));

            _centralManager.Verify();
        }

        [TestMethod]
        public void CreditIn_CoinInWithNullTransaction_TransactionRequestIsSent()
        {
            SetupTransactionRequestSend();

            Assert.IsNotNull(_currencyInHandler);

            _currencyInHandler.Invoke(new CurrencyInCompletedEvent(100000));

            _centralManager.Verify();
        }

        private void SetupTransactionRequestSend(bool expectSend = true)
        {
            if (expectSend)
                _centralManager.Setup(c =>
                    c.Send<TransactionRequest, CloseTranResponse>(It.IsAny<TransactionRequest>(),
                        It.IsAny<CancellationToken>())).Verifiable();
            else
                _centralManager.Setup(c =>
                        c.Send<TransactionRequest, CloseTranResponse>(It.IsAny<TransactionRequest>(),
                            It.IsAny<CancellationToken>()))
                    .Callback((Request request, CancellationToken token) => _requestSent = true);
        }

        private void SetupTransactionRequestError()
        {
            _centralManager
                .Setup(c => c.Send<TransactionRequest, CloseTranResponse>(It.IsAny<TransactionRequest>(),
                    It.IsAny<CancellationToken>())).Throws(new UnexpectedResponseException(CreateErrorResponse()))
                .Verifiable();
        }

        private CreditInService CreateCreditInService(
            bool nullEventBus = false,
            bool nullCentralManager = false,
            bool nullPlayerService = false,
            bool nullProperties = false,
            bool nullIdProvider = false,
            bool nullbank = false,
            bool nullGameDataService = false)
        {
            _sut = new CreditInService(
                nullEventBus ? null : _eventBus.Object,
                nullCentralManager ? null : _centralManager.Object,
                nullPlayerService ? null : _playerSession.Object,
                nullProperties ? null : _properties.Object,
                nullIdProvider ? null : _idProvider.Object,
                nullbank ? null : _bank.Object,
                nullGameDataService ? null : _gameDataService.Object);

            return _sut;
        }

        private CloseTranErrorResponse CreateErrorResponse()
        {
            return new CloseTranErrorResponse
            {
                Status = Status.Error
            };
        }
    }
}