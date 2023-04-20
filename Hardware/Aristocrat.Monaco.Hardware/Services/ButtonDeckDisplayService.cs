namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO.MemoryMappedFiles;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.ButtonDeck;
    using Contracts.Cabinet;
    using Kernel;
    using log4net;
    using NativeInterop;

    /// <summary>
    ///     Provides services for rendering onto deck display services.
    ///     Gen8 Rig button deck has two displays:
    ///     1. 800x256 for bet buttons.
    ///     2. 240x320 for bash button.
    /// </summary>
    public class ButtonDeckDisplayService : IButtonDeckDisplay, IDisposable, IService
    {
        private const string LcdButtonDeckDescription = "USBD480";
        private const int BetButtonImageLength = 800 * 256 * 2;
        private const int BashButtonImageLength = 240 * 320 * 2;

        private const int SharedMemBufferLength = BetButtonImageLength + BashButtonImageLength;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly uint[] _frameId = { 0, 0 };

        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _propertiesManager;
        private readonly UsbDisplay480[] _displays = new UsbDisplay480[2];

        private bool _disposed;

        private MemoryMappedFile _sharedMem;
        private MemoryMappedViewStream _sharedMemStream;

        private Mutex _sharedMemMutex;
        private static readonly SemaphoreSlim DriverAccessSemaphore = new(1, 1);

        private byte[] _virtualBashButtonImageData;
        private byte[] _virtualBetButtonImageData;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ButtonDeckDisplayService" /> class.
        /// </summary>
        public ButtonDeckDisplayService()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ButtonDeckDisplayService" /> class.
        /// </summary>
        public ButtonDeckDisplayService(IEventBus eventBus, IPropertiesManager propertiesManager)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc />
        public int DisplayCount
        {
            get
            {
                try
                {
                    DriverAccessSemaphore.Wait();
                    return UsbDisplay480.DisplayCount();
                }
                finally
                {
                    DriverAccessSemaphore.Release();
                }
            }
        }

        /// <inheritdoc />
        public bool IsSimulated { get; private set; }

        public int Crc { get; private set; }

        public int Seed { get; private set; }

        /// <inheritdoc />
        public string GetFirmwareId(int displayIndex)
        {
            try
            {
                DriverAccessSemaphore.Wait();

                return _displays[displayIndex]?.FirmwareID;
            }
            finally
            {
                DriverAccessSemaphore.Release();
            }
        }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public uint GetRenderedFrameId(int displayIndex)
        {
            return _frameId[displayIndex];
        }

        /// <inheritdoc />
        public byte[] GetRenderedFrame(int displayIndex)
        {
            return displayIndex switch
            {
                0 => _virtualBetButtonImageData,
                1 => _virtualBashButtonImageData,
                _ => Array.Empty<byte>()
            };
        }

        /// <inheritdoc />
        public void DrawFromSharedMemory()
        {
            try
            {
                _sharedMemMutex.WaitOne();
                unsafe
                {
                    var imagePtr = (byte*)0;
                    _sharedMemStream.SafeMemoryMappedViewHandle.AcquirePointer(ref imagePtr);
                    Draw(0, (IntPtr)imagePtr, BetButtonImageLength);
                    Draw(1, (IntPtr)(imagePtr + BetButtonImageLength), BashButtonImageLength);
                    _sharedMemStream.SafeMemoryMappedViewHandle.ReleasePointer();
                }
            }
            catch (AbandonedMutexException)
            {
            }
            finally
            {
                _sharedMemMutex.ReleaseMutex();
            }
        }

        /// <inheritdoc />
        [HandleProcessCorruptedStateExceptions]
        public void Draw(int displayIndex, byte[] imageData)
        {
            try
            {
                DriverAccessSemaphore.Wait();

                unsafe
                {
                    fixed (byte* data = imageData)
                    {
                        InternalDraw(displayIndex, (IntPtr)data, imageData.Length);
                    }
                }
            }
            finally
            {
                DriverAccessSemaphore.Release();
            }
        }

        /// <inheritdoc />
        public void Draw(int displayIndex, IntPtr imageData, int imageLength)
        {
            try
            {
                DriverAccessSemaphore.Wait();

                InternalDraw(displayIndex, imageData, imageLength);
            }
            finally
            {
                DriverAccessSemaphore.Release();
            }
        }

        private void InternalDraw(int displayIndex, IntPtr imageData, int imageLength)
        {
            // Do we have the actual hardware device?
            if (_displays[displayIndex] != null)
            {
                _displays[displayIndex].DrawScreen(imageData, imageLength);
            }
            else
            {
                var destBuffer = GetRenderedFrame(displayIndex);
                if (destBuffer == null)
                {
                    return;
                }

                Marshal.Copy(imageData, destBuffer, 0, imageLength);
            }

            ++_frameId[displayIndex];
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IButtonDeckDisplay) };


        private bool _isInitialized;

        /// <inheritdoc />
        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;

            Logger.Info("Initializing...");

            var displayCount = DisplayCount;
            Logger.Debug($"Display Count = {displayCount}");

            Task.Run(() => OpenDisplays());

            CreateSharedMemory();

            // Are we simulating the hardware?
            var simulateLcdButtonDeck = _propertiesManager.GetValue(HardwareConstants.SimulateLcdButtonDeck, "FALSE");
            simulateLcdButtonDeck = simulateLcdButtonDeck.ToUpperInvariant();
            IsSimulated = simulateLcdButtonDeck == "TRUE";

            // Set UsbButtonDeck flag to TRUE
            if (displayCount > 1 || IsSimulated)
            {
                _propertiesManager.SetProperty(HardwareConstants.UsbButtonDeck, "TRUE");
            }

            // Allocate image data for virtual button deck if no hardware devices connected and we are simulating.
            if (displayCount == 0 && IsSimulated)
            {
                _virtualBetButtonImageData = new byte[BetButtonImageLength];
                _virtualBashButtonImageData = new byte[BashButtonImageLength];
            }

            _eventBus.Subscribe<DeviceConnectedEvent>(
                this,
                _ =>
                {
                    Task.Run(OpenDisplays);
                },
                x => x.Description == LcdButtonDeckDescription);

            Logger.Info("Initialized");
        }

        public async Task<int> CalculateCrc(int seed)
        {
            int result;
            Crc = 0;
            Seed = 0;
            string displayName;

            try
            {
                await DriverAccessSemaphore.WaitAsync();

                displayName = _displays[0]?.Name;
                result = _displays[0]?.SetConfigValue((int)ConfigValues.CrcCalculation, seed) ?? -1;
            }
            finally
            {
                DriverAccessSemaphore.Release();
            }

            if (result >= 0)
            {
                Crc = GetCrcResult(_displays[0]);
                Seed = seed;
            }

            Logger.Debug($"CalculateCrc: display: {displayName}, seed: {seed}, result: {result}, CRC: {Crc}");
            return Crc;
        }

        /// <summary>Disposes the service.</summary>
        /// <param name="disposing">Whether or not managed resources should be disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                DriverAccessSemaphore.Wait();

                if (disposing)
                {
                    _sharedMemStream?.Dispose();
                    _sharedMem.Dispose();
                    _sharedMem = null;
                    _sharedMemMutex.Dispose();
                    CloseDisplays();
                    _eventBus?.UnsubscribeAll(this);
                }

                _isInitialized = false;
                _disposed = true;
            }
            finally
            {
                DriverAccessSemaphore.Release();
                DriverAccessSemaphore.Dispose();
            }
        }

        private static void PrintDisplayInfo(UsbDisplay480 display)
        {
            Logger.Debug($"{display.Name} has serial {display.Serial} with size {display.PixelWidth}x{display.PixelHeight} and firmware Id {display.FirmwareID}");
        }

        private static void ClearDisplay(UsbDisplay480 display)
        {
            var clearData = CreateImageData(display.PixelCount, Color.MediumBlue);

            unsafe
            {
                // Pin the managed array.
                fixed (ushort* imagePtr = clearData)
                {
                    display.DrawScreen((IntPtr)imagePtr, clearData.Length * 2);
                }
            }
        }

        private static ushort[] CreateImageData(int pixelCount, Color color)
        {
            return Enumerable.Repeat(color.AsR5G6B5Color(), pixelCount).ToArray();
        }

        private async Task OpenDisplays()
        {
            try
            {
                await DriverAccessSemaphore.WaitAsync();

                CloseDisplays();

                for (var i = 0; i < _displays.Length; i++)
                {
                    _displays[i] = OpenDisplay(i);
                    if (_displays[i] == null)
                    {
                        return;
                    }
                }

                Array.Sort(_displays, (x, y) => y.PixelWidth.CompareTo(x.PixelWidth));
            }
            finally
            {
                DriverAccessSemaphore.Release();
            }

            await CalculateCrc(0);
        }

        private static UsbDisplay480 OpenDisplay(int i)
        {
            var display = new UsbDisplay480();
            if (display.Open(i) != 1)
            {
                Logger.Debug($"Open({i}) Failed.");
                display.Dispose();
                return null;
            }

            PrintDisplayInfo(display);
            ClearDisplay(display);
            return display;
        }


        private static int GetCrcResult(UsbDisplay480 display)
        {
            const int maxWaitTimeMs = 30000;

            using var source = new CancellationTokenSource();
            var queryCrc = QueryCrc(display, source.Token);
            Task.WhenAny(queryCrc, Task.Delay(maxWaitTimeMs, source.Token)).GetAwaiter().GetResult();
            var result = 0;
            if (queryCrc.IsCompleted)
            {
                result = queryCrc.Result;
            }

            source.Cancel();
            return result;
        }

        private static Task<int> QueryCrc(UsbDisplay480 display, CancellationToken token)
        {
            const int communicationDelayMs = 100;
            const int validStatus = 4;
            const int lostDevice = 1;

            try
            {
                DriverAccessSemaphore.Wait(token);

                while (!token.IsCancellationRequested)
                {
                    var result = 0;
                    int status;
                    unsafe
                    {
                        status = display?.GetConfigValue((int)ConfigValues.CrcCalculation, &result) ?? lostDevice;
                    }

                    if (status > 0)
                    {
                        return Task.FromResult(status == validStatus ? result : 0);
                    }

                    Thread.Sleep(communicationDelayMs);
                }
            }
            finally
            {
                DriverAccessSemaphore.Release();
            }

            return Task.FromResult(0);
        }

        private void CloseDisplays()
        {
            for (var i = 0; i < _displays.Length; i++)
            {
                _displays[i]?.Close();
                _displays[i]?.Dispose();
                _displays[i] = null;
            }
        }

        private void CreateSharedMemory()
        {
            _sharedMem = MemoryMappedFile.CreateNew("usbdMem", SharedMemBufferLength);

            _sharedMemMutex = new Mutex(true, "usbdMutex", out _);
            _sharedMemStream = _sharedMem.CreateViewStream();
            _sharedMemMutex.ReleaseMutex();
        }

        private enum ConfigValues
        {
            CrcCalculation = 30
        }
    }

    public static class ColorExtensions
    {
        public static ushort AsR5G6B5Color(this Color color)
        {
            const uint mask5 = 0x1f; // mask bottom 5 bits
            const uint mask6 = 0x3f; // mask bottom 6 bits

            var fr = color.R / 255.0f;
            var fg = color.G / 255.0f;
            var fb = color.B / 255.0f;

            // Rescale to bit range.
            var r = (uint)(fr * mask5);
            var g = (uint)(fg * mask6);
            var b = (uint)(fb * mask5);

            // Repack
            var pixel565 = (r << 11) | (g << 5) | b;

            return (ushort)pixel565;
        }
    }
}
