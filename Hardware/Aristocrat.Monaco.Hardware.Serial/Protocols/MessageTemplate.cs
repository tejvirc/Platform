namespace Aristocrat.Monaco.Hardware.Serial.Protocols
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Define the behavior of a message template.
    ///     
    ///     Message template can be used to define fields within messages, where a protocol
    ///     has such definitions.  For example, some protocols might include message length fields,
    ///     CRCs, Sync characters; they might define the first byte of deliverable content as
    ///     a command or response field, separate from other optional data.  Use
    ///     <see cref="MessageTemplateElement"/>s to define the message shape.
    ///
    ///     There are restrictions on how various <see cref="MessageTemplateElementType"/> fields
    ///     may be used in a template.
    ///     1. There can be at most one length field, whether it's of type <see cref="MessageTemplateElementType.DataLength"/>
    ///        or <see cref="MessageTemplateElementType.FullLength"/>.
    ///     2. There can be at most one <see cref="MessageTemplateElementType.Crc"/> field.
    ///     3. There can be at most one <see cref="MessageTemplateElementType.VariableData"/> field, since it's the only
    ///        one whose length actually varies (based on length field).
    ///     4. There can be any number of <see cref="MessageTemplateElementType.FixedData"/> fields, but they must precede
    ///        a <see cref="MessageTemplateElementType.VariableData"/> field.
    /// </summary>
    public interface IMessageTemplate
    {
        /// <summary>
        ///     Ordered list of elements that make up a message
        /// </summary>
        List<MessageTemplateElement> Elements { get; }

        /// <summary>
        ///     CRC engine for input messages
        /// </summary>
        ICrcEngine CrcEngineIn { get; }

        /// <summary>
        ///     CRC engine for output messages
        /// </summary>
        ICrcEngine CrcEngineOut { get; }

        /// <summary>
        ///     CRC seed
        /// </summary>
        ushort CrcSeed { get; }

        /// <summary>
        ///     Get the total length of constant elements (not data fields)
        /// </summary>
        int NonDataLength { get; }

        /// <summary>
        ///     Get whether the message contains a length field.
        /// </summary>
        bool IncludesLengthElement { get; }

        /// <summary>
        ///     If there's a length field, how much do we have to read to know it?
        /// </summary>
        int LengthElementOffsetInclusive { get; }

        /// <summary>
        ///     Index of the VariableData field, if any; otherwise -1
        /// </summary>
        int GeneralDataIndex { get; }
    }

    /// <summary>
    ///     Define shape of a protocol message at the data level.
    /// </summary>
    /// <typeparam name="TCrcEngine"><see cref="ICrcEngine"/>: A </typeparam>
    public class MessageTemplate<TCrcEngine> : IMessageTemplate
        where TCrcEngine : ICrcEngine
    {
        /// <summary>
        ///     Construct
        /// </summary>
        /// <param name="elements">Elements</param>
        /// <param name="crcSeed">CRC seed</param>
        public MessageTemplate(List<MessageTemplateElement> elements, ushort crcSeed)
        {
            Elements = elements;
            CrcEngineIn = (TCrcEngine)Activator.CreateInstance(typeof(TCrcEngine));
            CrcEngineOut = (TCrcEngine)Activator.CreateInstance(typeof(TCrcEngine));
            CrcSeed = crcSeed;

            IncludesLengthElement = false;
            LengthElementOffsetInclusive = 0;
            GeneralDataIndex = -1;

            NonDataLength = elements
                .Where(
                    e => e.ElementType != MessageTemplateElementType.VariableData &&
                         e.ElementType != MessageTemplateElementType.FixedData &&
                         e.ElementType != MessageTemplateElementType.ConstantDataLengthMask &&
                         e.ElementType != MessageTemplateElementType.ConstantMask)
                .Sum(e => e.Length);

            var haveCrc = false;
            for (var index = 0; index < elements.Count; index++)
            {
                var element = elements[index];
                switch (element.ElementType)
                {
                    case MessageTemplateElementType.FullLength:
                    case MessageTemplateElementType.DataLength:
                    case MessageTemplateElementType.LengthPlusDataLength:
                    case MessageTemplateElementType.ConstantDataLengthMask:
                    case MessageTemplateElementType.ConstantMask:
                        if (IncludesLengthElement)
                        {
                            throw new ArgumentOutOfRangeException(nameof(IncludesLengthElement));
                        }
                        IncludesLengthElement = true;
                        LengthElementOffsetInclusive = NonDataLength + element.Length;
                        break;
                    case MessageTemplateElementType.Crc:
                        if (haveCrc)
                        {
                            throw new ArgumentOutOfRangeException(nameof(haveCrc));
                        }
                        haveCrc = true;
                        break;
                    case MessageTemplateElementType.VariableData:
                        if (GeneralDataIndex >= 0)
                        {
                            throw new ArgumentOutOfRangeException(nameof(GeneralDataIndex));
                        }
                        GeneralDataIndex = index;
                        element.Length = -1;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <inheritdoc />
        public List<MessageTemplateElement> Elements { get; }

        /// <inheritdoc />
        public ICrcEngine CrcEngineIn { get; }

        /// <inheritdoc />
        public ICrcEngine CrcEngineOut { get; }

        /// <inheritdoc />
        public ushort CrcSeed { get; }

        /// <inheritdoc />
        public int NonDataLength { get; }

        /// <inheritdoc />
        public bool IncludesLengthElement { get; }

        /// <inheritdoc />
        public int LengthElementOffsetInclusive { get; }

        /// <inheritdoc />
        public int GeneralDataIndex { get; }
    }

    /// <summary>
    ///     The allowed message element types
    /// </summary>
    public enum MessageTemplateElementType
    {
        /// <summary>
        ///     This will be a constant section
        /// </summary>
        Constant,

        /// <summary>
        ///     This holds fixed-length portion of data bytes
        /// </summary>
        FixedData,

        /// <summary>
        ///     This holds non-command portion of data bytes
        /// </summary>
        VariableData,

        /// <summary>
        ///     Length of full message.
        /// </summary>
        FullLength,

        /// <summary>
        ///     Length of just data portion(s) of message.
        /// </summary>
        DataLength,

        /// <summary>
        ///     Length of the length field plus the data portion(s) of message.
        /// </summary>
        LengthPlusDataLength,

        /// <summary>
        ///     Message data that contains a constant mask plus length
        /// </summary>
        ConstantDataLengthMask,

        /// <summary>
        ///     Message data that contains a constant mask without length
        /// </summary>
        ConstantMask,

        /// <summary>
        ///     This holds CRC
        /// </summary>
        Crc
    }

    /// <summary>
    ///     Define a single element in a template
    /// </summary>
    public struct MessageTemplateElement
    {
        /// <summary>
        ///     Which type of element
        /// </summary>
        public MessageTemplateElementType ElementType;

        /// <summary>
        ///     Length of element in bytes
        /// </summary>
        public int Length;

        /// <summary>
        ///     Actual content
        /// </summary>
        public byte[] Value;

        /// <summary>
        ///     Whether to reverse byte order for endian-ness.
        /// </summary>
        public bool BigEndian;

        /// <summary>
        ///     Whether to include this element in the CRC calculation
        /// </summary>
        public bool IncludedInCrc;
    }

    /// <summary>
    ///     Use this CRC engine when none is needed.
    /// </summary>
    public class NullCrcEngine : ICrcEngine
    {
        /// <inheritdoc />
        public ushort Crc => 0;

        /// <inheritdoc />
        public void Hash(byte[] bytes, uint start, uint count)
        {
            // no op
        }

        /// <inheritdoc />
        public void Initialize(ushort seed)
        {
            // no op
        }
    }
}
