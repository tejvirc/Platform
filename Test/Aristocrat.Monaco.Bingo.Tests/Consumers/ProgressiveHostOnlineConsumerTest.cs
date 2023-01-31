namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Application.Contracts;
    using Bingo.Consumers;
    using Common;
    using Common.Events;
    using Gaming.Contracts.Progressives;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ProgressiveHostOnlineConsumerTest
    {
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Default);
        private readonly Mock<ISharedConsumer> _sharedConsumer = new Mock<ISharedConsumer>(MockBehavior.Default);
        private readonly Mock<ISystemDisableManager> _systemDisable = new Mock<ISystemDisableManager>(MockBehavior.Default);
        private readonly Mock<IProtocolLinkedProgressiveAdapter> _linkedAdapter = new Mock<IProtocolLinkedProgressiveAdapter>(MockBehavior.Default);

        private ProgressiveHostOnlineConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false, false)] // EventBus
        [DataRow(false, true, false)] // System disable manager
        [DataRow(false, false, true)] // Protocol linked progressive adapter
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorArgumentTest(
            bool nullEventBus,
            bool nullSystemDisable,
            bool nullLinkedAdapter)
        {
            _target = CreateTarget(nullEventBus, nullSystemDisable, nullLinkedAdapter);
        }

        [TestMethod]
        public void ConsumerTest()
        {
            _linkedAdapter.Setup(x => x.ReportLinkUp(ProtocolNames.Bingo)).Verifiable();
            _systemDisable.Setup(x => x.Enable(BingoConstants.ProgresssiveHostOfflineKey)).Verifiable();

            _target.Consume(new ProgressiveHostOnlineEvent());
            _linkedAdapter.Verify();
            _systemDisable.Verify();
        }

        private ProgressiveHostOnlineConsumer CreateTarget(
            bool nullEventBus = false,
            bool nullSystemDisable = false,
            bool nullLinkedAdapter = false)
        {
            return new ProgressiveHostOnlineConsumer(
                nullEventBus ? null : _eventBus.Object,
                _sharedConsumer.Object,
                nullLinkedAdapter ? null : _linkedAdapter.Object,
                nullSystemDisable ? null : _systemDisable.Object);
        }
    }
}