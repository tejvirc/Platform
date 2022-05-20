namespace Aristocrat.Monaco.Hardware.Contracts.Communicator
{
    /// <summary>A dfu extensions.</summary>
    public static class DfuExtensions
    {
        /// <summary>The DfuDeviceStatus extension method that error text.</summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A string.</returns>
        public static string ErrorText(this DfuStatus @this)
        {
            var dfuErrorText = string.Empty;
            switch (@this)
            {
                case DfuStatus.ErrTarget:
                    dfuErrorText = "File is not targeted for use by this device";
                    break;

                case DfuStatus.ErrFile:
                    dfuErrorText = "File is for this device but fails some " +
                                   "vendor-specific verification test";
                    break;
                case DfuStatus.ErrWrite:
                    dfuErrorText = "Device is unable to write memory";
                    break;

                case DfuStatus.ErrErase:
                    dfuErrorText = "Memory erase function failed";
                    break;

                case DfuStatus.ErrCheckErased:
                    dfuErrorText = "Memory erase check failed";
                    break;

                case DfuStatus.ErrProg:
                    dfuErrorText = "Program memory function failed";
                    break;

                case DfuStatus.ErrVerify:
                    dfuErrorText = "Programmed memory failed verification";
                    break;
                case DfuStatus.ErrAddress:
                    dfuErrorText = "Cannot program memory due to received " +
                                   "address that is out of range";
                    break;

                case DfuStatus.ErrNotdone:
                    dfuErrorText = "Download incomplete";
                    break;

                case DfuStatus.ErrFirmware:
                    dfuErrorText = "Device firmware is corrupt and " +
                                   "cannot return to normal operation";
                    break;

                case DfuStatus.ErrVendor:
                    dfuErrorText = "Vendor specific error";
                    break;

                case DfuStatus.ErrUsbr:
                    dfuErrorText = "Device detected unexpected USB reset signaling";
                    break;

                case DfuStatus.ErrPor:
                    dfuErrorText = "Device detected unexpected power on reset";
                    break;

                case DfuStatus.ErrUnknown:
                    dfuErrorText = "Unknown error";
                    break;

                case DfuStatus.ErrStalledpkt:
                    dfuErrorText = "Device stalled an unexpected request";
                    break;
                case DfuStatus.Ok:
                    break;
            }

            return dfuErrorText;
        }
    }
}
