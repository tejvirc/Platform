namespace Aristocrat.Monaco.Asp.Client.Comms
{
    using Utilities;

    /// <summary>
    ///     In ASP5000 all commands and responses are in the form of packets. An odd number of bytes is allowed in a packet at
    ///     the application level.
    ///     The general formats of messages from the MCI are as follows:
    ///     Command
    ///     = Set_Parameter (0x01)	Class	Type	Parameter Number	Data
    ///     1 byte	        1 byte	1 byte	1 byte	            n bytes
    ///     Command
    ///     = Get_Parameter (0x02)	Class	Type	Parameter Number
    ///     1 byte	        1 byte	1 byte	1 byte
    ///     Command
    ///     = Set_Event (0x04)	Class	Type	Parameter Number
    ///     1 byte	    1 byte	1 byte	1 byte
    ///     Command
    ///     = Clr_Event (0x05)	Class	Type	Parameter Number
    ///     1 byte	        1 byte	1 byte	1 byte
    /// </summary>
    public class ApplicationMessage
    {
        public TransportMessage Packet { get; private set; }
        public ByteBuffer Buffer => Packet.Payload;

        public AppCommandTypes Command
        {
            get => (AppCommandTypes)Buffer[0];
            set => Buffer[0] = (byte)value;
        }

        public byte Class
        {
            get => Buffer[1];
            set => Buffer[1] = value;
        }

        public byte Type
        {
            get => Buffer[2];
            set => Buffer[2] = value;
        }

        public byte Param
        {
            get => Buffer[3];
            set => Buffer[3] = value;
        }

        public ByteBuffer Payload { get; private set; }
        public ByteArrayReader Reader { get; private set; }

        public void Reset(TransportMessage packet)
        {
            Packet = packet;
            Payload = new ByteBuffer(packet.Payload, 4);
            Reader = new ByteArrayReader(Payload.Data, Payload.Offset, Payload.Offset + Packet.Length);
        }
    }

    /// <summary>
    ///     The general formats of messages from the EGM are as follows:
    ///     Command
    ///     = Set_Parameter_Ack (0x81)	Class	Type	Parameter Number	Response Status
    ///     1 byte	    1 byte	1 byte	1 byte	            1 byte
    ///     Command
    ///     = Get_Parameter_Ack (0x82)	Class	Type	Parameter Number	Response Status	Data
    ///     1 byte	1 byte	1 byte	1 byte	1 byte	            n bytes
    ///     Command
    ///     = Set_Event_Ack (0x84)	Class	Type	Parameter Number    Response Status
    ///     1 byte	    1 byte	1 byte	1 byte	            1 byte
    ///     Command
    ///     = Clr_Event_Ack (0x85)	Class	Type	Parameter Number	Response Status
    ///     1 byte	    1 byte	1 byte	1 byte	            1 byte
    ///     The general format of  Events
    ///     Command
    ///     = Event_Report (0x80)	Class	Type	Parameter Number	Time Stamp 	    Data
    ///     1 byte	    1 byte	1 byte	1 byte	            4 bytes	        n bytes
    ///     where:
    ///     Command ( 1 byte )  	=  Command
    ///     Class ( 1 byte ) 	=  Device Class
    ///     Type ( 1 byte )	= Type of that device Class
    ///     Parameter Number ( 1 byte )	=  Parameter of the device
    ///     Data ( n bytes )	=  Data associated with the Parameter
    ///     Time Stamp (4 bytes)	=  In seconds from January 1st 1990
    ///     Response Status (1 byte )	=  Error status of response
    ///     0x00	Valid response
    ///     0x01	Bad or unsupported Command
    ///     0x02	Bad or unsupported Class
    ///     0x03	Bad or unsupported Type
    ///     0x04	Bad or unsupported Parameter
    ///     0x05	Invalid Parameter data
    /// </summary>
    public abstract class AppResponseBase
    {
        public byte[] Buffer { get; } = new byte[250];

        public AppResponseTypes Command
        {
            get => (AppResponseTypes)Buffer[0];
            set => Buffer[0] = (byte)value;
        }

        public byte Class
        {
            get => Buffer[1];
            set => Buffer[1] = value;
        }

        public byte Type
        {
            get => Buffer[2];
            set => Buffer[2] = value;
        }

        public byte Param
        {
            get => Buffer[3];
            set => Buffer[3] = value;
        }

        public int Size => GetSize();
        protected abstract int GetSize();
    }

    public class AppResponse : AppResponseBase
    {
        public AppResponseStatus ResponseStatus
        {
            get => (AppResponseStatus)Buffer[4];
            set => Buffer[4] = (byte)value;
        }

        protected override int GetSize()
        {
            return 5;
        }
    }

    public class AppDataResponse : AppResponse
    {
        private int _dataSize;

        public AppDataResponse()
        {
            DataWriter = new ByteArrayWriter(Buffer, base.GetSize());
        }

        public ByteArrayWriter DataWriter { get; }

        public int DataSize
        {
            get => _dataSize;
            set
            {
                _dataSize = value;
                DataWriter.Reset(Buffer, DataOffset);
            }
        }

        public virtual int DataOffset => base.GetSize();

        protected override int GetSize()
        {
            return DataOffset + DataSize;
        }
    }

    public class AppEventResponse : AppDataResponse
    {
        public uint TimeStamp
        {
            get => (uint)ByteArrayReader.ReadNumeric(Buffer, 4, 4);
            set => ByteArrayWriter.WriteNumeric(Buffer, 4, 4, value);
        }

        public override int DataOffset => base.DataOffset + 3;
    }

    public enum AppResponseStatus
    {
        ValidResponse = 0x00, // Valid response
        UnsupportedCommand = 0x01, //	Bad or unsupported Command
        UnsupportedClass = 0x02, //	Bad or unsupported Class
        UnsupportedType = 0x03, //	Bad or unsupported Type
        UnsupportedParameter = 0x04, //	Bad or unsupported Parameter
        InvalidParameterData = 0x05 //	Invalid Parameter data
    }

    public enum AppCommandTypes
    {
        SetParameter = 0x1,
        GetParameter = 0x2,
        SetEvent = 0x4,
        ClearEvent = 0x5
    }

    public enum AppResponseTypes
    {
        EventReport = 0x80,
        SetParameterAck = 0x81,
        GetParameterAck = 0x82,
        SetEventAck = 0x84,
        ClearEventAck = 0x85
    }
}