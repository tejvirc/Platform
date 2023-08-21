namespace Aristocrat.Sas.Client
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using log4net;

    /// <summary>
    /// Definition of the Utilities class.
    /// </summary>
    public static class Utilities
    {
        /// <summary>Create a logger for use in this class</summary>
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Dictionary<long, BillDenominationCodes> DenominationCodes = new Dictionary<long, BillDenominationCodes>
        {
            { 1_00, BillDenominationCodes.One },
            { 2_00, BillDenominationCodes.Two },
            { 5_00, BillDenominationCodes.Five },
            { 10_00, BillDenominationCodes.Ten },
            { 20_00, BillDenominationCodes.Twenty },
            { 25_00, BillDenominationCodes.TwentyFive },
            { 50_00, BillDenominationCodes.Fifty },
            { 100_00, BillDenominationCodes.OneHundred },
            { 200_00, BillDenominationCodes.TwoHundred },
            { 250_00, BillDenominationCodes.TwoHundredFifty },
            { 500_00, BillDenominationCodes.FiveHundred },
            { 1_000_00, BillDenominationCodes.OneThousand },
            { 2_000_00, BillDenominationCodes.TwoThousand },
            { 2_500_00, BillDenominationCodes.TwoThousandFiveHundred },
            { 5_000_00, BillDenominationCodes.FiveThousand },
            { 10_000_00, BillDenominationCodes.TenThousand },
            { 20_000_00, BillDenominationCodes.TwentyThousand },
            { 25_000_00, BillDenominationCodes.TwentyFiveThousand },
            { 50_000_00, BillDenominationCodes.FiftyThousand },
            { 100_000_00, BillDenominationCodes.OneHundredThousand },
            { 200_000_00, BillDenominationCodes.TwoHundredThousand },
            { 250_000_00, BillDenominationCodes.TwoHundredFiftyThousand },
            { 500_000_00, BillDenominationCodes.FiveHundredThousand },
            { 1_000_000_00, BillDenominationCodes.OneMillion }
        };
        private const int MinimumLongPollLength = 4;

        /// <summary>
        ///     Calculates a crc up to the crc offset and adds the crc at the crc offset.
        /// </summary>
        /// <param name="data">The data to generate a CRC from.</param>
        /// <returns>The CRCed message.</returns>
        public static byte[] CalculateAndAddCrc(byte[] data)
        {
            return CalculateAndAddCrc(data, (uint)data.Length);
        }

        /// <summary>
        ///     Performs a crc up to the crc offset and adds the crc at the crc offset.
        /// </summary>
        /// <param name="data">The data to generate a CRC from.</param>
        /// <param name="crcOffset">The offset where the CRC will be placed.</param>
        /// <returns>The CRCed message.</returns>
        public static byte[] CalculateAndAddCrc(byte[] data, uint crcOffset)
        {
            byte[] messageWithCrc = new byte[data.Length + 2];
            ushort crc = GenerateCrc(data, crcOffset);
            Array.Copy(data, messageWithCrc, data.Length);
            messageWithCrc[data.Length] = (byte)(crc & 0xFF);
            messageWithCrc[data.Length + 1] = (byte)((crc & 0xFF00) >> 8);
            
            return messageWithCrc;
        }

        /// <summary>
        ///     Generates a CRC for the message per section 5 of the SAS spec.
        /// </summary>
        /// <param name="data">The message to generate crc from</param>
        /// <param name="length">The numberOfBytesReturned of the data to check</param>
        /// <returns>The 2 byte crc.</returns>
        public static ushort GenerateCrc(byte[] data, uint length)
        {
            const uint nibble = 0xf;        // Octal 017
            const uint polynomial = 0x1081; // Octal 010201

            ushort crcValue = 0;

            if (data.Length < length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), @"Specified numberOfBytesReturned is longer than provided data");
            }

            for (uint index = 0; index < length; ++index)
            {
                uint currentByte = data[index];
                var quotient = (crcValue ^ currentByte) & nibble;
                crcValue = (ushort)((crcValue >> 4) ^ (quotient * polynomial));
                quotient = (crcValue ^ (currentByte >> 4)) & nibble;
                crcValue = (ushort)((crcValue >> 4) ^ (quotient * polynomial));
            }

            return crcValue;
        }

        /// <summary>
        ///     Performs a CRC check over a long poll and
        ///     returns true if the message is valid.
        /// </summary>
        /// <param name="data">The long poll to check, including the 2 byte CRC at the end.</param>
        /// <returns>Returns true if the message was valid.</returns>
        public static bool CheckCrcWithSasAddress(byte[] data)
        {
            if (data.Length < MinimumLongPollLength)
            {
                Logger.ErrorFormat($"Data numberOfBytesReturned {data.Length} is less than 4");
                return false;
            }

            ushort actualCrc = GenerateCrc(data, (uint)(data.Length - 2));
            ushort leastSignificantByte = (ushort)(actualCrc & 0x00FF);
            ushort mostSignificantByte = (ushort)(actualCrc >> 8);

            Logger.Debug($"calculated CRC with address is {leastSignificantByte:X2} {mostSignificantByte:X2}");

            // check if calculated and supplied crc matches
            return leastSignificantByte == data[data.Length - 2] &&
                   mostSignificantByte == data[data.Length - 1];
        }

        /// <summary>
        ///     Converts from a little-endian binary to an unsigned integer.
        /// </summary>
        /// <param name="data">data to convert from little-endian binary.</param>
        /// <param name="offset">offset in the data array.</param>
        /// <param name="length">number of bytes to convert (1, 2, 3, or 4).</param>
        /// <returns>The resulting unsigned integer of the conversion.</returns>
        public static uint FromBinary(byte[] data, uint offset, int length)
        {
            if (offset + length > data.Length)
            {
                throw new OverflowException("Offset+length is greater than data length.");
            }

            if (length < 1 || length > sizeof(uint))
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, $"Length must be 1-{sizeof(uint)}");
            }

            uint result = 0;
            const int byteSize = 8;

            for (var i = 0; i < length; i++)
            {
                result |= (uint)data[offset + i] << i * byteSize;
            }

            return result;
        }

        /// <summary>
        ///     Converts from a little-endian binary to an unsigned long.
        /// </summary>
        /// <param name="data">data to convert from little-endian binary.</param>
        /// <param name="offset">offset in the data array.</param>
        /// <param name="length">number of bytes to convert (1, 2, 3, or 4).</param>
        /// <returns>The resulting unsigned integer of the conversion.</returns>
        public static ulong FromBinary64Bits(byte[] data, int offset, int length)
        {
            if (offset + length > data.Length)
            {
                throw new OverflowException("Offset+length is greater than data length.");
            }

            if (length < 1 || length > sizeof(ulong))
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, $"Length must be 1-{sizeof(ulong)}");
            }

            ulong result = 0;
            const int byteSize = 8;

            for (var i = 0; i < length; i++)
            {
                result |= (ulong)data[offset + i] << i * byteSize;
            }

            return result;
        }

        /// <summary>
        ///     Converts from a binary coded decimal to a long and indicates
        ///     if the input data was valid BCD
        /// </summary>
        /// <param name="data">data to convert from BCD.</param>
        /// <param name="offset">offset in the data array.</param>
        /// <param name="length">number of bytes to convert.</param>
        /// <returns>The resulting value of the conversion and
        /// a flag indicating whether the BCD data was valid or not.</returns>
        public static (ulong number, bool validBcd) FromBcdWithValidation(byte[] data, uint offset, int length)
        {
            bool validBcd = true;
            ulong result = 0;
            var dataLength = data.Length;
            if (offset + length > dataLength)
            {
                return (result, false);
            }

            ulong multiplier = 1;
            for (var i = (int)offset + (length - 1); i >= offset; i--)
            {
                ulong ones = data[i] & 0xfU;
                ulong tens = (ulong)data[i] >> 4;

                // Check for validity
                if (ones > 9 || tens > 9)
                {
                    result = 0;
                    validBcd = false;
                    break;
                }

                result += ones * multiplier;
                multiplier *= 10;
                result += tens * multiplier;
                multiplier *= 10;
            }

            return (result, validBcd);
        }

        /// <summary>
        ///     Converts from a binary coded decimal to a long and indicates
        ///     if the input data was valid BCD
        /// </summary>
        /// <param name="data">data to convert from BCD.</param>
        /// <returns>The resulting value of the conversion and
        /// a flag indicating whether the BCD data was valid or not.</returns>
        public static (ulong number, bool validBcd) FromBcdWithValidation(byte[] data)
        {
            return FromBcdWithValidation(data, 0, data.Length);
        }

        /// <summary>
        ///     Turns the specified bytes into a string.
        /// </summary>
        /// <param name="data">The bytes to turn into a string.</param>
        /// <param name="offset">Where in the array to start.</param>
        /// <param name="length">How many bytes to read.</param>
        /// <returns>The string from the bytes.</returns>
        public static (string text, bool validData) FromBytesToString(byte[] data, uint offset, uint length)
        {
            return offset + length > data.Length ?
                (string.Empty, false) :
                (System.Text.Encoding.UTF8.GetString(data, (int)offset, (int)length), true);
        }

        /// <summary>
        ///     This method converts a 64-bit integer into a BCD string ( not ASCII ).
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        /// <param name="bcdLength">The number of BCDs in the byte array.</param>
        /// <returns>the BCD representation of the integer.</returns>
        public static byte[] ToBcd(ulong value, int bcdLength)
        {
            const int byteShift = 100;
            var result = new byte[bcdLength];
            var byteIndex = bcdLength - 1;

            while (value > 0 && byteIndex >= 0)
            {
                result[byteIndex--] = ToBcd(value);
                value /= byteShift;
            }

            return result;
        }

        /// <summary>
        ///     This method converts the first 2 digits to BCD string ( not ASCII ).
        ///     Note only the lowest 2 digits will be converted into a BCD.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        /// <returns>The 2 digit BCD</returns>
        public static byte ToBcd(ulong value)
        {
            const int mask = 0x0F;
            const int digitShift = 4;
            const int digitDivider = 10;

            var data = (byte)((value % digitDivider) & mask);
            value /= digitDivider;
            data |= (byte)((value % digitDivider) << digitShift);

            return data;
        }

        /// <summary>
        ///     Converts a BCD date from MMDDYYYY into a DateTime.
        ///     DateTime.Min is returned when the provided date is invalid.
        /// </summary>
        /// <param name="dateValue">The date you want to convert</param>
        /// <returns>The date time converted or DateTime.MinValue if it cannot be converted</returns>
        public static DateTime FromSasDate(ulong dateValue)
        {
            const string dateFormat = "MMddyyyy";
            var daysString = dateValue.ToString("D8", CultureInfo.InvariantCulture);

            return DateTime.TryParseExact(
                daysString,
                dateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date) ? date : DateTime.MinValue;
        }

        /// <summary>
        ///     Converts a BCD time from hhmmss into a DateTime.
        ///     DateTime.Min is returned when the provided date is invalid.
        /// </summary>
        /// <param name="timeValue">The date you want to convert</param>
        /// <returns>The date time converted or DateTime.MinValue if it cannot be converted</returns>
        public static DateTime FromSasTime(ulong timeValue)
        {
            const string timeFormat = "hhmmss";
            var timeString = timeValue.ToString("D8", CultureInfo.InvariantCulture);

            return DateTime.TryParseExact(
                timeString,
                timeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var time) ? time : DateTime.MinValue;
        }

        /// <summary>
        ///     Converts a BCD date time from MMDDYYYYHHMMSS into a DateTime.
        ///     DateTime.Min is returned when the provided date is invalid.
        /// </summary>
        /// <param name="dateTimeValue">The date you want to convert</param>
        /// <returns>The date time converted or DateTime.MinValue if it cannot be converted</returns>
        public static DateTime FromSasDateTime(ulong dateTimeValue)
        {
            const string dateTimeFormat = "MMddyyyyhhmmss";
            var dateTimeString = dateTimeValue.ToString("D14", CultureInfo.InvariantCulture);

            return DateTime.TryParseExact(
                dateTimeString,
                dateTimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var time) ? time : DateTime.MinValue;
        }

        /// <summary>
        ///     Converts the date into a byte array used by SAS.
        ///     Date is returned as a BCD in the format MMDDYYYY.
        /// </summary>
        /// <param name="date">The date you want to convert into bytes</param>
        /// <returns>A BCD Date (MMDDYYYY)</returns>
        public static byte[] ToSasDate(DateTime date)
        {
            const int monthIndex = 0;
            const int dayIndex = 1;
            const int yearIndex = 2;
            const int yearLength = 2;

            var dateBytes = new byte[SasConstants.Bcd8Digits];
            dateBytes[monthIndex] = ToBcd((ulong)date.Month);
            dateBytes[dayIndex] = ToBcd((ulong)date.Day);
            var year = ToBcd((ulong)date.Year, SasConstants.Bcd4Digits);
            Array.Copy(year, 0, dateBytes, yearIndex, yearLength);

            return dateBytes;
        }

        /// <summary>
        ///     Converts the time into a byte array used by SAS.
        ///     Time is returned as a BCD in the format HHMMSS 24-hour format.
        /// </summary>
        /// <param name="date">The time you want to convert into bytes</param>
        /// <returns>A BCD 24-hour time (HHMMSS)</returns>
        public static byte[] ToSasTime(DateTime date)
        {
            const int hourIndex = 0;
            const int minuteIndex = 1;
            const int secondIndex = 2;

            var timeBytes = new byte[SasConstants.Bcd6Digits];
            timeBytes[hourIndex] = ToBcd((ulong)date.Hour);
            timeBytes[minuteIndex] = ToBcd((ulong)date.Minute);
            timeBytes[secondIndex] = ToBcd((ulong)date.Second);

            return timeBytes;
        }

        /// <summary>
        ///     Converts the date into a byte array used by SAS.
        ///     Date is returned as a BCD in the format MMDDYYYYHHMMSS.
        /// </summary>
        /// <param name="dateTime">The date you want to convert into bytes</param>
        /// <returns>A BCD Date (MMDDYYYYHHMMSS)</returns>
        public static byte[] ToSasDateTime(DateTime dateTime)
        {
            const int monthIndex = 0;
            const int dayIndex = 1;
            const int yearIndex = 2;
            const int yearLength = 2;
            const int hourIndex = 4;
            const int minuteIndex = 5;
            const int secondIndex = 6;

            var dateTimeBytes = new byte[SasConstants.Bcd14Digits];
            dateTimeBytes[monthIndex] = ToBcd((ulong)dateTime.Month);
            dateTimeBytes[dayIndex] = ToBcd((ulong)dateTime.Day);
            var year = ToBcd((ulong)dateTime.Year, SasConstants.Bcd4Digits);
            Array.Copy(year, 0, dateTimeBytes, yearIndex, yearLength);
            dateTimeBytes[hourIndex] = ToBcd((ulong)dateTime.Hour);
            dateTimeBytes[minuteIndex] = ToBcd((ulong)dateTime.Minute);
            dateTimeBytes[secondIndex] = ToBcd((ulong)dateTime.Second);
            return dateTimeBytes;
        }

        /// <summary>
        ///     Converts from an unsigned long integer to little-endian byte array.
        /// </summary>
        /// <param name="data">data to convert</param>
        /// <param name="length">number of bytes to convert (1 to 8).</param>
        /// <returns>The resulting byte array of the conversion.</returns>
        public static byte[] ToBinary(ulong data, int length)
        {
            if (length < 1 || length > sizeof(ulong))
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, $"Must be 1-{sizeof(ulong)}");
            }

            const int byteSize = 8;
            if (data >= Math.Pow(2, length * byteSize))
            {
                throw new OverflowException("Length is not big enough for data size.");
            }

            var result = new byte[length];

            for (var i = 0; i < length; ++i)
            {
                result[i] = (byte)((data >> i * byteSize) & 0xFF);
            }

            return result;
        }

        /// <summary>
        ///     Converts a bill value in dollars to a denomination code
        /// </summary>
        /// <param name="value">The value of the bill in dollars</param>
        /// <returns>The denomination code for the bill or -1 if there isn't a matching code</returns>
        public static int ConvertBillValueToDenominationCode(long value)
        {
            if (!DenominationCodes.ContainsKey(value))
            {
                // unknown amount so give error code
                return -1;
            }

            return (int)DenominationCodes[value];
        }

        /// <summary> 
        ///     Converts a string having ASCII encoding to a list of bytes encoded in the BCD format.
        /// </summary>
        /// <param name="source">the source string</param>
        /// <param name="packed">indicates whether to pack the data.</param>
        /// <param name="numberOfBytesReturned">the number bytes returned in the list</param>
        /// <returns>a list of bytes encoded in the BCD format.</returns>
        public static IList<byte> AsciiStringToBcd(string source, bool packed, uint numberOfBytesReturned)
        {
            List<byte> builder = new List<byte>();
            if (!source.All(char.IsDigit))
            {
                // just give an empty list.
                return builder;
            }

            int i = 0;
            while (i < source.Length)
            {
                if (packed && !(i == 0 && (source.Length & 0x01) > 0))
                {
                    builder.Add((byte)(((source[i] - '0') << 4) | (source[i + 1] - '0') & 0x0F));
                    i += 2;
                }
                else
                {
                    // not packed or first character of odd numberOfBytesReturned string
                    builder.Add((byte)(source[i] - '0'));
                    i++;
                }
            }

            if (builder.Count < numberOfBytesReturned)
            {
                for (i = 0; i < numberOfBytesReturned - builder.Count; ++i)
                {
                    builder.Add(0);
                }
            }
            else if (builder.Count > numberOfBytesReturned)
            {
                builder.RemoveRange((int)numberOfBytesReturned, builder.Count - (int)numberOfBytesReturned);
            }

            return builder;
        }
    }
}
