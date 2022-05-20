namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using Application.Contracts;
    using Consumers;
    using Contracts;
    using Contracts.Bonus;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class PartialBonusPaidEventConsumerTests
    {
        private readonly Mock<ICurrencyInContainer> _currencyContainer = new Mock<ICurrencyInContainer>();
        private readonly Mock<IPersistentStorageManager> _storage = new Mock<IPersistentStorageManager>();
        private readonly Mock<IScopedTransaction> _scope = new Mock<IScopedTransaction>();
        private readonly Mock<IIdProvider> _idProvider = new Mock<IIdProvider>();

        private PartialBonusPaidEventConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Loose);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _storage.Setup(x => x.ScopedTransaction()).Returns(_scope.Object);

            _target = CreateTarget();
        }

        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorArgumentsTest(bool nullCurrencyContainer, bool nullStorage, bool nullIdProvider)
        {
            _target = CreateTarget(nullCurrencyContainer, nullStorage, nullIdProvider);
        }

        [TestMethod]
        public void ConsumeTest()
        {
            const long cashAmount = 10000;
            const long promoAmount = 20000;
            const long nonCashAmount = 30000;
            const long totalPaid = cashAmount + promoAmount + nonCashAmount;
            const long transactionId = 12345;
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
                    PaidCashableAmount = cashAmount, PaidPromoAmount = promoAmount, PaidNonCashAmount = nonCashAmount
                };

            _idProvider.Setup(x => x.GetNextTransactionId()).Returns(transactionId);
            _target.Consume(new PartialBonusPaidEvent(transaction, cashAmount, nonCashAmount, promoAmount));
            _currencyContainer.Verify(x => x.Credit(transaction, totalPaid, transactionId), Times.Once);
        }

        private PartialBonusPaidEventConsumer CreateTarget(
            bool nullCurrencyContainer = false,
            bool nullStorage = false,
            bool nullIdProvider = false)
        {
            return new PartialBonusPaidEventConsumer(
                nullCurrencyContainer ? null : _currencyContainer.Object,
                nullStorage ? null : _storage.Object,
                nullIdProvider ? null : _idProvider.Object);
        }
    }
}