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
    public class LPAAEnableDisableGameAutoRebetParserTest
    {   enum AutoPlay
        {
            Disable = 0,
            Enable = 1
            
        };
        private LPAAEnableDisableGameAutoRebetParser _target;    
        Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<bool>, LongPollSingleValueData<byte>>> _handler;
     
        [TestInitialize]
        public void MyTestInitialize()
        {
            _handler = new Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<bool>, LongPollSingleValueData<byte>>>(MockBehavior.Default);
            _target = new LPAAEnableDisableGameAutoRebetParser();
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.EnableDisableGameAutoRebet, _target.Command);
        }

        [TestMethod]
        public void ParseTest()
        {
            var command = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.EnableDisableGameAutoRebet, (byte)AutoPlay.Enable, TestConstants.FakeCrc, TestConstants.FakeCrc };

            //enable auto play command and handler returns true.
            LongPollReadSingleValueResponse<bool> response = new LongPollReadSingleValueResponse<bool>(true);
            _handler.Setup(m => m.Handle(It.IsAny<LongPollSingleValueData<byte>>())).Returns(response);
            var actual = _target.Parse(command).ToArray();
            Assert.AreEqual(command[0], actual[0]);

            //enable auto play command and handler returns false.
            response.Data = false;
            var expected = new List<byte> { TestConstants.SasAddress | TestConstants.Nack };
            _handler.Setup(m => m.Handle(It.IsAny<LongPollSingleValueData<byte>>())).Returns(response);
            actual = _target.Parse(command).ToArray();
            Assert.AreEqual(expected[0], actual[0]);
        }
    }
}
