namespace Aristocrat.Monaco.Asp.Client.Comms
{
    using Utilities;

    /// <summary>
    ///     Transport message type: this allows the TP to handle each message type individually.
    ///     Currently defined message types are as follows
    ///     - Query		    0x30	(from MCI)
    ///     - Response		0x31	(from EGM)
    ///     - Event			0x32	(from EGM)
    ///     - Control		0x33	(from MCI)
    ///     - Control_Ack 	0x34	(from EGM)
    /// </summary>
    public enum TransportMessageType
    {
        Query = 0x30, //	(from MCI)
        Control = 0x33, //	(from MCI)
        Response = 0x31, //	(from EGM)
        Event = 0x32, //	(from EGM)
        ControlAck = 0x34 //	(from EGM)	
    }

    /// <summary>
    ///     TP Control data:
    ///     Command(1 byte)
    ///     0x01	May resume sending events
    ///     0x02	Stop sending any more events
    ///     Control_Ack data
    ///     Command_ack(1 byte)
    ///     0x00    Received invalid Command
    ///     0x01	Received Resume
    ///     0x02	Received Stop
    /// </summary>
    public enum ControlData
    {
        ResumeSendingEvents = 0x01,
        StopSendingEvents = 0x02,
        ReceivedInvalidCommandAckData = 0x00,
        ReceivedResumeAckData = 0x01,
        ReceivedStopAckData = 0x02
    }

    /// <summary>
    ///     -----------------------------------------------------------------------------------------------------------------
    ///     | 1 byte | 1 byte | 1 ... 240 bytes of | . . . . . | 1 byte | 1 byte | 1 ...240 bytes of | 1 byte pad if needed to|
    ///     | Length | Type   | single AP msg      | ...       | Length | Type   | single AP msg     | maek even length       |
    ///     -----------------------------------------------------------------------------------------------------------------
    /// </summary>
    public sealed class TransportPacket
    {
        private readonly TransportMessage[] _messages = new TransportMessage[64];
        private int _messageCount;

        public TransportPacket()
        {
        }

        public TransportPacket(DataLinkPacket packet)
        {
            Reset(packet);
        }

        public DataLinkPacket Packet { get; private set; }
        public ByteBuffer Buffer => Packet.Payload;
        private int Size => GetSize();
        public int NumberOfBytesCanPack => Buffer.Data.Length - Buffer.Offset - Size;

        public TransportMessage this[int messageIndex] => messageIndex < _messageCount ? _messages[messageIndex] : null;

        private int GetSize()
        {
            var size = 0;
            for (var i = 0; i < _messageCount; i++)
            {
                size += _messages[i].Length + 1;
            }

            return size;
        }

        public TransportMessage AddMessage(int length, TransportMessageType type)
        {
            var message =
                _messages[_messageCount] == null
                    ? _messages[_messageCount] = new TransportMessage(new ByteBuffer(Buffer, Size))
                    : _messages[_messageCount];

            message.Buffer.Reset(Buffer, Size);
            message.Length = (byte)length;
            message.Type = type;
            _messageCount++;
            Packet.DataLength = Size;
            return message;
        }

        public void Reset(DataLinkPacket packet)
        {
            Packet = packet;
            while (Buffer.Length - GetSize() > 2)
            {
                var message =
                    _messages[_messageCount] == null
                        ? _messages[_messageCount] = new TransportMessage(new ByteBuffer(Buffer, GetSize()))
                        : _messages[_messageCount];
                message.Buffer.Reset(Buffer, GetSize());
                _messageCount++;
            }
        }

        public void Clear()
        {
            _messageCount = 0;
            Buffer.Clear();
            Packet.DataLength = 0;
        }

        public TransportMessage AddMessage(AppResponse response)
        {
            var msg = AddMessage(
                response.Size + 1,
                response.Command == AppResponseTypes.EventReport
                    ? TransportMessageType.Event
                    : TransportMessageType.Response);

            for (var i = 0; i < response.Size; i++)
            {
                msg.Buffer[i + 2] = response.Buffer[i];
            }

            return msg;
        }
    }

    /// <summary>
    ///     Encapsulates single AP message.
    /// </summary>
    public class TransportMessage
    {
        private readonly ByteBuffer _payload;

        public TransportMessage(ByteBuffer byteBuffer)
        {
            Buffer = byteBuffer;
            _payload = new ByteBuffer(Buffer, 2);
        }

        public int Length
        {
            get => Buffer[0];
            set => Buffer[0] = (byte)value;
        }

        public TransportMessageType Type
        {
            get => (TransportMessageType)Buffer[1];
            set => Buffer[1] = (byte)value;
        }

        public ControlData ControlData
        {
            get => (ControlData)Buffer[2];
            set => Buffer[2] = (byte)value;
        }

        public ByteBuffer Buffer { get; set; }

        public ByteBuffer Payload
        {
            get
            {
                _payload.Reset(Buffer, 2, Length);
                return _payload;
            }
        }
    }
}