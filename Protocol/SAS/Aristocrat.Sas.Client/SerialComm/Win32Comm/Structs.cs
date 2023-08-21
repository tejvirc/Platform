namespace Aristocrat.Sas.Client.SerialComm.Win32Comm
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///     All enums used to setup the port
    /// </summary>

    public enum AsciiCode
    {
        Null = 0x00,
        DC1 = 0x11,
        DC2 = 0x12,
        DC3 = 0x13,
        DC4 = 0x14,
    }

    public enum HsOutput
    {
        Handshake = 2,
        Gate = 3,
        Online = 1,
        None = 0
    };

    public enum Parity
    {
        None = 0,
        Odd = 1,
        Even = 2,
        Mark = 3,
        Space = 4
    };

    public enum StopBit
    {
        One = 0,
        OnePointFive = 1,
        Two = 2
    };

    /// <summary>
    ///     Structs to set and get the timeouts and COM states.
    /// </summary>
    
    [StructLayout(LayoutKind.Sequential)]
    internal struct CommTimeouts
    {
        internal int ReadIntervalTimeout;
        internal int ReadTotalTimeoutMultiplier;
        internal int ReadTotalTimeoutConstant;
        internal int WriteTotalTimeoutMultiplier;
        internal int WriteTotalTimeoutConstant;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ComStat
    {
        internal const uint FCtsHold = 0x1;
        internal const uint FDsrHold = 0x2;
        internal const uint FRlsdHold = 0x4;
        internal const uint FXoffHold = 0x8;
        internal const uint FXoffSent = 0x10;
        internal const uint FEof = 0x20;
        internal const uint FTxim = 0x40;
        internal uint Flags;
        internal uint cbInQue;
        internal uint cbOutQue;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Dcb
    {
        internal int DCBlength;
        internal int BaudRate;
        internal int PackedValues;
        internal short wReserved;
        internal short XonLim;
        internal short XoffLim;
        internal byte ByteSize;
        internal byte Parity;
        internal byte StopBits;
        internal byte XonChar;
        internal byte XoffChar;
        internal byte ErrorChar;
        internal byte EofChar;
        internal byte EvtChar;
        internal short wReserved1;

        internal void Init(bool parity)
        {
            DCBlength = 28;
            PackedValues = 0x0001;
            if (parity) PackedValues |= 0x0002;
        }
    }
}
