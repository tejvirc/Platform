namespace Aristocrat.Monaco.Gaming.UI.ViewModels.EdgeLight
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Hardware.Contracts.EdgeLighting;
    using Hardware.EdgeLight.Contracts;
    using Hardware.EdgeLight.Device;
    using Hardware.EdgeLight.Strips;
    using Kernel;
    using Monaco.UI.Common.ViewModels;

    public class EdgeLightSimulatorViewModel : ObservableObject
    {
        private readonly SimEdgeLightDevice _device;

        private EdgeLightMapping _selectedMapping;
        private Dictionary<int, DisplayStrip> _strips;
        private DisplayStrip _topStrip;
        private DisplayStrip _bottomStrip;
        private DisplayStrip _leftStrip;
        private DisplayStrip _rightStrip;
        private DisplayStrip _vbdStrip;

        public EdgeLightSimulatorViewModel()
        {
            Strips = new Dictionary<int, DisplayStrip>();

            var edgeLightDeviceFactory = ServiceManager.GetInstance().GetService<IEdgeLightDeviceFactory>() as SimEdgeLightDeviceFactory;
            _device = edgeLightDeviceFactory?.SimulatedEdgeLightDevice ?? throw new NullReferenceException("Failed to get simulated edge light device");

            _selectedMapping = _device.Mappings.FirstOrDefault();
            MapStrips();
            _device.StripsRendered += RenderStrips;
        }

        public List<EdgeLightMapping> Mappings => _device.Mappings;

        public EdgeLightMapping SelectedMapping
        {
            get => _selectedMapping;
            set
            {
                _selectedMapping = value;
                _device.LoadEdgeLightStrips(value.Name);
                MapStrips();
                OnPropertyChanged(nameof(SelectedMapping));
            }
        }

        public Dictionary<int, DisplayStrip> Strips
        {
            get => _strips;

            set
            {
                _strips = value;
                OnPropertyChanged(nameof(Strips));
            }
        }

        public DisplayStrip LeftStrip
        {
            get => _leftStrip;
            set
            {
                _leftStrip = value;
                OnPropertyChanged(nameof(LeftStrip));
            }
        }

        public DisplayStrip RightStrip
        {
            get => _rightStrip;
            set
            {
                _rightStrip = value;
                OnPropertyChanged(nameof(RightStrip));
            }
        }
        public DisplayStrip BottomStrip
        {
            get => _bottomStrip;
            set
            {
                _bottomStrip = value;
                OnPropertyChanged(nameof(BottomStrip));
            }
        }

        public DisplayStrip TopStrip
        {
            get => _topStrip;
            set
            {
                _topStrip = value;
                OnPropertyChanged(nameof(TopStrip));
            }
        }

        public DisplayStrip VbdStrip
        {
            get => _vbdStrip;
            set
            {
                _vbdStrip = value;
                OnPropertyChanged(nameof(VbdStrip));
            }
        }

        private void MapStrips()
        {
            ResetStripDisplay();

            var stripInfo = new Dictionary<int, DisplayStrip>();

            foreach (var strip in _selectedMapping.HardwareStrips)
            {
                var leds = Enumerable.Range(0, strip.LedCount)
                    .Select(_ => new Led(Color.FromArgb(0, 0, 0, 0)))
                    .ToList();

                stripInfo.Add(strip.Id, new DisplayStrip(strip.Id, leds));
            }

            Strips = stripInfo;

            foreach (var strip in _selectedMapping.LogicalStrips)
            {
                var leds = new List<Led>();
                foreach (var ledSegment in strip.LedSegments)
                {
                    leds.AddRange(GetDisplayLedSegment(_strips[ledSegment.HardwareStripId], ledSegment));
                }

                var logicalStrip = new DisplayStrip(strip.Id, leds);

                switch ((StripIDs)strip.Id)
                {
                    case StripIDs.MainCabinetLeft or StripIDs.NormalTopperLeft:
                        LeftStrip = logicalStrip;
                        break;

                    case StripIDs.MainCabinetRight or StripIDs.NormalTopperRight:
                        RightStrip = logicalStrip;
                        break;

                    case StripIDs.MainCabinetBottom or StripIDs.NormalTopperBottom:
                        BottomStrip = logicalStrip;
                        break;

                    case StripIDs.MainCabinetTop or StripIDs.NormalTopperTop:
                        TopStrip = logicalStrip;
                        break;

                    case StripIDs.VbdBottomStrip:
                        VbdStrip = logicalStrip;
                        break;
                }
            }
        }

        private void ResetStripDisplay()
        {
            Strips = null;
            LeftStrip = null;
            RightStrip = null;
            BottomStrip = null;
            TopStrip = null;
            VbdStrip = null;
        }

        private List<Led> GetDisplayLedSegment(DisplayStrip strip, LedSegment ledSegment)
        {
            var ledList = strip.Leds.GetRange(ledSegment.StartLedIndex, ledSegment.LedCount);

            if (!ledSegment.IsForward)
            {
                ledList.Reverse();
            }

            return ledList;
        }

        public void RenderStrips(object sender, StripsRenderedEventArgs e)
        {
            var strips = e.Strips;
            if (_strips == null) return;

            foreach (var strip in strips)
            {
                for (var i = 0; i < strip.LedCount; i++)
                {
                    var argb = strip.ColorBuffer[i];
                    var color = Color.FromArgb(
                        (byte)((double)strip.Brightness / 100 * 255),
                        argb.R,
                        argb.G,
                        argb.B);

                    _strips[strip.StripId].Leds[i].Color = color;
                }
            }
        }

        public class DisplayStrip
        {
            internal DisplayStrip(int id, List<Led> leds)
            {
                Id = id;
                Leds = leds;
            }

            public int Id { get; }

            public string HexId => $"0x{Id:X}";

            public List<Led> Leds { get; }
        }

        public class Led : ObservableObject
        {
            private Color _color;
            internal Led(Color color)
            {
                Color = color;
            }

            public Color Color
            {
                get => _color;
                set
                {
                    _color = value;
                    OnPropertyChanged(nameof(Color));
                }
            }
        }
    }
}
