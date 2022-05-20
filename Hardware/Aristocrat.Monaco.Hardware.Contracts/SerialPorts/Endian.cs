namespace Aristocrat.Monaco.Hardware.Contracts.SerialPorts
{
    using System;
    using System.Runtime.InteropServices;
    using System.Linq;

    /// <summary>
    ///     Endian attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class EndianAttribute : Attribute
    {
        /// <summary>
        ///     Get the endian-ness
        /// </summary>
        public Endianness Endianness { get; }

        /// <summary>
        ///     Construct an Endian attribute
        /// </summary>
        /// <param name="endianness">Endian-ness</param>
        public EndianAttribute(Endianness endianness)
        {
            Endianness = endianness;
        }
    }

    /// <summary>
    ///     Enumerate the endian possibilities.
    /// </summary>
    public enum Endianness
    {
        /// <summary>
        /// Big endian (MSB first)
        /// </summary>
        BigEndian,

        /// <summary>
        /// Little endian (LSB first)
        /// </summary>
        LittleEndian
    }

    /// <summary>
    /// Endian services.
    /// </summary>
    public class Endian
    {
        /// <summary>
        /// Enforce endian behavior
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="data">Byte data</param>
        public static void RespectEndianness(Type type, byte[] data)
        {
            var fields = type.GetFields().Where(f => f.IsDefined(typeof(EndianAttribute), false))
                .Select(
                    f => new
                    {
                        Field = f,
                        Attribute = (EndianAttribute)f.GetCustomAttributes(typeof(EndianAttribute), false)[0],
                        Offset = Marshal.OffsetOf(type, f.Name).ToInt32()
                    }).ToList();

            foreach (var field in fields)
            {
                if ((field.Attribute.Endianness == Endianness.BigEndian && BitConverter.IsLittleEndian) ||
                    (field.Attribute.Endianness == Endianness.LittleEndian && !BitConverter.IsLittleEndian))
                {
                    Array.Reverse(data, field.Offset, Marshal.SizeOf(field.Field.FieldType));
                }
            }
        }
    }
}
