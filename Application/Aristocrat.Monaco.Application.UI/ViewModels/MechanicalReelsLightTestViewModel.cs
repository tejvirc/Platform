namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Aristocrat.Monaco.Localization.Properties;
    using CommunityToolkit.Mvvm.Input;
    using Contracts.Localization;
    using Hardware.Contracts.EdgeLighting;
    using Hardware.Contracts.Reel;

    [CLSCompliant(false)]
    public class MechanicalReelsLightTestViewModel : INotifyPropertyChanged, IDisposable
    {
        private const int FlashIntervalMs = 100;
        private const int TestDurationMs = 3000;

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

        private IEdgeLightToken _offToken;
        private IEdgeLightToken _pattenToken;
        private bool _initialized;
        private List<int> _reelLightIdentifiers;
        private int _lightsPerReel;
        private bool _buttonEnabled;
        private bool _disposed;
        private CancellationTokenSource _cancellationTokenSource = new();

        public MechanicalReelsLightTestViewModel(
            IReelController reelController,
            IEdgeLightingController edgeLightingController)
        {
            _reelController =
                reelController ?? throw new ArgumentNullException(nameof(reelController));
            _edgeLightingController =
                edgeLightingController ?? throw new ArgumentNullException(nameof(edgeLightingController));

            FlashReelLightsCommand = new RelayCommand<object>(_ => Task.Run(FlashReelLights));
            InitializeLightIdList();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool ButtonEnabled
        {
            get => _buttonEnabled;
            set
            {
                if (_buttonEnabled == value)
                {
                    return;
                }

                _buttonEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonEnabled)));
            }
        }

        public ICommand FlashReelLightsCommand { get; }

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
            if (_cancellationTokenSource == null)
            {
                return;
            }

            _cancellationTokenSource.Cancel(true);
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                CancelTest();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            _disposed = true;
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

        private async Task FlashReelLights()
        {
            if (!_reelController.Connected)
            {
                return;
            }

            ButtonEnabled = false;

            TurnLightsOff();
            StartFlashing();
            await WaitForTestComplete();
        }

        private async void InitializeLightIdList()
        {
            if (!_reelController.Connected || _initialized)
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
            ButtonEnabled = true;
            _initialized = true;
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

            ClearPattern(ref _pattenToken);
            _pattenToken = _edgeLightingController.AddEdgeLightRenderer(pattern);
        }

        private void TurnLightsOff()
        {
            ClearPattern(ref _offToken);
            _offToken = _edgeLightingController.AddEdgeLightRenderer(_solidBlackPattern);
        }

        private async Task WaitForTestComplete()
        {
            try
            {
                var token = _cancellationTokenSource.Token;
                token.ThrowIfCancellationRequested();
                await Task.Delay(TestDurationMs, token);
            }
            catch (OperationCanceledException)
            {
                // Do nothing
            }
            finally
            {
                ClearPattern(ref _pattenToken);
                ClearPattern(ref _offToken);

                ButtonEnabled = true;
            }
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