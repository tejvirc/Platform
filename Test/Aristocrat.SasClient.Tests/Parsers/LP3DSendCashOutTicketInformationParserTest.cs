namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;

    [TestClass]
    public class LP3DSendCashOutTicketInformationParserTest
    {
        private const long TransactionAmount = 801250;
        private const long ValidationNumber = 33761282;
        private LP3DSendCashOutTicketInformationParser _target;

        [TestInitialize]
        public void Initialize()
        {
            _target = new LP3DSendCashOutTicketInformationParser();
        }

        [TestMethod]
        public void ParseTest()
        {
            var longPoll = new Collection<byte> { TestConstants.SasAddress, 0x4F };

            var expected = new List<byte> { TestConstants.SasAddress, 0x4F };
            expected.AddRange(Utilities.ToBcd(ValidationNumber, SasConstants.Bcd8Digits));
            expected.AddRange(Utilities.ToBcd(TransactionAmount, SasConstants.Bcd10Digits));

            var response = new LongPollSendCashOutTicketInformationResponse(
                ValidationNumber,
                TransactionAmount);

            var handler =
                new Mock<ISasLongPollHandler<LongPollSendCashOutTicketInformationResponse, LongPollData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<LongPollData>())).Returns(response);

            _target.InjectHandler(handler.Object);

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseNullTest()
        {
           var longPoll = new Collection<byte> { TestConstants.SasAddress, 0x4F };

            var handler =
                new Mock<ISasLongPollHandler<LongPollSendCashOutTicketInformationResponse, LongPollData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<LongPollData>()))
                .Returns((LongPollSendCashOutTicketInformationResponse)null);

            var actual = _target.Parse(longPoll);

            Assert.IsNull(actual);
        }
    }
}