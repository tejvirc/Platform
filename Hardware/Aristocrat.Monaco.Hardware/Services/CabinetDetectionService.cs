namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Windows.Input;
    using Cabinet;
    using Cabinet.Contracts;
    using Common;
    using Contracts;
    using Contracts.ButtonDeck;
    using Contracts.Cabinet;
    using Contracts.Persistence;
    using Contracts.Touch;
    using Kernel;
    using log4net;
    using TouchDevice = Cabinet.TouchDevice;

    public sealed class CabinetDetectionService : IService, ICabinetDetectionService
    {
        private const string CabinetXmlField = "CabinetXML";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly ICabinetManager _cabinetManager;
        private readonly ICabinetDisplaySettings _cabinetDisplaySettings;
        private readonly List<string> _nonTouchButtonDecks = new List<string> { VbdType.Bartop.GetDescription(typeof(VbdType)) };
        private readonly IPersistentStorageManager _storageManager;
        private readonly IPropertiesManager _propertiesManager;
        private readonly object _lock = new object();
        private readonly List<(IDisplayDevice, ITouchDevice)> _displayToTouchMappings = new List<(IDisplayDevice, ITouchDevice)>();
        private IPersistentStorageAccessor _accessor;
        private ICabinet _cabinet = new Cabinet();

        private readonly List<DisplayRole> _displayOrderInCabinet = new List<DisplayRole>
        {
            DisplayRole.Topper,
            DisplayRole.Top,
            DisplayRole.Main,
            DisplayRole.VBD
        };

        public CabinetDetectionService()
            : this(
                Container.Instance.GetInstance<ICabinetManager>(),
                Container.Instance.GetInstance<ICabinetDisplaySettings>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        public CabinetDetectionService(
            ICabinetManager cabinetManager,
            ICabinetDisplaySettings cabinetDisplaySettings,
            IPersistentStorageManager storageManager,
            IPropertiesManager propertiesManager)
        {
            _cabinetManager = cabinetManager ?? throw new ArgumentNullException(nameof(cabinetManager));
            _cabinetDisplaySettings =
                cabinetDisplaySettings ?? throw new ArgumentNullException(nameof(cabinetDisplaySettings));
            _storageManager = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        private string CabinetXml
        {
            get => (string)_accessor[CabinetXmlField];
            set
            {
                using var transaction = _accessor.StartTransaction();
                transaction[CabinetXmlField] = value;
                transaction.Commit();
            }
        }

        public int Id
        {
            get
            {
                lock (_lock)
                {
                    return _cabinet.Id;
                }
            }
        }

        public string ButtonDeckType
        {
            get
            {
                var screen = GetDisplayDeviceByItsRole(DisplayRole.VBD);
                return screen != null ? screen.Name : this.GetButtonDeckType(_propertiesManager).ToString();
            }
        }

        public IReadOnlyCollection<IDevice> CabinetExpectedDevices
        {
            get
            {
                lock (_lock)
                {
                    var identifiedDevicesList = _cabinet.IdentifiedDevices.ToList();

                    return identifiedDevicesList;
                }
            }
        }

        public IEnumerable<IDisplayDevice> ExpectedDisplayDevices => CabinetExpectedDevices.OfType<IDisplayDevice>();

        public IEnumerable<IDisplayDevice> ExpectedDisplayDevicesWithSerialTouch { get; private set; }

        public IEnumerable<ITouchDevice> ExpectedSerialTouchDevices { get; private set; }

        public IEnumerable<ITouchDevice> ExpectedTouchDevices => CabinetExpectedDevices.OfType<ITouchDevice>();

        public int NumberOfDisplaysConnectedDuringInitialization => ExpectedDisplayDevices.Count();

        public int NumberOfDisplaysConnected => ExpectedDisplayDevices.Count(d => d.Status == DeviceStatus.Connected);

        public bool IsDisplayExpected(DisplayRole role) => GetDisplayDeviceByItsRole(role) != null;

        public bool IsDisplayConnected(DisplayRole role) => GetDisplayDeviceByItsRole(role)?.Status == DeviceStatus.Connected;

        public bool IsDisplayConnectedOrNotExpected(DisplayRole role)
        {
            var display = GetDisplayDeviceByItsRole(role);
            return display == null || display.Status == DeviceStatus.Connected;
        }

        public bool IsDisplayExpectedAndDisconnected(DisplayRole role)
        {
            var display = GetDisplayDeviceByItsRole(role);
            return display != null && display.Status != DeviceStatus.Connected;
        }

        public bool IsDisplayExpectedAndConnected(DisplayRole role)
        {
            var display = GetDisplayDeviceByItsRole(role);
            return display != null && display.Status == DeviceStatus.Connected;
        }

        public CabinetType Type
        {
            get
            {
                lock (_lock)
                {
                    return _cabinet.CabinetType;
                }
            }
        }

        public IDisplayDevice GetDisplayDeviceByItsRole(DisplayRole role)
        {
            return ExpectedDisplayDevices.FirstOrDefault(d => d.Role == role);
        }

        public void RefreshCabinetDeviceStatus()
        {
            lock (_lock)
            {
                _cabinetManager.UpdateStatus(_cabinet);
            }
        }

        public bool IsCabinetType(string cabinetType)
        {
            var match = Regex.Match(Type.ToString(), cabinetType, RegexOptions.None);
            return match.Success;
        }

        public bool IsTouchVbd()
        {
            var simulateVirtualButtonDeck = _propertiesManager.GetValue(
                HardwareConstants.SimulateVirtualButtonDeck,
                "FALSE");
            if (simulateVirtualButtonDeck.Equals("TRUE", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var vbdDevice = GetDisplayDeviceByItsRole(DisplayRole.VBD);
            return vbdDevice != null && !_nonTouchButtonDecks.Contains(vbdDevice.Name);
        }

        public bool TouchscreensMapped { get; private set; }

        public DisplayRole? GetDisplayRoleMappedToTouchDevice(ITouchDevice touchDevice)
        {
            var touchDevices = _cabinet.IdentifiedDevices.OfType<Aristocrat.Cabinet.TouchDevice>();
            var displayDevices = _cabinet.IdentifiedDevices.OfType<Aristocrat.Cabinet.DisplayDevice>();

            // DisplayDevice has the VID PID of the touch device it's mapped to
            var mappings = displayDevices.Select(displayDevice => (
                    Display: displayDevice,
                    Touch: touchDevices.FirstOrDefault(touchDevice =>
                        touchDevice.ProductId == displayDevice.TouchProductId &&
                        touchDevice.VendorId == displayDevice.TouchVendorId)))
                .Where(mapping => mapping.Touch != null && mapping.Display != null)
                .Distinct()
                .ToList();

            var result = mappings.FirstOrDefault(x => x.Touch == (Aristocrat.Cabinet.TouchDevice)touchDevice);

            return result.Display?.Role;
        }

        public IDisplayDevice GetDisplayMappedToTouchDevice(ITouchDevice touchDevice)
        {
            var mapping = _displayToTouchMappings.FirstOrDefault(entry =>
                entry.Item2.ProductId == touchDevice.ProductId &&
                entry.Item2.VendorId == touchDevice.VendorId);

            return mapping.Item1;
        }

        public ITouchDevice GetTouchDeviceMappedToDisplay(IDisplayDevice displayDevice)
        {
            var mapping = _displayToTouchMappings.FirstOrDefault(
                entry =>
                    entry.Item1.DisplayId == displayDevice.DisplayId);

            return mapping.Item2;
        }

        public bool MapTouchscreens(bool persistMapping = false)
        {
            lock (_lock)
            {
                Logger.Info($"MapTouchscreens - CabinetType {_cabinet.CabinetType} Id {_cabinet.Id} - persistMappings {persistMapping}");
                var mainDisplayDevice = GetDisplayDeviceByItsRole(DisplayRole.Main);

                var serialTouchName = "?";
                var serialTouchProductString = "?";
                var serialTouchVersionNumber = 0;
                var serialTouchService = ServiceManager.GetInstance().TryGetService<ISerialTouchService>();

                Logger.Debug($"MapTouchscreens - ExpectedTouchDevices.Any() {ExpectedTouchDevices.Any()}");

                // Is this an LS cabinet and we have not disabled serial touch via command line?
                if (IsCabinetType(HardwareConstants.CabinetTypeRegexLs) && _propertiesManager.GetValue(HardwareConstants.SerialTouchDisabled, "false") == "false")
                {
                    // No, add the main display device with serial touch
                    var expectedDisplayDevicesWithSerialTouch = new List<IDisplayDevice> { mainDisplayDevice };
                    ExpectedDisplayDevicesWithSerialTouch = expectedDisplayDevicesWithSerialTouch;

                    // Try to get serial touch model and firmware
                    if (serialTouchService is { Initialized: true })
                    {
                        serialTouchName = serialTouchService.Model;
                        var serialTouchOutputIdentity = serialTouchService.OutputIdentity;
                        if (serialTouchOutputIdentity.Length >= 6)
                        {
                            serialTouchProductString = serialTouchOutputIdentity.Substring(0, 2);
                            serialTouchVersionNumber = int.Parse(serialTouchOutputIdentity.Substring(2));
                        }

                        Logger.Debug($"MapTouchscreens - Serial touch product {serialTouchProductString} model {serialTouchName} version {serialTouchVersionNumber}");
                    }

                    var serialTouchDevice = _cabinet.IdentifiedDevices.OfType<TouchDevice>().FirstOrDefault(t =>
                        t.CommunicationType == CommunicationTypes.Serial);

                    if (serialTouchDevice != null)
                    {
                        serialTouchDevice.ProductString = serialTouchProductString;
                        serialTouchDevice.VersionNumber = serialTouchVersionNumber;
                        serialTouchDevice.Name = serialTouchName;
                    }

                    Logger.Debug($"MapTouchscreens - LS cabinet matched, injecting {serialTouchProductString} serial touch {serialTouchName} version {serialTouchVersionNumber} " +
                                 $"for main display device {mainDisplayDevice.Name} Touch VID {mainDisplayDevice.TouchVendorId} Touch PID {mainDisplayDevice.TouchProductId}");

                    var expectedSerialTouchDevices = new List<ITouchDevice> { serialTouchDevice };
                    ExpectedSerialTouchDevices = expectedSerialTouchDevices;
                }

                // Invalidate current mapping
                TouchscreensMapped = false;
                _displayToTouchMappings.Clear();

                // Map touchscreens to displays
                var success = _cabinetDisplaySettings.MapTouchscreens(_cabinet, out var map);
                if (!success)
                {
                    Logger.Warn($"MapTouchscreens - FAILED mapping touchscreens to displays, returning TouchscreensMapped {TouchscreensMapped}");
                    return TouchscreensMapped;
                }

                TouchscreensMapped = persistMapping;
                _displayToTouchMappings.AddRange(map.ToList());

                // If missing some displays (developer mode) then we return true for success but with an empty mappings collection.
                if (!ExpectedDisplayDevices.Any() || !ExpectedTouchDevices.Any())
                {
                     Logger.Debug($"MapTouchscreens - ExpectedDisplayDevices.Any() {ExpectedDisplayDevices.Any()} ExpectedTouchDevices.Any() {ExpectedTouchDevices.Any()}, returning TouchscreensMapped TRUE");
                     return TouchscreensMapped = true;
                }

                // Persist if requested
                if (persistMapping)
                {
                    PersistCabinet();
                }

                Logger.Debug($"MapTouchscreens - returning TouchscreensMapped {TouchscreensMapped}");
                return TouchscreensMapped;
            }
        }

        private void PersistCabinet()
        {
            var cabinetXml = _cabinet.ToXml();
            CabinetXml = cabinetXml;
            Logger.Info($"MapTouchscreens - Persisted cabinet mapped touch screens. Detected cabinet :- {CabinetXml}");
        }

        public ITouchDevice TouchDeviceByCursorId(int touchDeviceId)
        {
            var tabletDevice = Tablet.TabletDevices.OfType<TabletDevice>()
                .FirstOrDefault(td => td.StylusDevices.Any(sd => sd.Id == touchDeviceId));
            int.TryParse(tabletDevice?.ProductId, out var pid);
            return CabinetExpectedDevices.OfType<ITouchDevice>().FirstOrDefault(t => t.ProductId == pid) ??
                   ExpectedTouchDevices.FirstOrDefault();
        }

        public HardwareFamily Family
        {
            get
            {
                lock (_lock)
                {
                    return _cabinet.Family;
                }
            }
        }

        public void ApplyDisplaySettings()
        {
            lock (_lock)
            {
                _cabinetDisplaySettings.Apply(_cabinet);
            }
        }

        public string GetFirmwareVersion(ITouchDevice device)
        {
            lock (_lock)
            {
                return device.FirmwareVersion();
            }
        }

        public DisplayRole GetTopmostDisplay()
        {
            return _displayOrderInCabinet.FirstOrDefault(role => GetDisplayDeviceByItsRole(role) != null);
        }

        public string Name => nameof(ICabinetDetectionService);

        public ICollection<Type> ServiceTypes => new[] { typeof(ICabinetDetectionService) };

        public void Initialize()
        {
            Logger.Info("Initializing");
            _accessor = GetAccessor();

            if (string.IsNullOrEmpty(CabinetXml))
            {
                _cabinet = _cabinetManager.IdentifiedCabinet.First();
                CabinetXml = _cabinet.ToXml();
                Logger.Info($"Persisted cabinet null or empty. Detected cabinet :- {CabinetXml}");
            }
            else
            {
                _cabinet = CabinetXml.ToCabinet();
                Logger.Info($"Persisted cabinet found :- {CabinetXml}");
            }

            ApplyDisplaySettings();
            MapTouchscreens();
            Logger.Info("Finished initialization");
        }

        private IPersistentStorageAccessor GetAccessor()
        {
            var blockName = GetType().ToString();

            return _storageManager.BlockExists(blockName)
                ? _storageManager.GetBlock(blockName)
                : _storageManager.CreateBlock(PersistenceLevel.Transient, blockName, 1);
        }
    }
}