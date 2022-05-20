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
    ///     Contains the tests for the LP08ConfigureBillDenominationsParser class
    /// </summary>
    [TestClass]
    public class LP08ConfigureBillDenominationsParserTest
    {
        private LP08ConfigureBillDenominationsParser _target;
        private readonly Mock<ISasLongPollHandler<LongPollResponse, LongPollBillDenominationsData>> _handler =
            new Mock<ISasLongPollHandler<LongPollResponse, LongPollBillDenominationsData>>(MockBehavior.Default);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP08ConfigureBillDenominationsParser();
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.ConfigureBillDenominations, _target.Command);
        }

        [TestMethod]
        public void HandleTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.ConfigureBillDenominations,
                0xFF, 0x80, 0x00, 0x00,       // configures $1,2,5,10,20,25,50,100, and $10,000
                0x00,                         // set to disable after accept
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            // match the denomination values passed in to command above, except use cents instead of dollars
            var expected = new List<ulong> { 1_00, 2_00, 5_00, 10_00, 20_00, 25_00, 50_00, 100_00, 10000_00 };

            _handler.Setup(m => m.Handle(It.IsAny<LongPollBillDenominationsData>())).Returns((LongPollResponse)null)
                .Callback<LongPollBillDenominationsData>(
                    data => CollectionAssert.AreEqual(expected, data.Denominations.ToList()));

            var actual = _target.Parse(command).ToArray();

            Assert.AreEqual(command[0], actual[0]);
        }
    }
}
