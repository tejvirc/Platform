namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.FirmwareCrcMonitor;
    using Contracts.Localization;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.ButtonDeck;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.Gds;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Kernel.Contracts;
    using log4net;
    using Monaco.Localization.Properties;
    using Util;

    /// <summary>
    ///     The FirmwareCrcMonitor class is responsible for initializing and verifying device's CRCs of their firmware to
    ///     ensure changes are detected upon logic door open/close and on power cycle.
    /// </summary>
    public class FirmwareCrcMonitor : IService, IDisposable
    {
        private const string CrcBlockMemberName = "Crc";
        private const string DefaultAlarmSoundFilePath = @"..\jurisdiction\DefaultAssets\alarm.ogg";
        private const int DesignatedCrcErrorNumber = 0; // CrcRequest will return 0 if something goes wrong.
        private const int NoteAcceptorPosition = 0;
        private const int PrinterPosition = 1;
        private const int LCDButtonDeckPosition = 2;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly int[] DeviceBlockPosition =
        {
            NoteAcceptorPosition,
            PrinterPosition,
            LCDButtonDeckPosition
        };

        private readonly IEventBus _bus;
        private readonly IPersistentStorageManager _storageManager;
        private readonly IPropertiesManager _properties;
        private readonly ISystemDisableManager _disableManager;
        private readonly string _blockName = typeof(FirmwareCrcMonitor).ToString();
        private readonly SemaphoreSlim _logicDoorLock = new SemaphoreSlim(1);
        private readonly IList<IFirmwareCrcBridge> _devices = new List<IFirmwareCrcBridge>(DeviceBlockPosition.Length);
        private readonly IAudio _audioService;

        private readonly IDictionary<int, int> _persistedDeviceToCrcMap =
            new Dictionary<int, int>();

        private readonly IDictionary<int, int> _currentDeviceToCrcMap =
            new Dictionary<int, int>();

        private int _seed;

        private bool _disposed;
        private bool _newBlockGenerated;

        private string _firmwareCrcErrorSoundFilePath;

        public FirmwareCrcMonitor()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<IAudio>())
        {
        }

        public FirmwareCrcMonitor(
            IEventBus bus,
            IPersistentStorageManager storageManager,
            IPropertiesManager properties,
            ISystemDisableManager disableManager,
            IAudio audioService)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _storageManager = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            CreateBlock();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public string Name => nameof(FirmwareCrcMonitor);

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(FirmwareCrcMonitor) };

        /// <inheritdoc />
        public void Initialize()
        {
            if (!(bool)_properties.GetProperty(ApplicationConstants.FirmwareCrcMonitorEnabled, false))
            {
                return;
            }

            Logger.Info("Initializing FirmwareCrcMonitor...");
            _seed = (int)_properties.GetProperty(ApplicationConstants.FirmwareCrcMonitorSeed, GdsConstants.DefaultSeed);

            _firmwareCrcErrorSoundFilePath = _properties?.GetValue(ApplicationConstants.FirmwareCrcErrorSoundKey, DefaultAlarmSoundFilePath);

            InitializeDeviceMappings();

            _bus.Subscribe<PlatformBootedEvent>(this, Handle);
            _bus.Subscribe<ClosedEvent>(this, Handle, evt => evt.LogicalId == (int)DoorLogicalId.Logic);
            Logger.Info("Initializing FirmwareCrcMonitor...complete!");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _bus.UnsubscribeAll(this);
                _logicDoorLock.Dispose();
            }

            _disposed = true;
        }

        private async Task<bool> VerifyAllDevices()
        {
            await GetCurrentCrcs();
            return _persistedDeviceToCrcMap.Keys.Count == _currentDeviceToCrcMap.Keys.Count &&
                   _persistedDeviceToCrcMap.Keys.All(
                       k => _currentDeviceToCrcMap.ContainsKey(k) &&
                            Equals(_currentDeviceToCrcMap[k], _persistedDeviceToCrcMap[k]));
        }

        private async Task GetCurrentCrcs()
        {
            _disableManager.Disable(
                ApplicationConstants.MonitorVerifyingDisableKey,
                SystemDisablePriority.Immediate,
                () => Localizer.ForLockup().GetString(ResourceKeys.VerifyingCRCSignaturesLockupText));
            _currentDeviceToCrcMap.Clear();
            foreach (var i in DeviceBlockPosition)
            {
                if (_devices[i] != null)
                {
                    _currentDeviceToCrcMap[i] = await _devices[i].CalculateCrc(_seed);
                }
                else
                {
                    _currentDeviceToCrcMap[i] = DesignatedCrcErrorNumber;
                }
            }

            _disableManager.Enable(ApplicationConstants.MonitorVerifyingDisableKey);
        }

        private void InitializeDeviceMappings()
        {
            // Add new devices here as required, must map to DeviceBlockPosition
            var serviceManager = ServiceManager.GetInstance();
            _devices.Add(serviceManager.TryGetService<INoteAcceptor>());
            _devices.Add(serviceManager.TryGetService<IPrinter>());
            _devices.Add(serviceManager.TryGetService<IButtonDeckDisplay>());
        }

        private void CreateBlock()
        {
            if (_storageManager.BlockExists(_blockName))
            {
                return;
            }

            _storageManager.CreateBlock(
                PersistenceLevel.Static,
                _blockName,
                DeviceBlockPosition.Length);
            _newBlockGenerated = true;
        }

        private async Task InitializePersistence()
        {
            var block = _storageManager.GetBlock(_blockName);
            if (_newBlockGenerated)
            {
                await GetCurrentCrcs();
                PersistNewCrcSignatures(block);
            }
            else
            {
                GetCrcSignaturesFromPersistence(block);
            }
        }

        private void DisableEgmDueToSignatureMismatch(string disableMessageSuffix)
        {
            _disableManager.Disable(
                ApplicationConstants.MonitorSignatureMismatchDisableKey,
                SystemDisablePriority.Immediate,
                () => Localizer.ForLockup().FormatString(
                    ResourceKeys.DeviceSignatureMismatchText,
                    disableMessageSuffix));
        }

        private string GetHardwareName(int blockPosition)
        {
            switch (blockPosition)
            {
                case NoteAcceptorPosition:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoteAcceptorLabel);
                case PrinterPosition:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PrinterLabel);
                case LCDButtonDeckPosition:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ButtonDeck);
                default:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Unknown);
            }
        }

        private string GenerateMismatchedDevicesString()
        {
            return string.Join(",",
                _persistedDeviceToCrcMap.Where(pair => !pair.Value.Equals(_currentDeviceToCrcMap[pair.Key]))
                     .Select(pair => GetHardwareName(pair.Key)));
        }

        private void GetCrcSignaturesFromPersistence(IPersistentStorageAccessor block)
        {
            var results = block.GetAll();
            foreach (var result in results)
            {
                if (DeviceBlockPosition.Contains(result.Key) &&
                    result.Value.ContainsKey(CrcBlockMemberName))
                {
                    _persistedDeviceToCrcMap.Add(
                        result.Key,
                        (int)result.Value[CrcBlockMemberName]);
                }
            }
        }

        private void PersistNewCrcSignatures(IPersistentStorageAccessor block)
        {
            using (var transaction = block.StartTransaction())
            {
                _persistedDeviceToCrcMap.Clear();
                foreach (var pair in _currentDeviceToCrcMap)
                {
                    _persistedDeviceToCrcMap[pair.Key] = pair.Value;
                    transaction[pair.Key, CrcBlockMemberName] = pair.Value;
                }

                transaction.Commit();
            }
        }

        private async Task Handle(ClosedEvent evt, CancellationToken token)
        {
            await _logicDoorLock.WaitAsync(token);
            try
            {
                if (!_disableManager.CurrentDisableKeys.Contains(
                    ApplicationConstants.MonitorSignatureMismatchDisableKey))
                {
                    await ValidateFirmwareCrc();
                }
                else
                {
                    var block = _storageManager.GetBlock(_blockName);
                    PersistNewCrcSignatures(block);
                    _disableManager.Enable(ApplicationConstants.MonitorSignatureMismatchDisableKey);
                }
            }
            finally
            {
                _logicDoorLock.Release();
            }
        }

        private async Task Handle(PlatformBootedEvent evt, CancellationToken token)
        {
            await InitializePersistence();
            await ValidateFirmwareCrc();
        }

        private async Task ValidateFirmwareCrc()
        {
            if (!await VerifyAllDevices())
            {
                var mismatchedDeviceNames = GenerateMismatchedDevicesString();
                PlayFirmwareCrcErrorSound();
                DisableEgmDueToSignatureMismatch(mismatchedDeviceNames);
                _bus.Publish(new FirmwareCrcMismatchedEvent(mismatchedDeviceNames));
            }
        }

        private void PlayFirmwareCrcErrorSound()
        {
            if (!(bool)_properties.GetProperty(KernelConstants.IsInspectionOnly, false))
            {
                _audioService.LoadSound(SoundName.FirmwareCrcErrorSound, _firmwareCrcErrorSoundFilePath);
                _audioService.PlaySound(_properties, SoundName.FirmwareCrcErrorSound);
            }
        }
    }
}