namespace Aristocrat.SasClient.Tests.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;

    [TestClass]
    public class LP01ShutdownParserTest
    {
        private const byte ClientNumber = 41;
        private LP01ShutdownParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            var handler = new Mock<ISasLongPollHandler<LongPollResponse, LongPollSingleValueData<byte>>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.Is<LongPollSingleValueData<byte>>(x => x.Value == ClientNumber)))
                .Returns((LongPollResponse)null);

            _target = new LP01ShutdownParser(new SasClientConfiguration { ClientNumber = ClientNumber });
            _target.InjectHandler(handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.Shutdown, _target.Command);
        }

        [TestMethod]
        public void HandleTest()
        {
            var command = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.Shutdown, TestConstants.FakeCrc, TestConstants.FakeCrc };

            var actual = _target.Parse(command).ToArray();

            Assert.AreEqual(command[0], actual[0]);
        }
    }
}