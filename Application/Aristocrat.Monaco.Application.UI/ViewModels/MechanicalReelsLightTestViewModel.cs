namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Timers;
    using System.Windows.Input;
    using Aristocrat.Monaco.Localization.Properties;
    using Contracts.Localization;
    using Hardware.Contracts.Reel;
    using Monaco.Common;
    using MVVM.Command;

    [CLSCompliant(false)]
    public class MechanicalReelsLightTestViewModel
    {
        private const int FlashIntervalMs = 500;
        private const int MaximumFlashCycles = 6;

        private readonly IReelController _reelController;

        private bool _initialized;
        private bool _flashState;
        private Timer _flashTimer;
        private int _numberOfFlashCycles;
        private List<int> _reelLightIdentifiers;

        public MechanicalReelsLightTestViewModel(IReelController reelController)
        {
            _reelController = reelController;

            FlashReelLightsCommand = new ActionCommand<object>(_ => FlashReelLights());
            InitializeLightIdList();
        }

        public ICommand FlashReelLightsCommand { get; }

        public IReadOnlyCollection<string> ReelLightColors { get; } = new[]
        {
            "White", "Red", "Green", "Blue", "Yellow", "Orange", "Purple"
        };

        public List<string> ReelLightIdNames { get; set; }

        public int SelectedReelLightColorIndex { get; set; }

        public int SelectedReelLightIdIndex { get; set; }

        public void Unload()
        {
            StopFlashTimer();
        }

        private void FlashReelLights()
        {
            if (_reelController.Connected)
            {
                StartFlashTimer(FlashIntervalMs);
            }
        }

        private async void InitializeLightIdList()
        {
            if (_reelController.Connected && !_initialized)
            {
                var lightText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MechanicalReels_Light);
                var allLightsText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MechanicalReels_AllLights);

                var ids = await _reelController.GetReelLightIdentifiers();


                ReelLightIdNames = new List<string> { allLightsText };
                _reelLightIdentifiers = new List<int>(ids);

                foreach (var id in _reelLightIdentifiers)
                {
                    ReelLightIdNames.Add($"{lightText} {id}");
                }

                SelectedReelLightIdIndex = 0;
                _initialized = true;
            }
        }

        private void OnFlashTick(object sender, ElapsedEventArgs args)
        {
            _flashState = !_flashState;
            ++_numberOfFlashCycles;

            if (_numberOfFlashCycles <= MaximumFlashCycles)
            {
                SetFlashReelLights(_flashState);
            }
            else
            {
                StopFlashTimer();
            }
        }

        private void SetFlashReelLights(bool isLightOn)
        {
            var selectedLightsIndex = SelectedReelLightIdIndex;
            if (selectedLightsIndex == -1)
            {
                return;
            }

            IList<ReelLampData> reelLampData = new List<ReelLampData>();

            var color = Color.FromName(ReelLightColors.ElementAt(SelectedReelLightColorIndex));

            if (selectedLightsIndex == 0)
            {
                // Flash all lights
                foreach (var id in _reelLightIdentifiers)
                {
                    reelLampData.Add(new ReelLampData(color, isLightOn, id));
                }
            }
            else
            {
                // Flash the one light specified
                foreach (var id in _reelLightIdentifiers)
                {
                    reelLampData.Add(new ReelLampData(Color.Black, false, id));
                }

                // If the state is turning the lights on then set the appropriate lamp
                if (isLightOn)
                {
                    reelLampData[selectedLightsIndex - 1] = new ReelLampData(color, true, selectedLightsIndex);
                }
            }

            _reelController.SetLights(reelLampData.ToArray()).FireAndForget();
        }

        private void StartFlashTimer(int interval)
        {
            StopFlashTimer();

            _flashTimer = new Timer(interval) { AutoReset = true };
            _flashTimer.Elapsed += OnFlashTick;
            _flashTimer.Start();
        }

        private void StopFlashTimer()
        {
            _flashTimer?.Stop();
            _flashTimer?.Dispose();
            _flashTimer = null;
            _numberOfFlashCycles = 0;
            _flashState = false;
        }
    }
}