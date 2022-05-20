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
    ///     Contains the tests for the LPa8EnableJackpotHandpayResetMethodParser class
    /// </summary>
    [TestClass]
    public class LPa8EnableJackpotHandpayResetMethodParserTest
    {
        private readonly LPa8EnableJackpotHandpayResetMethodParser _target = new LPa8EnableJackpotHandpayResetMethodParser();

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.EnableJackpotHandpayResetMethod, _target.Command);
        }

        [TestMethod]
        public void ParseSucceedTest()
        {
            var response = new EnableJackpotHandpayResetMethodResponse(AckCode.ResetMethodEnabled);

            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.EnableJackpotHandpayResetMethod,
                (byte)ResetMethod.StandardHandpay
            };

            var handler = new Mock<ISasLongPollHandler<EnableJackpotHandpayResetMethodResponse, EnableJackpotHandpayResetMethodData>>(MockBehavior.Strict);

            handler.Setup(m => m.Handle(It.IsAny<EnableJackpotHandpayResetMethodData>())).Returns(response);
            _target.InjectHandler(handler.Object);

            var expected = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.EnableJackpotHandpayResetMethod,
                (byte)response.Code
            };

            var actual = _target.Parse(command).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
