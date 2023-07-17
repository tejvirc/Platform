namespace Aristocrat.Monaco.Hardware.Contracts.PWM
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    /// <summary>Direction of diverter.</summary>
    public enum DivertorState
    {
        /// <summary>Direction of diverter to none..</summary>
        [Description("None")]
        None,
        /// <summary>Direction of diverter to hopper.</summary>
        [Description("Hopper")]
        DivertToHopper,
        /// <summary>Direction of diverter to cashbox.</summary>
        [Description("Cashbox")]
        DivertToCashbox
    }

    /// <summary>States of coin acceptor while coin in.</summary>
    public enum AcceptorState
    {
        /// <summary>coin acceptor accepts coin.</summary>
        Accept,
        /// <summary>coin acceptor rejects coin.</summary>
        Reject
    }

    /// <summary>LARGE_INTEGER struct.</summary>
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    [CLSCompliant(false)]
    public struct LARGE_INTEGER
    {
        /// <summary>QuadPart of LARGE_INTEGER.</summary>
        [FieldOffset(0)] public Int64 QuadPart;
        /// <summary>LowPart of LARGE_INTEGER.</summary>
        [FieldOffset(0)] public UInt32 LowPart;
        /// <summary>HighPart of LARGE_INTEGER.</summary>
        [FieldOffset(4)] public Int32 HighPart;
    }

    /// <summary>ChangeRecord struct.</summary>
    [CLSCompliant(false)]
    public struct ChangeRecord
    {
        /// <summary>oldValue on device.</summary>
        public byte oldValue;
        /// <summary>newValue on device.</summary>
        public byte newValue;
        /// <summary>elapsedSinceLastChange for new value.</summary>
        public LARGE_INTEGER elapsedSinceLastChange;
        /// <summary>Transaction id for .</summary>
        public byte changeId;
    };
    /// <summary>Definition of the Pwm Device interface.</summary>
    [CLSCompliant(false)]
    public interface IPwmDevice
    {
        /// <summary>Provides the interface to connect with phyical device.</summary>
        /// <returns>Device interface.</returns>
        PwmDeviceConfig DeviceConfig { get; }

        /// <summary>We initialize the device here.</summary>
        void ConfigureDevice();

        /// <summary>We initialize the device here.</summary>
        void Initialize();

        /// <summary>We cleanup the device here.</summary>
        void Cleanup();
        /// <summary>Gets available inputs synchronously.</summary>
        /// <returns>ChangeRecord of inputs and read status.</returns>
        (bool, ChangeRecord) ReadSync();

        /// <summary>Gets available inputs asynchronously.</summary>
        /// <returns>ChangeRecord of inputs and read status.</returns>
        (bool, ChangeRecord) ReadAsync();

        /// <summary>Gets available inputs.</summary>
        /// <returns>ChangeRecord of inputs and read status.</returns>
        (bool, ChangeRecord) Read();

        /// <summary>Provides the device name.</summary>
        string Name { get; }
    }
}
