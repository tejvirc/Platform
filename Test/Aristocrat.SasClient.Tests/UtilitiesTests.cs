namespace Aristocrat.SasClient.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sas.Client;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
    public class UtilitiesTests
    {
        [DataRow((ulong)12302019, 12, 30, 2019, DisplayName = "Valid date")]
        [DataRow((ulong)02292020, 2, 29, 2020, DisplayName = "Leap year test")]
        [DataTestMethod]
        public void FromSasDateTest(ulong sasDate, int month, int day, int year)
        {
            Assert.AreEqual(new DateTime(year, month, day), Utilities.FromSasDate(sasDate));
        }

        [DataRow((ulong)13302019, DisplayName = "Invalid month test")]
        [DataRow((ulong)12332019, DisplayName = "Invalid day test")]
        [DataRow((ulong)02292019, DisplayName = "Not in a leap year test")]
        [DataTestMethod]
        public void FromSasDateInvalidDateTest(ulong sasDate)
        {
            Assert.AreEqual(DateTime.MinValue, Utilities.FromSasDate(sasDate));
        }

        [DataRow(12, 30, 2019, new byte[] { 0x12, 0x30, 0x20, 0x19 }, DisplayName = "Valid date")]
        [DataRow(2, 29, 2020, new byte[] { 0x02, 0x29, 0x20, 0x20 }, DisplayName = "Leap year test")]
        [DataTestMethod]
        public void ToSasDateTest(int month, int day, int year, byte[] expectedResults)
        {
            CollectionAssert.AreEquivalent(expectedResults, Utilities.ToSasDate(new DateTime(year, month, day)));
        }

        [TestMethod]
        public void ToSasTimeTest()
        {
            var expectedTime = new byte[] { 0x14, 0x30, 0x15 };
            var currentDate = DateTime.Now;
            var convertingTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 14, 30, 15);

            CollectionAssert.AreEqual(expectedTime, Utilities.ToSasTime(convertingTime));
        }

        [DataRow((ulong)1234567890, new byte[] { 0x12, 0x34, 0x56, 0x78, 0x90 }, DisplayName = "Expected count converts correctly")]
        [DataRow((ulong)1234567890, new byte[] { 0x00, 0x00, 0x00, 0x12, 0x34, 0x56, 0x78, 0x90 }, DisplayName = "Extra size pads zeros")]
        [DataTestMethod]
        public void MultiByteToBcdTest(ulong convertingNumber, byte[] expectedResult)
        {
            CollectionAssert.AreEquivalent(expectedResult, Utilities.ToBcd(convertingNumber, expectedResult.Length));
        }

        [DataRow((ulong)3, (byte)0x03, DisplayName = "Extra size pads zeros")]
        [DataRow((ulong)93, (byte)0x93, DisplayName = "Exact size converts correctly")]
        [DataRow((ulong)123, (byte)0x23, DisplayName = "Larger than a single bcd returns the lower bytes")]
        [DataTestMethod]
        public void SingleByteToBcdTest(ulong convertingNumber, byte expectedResult)
        {
            Assert.AreEqual(expectedResult, Utilities.ToBcd(convertingNumber));
        }

        [DataRow(new byte[] { 0x12, 0x34 }, (uint)0, SasConstants.Bcd4Digits, 1234, true, DisplayName = "Happy path")]
        [DataRow(new byte[] { 0x1A, 0x34 }, (uint)0, SasConstants.Bcd4Digits, 0, false, DisplayName = "Invalid BCD")]
        [DataRow(new byte[] { 0x1A, 0x34 }, (uint)3, SasConstants.Bcd4Digits, 0, false, DisplayName = "Offset > data size")]
        [DataRow(new byte[] { 0x1A, 0x34 }, (uint)0, SasConstants.Bcd6Digits, 0, false, DisplayName = "length > data size")]
        [DataRow(new byte[] { 0x1A, 0x34 }, (uint)1, SasConstants.Bcd4Digits, 0, false, DisplayName = "offset + length > data size")]
        [DataTestMethod]
        public void FromBcdWithValidationTest(byte[] data, uint offset, int length, long expectedNumber, bool expectedValidation)
        {
            var expectedResult = ((ulong)expectedNumber, expectedValidation);
            Assert.AreEqual(expectedResult, Utilities.FromBcdWithValidation(data, offset, length));
        }

        [TestMethod]
        public void FromBcdWithValidationShortTest()
        {
            var expected = ((ulong)1234, true);
            var actual = Utilities.FromBcdWithValidation(new byte[] { 0x12, 0x34 });

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void FromBytesToStringTest()
        {
            var expected = ("abc", true);
            var actual = Utilities.FromBytesToString(new[] { (byte)'a', (byte)'b', (byte)'c' }, 0, 3);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void FromBytesToStringWrongOffsetLengthTest()
        {
            var expected = (string.Empty, false);
            var actual = Utilities.FromBytesToString(new[] { (byte)'a', (byte)'b', (byte)'c' }, 3, 3);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AsciiStringToBcdTest()
        {
            var expected = new List<byte> { 0x12, 0x34, 0x56 };
            var actual = Utilities.AsciiStringToBcd("123456", true, 3).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AsciiStringToBcdOddLengthTruncateTest()
        {
            var expected = new List<byte> { 0x1, 0x23, 0x45 };
            var actual = Utilities.AsciiStringToBcd("1234567", true, 3).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AsciiStringToBcdFillTest()
        {
            var expected = new List<byte> { 0x12, 0x34, 0x56, 0x00 };
            var actual = Utilities.AsciiStringToBcd("123456", true, 4).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AsciiStringToBcdNotADigitTest()
        {
            var expected = new List<byte>();
            var actual = Utilities.AsciiStringToBcd("1A3456", true, 4).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GenerateCrcExceptionTest()
        {
            var data = new[] { (byte)0x00 };
            Utilities.GenerateCrc(data, 4);

            // should throw exception
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void FromBinaryExceptionTest()
        {
            var data = new[] { (byte)0x00 };

            // we only support length of 1-4
            Utilities.FromBinary(data, 0, 5);

            // should throw exception
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FromBinaryException2Test()
        {
            var data = new[] { (byte)0x00 };

            // we only support length of 1-4
            Utilities.FromBinary(data, 0, 0);

            // should throw exception
        }

        [DataRow(new[] { (byte)0x00 }, (uint)0, 1, (uint)0, DisplayName = "0 with one byte")]
        [DataRow(new[] { (byte)0x01 }, (uint)0, 1, (uint)1, DisplayName = "1 with one byte")]
        [DataRow(new[] { (byte)0x0D }, (uint)0, 1, (uint)13, DisplayName = "13 with one byte")]
        [DataRow(new[] { (byte)0x45 }, (uint)0, 1, (uint)69, DisplayName = "69 with one byte")]
        [DataRow(new[] { (byte)0xFF }, (uint)0, 1, (uint)255, DisplayName = "255 with one byte")]
        [DataRow(new[] { (byte)0x00, (byte)0x00 }, (uint)0, 2, (uint)0, DisplayName = "0 with two bytes")]
        [DataRow(new[] { (byte)0x01, (byte)0x00 }, (uint)0, 2, (uint)1, DisplayName = "1 with two bytes")]
        [DataRow(new[] { (byte)0x0D, (byte)0x00 }, (uint)0, 2, (uint)13, DisplayName = "13 with two bytes")]
        [DataRow(new[] { (byte)0x45, (byte)0x00 }, (uint)0, 2, (uint)69, DisplayName = "69 with two bytes")]
        [DataRow(new[] { (byte)0xFF, (byte)0x00 }, (uint)0, 2, (uint)255, DisplayName = "255 with two bytes")]
        [DataRow(new[] { (byte)0x39, (byte)0x01 }, (uint)0, 2, (uint)313, DisplayName = "313 with two bytes")]
        [DataRow(new[] { (byte)0xFF, (byte)0xFF }, (uint)0, 2, (uint)65535, DisplayName = "65535 with two bytes")]
        [DataRow(new[] { (byte)0x00, (byte)0x00, (byte)0x00 }, (uint)0, 3, (uint)0, DisplayName = "0 with three bytes")]
        [DataRow(new[] { (byte)0xFF, (byte)0xFF, (byte)0x00 }, (uint)0, 3, (uint)65535, DisplayName = "65535 with three bytes")]
        [DataRow(new[] { (byte)0x89, (byte)0xA2, (byte)0x0A }, (uint)0, 3, (uint)696969, DisplayName = "696969 with three bytes")]
        [DataRow(new[] { (byte)0xFF, (byte)0xFF, (byte)0xFF }, (uint)0, 3, (uint)16777215, DisplayName = "16777215 with three bytes")]
        [DataRow(new[] { (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00 }, (uint)0, 4, (uint)0, DisplayName = "0 with four bytes")]
        [DataRow(new[] { (byte)0x89, (byte)0xA2, (byte)0x0A, (byte)0x00 }, (uint)0, 4, (uint)696969, DisplayName = "16777215 with four bytes")]
        [DataRow(new[] { (byte)0xFF, (byte)0xFF, (byte)0xFF, (byte)0x00 }, (uint)0, 4, (uint)16777215, DisplayName = "16777215 with four bytes")]
        [DataRow(new[] { (byte)0xC9, (byte)0x7D, (byte)0x27, (byte)0x04 }, (uint)0, 4, (uint)69696969, DisplayName = "69696969 with four bytes")]
        [DataTestMethod]
        public void ValidConversionFromBinaryToUintTest(byte[] testData, uint offset, int length, uint expectedOutput)
        {
            var actual = Utilities.FromBinary(testData, offset, length);
            Assert.AreEqual(expectedOutput, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void FromLongBinaryExceptionTest()
        {
            var data = new[] { (byte)0x00 };

            // we only support length of 1-8
            Utilities.FromBinary64Bits(data, 0, 9);

            // should throw exception
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FromLongBinaryException2Test()
        {
            var data = new[] { (byte)0x00 };

            // we only support length of 1-8
            Utilities.FromBinary64Bits(data, 0, 0);

            // should throw exception
        }

        [DataRow(new byte[] { 0x00 }, 0, 1, 0u, DisplayName = "0 with one byte")]
        [DataRow(new byte[] { 0x01 }, 0, 1, 1u, DisplayName = "1 with one byte")]
        [DataRow(new byte[] { 0x45 }, 0, 1, 69u, DisplayName = "69 with one byte")]
        [DataRow(new byte[] { 0xFF }, 0, 1, 255u, DisplayName = "255 with one byte")]
        [DataRow(new byte[] { 0x00, 0x00 }, 0, 2, 0u, DisplayName = "0 with two bytes")]
        [DataRow(new byte[] { 0x45, 0x00 }, 0, 2, 69u, DisplayName = "69 with two bytes")]
        [DataRow(new byte[] { 0x39, 0x01 }, 0, 2, 313u, DisplayName = "313 with two bytes")]
        [DataRow(new byte[] { 0xFF, 0xFF }, 0, 2, 65535u, DisplayName = "65535 with two bytes")]
        [DataRow(new byte[] { 0x00, 0x00, 0x00 }, 0, 3, 0u, DisplayName = "0 with three bytes")]
        [DataRow(new byte[] { 0xFF, 0xFF, 0x00 }, 0, 3, 65535u, DisplayName = "65535 with three bytes")]
        [DataRow(new byte[] { 0x89, 0xA2, 0x0A }, 0, 3, 696969u, DisplayName = "696969 with three bytes")]
        [DataRow(new byte[] { 0xFF, 0xFF, 0xFF }, 0, 3, 16777215u, DisplayName = "16777215 with three bytes")]
        [DataRow(new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0, 4, 0u, DisplayName = "0 with four bytes")]
        [DataRow(new byte[] { 0x89, 0xA2, 0x0A, 0x00 }, 0, 4, 696969u, DisplayName = "16777215 with four bytes")]
        [DataRow(new byte[] { 0xFF, 0xFF, 0xFF, 0x00 }, 0, 4, 16777215u, DisplayName = "16777215 with four bytes")]
        [DataRow(new byte[] { 0xC9, 0x7D, 0x27, 0x04 }, 0, 4, 69696969u, DisplayName = "69696969 with four bytes")]
        [DataRow(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, 5, 0u, DisplayName = "0 with five bytes")]
        [DataRow(new byte[] { 0xAA, 0xAA, 0xAA, 0xAA, 0xAA }, 0, 5, 0xAAAAAAAAAAu, DisplayName = "733,007,751,850 with five bytes")]
        [DataRow(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, 6, 0u, DisplayName = "0 with six bytes")]
        [DataRow(new byte[] { 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA }, 0, 6, 0xAAAAAAAAAAAAu, DisplayName = "187,649,984,473,770 with six bytes")]
        [DataRow(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, 7, 0u, DisplayName = "0 with seven bytes")]
        [DataRow(new byte[] { 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA }, 0, 7, 0xAAAAAAAAAAAAAAu, DisplayName = "48,038,396,025,285,290 with seven bytes")]
        [DataRow(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, 8, 0u, DisplayName = "0 with eight bytes")]
        [DataRow(new byte[] { 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA }, 0, 8, 0xAAAAAAAAAAAAAAAAu, DisplayName = "1.2297829382473E+19 with eight bytes")]
        [DataTestMethod]
        public void ValidConversionFromBinaryToUint64Test(byte[] testData, int offset, int length, ulong expectedOutput)
        {
            var actual = Utilities.FromBinary64Bits(testData, offset, length);
            Assert.AreEqual(expectedOutput, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void FromBinaryWithBadOffsetTest()
        {
            // If the offset is greater than the byte array length then
            // we will overflow
            Utilities.FromBinary(new byte[1], 1, 1);
        }

        [DataRow(696969696969696969u, 8, new byte[] { 0xC9, 0x26, 0x17, 0x27, 0x3F, 0x22, 0xAC, 0x09 }, DisplayName = "696969696969696969 as 8 bytes")]
        [DataRow(6969696969696969u, 7, new byte[] { 0xC9, 0x26, 0x12, 0x08, 0xE7, 0xC2, 0x18 }, DisplayName = "6969696969696969 as 7 bytes")]
        [DataRow(69696969696969u, 6, new byte[] { 0xC9, 0xE6, 0x1E, 0x97, 0x63, 0x3F }, DisplayName = "69696969696969 as 6 bytes")]
        [DataRow(696969696969u, 5, new byte[] { 0xC9, 0x96, 0xA1, 0x46, 0xA2 }, DisplayName = "696969696969 as 5 bytes")]
        [DataRow(69696969u, 4, new byte[] { 0xC9, 0x7D, 0x27, 0x04 }, DisplayName = "69696969 as 4 bytes")]
        [DataRow(696969u, 4, new byte[] { 0x89, 0xA2, 0x0A, 0x00 }, DisplayName = "69696969 as 4 bytes")]
        [DataRow(696969u, 3, new byte[] { 0x89, 0xA2, 0x0A }, DisplayName = "696969 as 3 bytes")]
        [DataRow(6969u, 2, new byte[] { 0x39, 0x1B }, DisplayName = "6969 as 2 bytes")]
        [DataRow(1u, 1, new byte[] { 0x01 }, DisplayName = "1 as 1 byte")]
        [DataRow(0u, 1, new byte[] { 0x00 }, DisplayName = "0 as 1 byte")]
        [DataTestMethod]
        public void ValidConversionFromUintToBinary(ulong testData, int testLength, byte[] expectedBytes)
        {
            // This test is to verify that the ToBinary method performs
            // a valid conversion of a uint to a byte array.
            var actual = Utilities.ToBinary(testData, testLength);
            Assert.IsTrue(expectedBytes.SequenceEqual(actual));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ToBinaryExceptionTest()
        {
            // we only support length of 1-8
            Utilities.ToBinary(0, 9);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void ToBinaryWithInsufficientLengthOneByteTest()
        {
            // 256 needs 2 bytes
            Utilities.ToBinary(0x100, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void ToBinaryWithInsufficientLengthTwoByteTest()
        {
            // 65536 needs 3 bytes
            Utilities.ToBinary(0x10000, 2);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void ToBinaryWithInsufficientLength3ByteTest()
        {
            // 16777216 needs 4 bytes
            Utilities.ToBinary(0x1000000, 3);
        }

        [TestMethod]
        public void CheckCrcWithSasAddressNotEnoughDataTest()
        {
            var data = new[] { (byte)0x00 };
            Assert.IsFalse(Utilities.CheckCrcWithSasAddress(data));
        }

        [TestMethod]
        public void ConvertBillValueToDenominationCodeTest()
        {
            // check $1
            Assert.AreEqual((int)BillDenominationCodes.One, Utilities.ConvertBillValueToDenominationCode(100));

            // check illegal value
            Assert.AreEqual(-1, Utilities.ConvertBillValueToDenominationCode(123));
        }
    }
}