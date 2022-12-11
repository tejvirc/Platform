namespace Aristocrat.Monaco.Hardware.Contracts.Dfu
{
    using System;
    using System.Globalization;
    using Kernel;
    using ProtoBuf;

    /// <summary>Valid DFU error event Id enumerations.</summary>
    public enum DfuErrorEventId
    {
        /// <summary>Indicates no error.</summary>
        None = 0,

        /// <summary>Indicates file is not targeted for use by this device.</summary>
        DfuErrorTarget,

        /// <summary>Indicates file is for this device but fails some vendor-specific verification test.</summary>
        DfuErrorFile,

        /// <summary>Indicates device is unable to write memory.</summary>
        DfuErrorWrite,

        /// <summary>Indicates memory erase function failed.</summary>
        DfuErrorErase,

        /// <summary>Indicates memory erase check failed.</summary>
        DfuErrorCheckErased,

        /// <summary>Indicates program memory function failed.</summary>
        DfuErrorProgramMemory,

        /// <summary>Indicates programmed memory failed verification.</summary>
        DfuErrorVerify,

        /// <summary>Indicates cannot program memory due to received address that is out of range.</summary>
        DfuErrorAddress,

        /// <summary>
        ///     Indicates received DFU_DNLOAD with wLength = 0 (download complete), but device does not think it has all of
        ///     the data yet.
        /// </summary>
        DfuErrorNotDone,

        /// <summary>Indicates device's firmware is corrupt.  It cannot return to runtime (non-DFU) operation.</summary>
        DfuErrorFirmware,

        /// <summary>Indicates a vendor-specific error.</summary>
        DfuErrorVendor,

        /// <summary>Indicates device detected unexpected USB reset signaling.</summary>
        DfuErrorUsbReset,

        /// <summary>Indicates device detected unexpected power on reset.</summary>
        DfuErrorPower,

        /// <summary>Indicates something went wrong, but the the device does not know what it was.</summary>
        DfuErrorUnknown,

        /// <summary>Indicates device staled an unexpected request.</summary>
        DfuErrorStalled,

        /// <summary>Indicates claim interface error.</summary>
        ClaimInterface,

        /// <summary>Indicates release interface error.</summary>
        ReleaseInterface,

        /// <summary>Indicates control transfer error.</summary>
        ControlTransfer,

        /// <summary>Indicates reset device error.</summary>
        ResetDevice,

        /// <summary>Indicates an error loading the file to download.</summary>
        LoadFile,

        /// <summary>Indicates an error with the size of the file to download.</summary>
        FileSize
    }

    /// <summary>Definition of the DfuErrorEvent class.</summary>
    /// <remarks>Posted when a communication error occurs with the DFU.</remarks>
    [ProtoContract]
    public class DfuErrorEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DfuErrorEvent" /> class.
        /// </summary>
        public DfuErrorEvent()
            :this(DfuErrorEventId.None, 0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DfuErrorEvent" /> class.
        /// </summary>
        /// <param name="id">ID of the error event.</param>
        /// <param name="vendorSpecificErrorIndex">A vendor specific error index when a DFU vendor error is reported.</param>
        public DfuErrorEvent(DfuErrorEventId id, int vendorSpecificErrorIndex)
        {
            Id = id;
            VendorSpecificErrorIndex = vendorSpecificErrorIndex;
        }

        /// <summary>Gets the ID of the error event.</summary>
        [ProtoMember(1)]
        public DfuErrorEventId Id { get; }

        /// <summary>Gets the vendor specific error index of the error event.</summary>
        [ProtoMember(2)]
        public int VendorSpecificErrorIndex { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [Id={1}, VendorSpecificErrorIndex={2}]",
                GetType().Name,
                Id,
                VendorSpecificErrorIndex);
        }
    }
}