namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LPParsers;

    [TestClass]
    public class LP1FSendMachineIdAndInfoParserTest
    {
        private LP1FSendMachineIdAndInfoParser _target = new LP1FSendMachineIdAndInfoParser(new SasClientConfiguration());

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendMachineIdAndInformation, _target.Command);
        }

        [DataRow("123456", "123", "123456", "123")]
        [DataRow("12345678910", "123456", "123456", "123")]
        [DataRow("123", "12", "123\0\0\0", "012")]
        [DataTestMethod]
        public void HandleTest(string payTableId, string additionalId, string expectedPaytableId, string expectedAdditionalId)
        {
            var handler = new Mock<ISasLongPollHandler<LongPollMachineIdAndInfoResponse, LongPollData>>(MockBehavior.Strict);
            handler.Setup(c => c.Handle(It.IsAny<LongPollData>()))
                .Returns(new LongPollMachineIdAndInfoResponse("AT", additionalId, 1, 0xC9, 0x12, 0, payTableId, "9050"));
            _target.InjectHandler(handler.Object);

            var gameIdExpected = Encoding.ASCII.GetBytes("AT");
            var additionalIdExpected = Encoding.ASCII.GetBytes(expectedAdditionalId);
            byte denomValueExpected = 0x01;
            byte maxBetExpected = 0xC9;
            byte progGroupExpected = 0x12;
            byte[] gameOptionsExpected = { 0x0, 0x0 };
            var paytableIdExpected = Encoding.ASCII.GetBytes(expectedPaytableId);
            var basePercentExpected = Encoding.ASCII.GetBytes("9050");

            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendMachineIdAndInformation
            };

            var actual = _target.Parse(command).ToList();
            var expected = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendMachineIdAndInformation,
            };
            expected.AddRange(gameIdExpected);
            expected.AddRange(additionalIdExpected);
            expected.Add(denomValueExpected);
            expected.Add(maxBetExpected);
            expected.Add(progGroupExpected);
            expected.AddRange(gameOptionsExpected);
            expected.AddRange(paytableIdExpected);
            expected.AddRange(basePercentExpected);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HandleNullTest()
        {
            var handler = new Mock<ISasLongPollHandler<LongPollMachineIdAndInfoResponse, LongPollData>>(MockBehavior.Strict);
            handler.Setup(c => c.Handle(It.IsAny<LongPollData>()))
                .Returns((LongPollMachineIdAndInfoResponse)null);
            _target.InjectHandler(handler.Object);

            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendMachineIdAndInformation
            };

            var actual = _target.Parse(command).ToList();

            var expected = new List<byte>
            {
                TestConstants.SasAddress | TestConstants.Nack
            };

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
