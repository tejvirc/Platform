namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using Aristocrat.Extensions.CommunityToolkit;
    using Contracts.ConfigWizard;
    using Contracts.Localization;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using Monaco.Localization.Properties;
    using Monaco.UI.Common.Extensions;

    [CLSCompliant(false)]
    public class EdgeLightingTestViewModel : INotifyPropertyChanged
    {
        private static readonly StripData NoEdgeLighting = new StripData(-1);
        private static readonly Regex ParseCapitalizedWords = new Regex("(?!^)(?=[A-Z])");

        private readonly object _lock = new object();
        private readonly IEdgeLightingController _edgeLightingController;
        private readonly IEventBus _eventBus;

        private readonly string _selectedLedCountFormat =
            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LedSelectedCount);

        private int _brightness = EdgeLightingBrightnessLimits.MaximumBrightness;
        private IEdgeLightToken _token;
        private LightShow _selectedLightShow;
        private int _selectedColorIndex;
        private int _selectedStripIndex;
        private int _startLed;
        private int _endLed;
        private IInspectionService _reporter;

        public EdgeLightingTestViewModel()
            : this(
                ServiceManager.GetInstance().GetService<IEdgeLightingController>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        public EdgeLightingTestViewModel(IEdgeLightingController edgeLightingController, IEventBus eventBus)
        {
            _edgeLightingController =
                edgeLightingController ?? throw new ArgumentNullException(nameof(edgeLightingController));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public void SetTestReporter(IInspectionService reporter)
        {
            _reporter = reporter;
        }

        public ObservableCollection<StripData> Strips { get; } = new ObservableCollection<StripData>();

        public int Brightness
        {
            get => _brightness;
            set
            {
                _brightness = value;
                _edgeLightingController.SetBrightnessForPriority(_brightness, StripPriority.PlatformTest);
            }
        }

        public IReadOnlyCollection<string> StripColors { get; } = new[]
        {
            "Off", "White", "Red", "Green", "Blue", "Yellow", "Orange", "Purple"
        };

        public int SelectedColorIndex
        {
            get => _selectedColorIndex;
            set
            {
                if (value == _selectedColorIndex)
                {
                    return;
                }

                SetProperty(ref _selectedLightShow, LightShows.First(), nameof(SelectedLightShow));
                SetProperty(ref _selectedColorIndex, value, nameof(SelectedColorIndex));
                TestSelectionChanged();
            }
        }

        public int SelectedStripIndex
        {
            get => _selectedStripIndex;
            set
            {
                if (value == _selectedStripIndex)
                {
                    return;
                }

                SetProperty(ref _selectedLightShow, LightShows.First(), nameof(SelectedLightShow));
                SetProperty(ref _selectedStripIndex, value, nameof(SelectedStripIndex));
                SetProperty(ref _startLed, Math.Min(1, SelectedStrip.LedCount), nameof(StartLed));
                SetProperty(ref _endLed, SelectedStrip.LedCount, nameof(EndLed));
                OnPropertyChanged(nameof(SelectedStrip), nameof(SelectedLedCount));
                TestSelectionChanged();
            }
        }

        public IReadOnlyCollection<LightShow> LightShows { get; } = new List<LightShow>
        {
            new LightShow { ResourceKey = ResourceKeys.EdgeLightingTestClear, PatternParameters = null },
            new LightShow
            {
                ResourceKey = ResourceKeys.EdgeLightingTestRainbow,
                PatternParameters = new RainbowPatternParameters { Priority = StripPriority.PlatformTest }
            },
            new LightShow
            {
                ResourceKey = ResourceKeys.EdgeLightingTestChaser,
                PatternParameters = new ChaserPatternParameters { Priority = StripPriority.PlatformTest }
            },
            new LightShow
            {
                ResourceKey = ResourceKeys.EdgeLightingTestBlink,
                PatternParameters = new BlinkPatternParameters
                {
                    OnColor = Color.Red, OffColor = Color.Black, Priority = StripPriority.PlatformTest
                }
            }
        };

        public LightShow SelectedLightShow
        {
            get => _selectedLightShow;
            set
            {
                SetProperty(ref _selectedLightShow, value, nameof(SelectedLightShow));
                SetProperty(ref _selectedStripIndex, 0, nameof(SelectedStripIndex));
                SetProperty(ref _selectedColorIndex, 0, nameof(SelectedColorIndex));
                SetProperty(ref _startLed, Math.Min(1, SelectedStrip.LedCount), nameof(StartLed));
                SetProperty(ref _endLed, SelectedStrip.LedCount, nameof(EndLed));
                OnPropertyChanged(nameof(SelectedStrip), nameof(SelectedLedCount));
                TestSelectionChanged();
            }
        }

        public StripData SelectedStrip
        {
            get
            {
                lock (_lock)
                {
                    return Strips.ElementAtOrDefault(SelectedStripIndex) ?? NoEdgeLighting;
                }
            }
        }

        public int StartLed
        {
            get => _startLed;
            set
            {
                SetProperty(ref _startLed, value, nameof(StartLed));
                OnPropertyChanged(nameof(SelectedLedCount));
                TestSelectionChanged();
            }
        }

        public int EndLed
        {
            get => _endLed;
            set
            {
                SetProperty(ref _endLed, value, nameof(EndLed));
                OnPropertyChanged(nameof(SelectedLedCount));
                TestSelectionChanged();
            }
        }

        public string SelectedLedCount => string.Format(_selectedLedCountFormat, StartLed, EndLed);

        public bool TestMode
        {
            set
            {
                if (value)
                {
                    Initialize();
                }
                else
                {
                    UnInitialize();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (field == null && value == null)
            {
                return;
            }

            field = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                OnPropertyChanged(propertyName);
            }
        }

        private void Initialize()
        {
            UnInitialize();
            _eventBus.Subscribe<EdgeLightingStripsChangedEvent>(this, HandleStripChanged);
            HandleStripChanged(null);
            SelectedLightShow = LightShows.First();
            _edgeLightingController.SetBrightnessForPriority(Brightness, StripPriority.PlatformTest);
        }

        private void UnInitialize()
        {
            RemoveRenderer();
            _eventBus.UnsubscribeAll(this);
        }

        private void RemoveRenderer()
        {
            if (_token == null)
            {
                return;
            }

            _edgeLightingController.RemoveEdgeLightRenderer(_token);
            _token = null;
        }

        private void HandleStripChanged(EdgeLightingStripsChangedEvent obj)
        {
            var stripsData =
                _edgeLightingController.StripIds.Select(
                    x => new StripData(x, _edgeLightingController.GetStripLedCount(x)));
            Execute.OnUIThread(
                () =>
                {
                    lock (_lock)
                    {
                        var varCurrentString = SelectedStrip;
                        Strips.Clear();
                        Strips.Add(new StripData(-1));
                        Strips.AddRange(stripsData);
                        var currentIndex = Strips.IndexOf(varCurrentString);
                        SelectedStripIndex = currentIndex < 0 ? 0 : currentIndex;
                    }
                });
        }

        private void TestSelectionChanged()
        {
            RemoveRenderer();
            if (SelectedLightShow?.PatternParameters != null)
            {
                RunTestPatternChoice();
            }
            else
            {
                RunIndividualLedTest();
            }
        }

        private void RunIndividualLedTest()
        {
            var color = SelectedColorIndex == 0
                ? Color.Black
                : Color.FromName(StripColors.ElementAt(SelectedColorIndex));

            if (SelectedStripIndex == 0)
            {
                _reporter?.SetTestName($"Strips Color {color.Name}");
                _token = _edgeLightingController.AddEdgeLightRenderer(
                    new SolidColorPatternParameters { Priority = StripPriority.PlatformTest, Color = color });
            }
            else
            {
                _reporter?.SetTestName($"Strip {SelectedStrip.StripId}, LEDs {StartLed - 1}-{EndLed}, multiple colors");
                _token = _edgeLightingController.AddEdgeLightRenderer(
                    new IndividualLedPatternParameters
                    {
                        Priority = StripPriority.PlatformTest,
                        StripUpdateFunction = GetStripColors,
                        Strips = new[] { SelectedStrip.StripId }
                    });
            }
        }

        private Color[] GetStripColors(int stripId, int ledCount)
        {
            var colorIndex = SelectedColorIndex;
            var color = colorIndex == 0 ? Color.Black : Color.FromName(StripColors.ElementAt(colorIndex));
            return Enumerable.Range(0, ledCount)
                .Select(x => x >= StartLed - 1 && x < EndLed ? color : Color.Black).ToArray();
        }

        private void RunTestPatternChoice()
        {
            _reporter?.SetTestName($"Pattern {SelectedLightShow.ResourceKey}");
            _token = _edgeLightingController.AddEdgeLightRenderer(SelectedLightShow.PatternParameters);
        }

        private bool HasEdgeLightStrip(int stripId)
        {
            return Strips.Any(s => s.StripId == stripId);
        }

        public class StripData
        {
            public StripData(int stripId, int ledCount = 0)
            {
                StripId = stripId;
                LedCount = ledCount;
            }

            public string StripName =>
                StripId < 0 ?
                Localizer.For(CultureFor.Operator).GetString(Resources.All) :
                $"{StripId} (0x{StripId:X2})  {GetReadableName((StripIDs)StripId)}";

            public int StripId { get; }

            public int LedCount { get; }

            public static bool operator ==(StripData left, StripData right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(StripData left, StripData right)
            {
                return !Equals(left, right);
            }

            public override bool Equals(object obj)
            {
                return obj != null &&
                       (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals(obj as StripData));
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (StripId * 397) ^ LedCount;
                }
            }

            protected bool Equals(StripData other)
            {
                return StripId == other?.StripId && LedCount == other.LedCount;
            }

            private string GetReadableName(StripIDs id)
            {
                var name = Enum.GetName(typeof(StripIDs), id) ??
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UnknownStripIdEnumeration);
                return ParseCapitalizedWords.Replace(name, " ");
            }
        }

        public class LightShow
        {
            public string ResourceKey { get; set; }

            public string Name => Localizer.For(CultureFor.Operator).GetString(ResourceKey);

            public PatternParameters PatternParameters { get; set; }
        }
    }
}
