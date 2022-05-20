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
    ///     Contains the tests for the LP4CSetValidationIdNumberParser class
    /// </summary>
    [TestClass]
    public class LP4CSetValidationIdParserTest
    {
        private LP4CSetValidationIdNumberParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP4CSetValidationIdNumberParser();
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SetSecureEnhancedValidationId, _target.Command);
        }

        [TestMethod]
        public void ParseWhenUsingSecureEnhancedValidationTest()
        {
            var command = new List<byte>
            {
                // the hard coded numbers are randomly selected values for the test and have no significance
                TestConstants.SasAddress, (byte)LongPoll.SetSecureEnhancedValidationId,
                0x00, 0x12, 0x34,    // machine validation number
                0x11, 0x22, 0x33,    // sequence number
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var response = new LongPoll4CResponse
            {
                MachineValidationId = 0x001234,
                SequenceNumber = 0x112233,
                UsingSecureEnhancedValidation = true
            };

            Mock<ISasLongPollHandler<LongPoll4CResponse, LongPoll4CData>> handler
                = new Mock<ISasLongPollHandler<LongPoll4CResponse, LongPoll4CData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<LongPoll4CData>())).Returns(response);
            _target.InjectHandler(handler.Object);

            var expected = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SetSecureEnhancedValidationId,
                // these get returned with reverse endian
                0x34, 0x12, 0x00,    // machine validation number
                0x33, 0x22, 0x11     // sequence number
            };

            var actual = _target.Parse(command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseWhenNotUsingSecureEnhancedValidationTest()
        {
            var command = new List<byte>
            {
                // the hard coded numbers are randomly selected values for the test and have no significance
                TestConstants.SasAddress, (byte)LongPoll.SetSecureEnhancedValidationId,
                0x00, 0x00, 0x12, 0x34,    // machine validation number
                0x11, 0x22, 0x33, 0x44,    // sequence number
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var response = new LongPoll4CResponse
            {
                MachineValidationId = 0x00001234,
                SequenceNumber = 0x11223344,
                UsingSecureEnhancedValidation = false
            };

            Mock<ISasLongPollHandler<LongPoll4CResponse, LongPoll4CData>> handler
                = new Mock<ISasLongPollHandler<LongPoll4CResponse, LongPoll4CData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<LongPoll4CData>())).Returns(response);
            _target.InjectHandler(handler.Object);

            var actual = _target.Parse(command);

            Assert.IsNull(actual);
        }
    }
}
