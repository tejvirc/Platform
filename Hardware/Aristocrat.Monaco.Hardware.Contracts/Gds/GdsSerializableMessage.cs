namespace Aristocrat.Monaco.Hardware.Contracts.Gds
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>
    ///     Defines the interface for data messages that are serializable into GDS messages
    /// </summary>
    [Serializable]
    public class GdsSerializableMessage
    {
        private static readonly BinarySerializer Serializer = new BinarySerializer();

        private static Dictionary<GdsConstants.ReportId, Type> _messageTypes = new Dictionary<GdsConstants.ReportId, Type>();

        /// <summary>
        ///     Parameter-less constructor.
        /// </summary>
        public GdsSerializableMessage() : this(0)
        {
        }

        /// <summary>
        ///     Constructor that sets the command report ID
        /// </summary>
        /// <param name="reportId">Report ID</param>
        public GdsSerializableMessage(GdsConstants.ReportId reportId) : this(reportId, int.MaxValue)
        {
        }

        /// <summary>
        ///     Constructor that sets the report ID and max packet size
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="maxPacketSize">Maximum packet size</param>
        protected GdsSerializableMessage(GdsConstants.ReportId reportId, int maxPacketSize)
        {
            ReportId = reportId;
            MaxPacketSize = maxPacketSize;
        }

        /// <summary>
        ///     Unique GDS report ID for this type of message
        /// </summary>
        [FieldOrder(0)]
        [FieldBitLength(8)]
        public GdsConstants.ReportId ReportId { get; set; }

        /// <summary>
        ///     Maximum packet size for large commands.
        /// </summary>
        [Ignore]
        public int MaxPacketSize { get; }

        /// <summary>
        ///     Serialize into stream.
        /// </summary>
        /// <param name="stream">Stream to serialize into.</param>
        public void Serialize(Stream stream)
        {
            Serializer.Serialize(stream, this);
        }

        /// <summary>
        ///     Initialize the static portion of the class
        /// </summary>
        /// <param name="isSpecialCaseUic">True for a UIC Card Reader, to deal
        /// with its non-compliant messaging.</param>
        public static void Initialize(bool isSpecialCaseUic)
        {
            if (_messageTypes.Count > 0)
            {
                return;
            }

            // create one of each message type, temporarily
            var messageSet = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.BaseType == typeof(GdsSerializableMessage)).ToList()
                .Select(t => t.GetConstructor(new Type[] { })?.Invoke(new object[] { }));

            foreach (var message in messageSet)
            {
                if (message is GdsSerializableMessage gdsMessage)
                {
                    // Special case: only load UicLightControl for UIC card reader,
                    // otherwise load normal LightControl.
                    if (gdsMessage.ReportId == GdsConstants.ReportId.CardReaderLightControl &&
                        (isSpecialCaseUic ^ (gdsMessage.GetType() == typeof(CardReader.LightControl))))
                    {
                        continue;
                    }

                    lock(_messageTypes)
                    {
                        _messageTypes[gdsMessage.ReportId] = gdsMessage.GetType();
                    }
                }
            }
        }

        /// <summary>
        ///     Deserialize from byte stream.
        /// </summary>
        /// <param name="bytes">Input bytes</param>
        /// <returns>Deserialized message</returns>
        public static GdsSerializableMessage Deserialize(byte[] bytes)
        {
            var type = GetMessageType((GdsConstants.ReportId)bytes[0]);
            var obj = Serializer.Deserialize(bytes, type);

            return obj as GdsSerializableMessage;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"[ReportId={ReportId}]");
        }

        private static Type GetMessageType(GdsConstants.ReportId reportId)
        {
            if (_messageTypes.ContainsKey(reportId))
            {
                return _messageTypes[reportId];
            }

            return typeof(GdsSerializableMessage);
        }
    }
}
