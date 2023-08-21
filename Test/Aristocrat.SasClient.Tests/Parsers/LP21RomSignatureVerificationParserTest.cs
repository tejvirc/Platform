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
    ///     Contains the tests for the LP21RomSignatureVerificationParser class
    /// </summary>
    [TestClass]
    public class LP21RomSignatureVerificationParserTest
    {
        private const byte ClientNumber = 31;

        private LP21RomSignatureVerificationParser _target;
        private Mock<ISasLongPollHandler<LongPollResponse, RomSignatureData>> _handler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP21RomSignatureVerificationParser(new SasClientConfiguration { ClientNumber = ClientNumber });

            _handler = new Mock<ISasLongPollHandler<LongPollResponse, RomSignatureData>>(MockBehavior.Default);
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.RomSignatureVerification, _target.Command);
        }

        [TestMethod]
        public void HandleTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.RomSignatureVerification,
                0xFF, 0xFF, // Seed
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(command).ToArray();
            Assert.AreEqual(command[0], actual[0]);
            _handler.Verify(
                x => x.Handle(
                    It.Is<RomSignatureData>(data => data.ClientNumber == ClientNumber && data.Seed == 0xFFFF)));
        }
    }
}