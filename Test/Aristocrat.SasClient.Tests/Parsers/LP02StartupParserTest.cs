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
    public class LP02StartupParserTest
    {
        private const byte ClientNumber = 41;
        private LP02StartupParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            var handler = new Mock<ISasLongPollHandler<LongPollResponse, LongPollSASClientConfigurationData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.Is<LongPollSASClientConfigurationData>(x => x.ClientConfiguration.ClientNumber == ClientNumber)))
                .Returns((LongPollResponse)null);

            _target = new LP02StartupParser(new SasClientConfiguration { ClientNumber = ClientNumber });
            _target.InjectHandler(handler.Object);
        }
        
        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.Startup, _target.Command);
        }

        [TestMethod]
        public void ParseTest()
        {
            var command = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.Startup, TestConstants.FakeCrc, TestConstants.FakeCrc };

            var actual = _target.Parse(command).ToArray();

            Assert.AreEqual(command[0], actual[0]);
        }
    }
}