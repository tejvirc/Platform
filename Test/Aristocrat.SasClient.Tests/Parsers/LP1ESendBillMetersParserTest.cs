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
    ///     Contains the tests for the LP1ESendBillMetersParser class
    /// </summary>
    [TestClass]
    public class LP1ESendBillMetersParserTest
    {
        private LP1ESendBillMetersParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP1ESendBillMetersParser(new SasClientConfiguration());
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendBillCountMeters, _target.Command);
        }

        [TestMethod]
        public void HandleTest()
        {
            var command = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.SendBillCountMeters };

            var response = new LongPollReadMultipleMetersResponse();
            response.Meters.Add(SasMeters.DollarIn1, new LongPollReadMeterResponse(SasMeters.DollarIn1, 12345678));
            response.Meters.Add(SasMeters.DollarsIn5, new LongPollReadMeterResponse(SasMeters.DollarsIn5, 23456789));
            response.Meters.Add(SasMeters.DollarsIn10, new LongPollReadMeterResponse(SasMeters.DollarsIn10, 34567890));
            response.Meters.Add(SasMeters.DollarsIn20, new LongPollReadMeterResponse(SasMeters.DollarsIn20, 45678901));
            response.Meters.Add(SasMeters.DollarsIn50, new LongPollReadMeterResponse(SasMeters.DollarsIn50, 56789012));
            response.Meters.Add(SasMeters.DollarsIn100, new LongPollReadMeterResponse(SasMeters.DollarsIn100, 67890123));

            var handler = new Mock<ISasLongPollHandler<LongPollReadMultipleMetersResponse, LongPollReadMultipleMetersData>>(MockBehavior.Strict);
            handler.Setup(m => m.Handle(It.IsAny<LongPollReadMultipleMetersData>())).Returns(response);
            _target.InjectHandler(handler.Object);

            var expected = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendBillCountMeters,
                0x12, 0x34, 0x56, 0x78,  // $1   bills accepted
                0x23, 0x45, 0x67, 0x89,  // $5   bills accepted
                0x34, 0x56, 0x78, 0x90,  // $10  bills accepted
                0x45, 0x67, 0x89, 0x01,  // $20  bills accepted
                0x56, 0x78, 0x90, 0x12,  // $50  bills accepted
                0x67, 0x89, 0x01, 0x23   // $100 bills accepted                   
            };

            var actual = _target.Parse(command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
