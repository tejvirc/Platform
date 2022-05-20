namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using Contracts.Client;
    using Hardware.Contracts.Button;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.AftTransferProvider;
    using Sas.Consumers;
    using Test.Common;

    [TestClass]
    public class UpEventConsumerTests
    {
        private Mock<IHardCashOutLock> _hardCashOutLock;
        private Mock<IAftOffTransferProvider> _aftOffTransferProvider;
        private UpEventConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ISharedConsumer>(MockBehavior.Default);

            _hardCashOutLock = new Mock<IHardCashOutLock>(MockBehavior.Default);
            _aftOffTransferProvider = new Mock<IAftOffTransferProvider>(MockBehavior.Default);

            _target = new UpEventConsumer(_hardCashOutLock.Object, _aftOffTransferProvider.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
            MoqServiceManager.RemoveInstance();
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullHardCashOutLock()
        {
            _target = new UpEventConsumer(null, _aftOffTransferProvider.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullAftOffTransferProviderLock()
        {
            _target = new UpEventConsumer(_hardCashOutLock.Object, null);
        }

        [DataRow(ButtonLogicalId.Button30, true, true, DisplayName = "Host cashout and AFT waiting for keyoff")]
        [DataRow(ButtonLogicalId.Button30, true, false, DisplayName = "Host cashout waiting for keyoff")]
        [DataRow(ButtonLogicalId.Button30, false, true, DisplayName = "AFT waiting for keyoff")]
        [DataRow(ButtonLogicalId.Button30, false, true, DisplayName = "Nothing waiting for keyoff")]
        [DataTestMethod]
        public void ConsumeTest(ButtonLogicalId buttonLogicalId, bool hardCashoutWaiting, bool aftOffWaiting)
        {
            _hardCashOutLock.Setup(x => x.WaitingForKeyOff).Returns(hardCashoutWaiting);
            _aftOffTransferProvider.Setup(x => x.WaitingForKeyOff).Returns(aftOffWaiting);
            if (hardCashoutWaiting)
            {
                _hardCashOutLock.Setup(x => x.OnKeyedOff()).Verifiable();
            }
            else
            {
                _hardCashOutLock.Setup(x => x.OnKeyedOff()).Callback(() => Assert.Fail("Hard Cashout Lockup is not waiting for key off request"));
            }

            if (aftOffWaiting)
            {
                _aftOffTransferProvider.Setup(x => x.OnKeyedOff()).Verifiable();
            }
            else
            {
                _aftOffTransferProvider.Setup(x => x.OnKeyedOff()).Callback(() => Assert.Fail("Aft Off is not waiting for key off request"));
            }

            _target.Consume(new UpEvent((int)buttonLogicalId));

            _hardCashOutLock.Verify();
            _aftOffTransferProvider.Verify();
        }
    }
}