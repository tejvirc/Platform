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
    public class LP48SendLastAcceptedBillInformationParserTests
    {
        private LP48SendLastAcceptedBillInformationParser _target;
        private Mock<ISasLongPollHandler<SendLastAcceptedBillInformationResponse, LongPollData>> _handler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP48SendLastAcceptedBillInformationParser();

            _handler = new Mock<ISasLongPollHandler<SendLastAcceptedBillInformationResponse, LongPollData>>(MockBehavior.Strict);
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendLastBillInformation, _target.Command);
        }

        [TestMethod]
        public void HandleTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendLastBillInformation
            };

            _handler.Setup(c => c.Handle(It.IsAny<LongPollData>())).Returns(new SendLastAcceptedBillInformationResponse(BillAcceptorCountryCode.Euro, BillDenominationCodes.Ten, 1234));

            var actual = _target.Parse(command).ToArray();

            var expectedResults = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendLastBillInformation,
                0x39, // Country Code 1byte BCD
                0x03, // Denomination Code   1byte BCD
                0x00, 0x00, 0x12, 0x34 // Count 4byte BCD
            };

            CollectionAssert.AreEqual(expectedResults, actual.ToList());
        }
    }
}