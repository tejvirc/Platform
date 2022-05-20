namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;
    using System;

    /// <summary>
    ///     Contains the tests for the LP7ESendDateTimeParserTest class
    /// </summary>
    [TestClass]
    public class LP7ESendDateTimeParserTest
    {
        private LP7ESendDateTimeParser _target = new LP7ESendDateTimeParser();
        private const ulong _testDate = (ulong)(10231984085934); /// 10, 23, 1984 at 8:59:34

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SetCurrentDateTime, _target.Command);
        }

        [TestMethod]
        /// <summary>
        ///     Test the parser 
        /// </summary>
        public void ParseSucceedTest()
        {
            DateTime testDateTime = Utilities.FromSasDateTime(_testDate);
            var dateToBCD = Utilities.ToBcd(_testDate, 7);
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SetCurrentDateTime
            };

            var response = new LongPollDateTimeResponse(testDateTime);
            var handler = new Mock<ISasLongPollHandler<LongPollDateTimeResponse, LongPollDateTimeData>>(MockBehavior.Strict);

            handler.Setup(m => m.Handle(It.IsAny<LongPollDateTimeData>())).Returns(response);
            _target.InjectHandler(handler.Object);

            var expected = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SetCurrentDateTime
            };
            expected.AddRange(dateToBCD);

            var actual = _target.Parse(command).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
