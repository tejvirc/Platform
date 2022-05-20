namespace Aristocrat.Monaco.Application.SerialGat
{
    using System;
    using System.Runtime.InteropServices;
    using Hardware.Contracts.SerialPorts;

    /// <summary>
    /// Types of commands from GAT Master to EGM.
    /// </summary>
    public enum GatQuery
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Unknown = 0x00,

        /// <summary>
        /// Status Query
        /// </summary>
        Status = 0x01,

        /// <summary>
        /// Last Authentication Status Query
        /// </summary>
        LastAuthenticationStatus = 0x02,

        /// <summary>
        /// Last Authentication Results Query
        /// </summary>
        LastAuthenticationResults = 0x03,

        /// <summary>
        /// Initiate Authentication Calculation Query
        /// </summary>
        InitiateAuthenticationCalculation = 0x04,

        /// <summary>
        /// Maximum valid value
        /// </summary>
        Max = InitiateAuthenticationCalculation
    }

    /// <summary>
    /// Types of responses to GAT Master from EGM.
    /// </summary>
    public enum GatResponse
    {
        /// <summary>
        /// Status Response
        /// </summary>
        Status = 0x81,

        /// <summary>
        /// Last Authentication Status Response
        /// </summary>
        LastAuthenticationStatus = 0x82,

        /// <summary>
        /// Last Authentication Results Response
        /// </summary>
        LastAuthenticationResults = 0x83,

        /// <summary>
        /// Initiate Authentication Calculation Response
        /// </summary>
        InitiateAuthenticationCalculation = 0x84,
    }

    /// <summary>
    ///     This status gets reported to master
    /// </summary>
    [Flags]
    public enum GatStatus : byte
    {
        /// <summary>
        /// Idle
        /// </summary>
        Idle = 0b0100,

        /// <summary>
        /// Calculation requested but not yet started
        /// </summary>
        CalcRequested = 0b0001,

        /// <summary>
        /// Calculation in progress
        /// </summary>
        Calculating = 0b1001,

        /// <summary>
        /// Calculation has finished
        /// </summary>
        CalcDone = 0b0110,

        /// <summary>
        /// Calculation had error (no info available)
        /// </summary>
        CalcError = 0b1100,

        /// <summary>
        /// Last results are available, generally coincident with CalcDone
        /// </summary>
        LastResultsAvailable = 0b0110,
    }

    /// <summary>
    /// Types of data format supported
    /// </summary>
    [Flags]
    public enum GatDataFormat : byte
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Plain text (ASCII)
        /// </summary>
        PlainText = 0x01,

        /// <summary>
        /// XML (either UTF-8 or UTF-16 encoded)
        /// </summary>
        Xml = 0x02,
    }

    /// <summary>
    /// Values for Result status byte
    /// </summary>
    [Flags]
    public enum GatResultStatus : byte
    {
        /// <summary>
        /// None
        /// </summary>
        Default = 0,

        /// <summary>
        /// Indicates error in Result Query
        /// </summary>
        Error = 1,

        /// <summary>
        /// Indicates final data frame of a Result Response
        /// </summary>
        LastFrame = 2,
    }

    /// <summary>
    /// Values for Authentication status byte
    /// </summary>
    [Flags]
    public enum GatAuthenticationStatus : byte
    {
        /// <summary>
        /// Default / unset
        /// </summary>
        Default = 0,

        /// <summary>
        /// Acknowledge the command
        /// </summary>
        Acknowledge = 1,

        /// <summary>
        /// Calculation has started
        /// </summary>
        CalcStarted = 2,

        /// <summary>
        /// Requested level isn't allowed
        /// </summary>
        LevelError = 4
    }

    /// <summary>
    /// Possible versions of GAT that we support (packed BCD format)
    /// </summary>
    public enum GatVersion : short
    {
        /// <summary>
        /// Legacy declares itself v2.00
        /// </summary>
        Version20 = 0x0002,

        /// <summary>
        /// Standard version 3.50
        /// </summary>
        Version35 = 0x5003
    }

    /// <summary>
    ///     Status Response data layout
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StatusResponse
    {
        /// <summary>
        /// Protocol version (packed BCD) (e.g. 0x0350 for version 3.50)
        /// </summary>
        public GatVersion Version;

        /// <summary>
        /// Current status of engine
        /// </summary>
        public GatStatus Status;

        /// <summary>
        /// Supported data formats
        /// </summary>
        public GatDataFormat Format;
    }

    /// <summary>
    ///     Last Authentication Status Response data layout
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LastAuthenticationStatusResponse
    {
        /// <summary>
        /// What level of authentication was run
        /// </summary>
        public byte AuthenticationLevel;

        /// <summary>
        /// How many seconds since authentication ran.
        /// </summary>
        [Endian(Endianness.BigEndian)]
        public int SecondsSinceLastCalc;
    }

    /// <summary>
    ///     Last Authentication Result Query data layout
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LastAuthenticationResultQuery
    {
        /// <summary>
        /// Desired data format
        /// </summary>
        public GatDataFormat Format;

        /// <summary>
        /// Desired frame number of authentication result
        /// </summary>
        [Endian(Endianness.BigEndian)]
        public short FrameNumber;
    }

    /// <summary>
    ///     Last Authentication Result Response data layout
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LastAuthenticationResultResponseHeader
    {
        /// <summary>
        /// Status
        /// </summary>
        public GatResultStatus Status;

        /// <summary>
        /// Frame number of authentication result
        /// </summary>
        [Endian(Endianness.BigEndian)]
        public short FrameNumber;
    }

    /// <summary>
    ///     Initiate Authentication Result Query data layout
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InitiateAuthenticationCalculationQueryHeader
    {
        /// <summary>
        /// Authentication level
        /// </summary>
        public byte AuthenticationLevel;
    }

    /// <summary>
    ///     Initiate Authentication Calculation Response data layout
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InitiateAuthenticationCalculationResponse
    {
        /// <summary>
        /// Status
        /// </summary>
        public GatAuthenticationStatus Status;
    }

    /// <summary>
    ///     This class provides helper functions for Serial GAT protocol.
    /// </summary>
    public class SerialGatUtils
    {

        /// <summary>
        /// Marshal simple packed struct, plus optionally some variable bytes, into byte[].
        /// Look for "endian" attributes to reverse integers between MSB and LSB as defined.
        /// </summary>
        /// <typeparam name="T">Struct type</typeparam>
        /// <param name="str">Struct</param>
        /// <param name="count">How many bytes of variable data</param>
        /// <param name="variableData">Byte buffer from which variable data comes</param>
        /// <param name="start">Where to start in the variable data</param>
        /// <returns>bytes</returns>
        public static byte[] GetBytes<T>(T str, int count = 0, byte[] variableData = null, int start = 0)
        {
            var size = Marshal.SizeOf(str);
            var arr = new byte[size + count];
            var h = default(GCHandle);

            try
            {
                h = GCHandle.Alloc(arr, GCHandleType.Pinned);

                Marshal.StructureToPtr(str, h.AddrOfPinnedObject(), false);

                if (variableData != null)
                {
                    Buffer.BlockCopy(variableData, start, arr, size, count);
                }
            }
            finally
            {
                if (h.IsAllocated)
                {
                    h.Free();
                }
            }

            Endian.RespectEndianness(typeof(T), arr);

            return arr;
        }

        /// <summary>
        /// Marshal a byte array into a packed data structure.
        /// Look for "endian" attributes to reverse integers between MSB and LSB as defined.
        /// </summary>
        /// <typeparam name="T">Struct type</typeparam>
        /// <param name="arr">Input byte array</param>
        /// <returns>Set of the struct and any extra bytes in their own array (payload)</returns>
        public static (T messageStruct, byte[] buffer) FromBytes<T>(byte[] arr) where T : struct
        {
            var extraBytes = Array.Empty<byte>();
            T str;
            var h = default(GCHandle);

            try
            {
                Endian.RespectEndianness(typeof(T), arr);

                h = GCHandle.Alloc(arr, GCHandleType.Pinned);

                str = Marshal.PtrToStructure<T>(h.AddrOfPinnedObject());

                if (arr.Length > Marshal.SizeOf(str))
                {
                    extraBytes = new byte[arr.Length - Marshal.SizeOf(str)];
                    Buffer.BlockCopy(arr, Marshal.SizeOf(str), extraBytes, 0, extraBytes.Length);
                }
            }
            finally
            {
                if (h.IsAllocated)
                {
                    h.Free();
                }
            }

            return (messageStruct: str, buffer: extraBytes);
        }
    }
}
