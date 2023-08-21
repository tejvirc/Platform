﻿namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;

    /// <summary>
    ///     Contains the tests for the LP07DisableBillAcceptorParser class
    /// </summary>
    [TestClass]
    public class LP07DisableBillAcceptorParserTest
    {
        private LP07DisableBillAcceptorParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP07DisableBillAcceptorParser();
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.DisableBillAcceptor, _target.Command);
        }

        [TestMethod]
        public void ParseSucceedTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.DisableBillAcceptor,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var response = new EnableDisableResponse { Succeeded = true };

            var handler = new Mock<ISasLongPollHandler<EnableDisableResponse, EnableDisableData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<EnableDisableData>())).Returns(response);
            _target.InjectHandler(handler.Object);

            var expected = new List<byte> { TestConstants.SasAddress };

            var actual = _target.Parse(command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseFailTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.DisableBillAcceptor,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var response = new EnableDisableResponse { Succeeded = false };

            var handler = new Mock<ISasLongPollHandler<EnableDisableResponse, EnableDisableData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<EnableDisableData>())).Returns(response);
            _target.InjectHandler(handler.Object);

            var expected = new List<byte> { TestConstants.SasAddress | TestConstants.Nack };

            var actual = _target.Parse(command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
