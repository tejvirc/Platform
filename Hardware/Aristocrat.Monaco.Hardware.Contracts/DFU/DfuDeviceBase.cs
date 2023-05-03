namespace Aristocrat.Monaco.Hardware.Contracts.Dfu
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Contracts;
    using Communicator;

    /// <summary>Base class for implementing Device Firmware Upgrade (DFU) logic.</summary>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.IDfuDevice" />
    public abstract class DfuDeviceBase : IDfuDevice
    {
        /// <summary>
        ///     Returns true if device is in DFU mode.
        /// </summary>
        public bool InDfuMode => Driver?.InDfuMode ?? false;

        private IDfuDriver Driver { get; set; }

        /// <inheritdoc />
        public int VendorId { get; private set; }

        /// <inheritdoc />
        public int ProductId { get; private set; }

        /// <inheritdoc />
        public bool IsDfuCapable => Driver?.CanDownload ?? false;

        /// <inheritdoc />
        public bool IsDfuInProgress => Driver?.IsDownloadInProgress ?? false;

        /// <inheritdoc />
        public event EventHandler<ProgressEventArgs> DownloadProgressed;

        /// <inheritdoc />
        public virtual Task<bool> Initialize(ICommunicator communicator)
        {
            Driver = communicator as IDfuDriver;
            if (Driver == null)
            {
                return Task.FromResult(false);
            }

            VendorId = communicator.VendorId;
            ProductId = communicator.ProductId;
            Driver.DownloadProgressed += ReportDownloadProgressed;
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public async Task<bool> Detach()
        {
            if (Driver == null)
            {
                return false;
            }

            return await Driver.EnterDfuMode();
        }

        /// <inheritdoc />
        public async Task<bool> Reconnect()
        {
            if (Driver == null)
            {
                return false;
            }

            return await Driver.ExitDfuMode();
        }

        /// <inheritdoc />
        public async Task<DfuStatus> Download(Stream firmware)
        {
            if (Driver == null)
            {
                return DfuStatus.ErrArgumentNull;
            }

            return await Driver.Download(firmware);
        }

        /// <inheritdoc />
        public Task<DfuStatus> Upload(Stream firmware)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Abort()
        {
            Driver?.AbortDownload();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases the unmanaged resources used by the Aristocrat.Monaco.Hardware.DfuDeviceBase and optionally
        ///     releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     True to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DownloadProgressed -= ReportDownloadProgressed;
            }
        }

        /// <summary>Raises the progress event.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected void OnDownloadProgressed(ProgressEventArgs e)
        {
            DownloadProgressed?.Invoke(this, e);
        }

        private void ReportDownloadProgressed(object sender, ProgressEventArgs e)
        {
            Task.Run(() => { OnDownloadProgressed(e); });
        }
    }
}