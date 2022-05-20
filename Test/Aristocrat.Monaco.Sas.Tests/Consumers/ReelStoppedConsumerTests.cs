namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using Hardware.Contracts.Reel;
    using Aristocrat.Sas.Client;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Consumers;
    using Test.Common;
    using Aristocrat.Monaco.Sas.Contracts.Client;
    using Aristocrat.Monaco.Sas.Exceptions;

    [TestClass]
    public class ReelStoppedConsumerTests
    {
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private Mock<IRteStatusProvider> _rteStatusProvider;
        private ReelStoppedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Default);
            _rteStatusProvider = new Mock<IRteStatusProvider>(MockBehavior.Default);
            _target = new ReelStoppedConsumer(_rteStatusProvider.Object, _exceptionHandler.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullRteStatusProviderTest()
        {
            _target = new ReelStoppedConsumer(null, _exceptionHandler.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullExceptionHandlerTest()
        {
            _target = new ReelStoppedConsumer(_rteStatusProvider.Object, null);
        }

        [DataRow(1, 100, true, false, true, DisplayName = "Reel 1, Step 100, Clent 1, Exception")]
        [DataRow(1, 100, false, true, true, DisplayName = "Reel 1, Step 100, Clent 2, Exception")]
        [DataRow(1, 100, true, true, true, DisplayName = "Reel 1, Step 100, Both clients, Exception")]
        [DataRow(1, -1, true, false, true, DisplayName = "Reel 1, Step -1, Clent 1, Exception")]
        [DataRow(1, 100, false, false, false, DisplayName = "Reel 1, Step 100, No clients, No exception")]
        [DataRow(0, 100, true, false, false, DisplayName = "Reel 0, Step 100, Clent 1, No exception")]
        [DataRow(9, 100, true, false, true, DisplayName = "Reel 9, Step 100, Clent 1, Exception")]
        [DataRow(10, 100, true, false, false, DisplayName = "Reel 10, Step 100, Clent 1, No exception")]
        [DataRow(1, 1000, true, false, true, DisplayName = "Reel 1, Step 1000, Clent 1, Exception")]
        [DataTestMethod]
        public void ConsumeTest(int reel, int step, bool client1RteEnabled, bool client2RteEnabled, bool expectException)
        {
            var expectedResult = new ReelNHasStoppedExceptionBuilder(reel, step);

            _rteStatusProvider.Setup(x => x.Client1RteEnabled).Returns(client1RteEnabled);
            _rteStatusProvider.Setup(x => x.Client2RteEnabled).Returns(client2RteEnabled);

            ReelNHasStoppedExceptionBuilder actual = null;
            _exceptionHandler.Setup(m => m.ReportException(It.IsAny<ReelNHasStoppedExceptionBuilder>(), It.IsAny<byte>()))
                .Callback((ISasExceptionCollection g, byte _) => actual = g as ReelNHasStoppedExceptionBuilder)
                .Verifiable();

            var @event = new ReelStoppedEvent(reel, step, true);

            _target.Consume(@event);

            if (expectException)
            {
                _exceptionHandler.Verify(x => x.ReportException(It.IsAny<ReelNHasStoppedExceptionBuilder>(), It.IsAny<byte>()), Times.AtLeastOnce);
                Assert.IsNotNull(actual);
                CollectionAssert.AreEquivalent(expectedResult, actual);
            }
            else
            {
                _exceptionHandler.Verify(x => x.ReportException(It.IsAny<ReelNHasStoppedExceptionBuilder>(), It.IsAny<byte>()), Times.Never);
            }
        }
    }
}
