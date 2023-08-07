namespace Aristocrat.Monaco.Hardware.EdgeLight.Manager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.MemoryMappedFiles;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;
    using log4net;

    /// <summary>
    ///     Shared memory Manager: Interfaces with GDK/game via shared memory.
    /// </summary>
    internal class SharedMemoryManager : ISharedMemoryManager, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private Header _header;
        private MemoryMappedFile _sharedMem;
        private Mutex _sharedMemMutex;
        private bool _initialized;

        private bool _disposed;

        public SharedMemoryManager()
            : this(
                EdgeLightRuntimeParameters.EdgeLightSharedMemoryName,
                EdgeLightRuntimeParameters.EdgeLightSharedMutexName)
        {
        }

        public SharedMemoryManager(string edgeLightSharedMemoryName, string edgeLightSharedMutexName)
        {
            _header = new Header();

            SharedMutexName = edgeLightSharedMutexName;
            SharedMemoryName = edgeLightSharedMemoryName;

            Initialize();
        }

        public GameEdgelightData GameData
        {
            get
            {
                GameEdgelightData gameInfo = null;
                DoAccessorAction(x => gameInfo = ReadLightData(x));
                return gameInfo;
            }
        }

        public GameEdgelightData GetGameData()
        {
            return GameData;
        }

        private string SharedMutexName { get; }

        private string SharedMemoryName { get; }

        public string Name => nameof(SharedMemoryManager);

        public ICollection<Type> ServiceTypes { get; } = new[] { typeof(ISharedMemoryManager) };

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SetPlatformStripCount(IReadOnlyCollection<StripData> strips = null)
        {
            DoAccessorAction(x => WritePlatformData(x, strips));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_sharedMemMutex != null)
                {
                    _sharedMemMutex.Dispose();
                }

                if (_sharedMem != null)
                {
                    _sharedMem.Dispose();
                }
            }

            _sharedMemMutex = null;
            _sharedMem = null;

            _disposed = true;
        }

        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            _sharedMem = MemoryMappedFile.CreateNew(SharedMemoryName, _header.TotalSize);
            _sharedMemMutex = new Mutex(true, SharedMutexName, out _);

            // We need to write the header
            using (var accessor = _sharedMem.CreateViewAccessor(0, _header.TotalSize, MemoryMappedFileAccess.ReadWrite))
            {
                var headerData = _header.DataBytes;

                accessor.WriteArray(0, headerData, 0, headerData.Length);

                // Set default brightness
                accessor.Write(_header.GameOffset, EdgeLightingBrightnessLimits.MaximumBrightness);
            }

            _sharedMemMutex.ReleaseMutex();
        }

        private void DoAccessorAction(Action<MemoryMappedViewAccessor> action)
        {
            try
            {
                _sharedMemMutex.WaitOne();
                using (var accessor = _sharedMem.CreateViewAccessor(
                    0,
                    _header.TotalSize,
                    MemoryMappedFileAccess.ReadWrite))
                {
                    action(accessor);
                }
            }
            catch (AbandonedMutexException e)
            {
                Logger.Error("No edge lighting available", e);

                // Try to recreate the mutex to keep going
                _sharedMemMutex.Dispose();

                _sharedMemMutex = new Mutex(true, SharedMutexName, out _);
            }
            finally
            {
                // Let runtime host access the shared memory for writing.
                _sharedMemMutex.ReleaseMutex();
            }
        }

        private (int brightness, List<int>) ReadGameData(UnmanagedMemoryAccessor accessor)
        {
            var start = _header.GameOffset;
            var brightness = accessor.ReadInt32(start);
            start += sizeof(int);
            var stripCount = accessor.ReadInt32(start);
            start += sizeof(int);
            var strips = new int[stripCount];
            accessor.ReadArray(start, strips, 0, stripCount);
            return (brightness, strips.ToList());
        }

        private GameEdgelightData ReadLightData(UnmanagedMemoryAccessor accessor)
        {
            var (brightness, controlledStrips) = ReadGameData(accessor);
            var lightData = new Dictionary<int, IEnumerable<byte>>();
            foreach (var stripId in controlledStrips)
            {
                var offset = _header.HeaderSize + stripId * EdgeLightConstants.StrideLength;
                var data = new byte[EdgeLightConstants.StrideLength];
                accessor.ReadArray(offset, data, 0, data.Length);
                lightData.Add(stripId, data);
            }

            return new GameEdgelightData(brightness, lightData);
        }

        private void WritePlatformData(UnmanagedMemoryAccessor accessor, IReadOnlyCollection<StripData> strips)
        {
            var start = _header.PlatformOffset;

            accessor.Write(start, strips.Count);
            start += sizeof(int);

            var writeArray = strips.SelectMany(x => new[] { x.StripId, x.LedCount }).ToArray();
            accessor.WriteArray(start, writeArray, 0, writeArray.Length);
        }
    }
}