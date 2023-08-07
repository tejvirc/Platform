namespace Aristocrat.Monaco.Hardware.Usb
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Contracts.Communicator;
    using log4net;

    /// <summary>
    ///     DFU Driver
    /// </summary>
    public class DfuDriver : IDfuDriver
    {
        private const int DfuSuffixSize = 16; // bytes
        private const int DefaultPollTimeout = 50; // milliseconds
        private const int WaitDeviceReadyOnReset = 30000; // milliseconds
        private const int WaitDeviceReadyPostInstall = 10000; // milliseconds
        private const int MaxWaitDeviceReadyOnReset = 50000; // milliseconds
        private const int MaxWaitForDeviceStatus = 30000; //milliseconds
        private const int DownloadBlockAttempts = 3;
        private const int ManifestWaitTime = 180000;
        private const int FirmwareSizeScalar = 35000000;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private int _blockNumber;
        private DfuCapabilities _capabilities;
        private int _pollTimeout = DefaultPollTimeout;
        private IDfuTranslator _translator;

        /// <summary>Gets or sets a vendor identifier.</summary>
        /// <value>The identifier of the vendor.</value>
        public int VendorId { get; set; }

        /// <summary>Gets or sets the identifier of the product.</summary>
        /// <value>The identifier of the product.</value>
        public int ProductId { get; set; }

        /// <summary>Gets or sets the dfu product identifier.</summary>
        /// <value>The dfu product identifier.</value>
        public int ProductIdDfu { get; set; }

        /// <summary>Gets or sets a value indicating whether the current download or upload is aborted.</summary>
        /// <value>True if aborted, false if not.</value>
        private bool Aborted { get; set; }

        /// <inheritdoc />
        public bool InDfuMode { get; private set; }

        /// <inheritdoc />
        public bool CanDownload => _capabilities?.CanDownload ?? false;

        /// <inheritdoc />
        public bool IsDownloadInProgress { get; private set; }

        /// <inheritdoc />
        public event EventHandler<ProgressEventArgs> DownloadProgressed;

        /// <inheritdoc />
        public async Task<bool> EnterDfuMode()
        {
            if (InDfuMode)
            {
                LogWarn("DfuDriver: already in DFU mode.");
                return false;
            }

            try
            {
                if (!await Detach())
                {
                    return false;
                }

                _translator = await DevicePostReset(VendorId, ProductIdDfu);
                if (_translator == null)
                {
                    LogError("DfuDriver: dfu device not found post reset.");
                    return false;
                }

                _capabilities = _translator.Capabilities();
                if (_capabilities == null)
                {
                    LogError("DfuDriver: failed to get dfu device capabilities.");
                    return false;
                }

                var deviceStatus = GetDeviceStatus();
                if (deviceStatus == null)
                {
                    return false;
                }

                if (deviceStatus.State == DfuState.DfuError)
                {
                    ClearDeviceError();
                }

                InDfuMode = true;
                return InDfuMode;
            }
            catch (NullReferenceException e)
            {
                LogError($"DfuDriver: error in entering dfu mode - {e.Message}");
                return false;
            }
            finally
            {
                if (!InDfuMode)
                {
                    _translator?.Dispose();
                    _translator = null;
                }
            }
        }

        /// <inheritdoc />
        public async Task<bool> ExitDfuMode()
        {
            try
            {
                if (!InDfuMode)
                {
                    return true;
                }

                _translator.CommsReset();
                _translator.Dispose();
                _translator = null;

                var device = await DevicePostReset(VendorId, ProductId);
                if (device == null)
                {
                    return false;
                }

                device.Dispose();
                InDfuMode = false;
                return true;
            }
            catch (NullReferenceException e)
            {
                LogError($"DfuDriver: error in exiting dfu mode - {e.Message}");
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<DfuStatus> Download(Stream firmware)
        {
            if (firmware == null)
            {
                return DfuStatus.ErrArgumentNull;
            }

            if (_translator == null ||
                !InDfuMode)
            {
                return DfuStatus.ErrNotInDfu;
            }

            var bufferSize = _capabilities.TransferSize;
            var buffer = new byte[bufferSize];
            var stream = firmware;
            IsDownloadInProgress = true;
            try
            {
                // Download
                var firmwareSize = stream.Length - DfuSuffixSize;
                var remainingBytes = firmwareSize;
                var reportedProgress = 0;
                _blockNumber = 0;
                while (!Aborted && remainingBytes > 0)
                {
                    Array.Clear(buffer, 0, bufferSize);
                    int bytesToRead;
                    if (remainingBytes > bufferSize)
                    {
                        bytesToRead = bufferSize;
                    }
                    else
                    {
                        bytesToRead = (int)remainingBytes;
                    }

                    var bytesRead = stream.Read(buffer, 0, bytesToRead);
                    var downloading = await DownloadBlock(buffer, bytesRead);
                    if (downloading != DfuStatus.Ok)
                    {
                        return downloading;
                    }

                    //report successful block download
                    var progress = (int)((firmwareSize - remainingBytes) * 100L / firmwareSize);
                    if (progress > reportedProgress)
                    {
                        OnDownloadProgress(progress);
                        reportedProgress = progress;
                    }
                    remainingBytes -= bytesRead;
                }

                if (Aborted)
                {
                    OnDownloadAborted();
                }

                // Installation
                var install = await InstallFirmware(firmwareSize);
                if (install != DfuStatus.Ok)
                {
                    return install;
                }

                // Device reset post installation
                if (_capabilities.ManifestationTolerant)
                {
                    var deviceStatus = GetDeviceStatus();
                    if (deviceStatus != null &&
                        deviceStatus.State == DfuState.DfuError)
                    {
                        ClearDeviceError();
                    }
                }

                if (!_capabilities.WillDetach)
                {
                    _translator.Detach(_capabilities.DetachTimeOut);
                    _translator.CommsReset();
                    await Task.Delay(MaxWaitDeviceReadyOnReset);
                    return DfuStatus.Ok;
                }

                await Task.Delay(WaitDeviceReadyPostInstall);
                return DfuStatus.Ok;
            }
            catch (Exception e) when (e is NullReferenceException || e is IOException)
            {
                AbortDownload();
                LogError($"DfuDriver: download error - {e.Message}");
                return DfuStatus.ErrFileRead;
            }
            finally
            {
                IsDownloadInProgress = false;
            }
        }

        /// <inheritdoc />
        public void AbortDownload()
        {
            Aborted = true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases the unmanaged resources used by the Aristocrat.Monaco.Hardware.Usb.DfuDriver and optionally releases
        ///     the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     True to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _translator?.Dispose();
                _translator = null;
            }
        }

        private static void LogError(string text)
        {
            Logger.ErrorFormat(CultureInfo.CurrentCulture, text);
        }

        private static void LogInfo(string text)
        {
            Logger.InfoFormat(CultureInfo.CurrentCulture, text);
        }

        private static void LogWarn(string text)
        {
            Logger.WarnFormat(CultureInfo.CurrentCulture, text);
        }

        private DfuDeviceStatus GetDeviceStatus()
        {
            var deviceStatus = _translator.GetStatus();
            if (deviceStatus == null)
            {
                LogError("DfuDriver: get status command failed");
                return null;
            }

            _pollTimeout = deviceStatus.Timeout;
            return deviceStatus;
        }

        private DfuState GetDeviceState()
        {
            return _translator.GetState();
        }

        private void ClearDeviceError()
        {
            if (!_translator.ClearStatus())
            {
                LogError("DfuDriver: clear status command failed");
            }
        }

        private Task<bool> Detach()
        {
            LogInfo("DfuDriver: detaching");
            using (var translator =
                new DfuTranslator { VendorId = VendorId, ProductId = ProductId })
            {
                if (!translator.Initialize())
                {
                    LogError("DfuDriver: translator failed to initialize");
                    return Task.FromResult(false);
                }

                var caps = translator.Capabilities();
                if (caps == null)
                {
                    LogError("DfuDriver: runtime capabilities is null");
                    return Task.FromResult(false);
                }

                translator.Detach(caps.DetachTimeOut);

                if (!caps.WillDetach)
                {
                    translator.CommsReset();
                }

                return Task.FromResult(true);
            }
        }

        private async Task<DfuStatus> DownloadBlock(byte[] block, int count)
        {
            // make sure device is ready to download
            var state = GetDeviceState();
            if (state != DfuState.DfuIdle &&
                state != DfuState.DfuDownloadIdle &&
                state != DfuState.DfuDownloadSync)
            {
                LogError("DfuDriver: Device is not ready for download.");
                _translator.ClearStatus();
                state = GetDeviceState();
                if (state != DfuState.DfuIdle &&
                    state != DfuState.DfuDownloadIdle)
                {
                    return DfuStatus.ErrInvalidDfuState;
                }
            }

            // download block
            var packet = block;
            var attempts = DownloadBlockAttempts;
            var retVal = 0;
            while (attempts-- > 0)
            {
                retVal = _translator.Download(_blockNumber, packet, count);
                if (retVal > 0)
                {
                    _blockNumber++;
                    break;
                }

                await Task.Delay(_pollTimeout);
            }

            if (retVal < 0)
            {
                LogError("DfuDriver: download command failed");
                return DfuStatus.ErrControlTransfer;
            }

            // this delay lets device get ready for get status query
            await Task.Delay(10);

            // solicit status
            var waited = 0;
            while (waited < MaxWaitForDeviceStatus)
            {
                var status = GetDeviceStatus();
                switch (status.State)
                {
                    case DfuState.DfuError:
                        if (_translator.ClearStatus())
                        {
                            continue;
                        }

                        return status.Status;
                    case DfuState.DfuDownloadIdle:
                        return DfuStatus.Ok;
                }

                await Task.Delay(_pollTimeout);
                waited += _pollTimeout;
            }

            LogError("DfuDriver: Device did not download successfully.");
            return DfuStatus.ErrUnknown;
        }

        private async Task<DfuStatus> InstallFirmware(long firmwareSize)
        {
            // send zero download packet
            var buffer = new byte[0];
            if (_translator.Download(_blockNumber++, buffer, 0) < 0)
            {
                LogError("DfuDriver: error in downloading zero length packet");
                return DfuStatus.ErrControlTransfer;
            }

            // solicit status
            var waited = 0;
            var manifest = false;
            while (!manifest && waited < MaxWaitForDeviceStatus)
            {
                var status = GetDeviceStatus();
                switch (status.State)
                {
                    case DfuState.DfuError:
                        if (_translator.ClearStatus())
                        {
                            continue;
                        }

                        LogError($"DfuDriver: firmware installation error - {status.Status.ErrorText()}");
                        return status.Status;
                    case DfuState.DfuManifest:
                        manifest = true;
                        break;
                }

                if (!manifest)
                {
                    await Task.Delay(_pollTimeout);
                    waited += _pollTimeout;
                }
            }

            if (!manifest)
            {
                LogError("DfuDriver: Device failed to enter manifest phase.");
                return DfuStatus.ErrInvalidDfuState;
            }

            // give device some time to manifest the software.
            LogInfo("DfuDriver: installing firmware.");
            var manifestWaitTime = Convert.ToInt32(ManifestWaitTime * decimal.Divide(firmwareSize, FirmwareSizeScalar)); 
            await Task.Delay(manifestWaitTime);
            LogInfo("DfuDriver: firmware installed.");
            return DfuStatus.Ok;
        }

        private async Task<DfuTranslator> DevicePostReset(int vendorId, int productId)
        {
            var waited = 0;
            var translator = new DfuTranslator
            {
                VendorId = vendorId,
                ProductId = productId
            };
            do
            {
                await Task.Delay(WaitDeviceReadyOnReset);
                if (translator.Initialize())
                {
                    return translator;
                }

                waited += WaitDeviceReadyOnReset;
            } while (waited < MaxWaitDeviceReadyOnReset);

            return null;
        }

        private void OnDownloadAborted()
        {
            if (_translator == null)
            {
                return;
            }

            if (!_translator.Abort())
            {
                LogError("DfuDriver: : abort command failed");
            }
        }

        private void OnDownloadProgress(int progress)
        {
            Task.Run(
                () =>
                {
                    var invoker = DownloadProgressed;
                    invoker?.Invoke(this, new ProgressEventArgs(progress));
                });
        }

        private void PrintCapabilities(DfuCapabilities caps)
        {
            if (caps == null)
            {
                return;
            }

            Console.WriteLine($@" Detach: {caps.WillDetach}");
            Console.WriteLine($@" ManifestationTolerant: {caps.ManifestationTolerant}");
            Console.WriteLine($@" CanDownload: {caps.CanDownload}");
            Console.WriteLine($@" CanUpload: {caps.CanUpload}");
            Console.WriteLine($@" DetachTimeOut: {caps.DetachTimeOut}");
            Console.WriteLine($@" TransferSize: {caps.TransferSize}");
        }
    }
}