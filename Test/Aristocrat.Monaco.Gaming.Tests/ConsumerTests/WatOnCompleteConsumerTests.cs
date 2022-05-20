namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using Accounting.Contracts;
    using Consumers;
    using Contracts;
    using Contracts.Barkeeper;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class WatOnCompleteConsumerTests
    {
        private Mock<ICurrencyInContainer> _currencyHandler;
        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<ISessionInfoService> _sessionInfoService;
        private Mock<IBarkeeperHandler> _barkeeperHandler;

        private WatOnCompleteConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _currencyHandler = new Mock<ICurrencyInContainer>(MockBehavior.Default);
            _persistentStorage = new Mock<IPersistentStorageManager>(MockBehavior.Default);
            _sessionInfoService = new Mock<ISessionInfoService>(MockBehavior.Default);
            _barkeeperHandler = new Mock<IBarkeeperHandler>(MockBehavior.Default);

            _persistentStorage.Setup(s => s.ScopedTransaction()).Returns(new Mock<IScopedTransaction>().Object);
            _target = new WatOnCompleteConsumer(_currencyHandler.Object, _sessionInfoService.Object, _persistentStorage.Object, _barkeeperHandler.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullCurrencyHandler()
        {
            _target = new WatOnCompleteConsumer(null, _sessionInfoService.Object, _persistentStorage.Object, _barkeeperHandler.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullSessionInfoService()
        {
            _target = new WatOnCompleteConsumer(_currencyHandler.Object, null, _persistentStorage.Object, _barkeeperHandler.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullPersistenceStorageManager()
        {
            _target = new WatOnCompleteConsumer(_currencyHandler.Object, _sessionInfoService.Object, null, _barkeeperHandler.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullBarkeeperHandler()
        {
            _target = new WatOnCompleteConsumer(_currencyHandler.Object, _sessionInfoService.Object, _persistentStorage.Object, null);
        }

        [TestMethod]
        public void ConsumeTest()
        {
            const long cashableAmount = 100;
            const long promoAmount = 200;
            const long nonCashAmount = 300;

            var transaction = new WatOnTransaction(
                0,
                DateTime.Now,
                cashableAmount,
                promoAmount,
                nonCashAmount,
                true,
                string.Empty)
            {
                AuthorizedCashableAmount = cashableAmount,
                TransferredCashableAmount = cashableAmount,
                AuthorizedPromoAmount = promoAmount,
                TransferredPromoAmount = promoAmount,
                AuthorizedNonCashAmount = nonCashAmount,
                TransferredNonCashAmount = nonCashAmount
            };

            _target.Consume(new WatOnCompleteEvent(transaction));
            _currencyHandler.Verify(x => x.Credit(transaction), Times.Once);
            _barkeeperHandler.Verify(x => x.OnCreditsInserted(transaction.TransactionAmount));
        }
    }
}