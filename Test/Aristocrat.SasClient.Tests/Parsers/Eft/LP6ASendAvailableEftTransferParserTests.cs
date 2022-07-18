namespace Aristocrat.SasClient.Tests.Parsers.Eft
{
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.EFT;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System.Collections.Generic;
    using System.Linq;
    using static Aristocrat.Sas.Client.EFT.AvailableEftTransferResponse;

    [TestClass]
    public class LP6ASendAvailableEftTransferParserTests
    {
        private LP6ASendAvailableEftTransferParser _target;
        private Mock<ISasLongPollHandler<AvailableEftTransferResponse, LongPollData>> _handler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _handler = new Mock<ISasLongPollHandler<AvailableEftTransferResponse, LongPollData>>(MockBehavior.Strict);
            _handler.Setup(m => m.Handle(It.IsAny<LongPollData>())).Returns(It.IsAny<AvailableEftTransferResponse>());
            _target = new LP6ASendAvailableEftTransferParser();
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest() => Assert.AreEqual(LongPoll.EftSendAvailableEftTransfers, _target.Command);

        [TestMethod]
        public void ParseTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.EftSendAvailableEftTransfers
            };

            _handler.Setup(c => c.Handle(It.IsAny<LongPollData>())).Returns(new AvailableEftTransferResponse { TransferAvailability = EftTransferAvailability.TransferFromGamingMachine });

            var actual = _target.Parse(command).ToArray();

            var expectedResults = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.EftSendAvailableEftTransfers,
                0, // Reserved
                0, // Reserved
                0, // Reserved
                2  // Transfer from EGM can occur
            };

            CollectionAssert.AreEqual(expectedResults, actual.ToList());
        }
    }
}
