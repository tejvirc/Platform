namespace Aristocrat.Monaco.Hardware.Contracts.Communicator
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    ///     Enumeration for DFU state
    /// </summary>
    public enum DfuState
    {
        /// <summary>
        ///     Unknown state
        /// </summary>
        Unknown = -1,

        /// <summary>
        ///     appIDLE
        /// </summary>
        AppIdle = 0,

        /// <summary>
        ///     appDETACH
        /// </summary>
        AppDetach,

        /// <summary>
        ///     dfuIDLE
        /// </summary>
        DfuIdle,

        /// <summary>
        ///     dfuDNLOAD-SYNC
        /// </summary>
        DfuDownloadSync,

        /// <summary>
        ///     dfuDNBUSY
        /// </summary>
        DfuDownloadBusy,

        /// <summary>
        ///     dfuDNLOAD-IDLE
        /// </summary>
        DfuDownloadIdle,

        /// <summary>
        ///     dfuMANIFEST-SYNC
        /// </summary>
        DfuManifestSync,

        /// <summary>
        ///     dfuMANIFEST
        /// </summary>
        DfuManifest,

        /// <summary>
        ///     dfuMANIFEST-WAIT-RESET
        /// </summary>
        DfuManifestWaitReset,

        /// <summary>
        ///     dfuUPLOAD-IDLE
        /// </summary>
        DfuUploadIdle,

        /// <summary>
        ///     dfuERROR
        /// </summary>
        DfuError
    }

    /// <summary>
    ///     DFU status while firmware download/upload is being executed.
    /// </summary>
    public enum DfuStatus
    {
        /// <summary>
        ///     No error condition is present.
        /// </summary>
        Ok = 0,

        /// <summary>
        ///     File is not targeted for use by this device.
        /// </summary>
        ErrTarget,

        /// <summary>
        ///     File is for this device but fails some vendor-specific verification test.
        /// </summary>
        ErrFile,

        /// <summary>
        ///     Device is unable to write memory.
        /// </summary>
        ErrWrite,

        /// <summary>
        ///     Memory erase function failed.
        /// </summary>
        ErrErase,

        /// <summary>
        ///     Memory erase check failed.
        /// </summary>
        ErrCheckErased,

        /// <summary>
        ///     Program memory function failed.
        /// </summary>
        ErrProg,

        /// <summary>
        ///     Programmed memory failed verification.
        /// </summary>
        ErrVerify,

        /// <summary>
        ///     Cannot program memory due to received address that is out of range.
        /// </summary>
        ErrAddress,

        /// <summary>
        ///     Received DFU_DNLOAD with wLength = 0, but device does not think it has all of the data yet.
        /// </summary>
        ErrNotdone,

        /// <summary>
        ///     Device’s firmware is corrupt. It cannot return to run-time (non-DFU) operations.
        /// </summary>
        ErrFirmware,

        /// <summary>
        ///     iString indicates a vendor-specific error.
        /// </summary>
        ErrVendor,

        /// <summary>
        ///     Device detected unexpected USB reset signaling.
        /// </summary>
        ErrUsbr,

        /// <summary>
        ///     Device detected unexpected power on reset.
        /// </summary>
        ErrPor,

        /// <summary>
        ///     Something went wrong, but the device does not know what it was.
        /// </summary>
        ErrUnknown,

        /// <summary>
        ///     Device stalled an unexpected request.
        /// </summary>
        ErrStalledpkt,

        /// <summary>
        ///     Indicates claim interface error.
        /// </summary>
        ErrClaimInterface,

        /// <summary>
        ///     Indicates release interface error.
        /// </summary>
        ErrReleaseInterface,

        /// <summary>
        ///     Indicates control transfer error.
        /// </summary>
        ErrControlTransfer,

        /// <summary>
        ///     Indicates reset device error.
        /// </summary>
        ErrResetDevice,

        /// <summary>
        ///     Indicates an error loading the file to download.
        /// </summary>
        ErrFileRead,

        /// <summary>
        ///     An enum constant representing the Error not in dfu option.
        /// </summary>
        ErrNotInDfu,

        /// <summary>
        ///     An enum constant representing the Error invalid dfu state option.
        /// </summary>
        ErrInvalidDfuState,

        /// <summary>
        ///     An enum constant representing the Error argument null option.
        /// </summary>
        ErrArgumentNull
    }

    /// <summary>
    ///     An interface that exposes DFU capable comms
    /// </summary>
    public interface IDfuDriver : IDisposable
    {
        /// <summary>Gets a value indicating whether this object is in dfu mode.</summary>
        /// <value>True if this object is in dfu mode, false if not.</value>
        bool InDfuMode { get; }

        /// <summary>Gets a value indicating whether the device can download.</summary>
        /// <value>True if it can download, false if not.</value>
        bool CanDownload { get; }

        /// <summary>Gets a value indicating whether download in progress.</summary>
        /// <value>True if download in progress, false if not.</value>
        bool IsDownloadInProgress { get; }

        /// <summary>Occurs when download progresses.</summary>
        event EventHandler<ProgressEventArgs> DownloadProgressed;

        /// <summary>Enter dfu mode.</summary>
        /// <returns>True if it succeeds, false if it fails.</returns>
        Task<bool> EnterDfuMode();

        /// <summary>Exit dfu mode.</summary>
        /// <returns>True if it succeeds, false if it fails.</returns>
        Task<bool> ExitDfuMode();

        /// <summary>Downloads the given firmware.</summary>
        /// <param name="firmware">The firmware.</param>
        /// <returns>Dfu Status.</returns>
        Task<DfuStatus> Download(Stream firmware);

        /// <summary>
        ///     Aborts the currently in progress download operation.
        /// </summary>
        void AbortDownload();
    }
}