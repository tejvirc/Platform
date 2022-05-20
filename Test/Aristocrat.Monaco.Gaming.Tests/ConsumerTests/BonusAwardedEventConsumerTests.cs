namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using Contracts;
    using Contracts.Bonus;
    using Consumers;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class BonusAwardedEventConsumerTests
    {
        private readonly Mock<ICurrencyInContainer> _currencyContainer = new Mock<ICurrencyInContainer>();
        private readonly Mock<IPersistentStorageManager> _storage = new Mock<IPersistentStorageManager>();
        private readonly Mock<IScopedTransaction> _scope = new Mock<IScopedTransaction>();
        private BonusAwardedEventConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Loose);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _storage.Setup(x => x.ScopedTransaction()).Returns(_scope.Object);
            _target = CreateTarget();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(true, false)]
        [DataRow(false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorArgumentsTest(bool nullCurrencyContainer, bool nullStorage)
        {
            _target = CreateTarget(nullCurrencyContainer, nullStorage);
        }

        [TestMethod]
        public void ConsumeTest()
        {
            const long cashAmount = 10000;
            const long promoAmount = 20000;
            const long nonCashAmount = 30000;
            var transaction =
                new BonusTransaction(
                    1,
                    DateTime.UtcNow,
                    "TestBonus",
                    cashAmount,
                    nonCashAmount,
                    promoAmount,
                    1,
                    1000,
                    PayMethod.Any)
                {
                    PaidCashableAmount = cashAmount,
                    PaidPromoAmount = promoAmount,
                    PaidNonCashAmount = nonCashAmount
                };
            _target.Consume(new BonusAwardedEvent(transaction));
            _currencyContainer.Verify(x => x.Credit(transaction), Times.Once);
        }

        private BonusAwardedEventConsumer CreateTarget(bool nullCurrencyContainer = false, bool nullStorage = false)
        {
            return new BonusAwardedEventConsumer(
                nullCurrencyContainer ? null : _currencyContainer.Object,
                nullStorage ? null : _storage.Object);
        }
    }
}