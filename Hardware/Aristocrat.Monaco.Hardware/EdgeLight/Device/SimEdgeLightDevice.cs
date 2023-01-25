namespace Aristocrat.Monaco.Hardware.EdgeLight.Device
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Serialization;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;
    using log4net;
    using Strips;

    public sealed class SimEdgeLightDevice : IEdgeLightDevice
    {
        private const string LogicalStripsCreationRuleXml = @".\EdgeLightStripsCreationRule.xml";

        public static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<IStrip> _physicalStrips = new List<IStrip>();
        private bool _connected;
        private bool _disposed;
        private int _systemBrightness = EdgeLightConstants.MaxChannelBrightness;

        public SimEdgeLightDevice()
        {
            LoadMappingsFromXmlDefinition(LogicalStripsCreationRuleXml);
        }

        public void LoadEdgeLightStrips(string edgeLightName)
        {
            var mapping = Mappings.FirstOrDefault(m => m.Name == edgeLightName);

            if (mapping == null)
            {
                throw new ArgumentException("Invalid Edge Lighting Name Provided");
            }

            NewStripsDiscovered(mapping.HardwareStrips.Select(h => new PhysicalStrip(h.Id, h.LedCount)), true);
        }

        public List<EdgeLightMapping> Mappings { get; private set; }

        public IReadOnlyList<IStrip> PhysicalStrips => _physicalStrips;

        public event EventHandler<EventArgs> ConnectionChanged;

        public event EventHandler<EventArgs> StripsChanged;

        public event EventHandler<StripsRenderedEventArgs> StripsRendered;

        public BoardIds BoardId { get; set; } = BoardIds.InvalidBoardId;

        public ICollection<EdgeLightDeviceInfo> DevicesInfo => new List<EdgeLightDeviceInfo> { GetDeviceInfo() };

        public void RenderAllStripData()
        {
            StripsRendered?.Invoke(this, new StripsRenderedEventArgs(_physicalStrips));
        }

        private void LoadMappingsFromXmlDefinition(string fileOrResource)
        {
            Stream stream;
            if (File.Exists(fileOrResource))
            {
                stream = new FileStream(fileOrResource, FileMode.Open, FileAccess.Read);
            }
            else
            {
                var localAssembly = Assembly.GetExecutingAssembly();
                stream = localAssembly.GetManifestResourceStream(fileOrResource);
            }

            if (stream == null)
            {
                throw new Exception($" The edge light mapping {fileOrResource} cannot be found");
            }

            using (stream)
            {
                var xmlSerializer = new XmlSerializer(typeof(EdgeLightMappings));
                var mappings = (EdgeLightMappings)xmlSerializer.Deserialize(stream);
                GetValidMappings(mappings);
            }
        }

        private void GetValidMappings(EdgeLightMappings mappings)
        {
            var validMappings = new List<EdgeLightMapping>();

            foreach (var mapping in mappings.Mappings)
            {
                var hardwareStrips = mapping.HardwareStrips.ToDictionary(h => h.Id);

                var valid = mapping.LogicalStrips.All(
                    l => l.LedSegments.All(
                        s => hardwareStrips.ContainsKey(s.HardwareStripId) &&
                             hardwareStrips[s.HardwareStripId].LedCount > 0));

                if (valid) validMappings.Add(mapping);
            }

            Mappings = validMappings;
        }

        public bool LowPowerMode { get; set; }

        public bool IsOpen
        {
            get => _connected;
            private set
            {
                if (_connected == value)
                {
                    return;
                }

                _connected = value;

                StripsChanged?.Invoke(this, EventArgs.Empty);
                ConnectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Close()
        {
            _physicalStrips.Clear();
            IsOpen = false;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Close();
            _disposed = true;
        }

        public bool CheckForConnection()
        {
            if(!IsOpen)
                Open();

            return IsOpen;
        }

        public void NewStripsDiscovered(IEnumerable<IStrip> strips, bool removeOldStrips)
        {
            if (removeOldStrips)
            {
                _physicalStrips.Clear();
            }

            var stripList = strips.ToList();
            _physicalStrips.AddRange(stripList);

            StripsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetSystemBrightness(int brightness)
        {
            if (brightness != _systemBrightness)
            {
                _systemBrightness = brightness < 0 ? 0 :( brightness > 100 ? 100 : brightness);
            }
        }

        private void Open()
        {
            if (IsOpen)
            {
                return;
            }

            Close();

            Logger.Debug("Simulated edge light device inserted.");

            ReadConfigurationFromFirmware();
        }

        private void ReadConfigurationFromFirmware()
        {
            LoadEdgeLightStrips(Mappings?.First().Name);

            IsOpen = true;
            Logger.Debug("Edge light device opened.");
        }

        private EdgeLightDeviceInfo GetDeviceInfo()
        {
            var info = new EdgeLightDeviceInfo();
            info.Manufacturer = "Aristocrat";
            info.Product = "Sim Edge Light";
            info.SerialNumber = "1";
            info.Version = 1;
            info.DeviceType = ElDeviceType.Cabinet;
            return info;
        }
    }
}