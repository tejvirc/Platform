namespace Aristocrat.Monaco.Sas.Tests.BonusProvider
{
    using System;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Gaming.Contracts.Bonus;
    using Gaming.Contracts.Meters;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.BonusProvider;
    using Test.Common;

    [TestClass]
    public class SasBonusProviderTests
    {
        private dynamic _accessor;
        private SasBonusProvider _target;

        private readonly Mock<ISasHost> _sasHost = new Mock<ISasHost>();
        private readonly Mock<ISasExceptionHandler> _exceptionHandler = new Mock<ISasExceptionHandler>();
        private readonly Mock<IBonusHandler> _bonusHandler = new Mock<IBonusHandler>();
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>();
        private readonly Mock<ISystemDisableManager> _systemDisableManager = new Mock<ISystemDisableManager>();
        private readonly Mock<IMeterManager> _meterManager = new Mock<IMeterManager>();
        private readonly Mock<IGameMeterManager> _gameMeterManager = new Mock<IGameMeterManager>();
        private readonly Mock<IAftLockHandler> _aftLockHandler = new Mock<IAftLockHandler>();
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>();
        private readonly Mock<IPersistentStorageManager> _storageManager = new Mock<IPersistentStorageManager>();

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _target = new SasBonusProvider(
                _sasHost.Object,
                _exceptionHandler.Object,
                _bonusHandler.Object,
                _propertiesManager.Object,
                _systemDisableManager.Object,
                _meterManager.Object,
                _aftLockHandler.Object,
                _eventBus.Object,
                _storageManager.Object);

            _accessor = new DynamicPrivateObject(_target);
        }

        [TestMethod]
        public void HandlerFailedEventHandleTest()
        {
            var expectedStatus = AftTransferStatusCode.GamingMachineUnableToPerformTransfer;

            var deviceId = 1;
            var transactionDateTime = DateTime.Now;
            var bonusId = "1";
            var cashableAmount = 100L;
            var nonCashAmount = 100L;
            var promoAmount = 100L;
            var gameId = 1;
            var denom = 100L;
            var payMethod = PayMethod.Any;


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

            _accessor.HandleEvent(@event);
            _sasHost.Verify(
                x => x.AftTransferFailed(It.IsAny<AftData>(), expectedStatus),
                Times.Once);
        }

        [TestMethod]
        public void HandlerCancelEventHandleTest()
        {
            var expectedStatus = AftTransferStatusCode.TransferCanceledByHost;

            var deviceId = 1;
            var transactionDateTime = DateTime.Now;
            var bonusId = "1";
            var cashableAmount = 100L;
            var nonCashAmount = 100L;
            var promoAmount = 100L;
            var gameId = 1;
            var denom = 100L;
            var payMethod = PayMethod.Any;


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

            _accessor.HandleEvent(@event);
            _sasHost.Verify(
                x => x.AftTransferFailed(It.IsAny<AftData>(), expectedStatus),
                Times.Once);
        }
    }
}
