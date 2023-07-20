namespace Aristocrat.SasClient.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Test.Common;
    using Moq;
    using Sas.Client;

    /// <summary>
    /// This class contains tests for the SasClient class
    /// </summary>
    [TestClass]
    public class SasClientTest
    {
        private const int threadJoinTimeout = 1000;
        private const string ReasonText = "Dummy";
        private SasClient _target;
        private dynamic _privateTarget;
        private readonly Mock<ISasCommPort> _commPort = new Mock<ISasCommPort>(MockBehavior.Strict);
        private readonly Mock<ISasExceptionQueue> _exceptionQueue = new Mock<ISasExceptionQueue>(MockBehavior.Strict);
        private readonly Mock<ISasMessageQueue> _messageQueue = new Mock<ISasMessageQueue>(MockBehavior.Strict);
        private readonly Mock<ISasParserFactory> _parserFactory = new Mock<ISasParserFactory>(MockBehavior.Strict);
        private readonly Mock<IPlatformCallbacks> _callbacks = new Mock<IPlatformCallbacks>(MockBehavior.Strict);
        private readonly Mock<IHostAcknowlegementProvider> _impliedAckHandler = new Mock<IHostAcknowlegementProvider>(MockBehavior.Default);
        private readonly SasClientConfiguration _configuration =
            new SasClientConfiguration { SasAddress = 1, ClientNumber = 1 };

        [TestInitialize]
        public void Initialize()
        {
            _target = new SasClient(
                _configuration,
                _callbacks.Object,
                _exceptionQueue.Object,
                _messageQueue.Object,
                _parserFactory.Object,
                _impliedAckHandler.Object);

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _privateTarget = new DynamicPrivateObject(_target);
            // use reflection to inject the comm port mock into the target
            typeof(SasClient).GetField("_port", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(_target, _commPort.Object);
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullCallbackTest()
        {
            _target = new SasClient(
                _configuration,
                null,
                _exceptionQueue.Object,
                _messageQueue.Object,
                _parserFactory.Object,
                _impliedAckHandler.Object);
            Assert.Fail("should have thrown an exception");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullExceptionQueueTest()
        {
            _target = new SasClient(
                _configuration,
                _callbacks.Object,
                null,
                _messageQueue.Object,
                _parserFactory.Object,
                _impliedAckHandler.Object);
            Assert.Fail("should have thrown an exception");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullMessageQueueTest()
        {
            _target = new SasClient(
                _configuration,
                _callbacks.Object,
                _exceptionQueue.Object,
                null,
                _parserFactory.Object,
                _impliedAckHandler.Object);
            Assert.Fail("should have thrown an exception");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullParserTest()
        {
            _target = new SasClient(
                _configuration,
                _callbacks.Object,
                _exceptionQueue.Object,
                _messageQueue.Object,
                null,
                _impliedAckHandler.Object);
            Assert.Fail("should have thrown an exception");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullImpliedAckHandlerTest()
        {
            _target = new SasClient(
                _configuration,
                _callbacks.Object,
                _exceptionQueue.Object,
                _messageQueue.Object,
                _parserFactory.Object,
                null);
            Assert.Fail("should have thrown an exception");
        }

        [TestMethod]
        public void IsRealTimeEventReportingActiveTest()
        {
            Assert.IsFalse(_target.IsRealTimeEventReportingActive);
            _target.IsRealTimeEventReportingActive = true;
            Assert.IsTrue(_target.IsRealTimeEventReportingActive);
        }

        [TestMethod]
        public void SendResponseTest()
        {
            int expected = 3;   // sending 3 bytes
            int length = 0;
            _commPort.Setup(m => m.SendRawBytes(It.IsAny<IReadOnlyCollection<byte>>()))
                .Returns(true)
                .Callback((IReadOnlyCollection<byte> c) => length = c.Count);
            _target.SendResponse(new List<byte> { 0x00, 0x01, 0x02 });  // fake response for testing

            Assert.AreEqual(expected, length);
        }

        [TestMethod]
        public void AttachToCommPortTest()
        {
            string expected = "COM1";
            string actual = string.Empty;
            _commPort.Setup(m => m.Open(It.IsAny<string>()))
                .Returns(true)
                .Callback((string s) => actual = s);
            _callbacks.Setup(m => m.ToggleCommunicationsEnabled(true, 1)).Verifiable();

            _target.AttachToCommPort(expected);  // fake comm port for testing

            Assert.AreEqual(expected, actual);

            _callbacks.Verify();
        }

        [TestMethod]
        public void ReleaseCommPortTest()
        {
            _commPort.Setup(m => m.Close()).Verifiable();
            _callbacks.Setup(m => m.ToggleCommunicationsEnabled(false, 1)).Verifiable();

            _target.ReleaseCommPort();

            _commPort.Verify();
            _callbacks.Verify();
        }

        [TestMethod]
        public void StopTest()
        {
            _privateTarget._running = true;
            _target.Stop();
            Assert.IsFalse(_privateTarget._running);
        }

        [TestMethod]
        public void DisposeTest()
        {
            _target.Dispose();

            // Dispose again to test already disposed path
            _target.Dispose();

            // nothing to validate.
        }

        [TestMethod]
        public void IgnoreStartByteWithoutWakeupBitSetTest()
        {
            _commPort.Setup(m => m.ReadOneByte(false))
                .Returns((0x33, false, true));
            _commPort.Setup(m => m.SasAddress).Returns(1).Verifiable();

            // It cannot be considered to be any poll.
            Assert.IsFalse(_privateTarget.GetPoll());

            Assert.IsFalse(_target.IsGeneralPoll);
            Assert.IsFalse(_target.IsOtherAddressPoll);
            Assert.IsFalse(_target.IsLongPoll);
            Assert.IsFalse(_target.IsGlobalPoll);
            _commPort.Verify();
        }

        [TestMethod]
        public void WakeupBitAndPollForOtherAddressTest()
        {
            _commPort.Setup(m => m.SasAddress).Returns(1).Verifiable();
            _commPort.Setup(m => m.ReadOneByte(false))
                .Returns((0x33, true, true));

            // GetPoll must return true because it's a poll for other address.
            Assert.IsTrue(_privateTarget.GetPoll());
            Assert.IsFalse(_target.IsGeneralPoll);
            Assert.IsTrue(_target.IsOtherAddressPoll);
            Assert.IsFalse(_target.IsLongPoll);
            Assert.IsFalse(_target.IsGlobalPoll);
            _commPort.Verify();
        }

        [TestMethod]
        public void IgnoreCommandWithoutWakeupBitTest()
        {
            // when the wakeup bit is not set, it won't read more than the first byte.
            _commPort.SetupSequence(m => m.ReadOneByte(It.IsAny<bool>()))
                .Returns((TestConstants.SasAddress, false, true)) // poll
                .Returns((0x01, false, true)) // LP01 - ignored
                .Returns((0x51, false, true)) // Crc-1 - ignored
                .Returns((0x08, false, true)); // Crc-2 - ignored

            _commPort.Setup(m => m.SasAddress).Returns(1).Verifiable();

            // It should fail to get a poll because the wakeup is not set.
            Assert.IsFalse(_privateTarget.GetPoll());

            Assert.IsFalse(_target.IsGeneralPoll);
            Assert.IsFalse(_target.IsOtherAddressPoll);
            Assert.IsFalse(_target.IsLongPoll);
            Assert.IsFalse(_target.IsGlobalPoll);

            _commPort.Verify();
        }

        /// <summary>
        ///     When a general poll with the wakeup bit is received
        /// </summary>
        [TestMethod]
        public void GetPollWithWakeupForAGeneralPollTest()
        {
            _commPort.Setup(m => m.SasAddress).Returns(1).Verifiable();

            // returns a general poll
            _commPort.Setup(m => m.ReadOneByte(false))
                .Returns((TestConstants.SasAddress | SasConstants.PollBit, true, true))
                .Verifiable();
            Assert.IsTrue(_privateTarget.GetPoll());
            Assert.IsTrue(_target.IsGeneralPoll);
            Assert.IsFalse(_target.IsOtherAddressPoll);
            Assert.IsFalse(_target.IsLongPoll);
            Assert.IsFalse(_target.IsGlobalPoll);

            _commPort.Verify();
        }

        /// <summary>
        ///     When a global poll with the wakeup bit is received
        /// </summary>
        [TestMethod]
        public void GetPollWithWakeupForAGlobalLongPollTest()
        {
            _commPort.Setup(m => m.SasAddress).Returns(1).Verifiable();

            _commPort.SetupSequence(m => m.ReadOneByte(It.IsAny<bool>()))
                .Returns((0x00, true, true)) // poll
                .Returns((0x01, false, true)) // LP01
                .Returns((0x51, false, true)) // Crc-1
                .Returns((0x08, false, true)); // Crc-2
            Assert.IsTrue(_privateTarget.GetPoll());
            Assert.IsFalse(_target.IsGeneralPoll);
            Assert.IsFalse(_target.IsOtherAddressPoll);
            Assert.IsTrue(_target.IsLongPoll);
            Assert.IsTrue(_target.IsGlobalPoll);

            _commPort.Verify();
        }

        /// <summary>
        ///     When a long poll command with the wakeup bit is received, all data
        ///     for this command should be gathered.
        /// </summary>
        [TestMethod]
        public void GetPollWithWakeupForALongPollTest()
        {
            _commPort.Setup(m => m.SasAddress).Returns(1).Verifiable();
            _commPort.SetupSequence(m => m.ReadOneByte(It.IsAny<bool>()))
                .Returns((TestConstants.SasAddress, true, true)) // poll
                .Returns((0x01, false, true)) // LP01
                .Returns((0x51, false, true)) // Crc-1
                .Returns((0x08, false, true)); // Crc-2
            Assert.IsTrue(_privateTarget.GetPoll());
            Assert.IsFalse(_target.IsGeneralPoll);
            Assert.IsFalse(_target.IsOtherAddressPoll);
            Assert.IsTrue(_target.IsLongPoll);
            Assert.IsFalse(_target.IsGlobalPoll);

            _commPort.Verify();

        }

        /// <summary>
        ///     When peeking a poll, the chirp interval must be checked prior to
        ///     try to get the next poll.
        /// </summary>
        [TestMethod]
        public void PeekPollWithChirpIntervalCheck()
        {
            _commPort.Setup(m => m.SasAddress).Returns(1).Verifiable();

            // the port is not available to return any byte.
            // so _target.State.IsPollAvailable is always false
            _commPort.Setup(m => m.ReadOneByte(false))
                .Returns((0xFF, false, false)); // poll

            // shorten the chirp interval so that the test can be done quickly.
            _target.PerformanceAuditor.ChirpInterval = 1;
            _privateTarget._running = true;
            Assert.AreEqual(0, _target.PerformanceAuditor.PollWatch.ElapsedMilliseconds);
            _privateTarget.PeekPoll();
            Assert.IsTrue(_privateTarget._running);
            Assert.IsFalse(_target.IsPollAvailable);
            Assert.IsTrue(_target.PerformanceAuditor.PollWatch.ElapsedMilliseconds >= _target.PerformanceAuditor.ChirpInterval);
        }

        /// <summary>
        ///     Integrates getting and processing a long poll command
        /// </summary>
        [TestMethod]
        [Ignore("Ignored, needs to be reviewed. Test are failing intermittently.")]
        public void RunWithLP01CommandWithWakeupTest()
        {
            var sent = new List<byte>();
            _callbacks.Setup(m => m.LinkUp(false, TestConstants.SasAddress)).Verifiable();

            _impliedAckHandler.SetupGet(x => x.Synchronized).Returns(true);
            _impliedAckHandler.Setup(x => x.CheckImpliedAck(false, It.IsAny<bool>(), It.IsAny<IReadOnlyCollection<byte>>()))
                .Returns(true);
            _impliedAckHandler.Setup(x => x.SetPendingImpliedAck(It.IsAny<IReadOnlyCollection<byte>>(), It.IsAny<IHostAcknowledgementHandler>())).Callback(() =>
            {
                _target.Stop();
            });
            _callbacks.Setup(m => m.ToggleCommunicationsEnabled(false, TestConstants.SasAddress));
            _commPort.Setup(m => m.Close()).Verifiable();
            _commPort.Setup(m => m.SasAddress).Returns(1).Verifiable();
            _commPort.Setup(m => m.SendRawBytes(It.IsAny<IReadOnlyCollection<byte>>()))
                .Returns(true)
                .Callback((IReadOnlyCollection<byte> c) => sent = c.ToList());

            var parser = new Mock<ILongPollParser>(MockBehavior.Default);
            parser.Setup(m => m.Parse(It.IsAny<IReadOnlyCollection<byte>>())).Returns(new List<byte> { TestConstants.SasAddress });

            _parserFactory.Setup(m => m.GetParserForLongPoll(LongPoll.Shutdown)).Returns(parser.Object);
            _callbacks.Setup(m => m.LinkUp(true, TestConstants.SasAddress)).Verifiable();
            _exceptionQueue.Setup(m => m.GetNextException()).Returns(new GenericExceptionBuilder(GeneralExceptionCode.None));

            // the wakeup bit is set.
            _commPort.SetupSequence(m => m.ReadOneByte(It.IsAny<bool>()))
                .Returns((TestConstants.SasAddress, true, true)) // poll
                .Returns((0x01, false, true)) // LP01
                .Returns((0x51, false, true)) // Crc-1
                .Returns((0x08, false, true)); // Crc-2

            var thread = new Thread(_target.Run);
            thread.Start();
            Assert.IsTrue(thread.Join(threadJoinTimeout));
            Assert.AreEqual(TestConstants.SasAddress, sent[0]);
            Assert.AreEqual(1, sent.Count);
            Assert.IsFalse(_target.IsGeneralPoll);
            Assert.IsFalse(_target.IsOtherAddressPoll);
            Assert.IsTrue(_target.IsLongPoll);

            _commPort.Verify();
            _callbacks.Verify();
        }

        [TestMethod]
        public void RunWithLP01CommandWithCrcErrorTest()
        {
            List<byte> sent = new List<byte>();

            var parser = new Mock<ILongPollParser>(MockBehavior.Default);
            parser.Setup(m => m.Parse(It.IsAny<IReadOnlyCollection<byte>>())).Returns(new List<byte> { TestConstants.SasAddress });
            _impliedAckHandler.SetupGet(x => x.Synchronized).Returns(true);
            _impliedAckHandler.Setup(x => x.CheckImpliedAck(false, It.IsAny<bool>(), It.IsAny<IReadOnlyCollection<byte>>()))
                .Returns(true);
            _callbacks.Setup(m => m.LinkUp(false, TestConstants.SasAddress)).Verifiable();
            _callbacks.Setup(m => m.ToggleCommunicationsEnabled(false, TestConstants.SasAddress));
            _commPort.Setup(m => m.Close()).Verifiable();
            _commPort.Setup(m => m.SasAddress).Returns(1).Verifiable();
            _commPort.Setup(m => m.SendRawBytes(It.IsAny<IReadOnlyCollection<byte>>()))
                .Returns(true)
                .Callback((IReadOnlyCollection<byte> c) =>
                {
                    sent = c.ToList();
                    _target.Stop();
                });
            _parserFactory.Setup(m => m.GetParserForLongPoll(LongPoll.Shutdown)).Returns(parser.Object);
            _callbacks.Setup(m => m.LinkUp(true, TestConstants.SasAddress)).Verifiable();
            _exceptionQueue.Setup(m => m.GetNextException()).Returns(new GenericExceptionBuilder(GeneralExceptionCode.None));

            // the wakeup bit is set.
            _commPort.SetupSequence(m => m.ReadOneByte(It.IsAny<bool>()))
                .Returns((TestConstants.SasAddress, true, true)) // poll
                .Returns((0x01, false, true))  // LP01
                .Returns((0x52, false, true))  // wrong Crc-1
                .Returns((0x08, false, true)); // Crc-2

            var thread = new Thread(_target.Run);
            thread.Start();
            Assert.IsTrue(thread.Join(threadJoinTimeout));
            Assert.AreEqual(TestConstants.SasAddress | SasConstants.Nack, sent[0]);
            Assert.AreEqual(1, sent.Count);
            _commPort.Verify();
            _callbacks.Verify();
        }

        [TestMethod]
        public void RunWithNullResponseFromParserTest()
        {
            var parser = new Mock<ILongPollParser>(MockBehavior.Default);
            parser.Setup(m => m.Parse(It.IsAny<IReadOnlyCollection<byte>>())).Returns((List<byte>)null)
                .Callback(() => _target.Stop());
            _impliedAckHandler.SetupGet(x => x.Synchronized).Returns(true);
            _impliedAckHandler.Setup(x => x.CheckImpliedAck(false, It.IsAny<bool>(), It.IsAny<IReadOnlyCollection<byte>>()))
                .Returns(true);
            _callbacks.Setup(m => m.LinkUp(false, TestConstants.SasAddress)).Verifiable();
            _callbacks.Setup(m => m.ToggleCommunicationsEnabled(false, TestConstants.SasAddress));
            _commPort.Setup(m => m.Close());
            _commPort.Setup(m => m.SasAddress).Returns(1)
                .Verifiable();
            _parserFactory.Setup(m => m.GetParserForLongPoll(LongPoll.Shutdown)).Returns(parser.Object);
            _callbacks.Setup(m => m.LinkUp(true, TestConstants.SasAddress)).Verifiable();
            _exceptionQueue.Setup(m => m.GetNextException()).Returns(new GenericExceptionBuilder(GeneralExceptionCode.None));

            // the wakeup bit is set.
            _commPort.SetupSequence(m => m.ReadOneByte(It.IsAny<bool>()))
                .Returns((TestConstants.SasAddress, true, true)) // poll
                .Returns((0x01, false, true)) // LP01
                .Returns((0x51, false, true)) // Crc-1
                .Returns((0x08, false, true)); // Crc-2

            var thread = new Thread(_target.Run);
            thread.Start();
            Assert.IsTrue(thread.Join(threadJoinTimeout));
            _commPort.Verify();
            _callbacks.Verify();
        }

        [TestMethod]
        public void RunWithPendingEmptyMessageTest()
        {
            _callbacks.Setup(m => m.LinkUp(false, TestConstants.SasAddress)).Verifiable();
            _callbacks.Setup(m => m.ToggleCommunicationsEnabled(false, TestConstants.SasAddress));
            _impliedAckHandler.SetupGet(x => x.Synchronized).Returns(true);
            _impliedAckHandler.Setup(x => x.CheckImpliedAck(false, It.IsAny<bool>(), It.IsAny<IReadOnlyCollection<byte>>()))
                .Returns(true);
            _commPort.Setup(m => m.Close()).Verifiable();
            _commPort.Setup(m => m.SasAddress).Returns(1).Verifiable();

            _commPort.Setup(m => m.ReadOneByte(false))
                .Returns((TestConstants.SasAddress | SasConstants.PollBit, true, true)) // general poll
                .Callback(() => _target.Stop());
            _callbacks.Setup(m => m.LinkUp(true, TestConstants.SasAddress)).Verifiable();
            _exceptionQueue.Setup(m => m.GetNextException()).Returns(new GenericExceptionBuilder(GeneralExceptionCode.None));
            _exceptionQueue.Setup(m => m.ConvertRealTimeExceptionToNormal(It.IsAny<ISasExceptionCollection>()))
                .Returns((byte)GeneralExceptionCode.None);

            var message = new SasEmptyMessage();
            _messageQueue.Setup(m => m.IsEmpty).Returns(false);
            _messageQueue.Setup(m => m.GetNextMessage()).Returns(message);

            var thread = new Thread(_target.Run);
            thread.Start();
            Assert.IsTrue(thread.Join(threadJoinTimeout));
            _commPort.Verify();
            _callbacks.Verify();
        }

        [TestMethod]
        public void RunWithPendingMessageTest()
        {
            List<byte> sent = new List<byte>();

            _callbacks.Setup(m => m.LinkUp(false, TestConstants.SasAddress)).Verifiable();
            _callbacks.Setup(m => m.ToggleCommunicationsEnabled(false, TestConstants.SasAddress));
            _impliedAckHandler.SetupGet(x => x.Synchronized).Returns(true);
            _impliedAckHandler.Setup(x => x.CheckImpliedAck(false, It.IsAny<bool>(), It.IsAny<IReadOnlyCollection<byte>>()))
                .Returns(true);
            _commPort.Setup(m => m.Close()).Verifiable();
            _commPort.Setup(m => m.SasAddress).Returns(1).Verifiable();
            _commPort.Setup(m => m.SendRawBytes(It.IsAny<IReadOnlyCollection<byte>>()))
                .Returns(true)
                .Callback((IReadOnlyCollection<byte> c) =>
                {
                    sent = c.ToList();
                    _target.Stop();
                });

            _commPort.SetupSequence(m => m.ReadOneByte(false))
                .Returns((TestConstants.SasAddress | SasConstants.PollBit, true, true)); // general poll
            _callbacks.Setup(m => m.LinkUp(true, TestConstants.SasAddress)).Verifiable();
            _exceptionQueue.Setup(m => m.GetNextException()).Returns(new GenericExceptionBuilder(GeneralExceptionCode.None));
            _exceptionQueue.Setup(m => m.ConvertRealTimeExceptionToNormal(It.IsAny<ISasExceptionCollection>()))
                .Returns((byte)GeneralExceptionCode.None);

            var message = new Mock<ISasMessage>(MockBehavior.Strict);
            message.Setup(m => m.MessageData).Returns(new List<byte> { 0x01 });
            _messageQueue.Setup(m => m.IsEmpty).Returns(false);
            _messageQueue.Setup(m => m.GetNextMessage()).Returns(message.Object);

            var thread = new Thread(_target.Run);
            thread.Start();

            Assert.IsTrue(thread.Join(threadJoinTimeout));
            Assert.AreEqual(3, sent.Count);
            _commPort.Verify();
            _callbacks.Verify();
        }

        [TestMethod]
        public void RunWithRealTimeExceptionTest()
        {
            List<byte> sent = new List<byte>();

            _callbacks.Setup(m => m.LinkUp(false, TestConstants.SasAddress)).Verifiable();
            _callbacks.Setup(m => m.ToggleCommunicationsEnabled(false, TestConstants.SasAddress));
            _impliedAckHandler.SetupGet(x => x.Synchronized).Returns(true);
            _impliedAckHandler.Setup(x => x.CheckImpliedAck(false, It.IsAny<bool>(), It.IsAny<IReadOnlyCollection<byte>>()))
                .Returns(true);
            _commPort.Setup(m => m.Close()).Verifiable();
            _commPort.Setup(m => m.SasAddress).Returns(TestConstants.SasAddress).Verifiable();
            _commPort.Setup(m => m.SendRawBytes(It.IsAny<IReadOnlyCollection<byte>>()))
                .Returns(true)
                .Callback((IReadOnlyCollection<byte> c) =>
                {
                    sent = c.ToList();
                    _target.Stop();
                });

            _commPort.Setup(m => m.ReadOneByte(false))
                .Returns((TestConstants.SasAddress | SasConstants.PollBit, true, true)); // general poll
            _callbacks.Setup(m => m.LinkUp(true, TestConstants.SasAddress)).Verifiable();
            _exceptionQueue.SetupSequence(m => m.GetNextException())
                .Returns(new GenericExceptionBuilder(GeneralExceptionCode.EgmPowerApplied))
                .Returns(new GenericExceptionBuilder(GeneralExceptionCode.None));
            _exceptionQueue.Setup(m => m.GetNextException()).Returns(new GenericExceptionBuilder(GeneralExceptionCode.EgmPowerApplied));
            _exceptionQueue.SetupSequence(m => m.ConvertRealTimeExceptionToNormal(It.IsAny<ISasExceptionCollection>()))
                .Returns(GeneralExceptionCode.EgmPowerApplied)
                .Returns(GeneralExceptionCode.None);

            _messageQueue.Setup(m => m.IsEmpty).Returns(true);

            _target.IsRealTimeEventReportingActive = true;

            var thread = new Thread(_target.Run);
            thread.Start();

            Assert.IsTrue(thread.Join(threadJoinTimeout));
            _target.IsRealTimeEventReportingActive = false;

            // should get address, 0xFF, ACPowerApplied, crc, crc
            Assert.AreEqual(5, sent.Count);
            _commPort.Verify();
            _callbacks.Verify();
        }

        [TestMethod]
        public void RunWithNonRealTimeExceptionTest()
        {
            List<byte> sent = new List<byte>();

            _callbacks.Setup(m => m.LinkUp(false, TestConstants.SasAddress)).Verifiable();
            _callbacks.Setup(m => m.ToggleCommunicationsEnabled(false, TestConstants.SasAddress));
            _impliedAckHandler.SetupGet(x => x.Synchronized).Returns(true);
            _impliedAckHandler.Setup(x => x.CheckImpliedAck(false, It.IsAny<bool>(), It.IsAny<IReadOnlyCollection<byte>>()))
                .Returns(true);
            _commPort.Setup(m => m.Close()).Verifiable();
            _commPort.Setup(m => m.SasAddress).Returns(TestConstants.SasAddress).Verifiable();
            _commPort.Setup(m => m.SendRawBytes(It.IsAny<IReadOnlyCollection<byte>>()))
                .Returns(true)
                .Callback((IReadOnlyCollection<byte> c) =>
                {
                    sent = c.ToList();
                    _target.Stop();
                });

            _commPort.Setup(m => m.ReadOneByte(false))
                .Returns(((byte)(TestConstants.SasAddress | SasConstants.PollBit), true, true)); // general poll
            _callbacks.Setup(m => m.LinkUp(true, TestConstants.SasAddress)).Verifiable();
            _exceptionQueue.SetupSequence(m => m.GetNextException())
                .Returns(new GenericExceptionBuilder(GeneralExceptionCode.EgmPowerApplied))
                .Returns(new GenericExceptionBuilder(GeneralExceptionCode.None));
            _exceptionQueue.Setup(m => m.GetNextException()).Returns(new GenericExceptionBuilder(GeneralExceptionCode.EgmPowerApplied));
            _exceptionQueue.Setup(m => m.ConvertRealTimeExceptionToNormal(It.IsAny<ISasExceptionCollection>())).Returns(GeneralExceptionCode.EgmPowerApplied);

            _messageQueue.Setup(m => m.IsEmpty).Returns(true);

            var thread = new Thread(_target.Run);
            thread.Start();
            Assert.IsTrue(thread.Join(threadJoinTimeout));
            Assert.AreEqual(1, sent.Count);
            _commPort.Verify();
            _callbacks.Verify();
        }

        [TestMethod]
        public void RunWithVariableLengthCommandTest()
        {
            var sent = new List<byte>();

            var parser = new Mock<ILongPollParser>(MockBehavior.Default);
            parser.Setup(m => m.Parse(It.IsAny<IReadOnlyCollection<byte>>())).Returns(new List<byte> { TestConstants.SasAddress, 0x11, 0x22, 0x33, 0x44 });
            _callbacks.Setup(m => m.LinkUp(false, TestConstants.SasAddress)).Verifiable();
            _callbacks.Setup(m => m.ToggleCommunicationsEnabled(false, TestConstants.SasAddress));
            _impliedAckHandler.SetupGet(x => x.Synchronized).Returns(true);
            _impliedAckHandler.Setup(x => x.CheckImpliedAck(false, It.IsAny<bool>(), It.IsAny<IReadOnlyCollection<byte>>()))
                .Returns(true);
            _commPort.Setup(m => m.Close()).Verifiable();
            _commPort.Setup(m => m.SasAddress).Returns(TestConstants.SasAddress).Verifiable();
            _commPort.Setup(m => m.SendRawBytes(It.IsAny<IReadOnlyCollection<byte>>()))
                .Returns(true)
                .Callback((IReadOnlyCollection<byte> c) =>
                {
                    sent = c.ToList();
                    _target.Stop();
                });
            _parserFactory.Setup(m => m.GetParserForLongPoll(LongPoll.SendAuthenticationInformation)).Returns(parser.Object);
            _callbacks.Setup(m => m.LinkUp(true, TestConstants.SasAddress)).Verifiable();
            _exceptionQueue.Setup(m => m.GetNextException()).Returns(new GenericExceptionBuilder(GeneralExceptionCode.None));

            _commPort.SetupSequence(m => m.ReadOneByte(It.IsAny<bool>()))
                .Returns((TestConstants.SasAddress, true, true)) // poll
                .Returns(((byte)LongPoll.SendAuthenticationInformation, false, true))
                .Returns((0x02, false, true)) // length of bytes before crc
                .Returns((0x01, false, true))
                .Returns((0x02, false, true))
                .Returns((0x90, false, true))
                .Returns((0xB3, false, true));

            var thread = new Thread(_target.Run);
            thread.Start();

            Assert.IsTrue(thread.Join(threadJoinTimeout));
            Assert.AreEqual(TestConstants.SasAddress, sent[0]);
            Assert.AreEqual(7, sent.Count);
            _commPort.Verify();
            _callbacks.Verify();
        }

        [TestMethod]
        public void RunWithGlobalPollTest()
        {
            _callbacks.Setup(m => m.LinkUp(false, TestConstants.SasAddress)).Verifiable();
            _impliedAckHandler.SetupGet(x => x.Synchronized).Returns(true);
            _impliedAckHandler.Setup(x => x.CheckImpliedAck(false, It.IsAny<bool>(), It.IsAny<IReadOnlyCollection<byte>>()))
                .Returns(true);
            _callbacks.Setup(m => m.ToggleCommunicationsEnabled(false, TestConstants.SasAddress));
            _commPort.Setup(m => m.Close()).Verifiable();
            _commPort.Setup(m => m.SasAddress).Returns(TestConstants.SasAddress).Verifiable();
            _exceptionQueue.Setup(m => m.GetNextException()).Returns(new GenericExceptionBuilder(GeneralExceptionCode.None));

            _commPort.Setup(m => m.ReadOneByte(false))
                .Returns((SasConstants.PollBit, true, true)) // global poll
                .Callback(() => _target.Stop());

            var thread = new Thread(_target.Run);
            thread.Start();

            Assert.IsTrue(thread.Join(threadJoinTimeout));
            _commPort.Verify();
            _callbacks.Verify();
        }

        [TestMethod]
        public void HandleLinkDownTest()
        {
            _callbacks.Setup(m => m.LinkUp(false, 1)).Verifiable();

            dynamic accessor = new DynamicPrivateObject(_target);

            accessor.HandleLinkDown(null, null);

            _callbacks.Verify();
        }

        /// <summary>
        /// The loop break cannot be recovered by a poll for other addresses.
        /// </summary>
        [TestMethod]
        public void UpdateLinkedDownStatusWhenPollAvailableTest()
        {
            _callbacks.Setup(m => m.LinkUp(true, 1)).Verifiable();
            _impliedAckHandler.SetupGet(x => x.Synchronized).Returns(true);

            _target.IsPollAvailable = true;
            _target.IsGeneralPoll = true;

            Assert.IsFalse(_target.LinkUp);
            dynamic accessor = new DynamicPrivateObject(_target);

            accessor.UpdateLinkDownStatus();
            Assert.IsTrue(_target.LinkUp);

            _callbacks.Verify();
        }

        /// <summary>
        /// When the loop break is detected, a chirp must sent.
        /// </summary>
        [TestMethod]
        public void UpdateLinkedDownStatusWhenLoopBreakTest()
        {
            _callbacks.Setup(m => m.LinkUp(false, 1)).Verifiable();
            _impliedAckHandler.SetupGet(x => x.Synchronized).Returns(true).Verifiable();
            _commPort.Setup(m => m.SendChirp()).Returns(true);

            _target.PerformanceAuditor.ChirpTimeout = 0;
            _target.StartLogChirp = true;
            dynamic accessor = new DynamicPrivateObject(_target);
            accessor.LinkUp = true;

            accessor.UpdateLinkDownStatus();
            Assert.IsFalse(_target.LinkUp);

            _callbacks.Verify();
            _commPort.Verify();
        }

        [TestMethod]
        public void InitializeTest()
        {
            _parserFactory.Setup(m => m.LoadSingleParser(It.IsAny<ILongPollParser>())).Verifiable();

            _target.Initialize();

            _parserFactory.Verify();
        }

        [TestMethod]
        public void ProcessLongPollMessageWhenRealTimeEventPendingTest()
        {
            _target.IsRealTimeEventReportingActive = true;
            _exceptionQueue.Setup(m => m.Peek())
                .Returns(new GenericExceptionBuilder(GeneralExceptionCode.EgmPowerApplied));
            _exceptionQueue.Setup(m => m.GetNextException())
                .Returns(new GenericExceptionBuilder(GeneralExceptionCode.EgmPowerApplied));
            _exceptionQueue.Setup(m => m.ConvertRealTimeExceptionToNormal(It.IsAny<ISasExceptionCollection>()))
                .Returns(GeneralExceptionCode.EgmPowerApplied);
            _messageQueue.Setup(m => m.IsEmpty).Returns(true);
            _impliedAckHandler.Setup(m => m.CheckImpliedAck(false, false, It.IsAny<byte[]>())).Returns(true);
            _impliedAckHandler.Setup(m => m.LastMessageNacked).Returns(false);
            _commPort.Setup(m => m.SendRawBytes(It.IsAny<IReadOnlyCollection<byte>>())).Returns(true).Verifiable();
            _commPort.Setup(m => m.SasAddress).Returns(1);

            dynamic accessor = new DynamicPrivateObject(_target);
            accessor._readData = new List<byte> { TestConstants.SasAddress };
            accessor.IsOtherAddressPoll = false;

            accessor.ProcessLongPollMessage();
            _impliedAckHandler.Verify();
            _commPort.Verify();
        }

        [TestMethod]
        public void HandleGeneralPollWhenNoRealTimeEventPendingTest()
        {
            _target.IsRealTimeEventReportingActive = true;
            _exceptionQueue.Setup(m => m.GetNextException())
                .Returns(new GenericExceptionBuilder(GeneralExceptionCode.None));
            _exceptionQueue.Setup(m => m.ConvertRealTimeExceptionToNormal(It.IsAny<ISasExceptionCollection>()))
                .Returns(GeneralExceptionCode.None);
            _impliedAckHandler.Setup(m => m.SetPendingImpliedAck(It.IsAny<List<byte>>(), It.IsAny<IHostAcknowledgementHandler>())).Verifiable();
            _messageQueue.Setup(m => m.IsEmpty).Returns(true);

            _commPort.Setup(m => m.SendRawBytes(It.IsAny<IReadOnlyCollection<byte>>())).Returns(true).Verifiable();
            _commPort.Setup(m => m.SasAddress).Returns(1);

            dynamic accessor = new DynamicPrivateObject(_target);
            accessor._readData = new List<byte> { TestConstants.SasAddress };

            accessor.HandleGeneralPoll();

            _impliedAckHandler.Verify();
        }

        [TestMethod]
        public void HandleGeneralPollWhenEePromDataErrorTest()
        {
            _target.IsRealTimeEventReportingActive = false;
            _exceptionQueue.Setup(m => m.GetNextException())
                .Returns(new GenericExceptionBuilder(GeneralExceptionCode.EePromDataError));
            _exceptionQueue.Setup(m => m.ConvertRealTimeExceptionToNormal(It.IsAny<ISasExceptionCollection>()))
                .Returns(GeneralExceptionCode.EePromDataError);
            _impliedAckHandler.Setup(m => m.SetPendingImpliedAck(It.IsAny<List<byte>>(), It.IsAny<IHostAcknowledgementHandler>())).Verifiable();
            _messageQueue.Setup(m => m.IsEmpty).Returns(true);
            _commPort.Setup(m => m.SendRawBytes(It.IsAny<IReadOnlyCollection<byte>>())).Returns(true).Verifiable();
            _commPort.SetupSet(m => m.SasAddress = 0);
            _callbacks.Setup(m => m.LinkUp(false, 1)).Verifiable();

            dynamic accessor = new DynamicPrivateObject(_target);
            accessor._readData = new List<byte> { TestConstants.SasAddress };

            accessor.HandleGeneralPoll();

            _impliedAckHandler.Verify();
            _commPort.Verify();
            Assert.IsFalse(accessor._running);
        }
    }
}