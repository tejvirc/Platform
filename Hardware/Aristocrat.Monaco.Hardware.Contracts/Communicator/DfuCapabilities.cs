namespace Aristocrat.Monaco.Hardware.Contracts.Communicator
{
    /// <summary>A dfu capabilities.</summary>
    public class DfuCapabilities
    {
        /// <summary>
        ///     Checks if the device will perform perform a bus
        ///     detach-attach sequence when it receives a DfuDetach request.
        /// </summary>
        /// <returns>True if explicit Reset is needed for entering DFU mode successfully.</returns>
        public bool WillDetach { get; set; }

        /// <summary>
        ///     Checks if the device is capable of another upload/download or needs explicit reset of the comms.
        /// </summary>
        /// <returns>True if manifestation tolerant.</returns>
        public bool ManifestationTolerant { get; set; }

        /// <summary>
        ///     Checks if the device is capable of upload.
        /// </summary>
        /// <returns> True if device can upload.</returns>
        public bool CanUpload { get; set; }

        /// <summary>
        ///     Checks if the device is capable of download.
        /// </summary>
        /// <returns> True if device can download.</returns>
        public bool CanDownload { get; set; }

        /// <summary>
        ///     Time, in milliseconds, that the device will wait for explicit reset before terminating the
        ///     request for DFU mode.
        /// </summary>
        /// <returns> Time in milliseconds.</returns>
        public int DetachTimeOut { get; set; }

        /// <summary>
        ///     Maximum number of bytes that the device can accept per control-write transaction.
        /// </summary>
        /// <returns> Size of transfer.</returns>
        public int TransferSize { get; set; }
    }
}