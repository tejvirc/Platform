namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    ///     A collection of methods which convert byte arrays to compiler supported data types,
    ///     and vice versa.
    /// </summary>
    internal static class FieldConverters
    {
        /// <summary>
        ///     The map of the FieldType enumeration to .NET types.
        /// </summary>
        public static readonly Dictionary<FieldType, Type> FieldTypeMap = new Dictionary<FieldType, Type>
        {
            { FieldType.Int16, typeof(short) },
            { FieldType.Int32, typeof(int) },
            { FieldType.Int64, typeof(long) },
            { FieldType.Bool, typeof(bool) },
            {
                FieldType.Byte, typeof(bool)
            }, // this is a hack. If you don't do this you get Ambiguous Exceptions during reflection in BlockFormat.Convert function.
            { FieldType.DateTime, typeof(DateTime) },
            { FieldType.String, typeof(string) },
            { FieldType.Guid, typeof(Guid) },
            { FieldType.UInt16, typeof(ushort) },
            { FieldType.UInt32, typeof(uint) },
            { FieldType.UInt64, typeof(ulong) },
            { FieldType.TimeSpan, typeof(TimeSpan) },
            { FieldType.UnboundedString, typeof(string) },
            { FieldType.Float, typeof(float) },
            { FieldType.Double, typeof(double) }
        };

        /// <summary>
        ///     The map of field types to .NET list types.
        /// </summary>
        private static readonly Dictionary<FieldType, IList> FieldListTypeMap = new Dictionary<FieldType, IList>
        {
            { FieldType.Int16, new List<short>() },
            { FieldType.Int32, new List<int>() },
            { FieldType.Int64, new List<long>() },
            { FieldType.Bool, new List<bool>() },
            { FieldType.Byte, new List<byte>() },
            { FieldType.DateTime, new List<DateTime>() },
            { FieldType.String, new List<string>() },
            { FieldType.Guid, new List<Guid>() },
            { FieldType.UInt16, new List<ushort>() },
            { FieldType.UInt32, new List<uint>() },
            { FieldType.UInt64, new List<ulong>() },
            { FieldType.TimeSpan, new List<TimeSpan>() },
            { FieldType.UnboundedString, new List<string>() },
            { FieldType.Float, new List<float>() },
            { FieldType.Double, new List<double>() },
        };

        /// <summary>
        ///     Converts strings from bytes.
        /// </summary>
        private static readonly UTF8Encoding StringEncoder = new UTF8Encoding();

        /// <summary>
        ///     Converts a byte array to an int.
        /// </summary>
        /// <param name="bytes"> The byte array. </param>
        /// <returns> An integer value. </returns>
        public static object IntConvert(byte[] bytes)
        {
            return BitConverter.ToInt32(FillConversionBytes(bytes, sizeof(int)), 0);
        }

        /// <summary>
        ///     Converts a byte array to an short.
        /// </summary>
        /// <param name="bytes"> The byte array. </param>
        /// <returns> A short value. </returns>
        public static object ShortConvert(byte[] bytes)
        {
            return BitConverter.ToInt16(FillConversionBytes(bytes, sizeof(short)), 0);
        }

        /// <summary>
        ///     Converts a byte array to a single byte.
        /// </summary>
        /// <param name="bytes"> The byte array. </param>
        /// <returns> A single byte. </returns>
        public static object ByteConvert(byte[] bytes)
        {
            return bytes[0];
        }

        /// <summary>
        ///     Converts a byte array to a long.
        /// </summary>
        /// <param name="bytes"> The byte array. </param>
        /// <returns> A long value. </returns>
        public static object LongConvert(byte[] bytes)
        {
            return BitConverter.ToInt64(FillConversionBytes(bytes, sizeof(long)), 0);
        }

        /// <summary>
        ///     Converts a byte array to a string.
        /// </summary>
        /// <param name="bytes"> The byte array. </param>
        /// <returns> The string represented by the byte array. </returns>
        public static object StringConvert(byte[] bytes)
        {
            return StringEncoder.GetString(bytes).TrimEnd('\0');
        }

        /// <summary>
        ///     Converts a byte array to a boolean.
        /// </summary>
        /// <param name="bytes"> The byte array. </param>
        /// <returns> The boolean represented by the byte array. </returns>
        public static object BoolConvert(byte[] bytes)
        {
            if (bytes.Length == 0) return false;
            return BitConverter.ToBoolean(bytes, 0);
        }

        /// <summary>
        ///     Converts a byte array to a date time.
        /// </summary>
        /// <param name="bytes"> The byte array. </param>
        /// <returns> The DateTime value represented by the byte array. </returns>
        public static object DateTimeConvert(byte[] bytes)
        {
            var timeValue = BitConverter.ToInt64(FillConversionBytes(bytes, sizeof(long)), 0);
            return DateTime.SpecifyKind(DateTime.FromBinary(timeValue), DateTimeKind.Utc);
        }

        /// <summary>
        ///     Converts a byte array to a TimeSpan.
        /// </summary>
        /// <param name="bytes"> The byte array. </param>
        /// <returns> The TimeSpan value represented by the byte array. </returns>
        public static object TimeSpanConvert(byte[] bytes)
        {
            var span = BitConverter.ToDouble(FillConversionBytes(bytes, sizeof(double)), 0);
            return TimeSpan.FromMilliseconds(span);
        }

        /// <summary>
        ///     Converts a byte array to a ushort.
        /// </summary>
        /// <param name="bytes"> The byte array. </param>
        /// <returns> The ushort value represented by the byte array. </returns>
        public static object UInt16Convert(byte[] bytes)
        {
            return BitConverter.ToUInt16(FillConversionBytes(bytes, sizeof(ushort)), 0);
        }

        /// <summary>
        ///     Converts a byte array to a uint.
        /// </summary>
        /// <param name="bytes"> The byte array. </param>
        /// <returns> The uint value represented by the byte array. </returns>
        public static object UInt32Convert(byte[] bytes)
        {
            return BitConverter.ToUInt32(FillConversionBytes(bytes, sizeof(uint)), 0);
        }

        /// <summary>
        ///     Converts a byte array to a ulong.
        /// </summary>
        /// <param name="bytes"> The byte array. </param>
        /// <returns> The ulong value represented by the byte array. </returns>
        public static object UInt64Convert(byte[] bytes)
        {
            return BitConverter.ToUInt64(FillConversionBytes(bytes, sizeof(ulong)), 0);
        }

        /// <summary>
        ///     Converts a byte array to a Guid.
        /// </summary>
        /// <param name="bytes"> The byte array. </param>
        /// <returns> The Guid value represented by the byte array. </returns>
        public static object GuidConvert(byte[] bytes)
        {
            const int guidByteLength = 16;
            var returnVal = default(Guid);

            if (bytes != null && bytes.Length == guidByteLength)
            {
                returnVal = new Guid(bytes);
            }

            return returnVal;
        }

        /// <summary>
        ///     Converts a byte array to a float.
        /// </summary>
        /// <param name="bytes"> The byte array. </param>
        /// <returns> The float value represented by the byte array. </returns>
        public static object FloatConvert(byte[] bytes)
        {
            return BitConverter.ToSingle(FillConversionBytes(bytes, sizeof(float)), 0);
        }

        /// <summary>
        ///     Converts a byte array to a double.
        /// </summary>
        /// <param name="bytes"> The byte array. </param>
        /// <returns> The double value represented by the byte array. </returns>
        public static object DoubleConvert(byte[] bytes)
        {
            return BitConverter.ToDouble(FillConversionBytes(bytes, sizeof(double)), 0);
        }

        /// <summary>
        ///     Returns an array of objects that are converted using the specified field
        ///     description and converter.
        /// </summary>
        /// <param name="bytes"> The bytes representing the objects. </param>
        /// <param name="fd"> The corresponding field description. </param>
        /// <param name="converter"> The converter. </param>
        /// <returns> An array of objects converted from bytes. </returns>
        public static object GetConvertedArray(byte[] bytes, FieldDescription fd, FieldConverter converter)
        {
            var genericList = FieldListTypeMap[fd.DataType];
            genericList.Clear();

            for (var i = 0; i < fd.Count; i++)
            {
                var temp = new byte[fd.Size];
                if (bytes != null && bytes.Length > (i * fd.Size))
                {
                    Buffer.BlockCopy(bytes, i * fd.Size, temp, 0, fd.Size);
                }

                genericList.Add(converter(temp));
            }

            var getArrayMethod = genericList.GetType().GetMethod("ToArray");
            return getArrayMethod?.Invoke(genericList, null);
        }

        /// <summary>
        ///     Fills a temporary byte array of the expected size with the input byte array data
        /// </summary>
        /// <param name="bytes">The input data</param>
        /// <param name="size">The expected size</param>
        /// <returns>The correctly sized byte array</returns>
        private static byte[] FillConversionBytes(byte[] bytes, int size)
        {
            var temp = new byte[size];
            if (bytes != null)
            {
                var tempLen = bytes.Length;

                if (tempLen > size)
                {
                    tempLen = size;
                }

                Buffer.BlockCopy(bytes, 0, temp, 0, tempLen);
            }

            return temp;
        }
    }
}
