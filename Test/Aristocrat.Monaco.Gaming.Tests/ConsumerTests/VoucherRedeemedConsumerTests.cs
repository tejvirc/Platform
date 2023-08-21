namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using Accounting.Contracts;
    using Consumers;
    using Contracts;
    using Contracts.Barkeeper;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class VoucherRedeemedConsumerTests
    {
        private VoucherRedeemedConsumer _target;
        private Mock<ICurrencyInContainer> _currencyInContainer;
        private Mock<IBarkeeperHandler> _barkeeperHandler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _currencyInContainer = new Mock<ICurrencyInContainer>(MockBehavior.Default);
            _barkeeperHandler = new Mock<IBarkeeperHandler>(MockBehavior.Default);
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _target = new VoucherRedeemedConsumer(_currencyInContainer.Object, _barkeeperHandler.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullCurrencyInContainer()
        {
            _target = new VoucherRedeemedConsumer(null, _barkeeperHandler.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullBarkeeperHandler()
        {
            _target = new VoucherRedeemedConsumer(_currencyInContainer.Object, null);
        }

        [TestMethod]
        public void ConsumeTest()
        {
            const long expectedAmount = 1000;
            var voucherTransaction = new VoucherInTransaction(0, DateTime.Now, expectedAmount, AccountType.Cashable, string.Empty)
            {
                TransactionId = 1
            };
            
            _target.Consume(new VoucherRedeemedEvent(voucherTransaction));
            _currencyInContainer.Verify(x => x.Credit(voucherTransaction));
            _barkeeperHandler.Verify(x => x.OnCreditsInserted(voucherTransaction.Amount));
        }
    }
}