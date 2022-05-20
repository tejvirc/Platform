namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.Aft;
    using Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     Contains tests for the LP75SetAftReceiptDataParser class
    /// </summary>
    [TestClass]
    public class LP75SetAftReceiptDataParserTest
    {
        private readonly Mock<ISasLongPollHandler<LongPollResponse, SetAftReceiptData>> _handler = new Mock<ISasLongPollHandler<LongPollResponse, SetAftReceiptData>>();
        private LP75SetAftReceiptDataParser _target;
        private readonly List<byte> _testAck = new List<byte>() { TestConstants.SasAddress };
        private readonly List<byte> _testNack = new List<byte>() { TestConstants.SasAddress | TestConstants.Nack };
        private const string TestValue = "Test Value";
        private const string LongTestValue = "123456789012345678901234567890";
        private const char TestBlankValue = ' ';
        private const int TestBlankValueLength = 1; // 1 char (space)
        private const int TestDefaultValueLength = 0; // 0 chars (null)
        private const int DataHeaderSize = 2; // Number of bytes in Data Code + Data Length
        private const TransactionReceiptDataField LocationField = TransactionReceiptDataField.Location;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP75SetAftReceiptDataParser();
            _target.InjectHandler(_handler.Object);

            var response = new LongPollResponse();
            _handler.Setup(x => x.Handle(It.IsAny<SetAftReceiptData>())).Returns(response);
        }

        [TestMethod]
        public void ParseCommandTooShortTest()
        {
            ParseTester(
                1,
                new Collection<byte> { LocationField },
                false,
                "Small Command Length",
                "minimum command length is 0x02");
        }

        [TestMethod]
        public void ParseCommandLengthLessThanSizeTest()
        {
            ParseTester(
                DataHeaderSize + TestBlankValueLength,
                new Collection<byte> { LocationField, TestValue.Length, TestValue },
                false,
                "Mismatched Command Length",
                "command length is less than total command size");
        }

        [TestMethod]
        public void ParseCommandLengthMoreThanSizeTest()
        {
            ParseTester(
                DataHeaderSize + TestValue.Length,
                new Collection<byte> { LocationField, TestBlankValueLength, TestBlankValue },
                false,
                "Mismatched Command Length",
                "command length is more than total command size");
        }

        [TestMethod]
        public void ParseDataLengthLessThanSizeTest()
        {
            ParseTester(
                DataHeaderSize + TestValue.Length,
                new Collection<byte> { LocationField, TestBlankValueLength, TestValue },
                false,
                "Mismatched Data Length",
                "stated data length is less than actual data length");
        }

        [TestMethod]
        public void ParseDataLengthMoreThanSizeTest()
        {
            ParseTester(
                DataHeaderSize + TestBlankValueLength,
                new Collection<byte> { LocationField, TestValue.Length, TestBlankValue },
                false,
                "Mismatched Data Length",
                "stated data length is more than actual data length");
        }

        [TestMethod]
        public void ParseDataSizeTooLargeTest()
        {
            ParseTester(
                DataHeaderSize + LongTestValue.Length,
                new Collection<byte> { LocationField, LongTestValue.Length, LongTestValue },
                false,
                "Location = " + LongTestValue,
                "data value is longer than max data size of 22");
        }

        [TestMethod]
        public void ParseDataFieldDoesNotExistTest()
        {
            ParseTester(
                DataHeaderSize + TestValue.Length,
                new Collection<byte> { 0xFF, TestValue.Length, TestValue },
                false,
                "Bad TransactionReceiptDataField",
                "The TransactionReceiptDataField code is not valid");
        }

        [TestMethod]
        public void ParseLocationDefaultTest()
        {
            ParseTester(
                DataHeaderSize,
                new Collection<byte> { LocationField, TestDefaultValueLength },
                true,
                "Location = Default");
        }

        [TestMethod]
        public void ParseLocationBlankTest()
        {
            ParseTester(
                DataHeaderSize + TestBlankValueLength,
                new Collection<byte> { LocationField, TestBlankValueLength, TestBlankValue },
                true,
                "Location = Blank");
        }

        [TestMethod]
        public void ParseLocationValidValueTest()
        {
            ParseTester(
                DataHeaderSize + TestValue.Length,
                new Collection<byte> { LocationField, TestValue.Length, TestValue },
                true,
                "Location = " + TestValue);
        }

        [TestMethod]
        public void ParseAllFieldsValidValueTest()
        {
            ParseTester(
                (DataHeaderSize + TestValue.Length) * 11,
                new Collection<byte>
                {
                    TransactionReceiptDataField.Location, TestValue.Length, TestValue,
                    TransactionReceiptDataField.Address1, TestValue.Length, TestValue,
                    TransactionReceiptDataField.Address2, TestValue.Length, TestValue,
                    TransactionReceiptDataField.InHouse1, TestValue.Length, TestValue,
                    TransactionReceiptDataField.InHouse2, TestValue.Length, TestValue,
                    TransactionReceiptDataField.InHouse3, TestValue.Length, TestValue,
                    TransactionReceiptDataField.InHouse4, TestValue.Length, TestValue,
                    TransactionReceiptDataField.Debit1, TestValue.Length, TestValue,
                    TransactionReceiptDataField.Debit2, TestValue.Length, TestValue,
                    TransactionReceiptDataField.Debit3, TestValue.Length, TestValue,
                    TransactionReceiptDataField.Debit4, TestValue.Length, TestValue,
                },
                true,
                "All Fields = " + TestValue);
        }

        [TestMethod]
        public void ParseAllFieldsMixedValidValuesTest()
        {
            ParseTester(
                (DataHeaderSize + TestValue.Length) * 5 +
                (DataHeaderSize + TestBlankValueLength) * 3 +
                DataHeaderSize * 3,
                new Collection<byte>
                {
                    TransactionReceiptDataField.Location, TestValue.Length, TestValue,
                    TransactionReceiptDataField.Address1, TestBlankValueLength, TestBlankValue,
                    TransactionReceiptDataField.Address2, TestDefaultValueLength,
                    TransactionReceiptDataField.InHouse1, TestValue.Length, TestValue,
                    TransactionReceiptDataField.InHouse2, TestValue.Length, TestValue,
                    TransactionReceiptDataField.InHouse3, TestBlankValueLength, TestBlankValue,
                    TransactionReceiptDataField.InHouse4, TestDefaultValueLength,
                    TransactionReceiptDataField.Debit1, TestDefaultValueLength,
                    TransactionReceiptDataField.Debit2, TestBlankValueLength, TestBlankValue,
                    TransactionReceiptDataField.Debit3, TestValue.Length, TestValue,
                    TransactionReceiptDataField.Debit4, TestValue.Length, TestValue,
                },
                true,
                "Location = " + TestValue +
                "Address1 = Blank" +
                "Address2 = Default" +
                "InHouse1 = " + TestValue +
                "InHouse2 = " + TestValue +
                "InHouse3 = Blank" +
                "InHouse4 = Default" +
                "Debit1 = Default" +
                "Debit2 = Blank" +
                "Debit3 = " + TestValue +
                "Debit4 = " + TestValue);
        }

        [TestMethod]
        public void ParseByBytesTest()
        {
            var longPoll = new Collection<byte>
            {
                0x01, 0x75, 0x8C, 0x00, 0x0C, 0x56, 0x47, 0x54, 0x20, 0x42,
                0x75, 0x69, 0x6C, 0x64, 0x69, 0x6E, 0x67, 0x01, 0x16, 0x33,
                0x30, 0x38, 0x20, 0x4D, 0x61, 0x6C, 0x6C, 0x6F, 0x72, 0x79,
                0x20, 0x53, 0x74, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x20, 0x52,
                0x64, 0x02, 0x0C, 0x46, 0x72, 0x61, 0x6E, 0x6B, 0x6C, 0x69,
                0x6E, 0x2C, 0x20, 0x54, 0x4E, 0x10, 0x06, 0x44, 0x6F, 0x6E,
                0x27, 0x74, 0x20, 0x11, 0x06, 0x46, 0x6F, 0x72, 0x67, 0x65,
                0x74, 0x12, 0x0D, 0x53, 0x75, 0x70, 0x65, 0x72, 0x20, 0x41,
                0x77, 0x65, 0x73, 0x6F, 0x6D, 0x65, 0x13, 0x13, 0x42, 0x75,
                0x66, 0x66, 0x65, 0x74, 0x20, 0x53, 0x75, 0x70, 0x72, 0x65,
                0x6D, 0x65, 0x20, 0x4D, 0x65, 0x61, 0x6C, 0x20, 0x09, 0x48,
                0x4F, 0x57, 0x20, 0x41, 0x42, 0x4F, 0x55, 0x54, 0x21, 0x06,
                0x41, 0x20, 0x4E, 0x49, 0x43, 0x45, 0x22, 0x07, 0x47, 0x41,
                0x4D, 0x45, 0x20, 0x4F, 0x46, 0x23, 0x06, 0x43, 0x48, 0x45,
                0x53, 0x53, 0x3F, 0x4D, 0x06,
            };

            ParseVerify(longPoll, true, "Poll entered as bytes");
        }

        private void ParseTester(int length, Collection<byte> data, bool success, string testCase, string reason = "")
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress,
                LongPoll.SetAftReceiptData,
                length,
            };

            foreach (var datum in data)
            {
                longPoll.Add(datum);
            }

            longPoll.Add(TestConstants.FakeCrc);
            longPoll.Add(TestConstants.FakeCrc);

            ParseVerify(longPoll, success, testCase, reason);
        }

        private void ParseVerify(Collection<byte> longPoll, bool success, string testCase, string reason = "")
        {
            var actual = _target.Parse(longPoll).ToList();

            CollectionAssert.AreEqual(
                success ? _testAck : _testNack,
                actual,
                testCase + " should have " + (success ? "Succeeded!" : (" Failed because " + reason + "!")));
        }
    }
}

