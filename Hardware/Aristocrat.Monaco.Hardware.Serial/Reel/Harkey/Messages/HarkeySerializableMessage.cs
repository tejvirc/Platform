namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using BinarySerialization;
    using log4net;
    using Quartz.Util;

    [Serializable]
    public class HarkeySerializableMessage
    {
        private const int EmptyData = 0;
        private const int FakeSequenceId = 0;
        private const int ProtocolIndex = 0;
        private const int SequenceIdIndex = 1;
        private const int CommandIdIndex = 2;
        private const byte MinimumCommandLength = 0x02;
        private static readonly BinarySerializer Serializer = new BinarySerializer();

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static ConcurrentDictionary<(HarkeyCommandId commandId, MessageMaskType messageType), Type> _noProtocolMessages
            = new ConcurrentDictionary<(HarkeyCommandId, MessageMaskType), Type>();
        private static ConcurrentDictionary<(HarkeyCommandId commandId, MessageMaskType messageType), Type> _noSequenceIdMessages
            = new ConcurrentDictionary<(HarkeyCommandId, MessageMaskType), Type>();
        private static ConcurrentDictionary<(HarkeyCommandId commandId, MessageMaskType messageType), Type> _standardMessages
            = new ConcurrentDictionary<(HarkeyCommandId, MessageMaskType), Type>();

        private static ConcurrentDictionary<Type, (bool isProtocolMessage, bool isSequencedMessage, byte messageLength)> _messageTypeDetails
            = new ConcurrentDictionary<Type, (bool, bool, byte)>();

        protected HarkeySerializableMessage()
            : this(0, 0, MinimumCommandLength, false)
        {
        }

        protected HarkeySerializableMessage(
            MessageMaskType messageType,
            HarkeyCommandId commandId,
            byte maxLength,
            bool useCommandInsteadOfProtocol)
        {
            Protocol = useCommandInsteadOfProtocol
                ? (byte)commandId
                : (byte)(messageType + maxLength);

            CommandId = commandId;
            MessageType = messageType;
            MaxLength = maxLength;
            UseCommandInsteadOfProtocol = useCommandInsteadOfProtocol;
        }

        [FieldOrder(ProtocolIndex)]
        [FieldBitLength(8)]
        public byte Protocol { get; set; }

        [FieldOrder(SequenceIdIndex)]
        [FieldBitLength(8)]
        public byte SequenceId { get; set; }

        [FieldOrder(CommandIdIndex)]
        [FieldBitLength(8)]
        public HarkeyCommandId CommandId { get; set; }

        [Ignore]
        public MessageMaskType MessageType { get; set; }

        [Ignore]
        public byte MaxLength { get; set; }

        [Ignore]
        public bool UseCommandInsteadOfProtocol { get; set; }

        public static HarkeySerializableMessage Deserialize(byte[] message)
        {
            if (message.Length < MinimumCommandLength)
            {
               Logger.Warn($"Harkey message length {message.Length} < minimum {MinimumCommandLength}");
               return null;
            }

            var messageType = GetMessageType(message);
            if (messageType == null)
            {
                return null;
            }

            var (isProtocolMessage, isSequencedMessage, messageLength) = _messageTypeDetails.TryGetAndReturn(messageType);
            if (!isProtocolMessage || !isSequencedMessage || message.Length < messageLength)
            {
                var messageList = message.ToList();

                // Add a fake protocol and sequence id if it is not in the data
                if (!isProtocolMessage)
                {
                    messageList.Insert(ProtocolIndex, message[ProtocolIndex]);
                }

                if (!isSequencedMessage)
                {
                    messageList.Insert(SequenceIdIndex, FakeSequenceId);
                }

                // Make sure the data matches the expected length of the type to deserialize
                while (messageList.Count < messageLength)
                {
                    messageList.Add(EmptyData);
                }

                message = messageList.ToArray();
            }

            var obj = Serializer.Deserialize(message, messageType);
            return obj as HarkeySerializableMessage;
        }

        public static void Initialize()
        {
            if (_noSequenceIdMessages.Any() || _noProtocolMessages.Any() || _standardMessages.Any())
            {
                return;
            }

            // create one of each message type, temporarily
            var messageSet = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.BaseType == typeof(HarkeySerializableMessage) && !t.IsAbstract).ToList()
                .Select(t => t.GetConstructor(new Type[] { })?.Invoke(new object[] { }));

            foreach (var message in messageSet)
            {
                if (!(message is HarkeySerializableMessage harkeyMessage))
                {
                    continue;
                }

                var commandKey = (harkeyMessage.CommandId, harkeyMessage.MessageType);

                if (harkeyMessage.Protocol == (byte)harkeyMessage.CommandId)
                {
                    _noProtocolMessages.TryAdd(commandKey, harkeyMessage.GetType());
                }
                else if (!(harkeyMessage is ISequencedCommand))
                {
                    _noSequenceIdMessages.TryAdd(commandKey, harkeyMessage.GetType());
                }
                else
                {
                    _standardMessages.TryAdd(commandKey, harkeyMessage.GetType());
                }

                _messageTypeDetails.TryAdd(
                    harkeyMessage.GetType(),
                    (harkeyMessage.Protocol != (byte)harkeyMessage.CommandId, harkeyMessage is ISequencedCommand, harkeyMessage.MaxLength));
            }
        }

        public byte[] Serialize()
        {
            List<byte> messageBytes;
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, this);
                messageBytes = new List<byte>(stream.ToArray());
            }

            switch (MessageType)
            {
                case MessageMaskType.Command:
                    if (UseCommandInsteadOfProtocol)
                    {
                        messageBytes.RemoveAt(CommandIdIndex);
                    }

                    if (!(this is ISequencedCommand))
                    {
                        messageBytes.RemoveAt(SequenceIdIndex);
                    }

                    break;
                case MessageMaskType.CommandResponse:
                    if (!(this is ISequencedCommand))
                    {
                        messageBytes.RemoveAt(SequenceIdIndex);
                    }

                    break;
            }

            return messageBytes.ToArray();
        }

        public override string ToString()
        {
            return $"{GetType()} [CommandId={CommandId}]";
        }

        private static Type GetMessageType(byte[] message)
        {
            const int noProtocolCommandIndex = 0;
            const int noSequenceIdCommandIndex = 1;
            const int standardCommandIndex = 2;
            var protocolByte = message[0];

            Type messageType = null;

            // Check for no protocol commands
            if (IsCommand(protocolByte) && _noProtocolMessages.ContainsKey(((HarkeyCommandId)message[noProtocolCommandIndex], MessageMaskType.Command)))
            {
                messageType = _noProtocolMessages.TryGetAndReturn(((HarkeyCommandId)message[noProtocolCommandIndex], MessageMaskType.Command));
                if (messageType == null)
                {
                    Logger.Warn($"Harkey command message type not found in protocol messages for protocol byte {protocolByte}");
                }
            }
            // Check for no sequence id commands
            else if (IsCommand(protocolByte) && _noSequenceIdMessages.ContainsKey(((HarkeyCommandId)message[noSequenceIdCommandIndex], MessageMaskType.Command)))
            {
                messageType = _noSequenceIdMessages.TryGetAndReturn(((HarkeyCommandId)message[noSequenceIdCommandIndex], MessageMaskType.Command));
                if (messageType == null)
                {
                    Logger.Warn($"Harkey command message type not found in sequence ID messages for protocol byte {protocolByte}");
                }
            }
            // Check for no protocol commands (soft reset, protocol, hardware, and unsolicited error)
            else if (_noProtocolMessages.ContainsKey(((HarkeyCommandId)message[noProtocolCommandIndex], (MessageMaskType)(protocolByte & 0xF0))))
            {
                var type = (MessageMaskType)(protocolByte & 0xF0);
                messageType = _noProtocolMessages.TryGetAndReturn(((HarkeyCommandId)message[0], type));
                if (messageType == null)
                {
                    Logger.Warn($"Harkey error response message type not found in protocol messages for protocol byte {protocolByte}");
                }
            }
            // Check for standard format commands
            else if (IsCommand(protocolByte) && message.Length >= standardCommandIndex + 1)
            {
                messageType = _standardMessages.TryGetAndReturn(((HarkeyCommandId)message[standardCommandIndex], MessageMaskType.Command));
                if (messageType == null)
                {
                    Logger.Warn($"Harkey command message type not found in standard messages for protocol byte {protocolByte}");
                }
            }
            // Check for no sequence id command responses
            else if (IsCommandResponse(protocolByte) && _noSequenceIdMessages.ContainsKey(((HarkeyCommandId)message[noSequenceIdCommandIndex], MessageMaskType.CommandResponse)))
            {
                messageType = _noSequenceIdMessages.TryGetAndReturn(((HarkeyCommandId)message[noSequenceIdCommandIndex], MessageMaskType.CommandResponse));
                if (messageType == null)
                {
                    Logger.Warn($"Harkey command response message type not found in sequence Id messages for protocol byte {protocolByte}");
                }
            }
            // Check for standard command responses
            else if (IsCommandResponse(protocolByte) && message.Length >= standardCommandIndex + 1)
            {
                messageType = _standardMessages.TryGetAndReturn(((HarkeyCommandId)message[standardCommandIndex], MessageMaskType.CommandResponse));
                if (messageType == null)
                {
                    Logger.Warn($"Harkey command response message type not found in standard messages for protocol byte {protocolByte}");
                }
            }
            else
            {
                Logger.Warn($"Harkey message type not supported for protocol byte {protocolByte}");
            }

            return messageType;
        }

        private static bool IsCommand(byte data)
        {
            return (data & (byte)MessageMaskType.Command) == (byte)MessageMaskType.Command;
        }

        private static bool IsCommandResponse(byte data)
        {
            return (data & (byte)MessageMaskType.CommandResponse) == (byte)MessageMaskType.CommandResponse;
        }

        private static bool IsHardwareErrorResponse(byte data)
        {
            return (data & (byte)MessageMaskType.HardwareErrorResponse) == (byte)MessageMaskType.HardwareErrorResponse;
        }

        private static bool IsProtocolErrorResponse(byte data)
        {
            return (data & (byte)MessageMaskType.ProtocolErrorResponse) == (byte)MessageMaskType.ProtocolErrorResponse;
        }

        private static bool IsUnsolicitedErrorResponse(byte data)
        {
            return (data & (byte)MessageMaskType.UnsolicitedErrorResponse) == (byte)MessageMaskType.UnsolicitedErrorResponse;
        }
    }
}