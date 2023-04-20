namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Dfu;
    using Contracts.SharedDevice;
    using DFU;
    using Kernel;
    using log4net;

    /// <summary>A dfu provider.</summary>
    /// <seealso cref="Aristocrat.Monaco.Kernel.IService" />
    /// <seealso cref="IDeviceService" />
    /// <seealso cref="IDfuProvider" />
    public class DfuProvider : IDfuProvider
    {
        private const int DfuSuffixSize = 16;
        private const ushort DefaultVendorId = 0xffff;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, IDfuAdapter>> _adapters = new();

        private readonly ConcurrentDictionary<(int VendorId, int ProductId), IDfuAdapter> _downloads = new();

        public DfuProvider()
            : this(new DfuFactory(ServiceManager.GetInstance().GetService<IEventBus>()))
        {
        }

        public DfuProvider(IDfuFactory dfuFactory)
        {
            DfuFactory = dfuFactory ?? throw new ArgumentNullException(nameof(dfuFactory));
        }

        /// <summary>Gets or sets the dfu factory.</summary>
        /// <value>The dfu factory.</value>
        protected IDfuFactory DfuFactory { get; set; }

        /// <inheritdoc />
        public bool Initialized { get; private set; }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IDfuProvider) };

        /// <inheritdoc />
        public bool IsDownloadInProgress(int vendorId, int productId)
        {
            return _downloads.ContainsKey((vendorId, productId));
        }

        /// <inheritdoc />
        public string DownloadFilename(int vendorId, int productId)
        {
            return _downloads.TryGetValue((vendorId, productId), out var adapter)
                ? adapter.DownloadFilename
                : null;
        }

        /// <inheritdoc />
        public async Task<bool> Download(string file)
        {
            if (!Initialized)
            {
                return false;
            }

            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            return await Task.Run(
                async () =>
                {
                    IDfuAdapter adapter = null;
                    try
                    {
                        //Retrieve Vendor and Product Ids
                        using (var stream = new FileStream(file, FileMode.Open))
                        {
                            adapter = Device(stream);

                            if (adapter == null)
                            {
                                Logger.Error("No device can be found for this firmware.");
                                return false;
                            }

                            adapter.DownloadFilename = file;
                            if (!_downloads.TryAdd((adapter.VendorId, adapter.ProductId), adapter))
                            {
                                Logger.Error("A download is currently active for this device.");
                                return false;
                            }

                            return await adapter.Download(stream);
                        }
                    }
                    finally
                    {
                        if (adapter != null)
                        {
                            _downloads.TryRemove((adapter.VendorId, adapter.ProductId), out _);
                        }
                    }
                });
        }

        public async Task Abort(int vendorId, int productId)
        {
            if (!_downloads.TryGetValue((vendorId, productId), out var adapter))
            {
                await Task.CompletedTask;
                return;
            }

            await adapter.Abort();
        }

        /// <inheritdoc />
        public void Register(IDfuDevice device)
        {
            if (device == null)
            {
                Logger.Error("DfuProvider: device is null");
                return;
            }

            var vendorId = device.VendorId;
            var productId = device.ProductId;
            if (!_adapters.TryGetValue(vendorId, out var devices))
            {
                devices = new ConcurrentDictionary<int, IDfuAdapter>();
                _adapters.TryAdd(vendorId, devices);
            }

            var adapter = DfuFactory.CreateAdapter(device);
            devices.AddOrUpdate(productId, adapter, (_, _) => adapter);
        }

        /// <inheritdoc />
        public void Unregister(IDfuDevice device)
        {
            if (device == null)
            {
                Logger.Error("DfuProvider: device is null");
                return;
            }

            var vendorId = device.VendorId;
            var productId = device.ProductId;
            if (!_adapters.TryGetValue(vendorId, out var devices))
            {
                return;
            }

            devices.TryRemove(productId, out _);
        }

        /// <inheritdoc />
        public void Initialize()
        {
            Initialized = true;
            Logger.Debug($"{Name} initialized");
        }

        /// <summary>Function to get the reference to device.</summary>
        /// <param name="stream">The firmware stream.</param>
        /// <returns>An IDeviceAdapter reference.</returns>
        private IDfuAdapter Device(Stream stream)
        {
            if (stream == null ||
                !stream.CanSeek)
            {
                return null;
            }

            try
            {
                var buffer = new byte[DfuSuffixSize];
                stream.Seek(-DfuSuffixSize, SeekOrigin.End);
                _ = stream.Read(buffer, 0, buffer.Length);
                stream.Seek(0, SeekOrigin.Begin); // reset the stream to beginning
                Array.Reverse(buffer);

                var vendorId = (ushort)(buffer[11] | (buffer[10] << 8));
                var productId = (ushort)(buffer[13] | (buffer[12] << 8));
                if (vendorId == DefaultVendorId)
                {
                    return null;
                }

                if (!_adapters.TryGetValue(vendorId, out var devices))
                {
                    return null;
                }

                if (devices.TryGetValue(productId, out var device))
                {
                    return device;
                }

                return devices.First().Value;
            }
            catch (IOException e)
            {
                Logger.Error(e.Message);
                return null;
            }
        }
    }
}