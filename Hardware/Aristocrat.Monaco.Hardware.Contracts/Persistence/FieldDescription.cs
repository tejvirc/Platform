namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml.Serialization;
    using log4net;

    /// <summary>Field types allowed in persistent storage</summary>
    public enum FieldType
    {
        /// <summary>
        ///     This is to make sure it doesn't default
        /// </summary>
        Unused,

        /// <summary>
        ///     This is an int
        /// </summary>
        Int32,

        /// <summary>
        ///     This is a short
        /// </summary>
        Int16,

        /// <summary>
        ///     This is a byte
        /// </summary>
        Byte,

        /// <summary>
        ///     This is a long
        /// </summary>
        Int64,

        /// <summary>
        ///     This is a string
        /// </summary>
        String,

        /// <summary>
        ///     This is a bool
        /// </summary>
        Bool,

        /// <summary>
        ///     This is a DateTime
        /// </summary>
        DateTime,

        /// <summary>
        ///     This is a ushort
        /// </summary>
        UInt16,

        /// <summary>
        ///     This is a uint
        /// </summary>
        UInt32,

        /// <summary>
        ///     This is a ulong
        /// </summary>
        UInt64,

        /// <summary>
        ///     This is a guid
        /// </summary>
        Guid,

        /// <summary>
        ///     This is a TimeSpan
        /// </summary>
        TimeSpan,

        /// <summary>
        ///     This is an unbounded string.
        /// </summary>
        UnboundedString,

        /// <summary>
        ///     This is a 32-bit floating point number
        /// </summary>
        Float,

        /// <summary>
        ///     This is a 64-bit floating point number
        /// </summary>
        Double

        // NOTE: If you add a type here make sure you add its size below in StandardFieldLength
    }

    /// <summary>
    ///     This is the description of an individual field of data within a Block.
    ///     The description includes items such as the field's name, data type, and size
    /// </summary>
    public sealed class FieldDescription
    {
        /// <summary>
        ///     Create a logger for use in this class.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>Encoder to be used when storing and retrieving the string type into the byte storage.</summary>
        private readonly UTF8Encoding _encoder = new UTF8Encoding();

        /// <summary>
        ///     The offset
        /// </summary>
        private int _offset = -1;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FieldDescription" /> class.
        /// </summary>
        public FieldDescription()
        {
            if (Offset == -1)
            {
                DefaultOffset = true;
            }

            if (Size == 0)
            {
                Size = DataType == FieldType.String ? 1024 : StandardFieldLength[(int)DataType];
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FieldDescription" /> class.
        /// </summary>
        /// <param name="ft">The field type</param>
        /// <param name="size">The size of the field</param>
        /// <param name="count">The number of fields if they are part of an array</param>
        /// <param name="offset">The offset</param>
        /// <param name="name">The name of the field</param>
        public FieldDescription(FieldType ft, int size, int count, int offset, string name)
        {
            DataType = ft;
            Size = size;
            Count = count;
            Offset = offset;
            FieldName = name;
            DefaultOffset = false;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FieldDescription" /> class.
        /// </summary>
        /// <param name="ft">The field type</param>
        /// <param name="size">The size of the field</param>
        /// <param name="count">The number of fields if they are part of an array</param>
        /// <param name="name">The name of the field</param>
        public FieldDescription(FieldType ft, int size, int count, string name)
        {
            DataType = ft;
            Size = size;
            Count = count;
            Offset = -1;
            FieldName = name;
            DefaultOffset = true;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FieldDescription" /> class.
        /// </summary>
        /// <param name="ft">The field type</param>
        /// <param name="count">The number of fields if they are part of an array</param>
        /// <param name="name">The name of the field</param>
        public FieldDescription(FieldType ft, int count, string name)
        {
            DataType = ft;
            Count = count;
            Offset = -1;
            DefaultOffset = true;
            FieldName = name;
            if (ft == FieldType.String)
            {
                var message = "Cannot default size on String type";
                Logger.Fatal(message);
                throw new FieldAccessException(message);
            }

            Size = StandardFieldLength[(int)ft];
        }

        /// <summary>
        ///     Gets the Standard lengths of the fields
        /// </summary>
        public static int[] StandardFieldLength => new[]
        {
            0, sizeof(int), sizeof(short), sizeof(byte), sizeof(long), 0, sizeof(bool), sizeof(long), sizeof(ushort),
            sizeof(uint), sizeof(ulong), Marshal.SizeOf(typeof(Guid)), sizeof(double), -1, sizeof(float), sizeof(double)
        };

        /// <summary>
        ///     Gets or sets a value indicating whether the offset is missing (true), or present (false)
        /// </summary>
        [XmlIgnore]
        public bool DefaultOffset { get; set; }

        /// <summary>Gets or sets the DataType property</summary>
        [XmlAttribute]
        public FieldType DataType { get; set; }

        /// <summary>Gets or sets the Size property</summary>
        [XmlAttribute]
        public int Size { get; set; }

        /// <summary>Gets or sets the Count property</summary>
        [XmlAttribute]
        public int Count { get; set; }

        /// <summary>Gets or sets the Offset property</summary>
        [XmlAttribute]
        public int Offset
        {
            get => _offset;

            set
            {
                DefaultOffset = false;
                _offset = value;
            }
        }

        /// <summary>Gets or sets the FieldName property</summary>
        [XmlAttribute]
        public string FieldName { get; set; }

        /// <summary>Gets the Length (in bytes) of the data to be stored</summary>
        public int Length
        {
            get
            {
                if (Count == 0)
                {
                    return Size;
                }

                return Size * Count;
            }
        }

        /// <summary>
        ///     Used to convert this object to a byte array.
        /// </summary>
        /// <returns>The contents of this FieldDescription instance as a byte array</returns>
        public byte[] GetBytes()
        {
            var returnValue = new byte[(int)FieldOffset.FieldName + FieldName.Length];

            returnValue[(int)FieldOffset.DataType] = (byte)DataType;
            returnValue[(int)FieldOffset.Size] = (byte)Size;
            Buffer.BlockCopy(
                BitConverter.GetBytes(Count),
                0,
                returnValue,
                (int)FieldOffset.Count,
                (int)FieldLength.Count);
            Buffer.BlockCopy(
                BitConverter.GetBytes(Offset),
                0,
                returnValue,
                (int)FieldOffset.Offset,
                (int)FieldLength.Offset);
            returnValue[(int)FieldOffset.FieldNameLength] = (byte)FieldName.Length;
            Buffer.BlockCopy(
                _encoder.GetBytes(FieldName),
                0,
                returnValue,
                (int)FieldOffset.FieldName,
                FieldName.Length);

            return returnValue;
        }

        /// <summary>
        ///     Used to initialize the state of this instance from a byte array.
        /// </summary>
        /// <param name="input">An input byte array used to populate the contents of this instance.</param>
        public void SetBytes(byte[] input)
        {
            DataType = (FieldType)input[(int)FieldOffset.DataType];
            Size = input[(int)FieldOffset.Size];
            Count = (ushort)BitConverter.ToInt16(input, (int)FieldOffset.Count);
            Offset = (ushort)BitConverter.ToInt16(input, (int)FieldOffset.Offset);
            FieldName = _encoder.GetString(input, (int)FieldOffset.FieldName, input[(int)FieldOffset.FieldNameLength]);
        }

        private enum FieldLength
        {
            DataType = 1,
            Size = 1,
            Count = 2,
            Offset = 2,
            FieldNameLength = 1
        }

        private enum FieldOffset
        {
            DataType = 0,
            Size = DataType + FieldLength.DataType,
            Count = Size + FieldLength.Size,
            Offset = Count + FieldLength.Count,
            FieldNameLength = Offset + FieldLength.Offset,
            FieldName = FieldNameLength + FieldLength.FieldNameLength
        }
    }
}
