using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages;

namespace Aristocrat.Monaco.Hardware.Serial.Tests.Reel.Harkey.Message
{
    [TestClass]
    public class HarkeySerializableMessageTests
    {
        const int ReelId = 0x01;
        const int ResponseCode = 0x20;
        const int SelectedReels = 0x07;
        const int SequenceId = 0x11;

        [TestInitialize]
        public void TestInitialize()
        {
            HarkeySerializableMessage.Initialize();
        }

        [TestMethod]
        public void NoProtocolCommandTest()
        {
            var expected = new AbortAndSlowSpin
            {
                SelectedReels = SelectedReels
            };

            var result = (AbortAndSlowSpin)HarkeySerializableMessage.Deserialize(expected.Serialize());

            TestStandardFields(expected, result);
            Assert.AreEqual(expected.SelectedReels, result.SelectedReels);
        }

        [TestMethod]
        public void NoSequenceIdCommandResponseTest()
        {
            var expected = new AbortAndSlowSpinResponse
            {
                ResponseCode = ResponseCode
            };

            var result = (AbortAndSlowSpinResponse)HarkeySerializableMessage.Deserialize(expected.Serialize());

            TestStandardFields(expected, result);
            Assert.AreEqual(expected.ResponseCode, result.ResponseCode);
        }

        [TestMethod]
        public void StandardCommandTest()
        {
            var expected = new HomeReel
            {
                SequenceId = SequenceId,
                ReelId = ReelId
            };

            var result = (HomeReel)HarkeySerializableMessage.Deserialize(expected.Serialize());

            TestStandardFields(expected, result);
            Assert.AreEqual(expected.ReelId, result.ReelId);
        }

        [TestMethod]
        public void StandardCommandResponseTest()
        {
            var expected = new HomeReelResponse
            {
                SequenceId = SequenceId,
                ResponseCode = ResponseCode
            };

            var result = (HomeReelResponse)HarkeySerializableMessage.Deserialize(expected.Serialize());

            TestStandardFields(expected, result);
            Assert.AreEqual(expected.ResponseCode, result.ResponseCode);
        }

        [TestMethod]
        public void VariableLengthCommandResponseTest()
        {
            const int ProtocolByte = 0xA4;
            const int CommandIdByte = 0x43;

            var expected = new SpinReelsToGoalResponse
            {
                Protocol = ProtocolByte,
                SequenceId = SequenceId,
                ResponseCode1 = ResponseCode
            };

            var data = new byte[] { ProtocolByte, SequenceId, CommandIdByte, ResponseCode };
            var result = (SpinReelsToGoalResponse)HarkeySerializableMessage.Deserialize(data);

            TestStandardFields(expected, result);
            Assert.AreEqual(expected.ResponseCode1, result.ResponseCode1);
            Assert.AreEqual(expected.ResponseCode2, result.ResponseCode2);
            Assert.AreEqual(expected.ResponseCode3, result.ResponseCode3);
            Assert.AreEqual(expected.ResponseCode4, result.ResponseCode4);
        }

        private void TestStandardFields(HarkeySerializableMessage expected, HarkeySerializableMessage result)
        {
            Assert.AreEqual(expected.CommandId, result.CommandId);
            Assert.AreEqual(expected.MaxLength, result.MaxLength);
            Assert.AreEqual(expected.MessageType, result.MessageType);
            Assert.AreEqual(expected.Protocol, result.Protocol);
            Assert.AreEqual(expected.SequenceId, result.SequenceId);
            Assert.AreEqual(expected.UseCommandInsteadOfProtocol, result.UseCommandInsteadOfProtocol);
        }
    }
}
