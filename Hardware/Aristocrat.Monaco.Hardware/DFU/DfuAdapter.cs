namespace Aristocrat.Monaco.Hardware.DFU
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Communicator;
    using Contracts.Dfu;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;

    /// <summary>A dfu adapter.</summary>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.SharedDevice.IDfuAdapter" />
    public class DfuAdapter : IDfuAdapter
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IDfuDevice _device;

        /// <summary>
        ///     Initializes a new instance of the Aristocrat.Monaco.Hardware.Contracts.SharedDevice.DfuAdapter class.
        /// </summary>
        public DfuAdapter(IDfuDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            VendorId = device.VendorId;
            ProductId = device.ProductId;
            device.DownloadProgressed += ReportDownloadProgressed;
        }

        /// <inheritdoc />
        public int ProductId { get; }

        /// <inheritdoc />
        public int VendorId { get; }

        /// <inheritdoc />
        public string DownloadFilename { get; set; } = string.Empty;

        /// <inheritdoc />
        public async Task<bool> Download(Stream firmware)
        {
            Logger.Debug("DfuAdapter: Download Request");

            // detach
            if (!await _device.Detach())
            {
                Logger.Error("DfuAdapter: Failed to enter DFU mode");
                return false;
            }

            // download
            var result = await _device.Download(firmware);
            if (result != 0)
            {
                Logger.Error("DfuAdapter: Download error");
                PublishEvent(new DfuErrorEvent((DfuErrorEventId)result, 0));
                return false;
            }

            PublishEvent(new DfuDownloadCompleteEvent());

            // reconnect
            await _device.Reconnect();

            return true;
        }

        /// <inheritdoc />
        public Task Abort()
        {
            Logger.Debug("DfuAdapter: Abort Request");

            return Task.Run(() => { _device.Abort(); });
        }

        private void ReportDownloadProgressed(object sender, ProgressEventArgs e)
        {
            PublishEvent(new DfuDownloadProgressEvent(e.Progress));
        }

        /// <summary>Publish event to event bus</summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="event">The event.</param>
        private static void PublishEvent<T>(T @event)
            where T : IEvent
        {
            Task.Run(
                () =>
                {
                    ServiceManager.GetInstance().TryGetService<IEventBus>()?.Publish(@event);
                });
        }
    }
}