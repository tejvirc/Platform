namespace Aristocrat.SasClient.Tests.Parsers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    ///     Contains the tests for the LP2EGameDelayParser class
    /// </summary>
    [TestClass]
    public class LP2EGameDelayParserTest
    {
        private readonly LP2EGameDelayParser _target = new LP2EGameDelayParser();

        private readonly Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<bool>, LongPollSingleValueData<uint>>> _handler =
            new Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<bool>, LongPollSingleValueData<uint>>>(MockBehavior.Strict);

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.DelayGame, _target.Command);
        }

        [TestMethod]
        public void ParseSucceedTest()
        {
            var command = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.DelayGame, 0, 0 };
            var responseGood = new LongPollReadSingleValueResponse<bool>(true);

            _handler.Setup(m => m.Handle(It.IsAny<LongPollSingleValueData<uint>>())).Returns(responseGood);
            _target.InjectHandler(_handler.Object);

            var result = _target.Parse(command);
            CollectionAssert.AreEquivalent(result.ToList(), new Collection<byte> { TestConstants.SasAddress });
        }
    }
}