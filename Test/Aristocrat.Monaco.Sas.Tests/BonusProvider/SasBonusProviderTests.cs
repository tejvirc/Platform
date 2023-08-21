namespace Aristocrat.Monaco.Sas.Tests.BonusProvider
{
    using System;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Gaming.Contracts.Bonus;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.BonusProvider;

    [TestClass]
    public class SasBonusProviderTests
    {
        private SasBonusProvider _target;

        private readonly Mock<ISasHost> _sasHost = new();
        private readonly Mock<ISasExceptionHandler> _exceptionHandler = new();
        private readonly Mock<IBonusHandler> _bonusHandler = new();
        private readonly Mock<IPropertiesManager> _propertiesManager = new();
        private readonly Mock<ISystemDisableManager> _systemDisableManager = new();
        private readonly Mock<IMeterManager> _meterManager = new();
        private readonly Mock<IAftLockHandler> _aftLockHandler = new();
        private readonly Mock<IEventBus> _eventBus = new();
        private readonly Mock<IPersistentStorageManager> _storageManager = new();

        private Action<BonusCancelledEvent> _cancelledEventHandler;
        private Action<BonusAwardedEvent> _awardedEventHandler;
        private Action<BonusFailedEvent> _failedEventHandler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<object>(),
                        It.IsAny<Action<BonusCancelledEvent>>(),
                        It.IsAny<Predicate<BonusCancelledEvent>>()))
                .Callback<object, Action<BonusCancelledEvent>, Predicate<BonusCancelledEvent>>(
                    (_, c, _) => _cancelledEventHandler = c);
            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<object>(),
                        It.IsAny<Action<BonusAwardedEvent>>(),
                        It.IsAny<Predicate<BonusAwardedEvent>>()))
                .Callback<object, Action<BonusAwardedEvent>, Predicate<BonusAwardedEvent>>(
                    (_, c, _) => _awardedEventHandler = c);
            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<object>(),
                        It.IsAny<Action<BonusFailedEvent>>(),
                        It.IsAny<Predicate<BonusFailedEvent>>()))
                .Callback<object, Action<BonusFailedEvent>, Predicate<BonusFailedEvent>>(
                    (_, c, _) => _failedEventHandler = c);

            _target = CreateTarget();
        }

        [DataRow(true, false, false, false, false, false, false, false, false)]
        [DataRow(false, true, false, false, false, false, false, false, false)]
        [DataRow(false, false, true, false, false, false, false, false, false)]
        [DataRow(false, false, false, true, false, false, false, false, false)]
        [DataRow(false, false, false, false, true, false, false, false, false)]
        [DataRow(false, false, false, false, false, true, false, false, false)]
        [DataRow(false, false, false, false, false, false, true, false, false)]
        [DataRow(false, false, false, false, false, false, false, true, false)]
        [DataRow(false, false, false, false, false, false, false, false, true)]
        [DataTestMethod]
        public void NullConstructorArgumentTest(
            bool nullHost,
            bool nullException,
            bool nullBonus,
            bool nullProperties,
            bool nullDisable,
            bool nullMeter,
            bool nullLock,
            bool nullEvent,
            bool nullStorage)
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => _ = CreateTarget(
                    nullHost,
                    nullException,
                    nullBonus,
                    nullProperties,
                    nullDisable,
                    nullMeter,
                    nullLock,
                    nullEvent,
                    nullStorage));
        }

        [TestMethod]
        public void HandlerFailedEventHandleTest()
        {
            const AftTransferStatusCode expectedStatus = AftTransferStatusCode.GamingMachineUnableToPerformTransfer;
            const int deviceId = 1;
            const string bonusId = "1";
            const long cashableAmount = 100L;
            const long nonCashAmount = 100L;
            const long promoAmount = 100L;
            const int gameId = 1;
            const long denom = 100L;
            const PayMethod payMethod = PayMethod.Any;

            var transactionDateTime = DateTime.Now;

            var transaction = new BonusTransaction(
                deviceId,
                transactionDateTime,
                bonusId,
                cashableAmount,
                nonCashAmount,
                promoAmount,
                gameId,
                denom,
                payMethod);

            var @event = new BonusFailedEvent(transaction);

            Assert.IsNotNull(_failedEventHandler);
            _failedEventHandler(@event);
            _sasHost.Verify(
                x => x.AftTransferFailed(It.IsAny<AftData>(), expectedStatus),
                Times.Once);
        }

        [TestMethod]
        public void HandlerCancelEventHandleTest()
        {
            const AftTransferStatusCode expectedStatus = AftTransferStatusCode.TransferCanceledByHost;
            const int deviceId = 1;
            const string bonusId = "1";
            const long cashableAmount = 100L;
            const long nonCashAmount = 100L;
            const long promoAmount = 100L;
            const int gameId = 1;
            const long denom = 100L;
            const PayMethod payMethod = PayMethod.Any;

            var transactionDateTime = DateTime.Now;

            var transaction = new BonusTransaction(
                deviceId,
                transactionDateTime,
                bonusId,
                cashableAmount,
                nonCashAmount,
                promoAmount,
                gameId,
                denom,
                payMethod);

            var @event = new BonusCancelledEvent(transaction);

            Assert.IsNotNull(_cancelledEventHandler);
            _cancelledEventHandler(@event);
            _sasHost.Verify(
                x => x.AftTransferFailed(It.IsAny<AftData>(), expectedStatus),
                Times.Once);
        }

        private SasBonusProvider CreateTarget(
            bool nullHost = false,
            bool nullException = false,
            bool nullBonus = false,
            bool nullProperties = false,
            bool nullDisable = false,
            bool nullMeter = false,
            bool nullLock = false,
            bool nullEvent = false,
            bool nullStorage = false)
        {
            return new SasBonusProvider(
                nullHost ? null : _sasHost.Object,
                nullException ? null : _exceptionHandler.Object,
                nullBonus ? null : _bonusHandler.Object,
                nullProperties ? null : _propertiesManager.Object,
                nullDisable ? null : _systemDisableManager.Object,
                nullMeter ? null : _meterManager.Object,
                nullLock ? null : _aftLockHandler.Object,
                nullEvent ? null : _eventBus.Object,
                nullStorage ? null : _storageManager.Object);
        }
    }
}
