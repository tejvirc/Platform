namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using Aristocrat.Monaco.Localization.Properties;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Contracts.ConfigWizard;
    using Contracts.Localization;
    using Hardware.Contracts.EdgeLighting;
    using Hardware.Contracts.Reel;

    [CLSCompliant(false)]
    public class MechanicalReelsLightTestViewModel : ObservableObject
    {
        private const int FlashIntervalMs = 100;

        private static readonly Color OffColor = Color.Black;

        private static readonly int[] AllStripIds =
        {
            (int)StripIDs.StepperReel1,
            (int)StripIDs.StepperReel2,
            (int)StripIDs.StepperReel3,
            (int)StripIDs.StepperReel4,
            (int)StripIDs.StepperReel5
        };

        private readonly PatternParameters _solidBlackPattern = new SolidColorPatternParameters
        {
            Color = OffColor,
            Priority = StripPriority.PlatformTest,
            Strips = AllStripIds
        };

        private readonly IReelController _reelController;
        private readonly IEdgeLightingController _edgeLightingController;
        private readonly IInspectionService _reporter;

        private IEdgeLightToken _offToken;
        private IEdgeLightToken _patternToken;
        private bool _initialized;
        private List<int> _reelLightIdentifiers;
        private int _lightsPerReel;
        private bool _testActive;

        public MechanicalReelsLightTestViewModel(
            IReelController reelController,
            IEdgeLightingController edgeLightingController,
            IInspectionService reporter)
        {
            _reelController =
                reelController ?? throw new ArgumentNullException(nameof(reelController));
            _edgeLightingController =
                edgeLightingController ?? throw new ArgumentNullException(nameof(edgeLightingController));
            _reporter = reporter;

            InitializeLightIdList();
        }

        public bool Initialized
        {
            get => _initialized;

            private set
            {
                if (_initialized == value)
                {
                    return;
                }

                _initialized = value;
                OnPropertyChanged(nameof(Initialized));
            }
        }

        public bool TestActive
        {
            get => _testActive;

            set
            {
                if (_testActive == value)
                {
                    return;
                }

                _testActive = value;
                OnPropertyChanged(nameof(TestActive));

                if (value)
                {
                    StartTest();
                }
                else
                {
                    CancelTest();
                }
            }
        }

        public IReadOnlyCollection<string> ReelLightColors { get; } = new[]
        {
            "White", "Red", "Green", "Blue", "Yellow", "Orange", "Purple"
        };

        public List<string> ReelLightIdNames { get; set; }

        public int SelectedReelLightColorIndex { get; set; }

        public int SelectedReelLightIdIndex { get; set; }

        private Color SelectedColor => Color.FromName(ReelLightColors.ElementAt(SelectedReelLightColorIndex));

        public void CancelTest()
        {
            ClearPattern(ref _patternToken);
            ClearPattern(ref _offToken);
            TestActive = false;
        }

        private void ClearPattern(ref IEdgeLightToken token)
        {
            if (token == null)
            {
                return;
            }

            _edgeLightingController.RemoveEdgeLightRenderer(token);
            token = null;
        }

        private async void InitializeLightIdList()
        {
            if (!_reelController.Connected || Initialized)
            {
                return;
            }

            var lightText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MechanicalReels_Light);
            var allLightsText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MechanicalReels_AllLights);

            var ids = await _reelController.GetReelLightIdentifiers();

            ReelLightIdNames = new List<string> { allLightsText };
            _reelLightIdentifiers = new List<int>(ids);
            _lightsPerReel = _reelLightIdentifiers.Count / _reelController.ConnectedReels.Count;

            foreach (var id in _reelLightIdentifiers)
            {
                ReelLightIdNames.Add($"{lightText} {id}");
            }

            SelectedReelLightIdIndex = 0;
            Initialized = true;
        }

        private Color[] GetStripOnColors(int stripId, int ledCount)
        {
            var colors = new List<Color>();

            for (var i = 0; i < ledCount; i++)
            {
                if (SelectedReelLightIdIndex == 0)
                {
                    colors.Add(SelectedColor);
                }
                else
                {
                    var modValue = (SelectedReelLightIdIndex - 1) % ledCount;
                    colors.Add(modValue == i ? SelectedColor : OffColor);
                }
            }

            return colors.ToArray();
        }

        private int[] GetTargetedStrips()
        {
            if (SelectedReelLightIdIndex == 0)
            {
                return AllStripIds;
            }

            var targetedStrip = (SelectedReelLightIdIndex - 1) / _lightsPerReel;
            return new[] { AllStripIds[targetedStrip] };
        }

        private void StartFlashing()
        {
            var pattern = new IndividualLedBlinkPatternParameters
            {
                StripOnUpdateFunction = GetStripOnColors,
                StripOffUpdateFunction = GetStripOffColors,
                OnTime = FlashIntervalMs,
                OffTime = FlashIntervalMs,
                Strips = GetTargetedStrips(),
                Priority = StripPriority.PlatformTest
            };

            ClearPattern(ref _patternToken);
            _patternToken = _edgeLightingController.AddEdgeLightRenderer(pattern);
            var reels = SelectedReelLightIdIndex == 0 ? "all reels" : $"reel {(SelectedReelLightIdIndex - 1) / _lightsPerReel}";
            _reporter?.SetTestName($"Lights, {reels}");
        }

        private void StartTest()
        {
            if (!_reelController.Connected)
            {
                return;
            }

            TurnLightsOff();
            StartFlashing();
        }

        private void TurnLightsOff()
        {
            ClearPattern(ref _offToken);
            _offToken = _edgeLightingController.AddEdgeLightRenderer(_solidBlackPattern);
        }

        private static Color[] GetStripOffColors(int stripId, int ledCount)
        {
            var colors = new List<Color>();

            for (var i = 0; i < ledCount; i++)
            {
                colors.Add(OffColor);
            }

            return colors.ToArray();
        }
    }
}