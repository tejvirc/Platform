namespace Aristocrat.SasClient.Tests.Parsers
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    ///     Contains the tests for the LP7FReadDateTimeParser class
    /// </summary>
    [TestClass]
    public class LP7FReadDateTimeParserTest
    {
        private const ulong TestDate = 10231984085934; // 10, 23, 1984 at 8:59:34
        private const ulong BadDate = 13231984085934; // 13, 23, 1984 at 8:59:34
        private const int LongPollLength = 7;
        private const int WrongLengthMonth = 0;
        private const int WrongLengthDay = 1;
        private const int WrongLengthYear = 2;
        private const int WrongLengthHour = 4;
        private const int WrongLengthMin = 5;
        private const int WrongLengthSec = 6;
        private readonly LP7FReadDateTimeParser _target = new LP7FReadDateTimeParser();

        [TestInitialize]
        public void InitTest()
        {
            var handler = new Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<bool>, LongPollDateTimeData>>(MockBehavior.Strict);
            var response = new LongPollReadSingleValueResponse<bool>(true);
            handler.Setup(m => m.Handle(It.IsAny<LongPollDateTimeData>())).Returns(response);
            _target.InjectHandler(handler.Object);

        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.ReceiveDateTime, _target.Command);
        }

        [DataRow(TestDate, true, LongPollLength, DisplayName = "Test a good date")]
        [DataRow(BadDate, false, LongPollLength, DisplayName = "Test a bad date")]
        [DataRow(TestDate, false, WrongLengthMonth, DisplayName = "Test for wrong length month")]
        [DataRow(TestDate, false, WrongLengthDay, DisplayName = "Test for wrong length day")]
        [DataRow(TestDate, false, WrongLengthYear, DisplayName = "Test for wrong length year")]
        [DataRow(TestDate, false, WrongLengthHour, DisplayName = "Test for wrong length hour")]
        [DataRow(TestDate, false, WrongLengthMin, DisplayName = "Test for wrong length min")]
        [DataRow(TestDate, false, WrongLengthSec, DisplayName = "Test for wrong length sec")]
        [DataTestMethod]
        public void TestDates(ulong dateTime, bool ack, int longPollLength)
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.ReceiveDateTime
            };

            if (longPollLength != 0)
            {
                var dateToBCD = Utilities.ToBcd(dateTime, longPollLength);
                command.AddRange(dateToBCD);
            }

            var actual = _target.Parse(command).ToList();

            if (ack)
            {
                CollectionAssert.AreEqual(new Collection<byte> { command.First() }, actual);
            }
            else
            {
                CollectionAssert.AreEqual(new Collection<byte> { (byte)(command.First() | SasConstants.Nack) }, actual);
            }
        }
    }
}
