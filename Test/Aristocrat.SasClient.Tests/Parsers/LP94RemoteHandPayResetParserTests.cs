namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;

    [TestClass]
    public class LP94RemoteHandPayResetParserTests
    {
        private LP94RemoteHandPayResetParser _target;
        private Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<HandPayResetCode>, LongPollData>> _handler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _handler =
                new Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<HandPayResetCode>, LongPollData>>(
                    MockBehavior.Default);
            _target = new LP94RemoteHandPayResetParser();
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.RemoteHandpayReset, _target.Command);
        }

        [DataRow(HandPayResetCode.NotInHandpay)]
        [DataRow(HandPayResetCode.HandpayWasReset)]
        [DataRow(HandPayResetCode.UnableToResetHandpay)]
        [DataTestMethod]
        public void ParseTest(HandPayResetCode resetCode)
        {
            var expectedResponse = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.RemoteHandpayReset,
                (byte)resetCode
            };

            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.RemoteHandpayReset,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            _handler.Setup(x => x.Handle(It.IsAny<LongPollData>()))
                .Returns(new LongPollReadSingleValueResponse<HandPayResetCode>(resetCode));
            var response = _target.Parse(command);
            CollectionAssert.AreEquivalent(expectedResponse, response.ToList());
        }
    }
}