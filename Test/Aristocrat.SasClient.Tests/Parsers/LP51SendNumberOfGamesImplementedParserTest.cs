namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;

    /// <summary>
    ///     Contains the tests for the LP51SendNumberOfGamesImplementedParser class
    /// </summary>
    [TestClass]
    public class LP51SendNumberOfGamesImplementedParserTest
    {
        private LP51SendNumberOfGamesImplementedParser _target = new LP51SendNumberOfGamesImplementedParser();

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendNumberOfGames, _target.Command);
        }

        [TestMethod]
        public void ParseSucceedTest()
        {
            // Using a number greater than 0xFF to check multi-byte BCD functionality
            int numberOfGamesImplemented = 987;

            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendNumberOfGames
            };

            var response = new LongPollReadSingleValueResponse<int>(numberOfGamesImplemented);

            var handler = new Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<int>, LongPollData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<LongPollData>())).Returns(response);
            _target.InjectHandler(handler.Object);

            var expected = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendNumberOfGames, 0x09, 0x87
            };

            var actual = _target.Parse(command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
