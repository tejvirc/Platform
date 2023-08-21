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
    ///     Contains the tests for the LP09DisableEnableGameNParser class
    /// </summary>
    [TestClass]
    public class LP09DisableEnableGameNParserTest
    {
        private const byte DisableGame = 0x00;
        private const byte InvalidDisableCode = 0x05;
        private const long _oneCentDenom = 1000;
        
        private List<byte> _command = new List<byte>
        {
            TestConstants.SasAddress, (byte)LongPoll.EnableDisableGameN,
            0x00, 0x01,  // game number
            DisableGame, // disable
            TestConstants.FakeCrc, TestConstants.FakeCrc
        };
        private readonly Mock<ISasLongPollHandler<EnableDisableResponse, EnableDisableData>> _handler
            = new Mock<ISasLongPollHandler<EnableDisableResponse, EnableDisableData>>(MockBehavior.Default);
        private LP09DisableEnableGameNParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP09DisableEnableGameNParser();
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.EnableDisableGameN, _target.Command);
        }

        [TestMethod]
        public void ParseSucceedTest()
        {
            var response = new EnableDisableResponse
            {
                Succeeded = true,
                Busy = false
            };

            _handler.Setup(m => m.Handle(It.IsAny<EnableDisableData>())).Returns(response);
            _target.InjectHandler(_handler.Object);

            var expected = new List<byte>
            {
                TestConstants.SasAddress
            };

            var actual = _target.Parse(_command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseFailTest()
        {
            var response = new EnableDisableResponse
            {
                Succeeded = false,
                Busy = false
            };

            _handler.Setup(m => m.Handle(It.IsAny<EnableDisableData>())).Returns(response);
            _target.InjectHandler(_handler.Object);

            var expected = new List<byte>
            {
                TestConstants.SasAddress | TestConstants.Nack
            };

            var actual = _target.Parse(_command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseBusyTest()
        {
            var response = new EnableDisableResponse
            {
                Succeeded = false,
                Busy = true
            };

            _handler.Setup(m => m.Handle(It.IsAny<EnableDisableData>())).Returns(response);
            _target.InjectHandler(_handler.Object);

            var expected = new List<byte>
            {
                TestConstants.SasAddress, TestConstants.BusyResponse
            };

            var actual = _target.Parse(_command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseInvalidEnableDisableTest()
        {
            _command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.EnableDisableGameN,
                0x00, 0x01,  // game number
                InvalidDisableCode, // disable
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var expected = new List<byte>
            {
                TestConstants.SasAddress | TestConstants.Nack
            };

            var actual = _target.Parse(_command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseInvalidBcdGameNumberTest()
        {
            _command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.EnableDisableGameN,
                0x00, 0x0A,  // invalid BCD for game number
                DisableGame, // disable
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var expected = new List<byte>
            {
                TestConstants.SasAddress | TestConstants.Nack
            };

            var actual = _target.Parse(_command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseWithDenomSucceedTest()
        {
            var response = new EnableDisableResponse
            {
                Succeeded = true,
                Busy = false
            };

            _handler.Setup(c => c.Handle(It.IsAny<EnableDisableData>())).Returns(response);
            _target.InjectHandler(_handler.Object);

            _command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.EnableDisableGameN,
                0x00, 0x01,
                DisableGame,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var expected = new List<byte>
            {
                TestConstants.SasAddress
            };

            var actual = _target.Parse(_command, _oneCentDenom).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseWithDenomFailureTest()
        {
            var response = new EnableDisableResponse
            {
                Succeeded = false,
                Busy = false,
                ErrorCode = MultiDenomAwareErrorCode.SpecificDenomNotSupported
            };

            _handler.Setup(c => c.Handle(It.IsAny<EnableDisableData>())).Returns(response);
            _target.InjectHandler(_handler.Object);

            _command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.EnableDisableGameN,
                0x00, 0x01,
                DisableGame,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var expected = new List<byte>
            {
                TestConstants.SasAddress,
                0, // error message for multi-denom aware
                (byte)MultiDenomAwareErrorCode.SpecificDenomNotSupported
            };

            var actual = _target.Parse(_command, _oneCentDenom).ToList();

            CollectionAssert.AreEqual(expected, actual);

        }
    }
}
