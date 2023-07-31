namespace Aristocrat.SasClient.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Timer = System.Timers.Timer;

    
    [TestClass]
    public class HostAcknowledgementProviderTests
    {
        private const int waitTimeout = 2000;
        private HostAcknowledgementProvider _target;
        private Mock<ISasExceptionQueue> _exceptionQueue;
        private Mock<ISasMessageQueue> _messageQueue;

        private ManualResetEvent _waiter;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _exceptionQueue = new Mock<ISasExceptionQueue>(MockBehavior.Default);
            _messageQueue = new Mock<ISasMessageQueue>(MockBehavior.Default);
 
            _target = new HostAcknowledgementProvider();

            Assert.IsFalse(_target.Synchronized);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            // dispose twice to ensure it doesn't throw an exception
            _target?.Dispose();
            _target?.Dispose();
        }

        [TestMethod]
        public void LinkDownTest()
        {
            _target.ImpliedNackHandler = _exceptionQueue.Object.ClearPendingException;
            _exceptionQueue.Setup(x => x.ClearPendingException()).Verifiable();

            SynchronizeTarget(_target);
            _target.LinkDown();
            Assert.IsFalse(_target.Synchronized);
            _exceptionQueue.Verify();
        }

        [DataRow(false, new byte[] { 0x81 }, new byte[] { 0x82 }, DisplayName = "Different first byte clears pending ACKs")]
        [DataRow(false, new byte[] { 0x81 }, new byte[] { 0x81, 0x21 }, DisplayName = "Different message lengths clears pending ACKs")]
        [DataRow(false, new byte[] { 0x01, 0x21 }, new byte[] { 0x01, 0x22 }, DisplayName = "Different second byte clears pending ACKs")]
        [DataRow(false, new byte[] { 0x01, 0xB0, 0x02, 0x01, 0x11, 0xB0, 0xC7 }, new byte[] { 0x01, 0xB0, 0x02, 0x01, 0x12, 0x2B, 0xF5 }, DisplayName = "Different command with Multi Denom extensions clears pending ACKs")]
        [DataRow(true, new byte[] { 0x81 }, new byte[] { 0x82 }, DisplayName = "Global Broadcast always clears")]
        [DataTestMethod]
        public void CheckImpliedAckSuccessfulTest(
            bool globalBroadcast,
            byte[] lastSentBytes,
            byte[] currentBytes)
        {
            SynchronizeTarget(_target);
            var callback = new HostAcknowledgementHandler { ImpliedAckHandler = _messageQueue.Object.MessageAcknowledged };
            _target.SetPendingImpliedAck(lastSentBytes, callback);

            _messageQueue.Setup(x => x.MessageAcknowledged()).Verifiable();

            // Call twos times to verify that we have successfully cleared any pending ACKs
            Assert.IsTrue(_target.CheckImpliedAck(globalBroadcast, false, currentBytes));
            Assert.IsTrue(_target.CheckImpliedAck(globalBroadcast, false, currentBytes));

            _messageQueue.Verify();
        }

        [TestMethod]
        public void CheckImpliedAckFailedFinalNackTest()
        {
            var currentBytes = new List<byte> { 0x01, 0x21, 0x31, 0x51, 0x22 };
            var lastBytes = new List<byte> { 0x01, 0x21, 0x21, 0x41, 0x12 };
            var callbacks = new HostAcknowledgementHandler { ImpliedAckHandler = _exceptionQueue.Object.ExceptionAcknowledged, ImpliedNackHandler = _exceptionQueue.Object.ClearPendingException };

            SynchronizeTarget(_target);
            _target.SetPendingImpliedAck(lastBytes, callbacks);

            _exceptionQueue.Setup(x => x.ClearPendingException()).Verifiable();

            // Call twos times to verify that we have failed final NACK
            Assert.IsTrue(_target.CheckImpliedAck(false, false, currentBytes));
            Assert.IsFalse(_target.CheckImpliedAck(false, false, currentBytes));

            _exceptionQueue.Verify();
        }

        [TestMethod]
        public void SynchronizedStateUpdateTest()
        {
            _target.CheckImpliedAck(true, true, new List<byte>());
            Assert.IsFalse(_target.Synchronized);

            Assert.IsFalse(_target.CheckImpliedAck(true, false, new List<byte>()));
            Assert.IsFalse(_target.Synchronized);
            Assert.IsFalse(_target.CheckImpliedAck(false, true, new List<byte>()));
            Assert.IsFalse(_target.Synchronized);
            Assert.IsTrue(_target.CheckImpliedAck(false, false, new List<byte>()));
            Assert.IsTrue(_target.Synchronized);
        }

        [TestMethod]
        public void LastMessageNackedTest()
        {
            _target.CheckImpliedAck(true, true, new List<byte>());
            Assert.IsFalse(_target.Synchronized);

            Assert.IsTrue(_target.CheckImpliedAck(false, false, new List<byte>()));
            Assert.IsTrue(_target.Synchronized);

            Assert.IsFalse(_target.LastMessageNacked);
            _target.SetPendingImpliedAck(new List<byte>(), It.IsAny<IHostAcknowledgementHandler>());
            Assert.IsTrue(_target.CheckImpliedAck(false, false, new List<byte>()));

            Assert.IsTrue(_target.LastMessageNacked);
        }

        [TestMethod]
        public void ImpliedAckTimeOutOnElapsedTest()
        {
            _waiter = new ManualResetEvent(false);
            _target = new HostAcknowledgementProvider(new Timer { Interval = 10, Enabled = true });

            // method will be called if the timer expires 
            _target.SynchronizationLost += HandleLinkDown;

            Assert.IsTrue(_waiter.WaitOne(waitTimeout));
            Assert.IsFalse(_target.Synchronized);
        }

        private void HandleLinkDown(object source, EventArgs e)
        {
            _waiter.Set();
        }

        private static void SynchronizeTarget(IHostAcknowlegementProvider target)
        {
            target.CheckImpliedAck(true, true, new List<byte>());
            target.CheckImpliedAck(false, false, new List<byte>());
            Assert.IsTrue(target.Synchronized);
        }
    }
}