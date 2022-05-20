namespace Aristocrat.SasClient.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;

    [TestClass]
    public class SasPriorityMessageQueueTests
    {
        private const ushort FakeSignature = 0xFFFF;

        private readonly RomSignatureVerificationResponse _testResponse =
            new RomSignatureVerificationResponse(TestConstants.SasAddress, FakeSignature);

        private SasPriorityMessageQueue _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new SasPriorityMessageQueue();
        }

        [TestMethod]
        public void QueueIsEmptyTest()
        {
            Assert.IsTrue(_target.IsEmpty);

            _target.QueueMessage(_testResponse);
            Assert.IsFalse(_target.IsEmpty);

            _target.GetNextMessage();
            _target.MessageAcknowledged();
            Assert.IsTrue(_target.IsEmpty);
        }

        [TestMethod]
        public void GetNextMessageDoesNotRemoveMessageUntilAcknowledged()
        {
            _target.QueueMessage(_testResponse);

            var nextMessage = _target.GetNextMessage();
            Assert.AreEqual(_testResponse.MessageData, nextMessage.MessageData);

            Assert.AreEqual(_testResponse.MessageData, nextMessage.MessageData);
            _target.MessageAcknowledged();

            nextMessage = _target.GetNextMessage();
            Assert.IsTrue(nextMessage is SasEmptyMessage);
            Assert.AreEqual(0, nextMessage.MessageData.Count);
        }

        [TestMethod]
        public void GetNextMessageDoesNotRemoveMessageAndCanBeCleared()
        {
            _target.QueueMessage(_testResponse);

            var nextMessage = _target.GetNextMessage();
            Assert.AreEqual(_testResponse.MessageData, nextMessage.MessageData);

            Assert.AreEqual(_testResponse.MessageData, nextMessage.MessageData);
            _target.ClearPendingMessage();
            _target.MessageAcknowledged(); // Make sure we don't clear a message when we don't have one pending

            nextMessage = _target.GetNextMessage();
            Assert.AreEqual(_testResponse.MessageData, nextMessage.MessageData);
        }
    }
}