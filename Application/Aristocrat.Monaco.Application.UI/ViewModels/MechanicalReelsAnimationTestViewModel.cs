namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Capabilities;
    using Hardware.Contracts.Reel.ControlData;
    using Models;
    using Monaco.Common;
    using Monaco.Localization.Properties;
    using MVVM.Command;

    /// <summary>
    ///     The MechanicalReelsAnimationTestViewModel class
    /// </summary>
    [CLSCompliant(false)]
    public class MechanicalReelsAnimationTestViewModel : INotifyPropertyChanged
    {
        private const string SampleStepperCurve = "SampleStepperCurve";
        private const int StopReelsMs = 50;
        private const int DefaultRpm = 20;

        private const int MinStop = 0;
        private const int MaxStop = 21;
        private const int MiddleStop = 11;
        private const int MiddleStep = 105;
        private const int InitialOffSet = 5;
        private const int StepsPerStop = 9;
        private const int WrapAroundSteps = 10;

        private readonly IReelAnimationCapabilities _animationCapabilities;
        private readonly IReelController _reelController;
        private readonly Action _updateScreenCallback;
        private ObservableCollection<ReelInfoItem> _reelInfo;
        private bool _checkHasFault = true;
        private bool _homeEnabled = true;
        private bool _nudgeEnabled;
        private bool _spinEnabled;
        private bool _allReelsIdle;
        private bool _allReelsIdleUnknown;
        private bool _testActive;

        /// <summary>
        ///     Instantiates a new instance of the MechanicalReelsAnimationTestViewModel class
        /// </summary>
        /// <param name="reelController">The reel controller</param>
        /// <param name="reelInfo">Collection of information about each reel</param>
        /// <param name="updateScreenCallback">call back to update the screen</param>
        public MechanicalReelsAnimationTestViewModel(
            IReelController reelController,
            ObservableCollection<ReelInfoItem> reelInfo,
            Action updateScreenCallback)
        {
            _reelController =
                reelController ?? throw new ArgumentNullException(nameof(reelController));

            if (_reelController.HasCapability<IReelAnimationCapabilities>())
            {
                _animationCapabilities = _reelController.GetCapability<IReelAnimationCapabilities>();
            }

            HomeCommand = new ActionCommand<object>(_ => ReelHomeTest().FireAndForget());
            NudgeCommand = new ActionCommand<object>(_ => ReelNudgeTest().FireAndForget());
            PlayStepperCurveCommand = new ActionCommand<object>(_ => StepperCurveTest().FireAndForget());
            StopCommand = new ActionCommand<object>(_ => PrepareStopReels().FireAndForget());
            _reelInfo = reelInfo;
            _updateScreenCallback = updateScreenCallback;
        }

        /// <summary>
        ///     Command for the nudge button
        /// </summary>
        public ICommand NudgeCommand { get; }

        /// <summary>
        ///     Command for the stop button
        /// </summary>
        public ICommand StopCommand { get; }

        /// <summary>
        ///     Command for the home button
        /// </summary>
        public ICommand HomeCommand { get; }

        /// <summary>
        ///     Command for the spin button
        /// </summary>
        public ICommand PlayStepperCurveCommand { get; }

        /// <summary>
        ///     Collection of reel info
        /// </summary>
        public ObservableCollection<ReelInfoItem> ReelInfo
        {
            get => _reelInfo;

            set
            {
                _reelInfo = value;
                RaisePropertyChanged(nameof(ReelInfo));
            }
        }

        /// <summary>
        ///     Occurs when a property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Index of the stepper curve to play
        /// </summary>
        public int SelectedStepperCurveIndex { get; set; } = 0;

        /// <summary>
        ///     This list of friendly names of sample stepper curves 
        /// </summary>
        public IEnumerable<string> SampleStepperCurves { get; } = new[] { SampleStepperCurve };

        /// <summary>
        ///     Controls if the simulator reels are visible
        /// </summary>
        public bool ReelsVisible { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating of the test is active
        /// </summary>
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
                RaisePropertyChanged(nameof(TestActive));
                RaisePropertyChanged(nameof(StopEnabled));
                RaisePropertyChanged(nameof(NudgeEnabled));
                RaisePropertyChanged(nameof(HomeEnabled));
            }
        }

        /// <summary>
        ///     Controls if the home button is enabled
        /// </summary>
        public bool HomeEnabled
        {
            get => _homeEnabled && !TestActive && (AllReelsIdle || AllReelsIdleUnknown)
                   || HasFault
                   || _reelController.LogicalState == ReelControllerState.Disabled;

            set
            {
                if (_homeEnabled == value)
                {
                    return;
                }

                _homeEnabled = value;
                RaisePropertyChanged(nameof(HomeEnabled));
            }
        }

        /// <summary>
        ///     Controls if the nudge button is enabled
        /// </summary>
        public bool NudgeEnabled
        {
            get => _nudgeEnabled && AllReelsIdle && !HasFault && !TestActive && _animationCapabilities is not null;

            set
            {
                if (_nudgeEnabled == value)
                {
                    return;
                }

                _nudgeEnabled = value;
                RaisePropertyChanged(nameof(NudgeEnabled));
            }
        }

        /// <summary>
        ///     Controls if the stop button is enabled
        /// </summary>
        public bool StopEnabled => TestActive;

        /// <summary>
        ///     Controls if the play stepper curve button is enabled
        /// </summary>
        public bool PlayCurveEnabled
        {
            get => _spinEnabled && AllReelsIdle && !HasFault && !TestActive && _animationCapabilities is not null;

            set
            {
                if (_spinEnabled == value)
                {
                    return;
                }

                _spinEnabled = value;
                RaisePropertyChanged(nameof(PlayCurveEnabled));
            }
        }

        /// <summary>
        ///     Indicates that a fault exists, buttons will not be enabled
        /// </summary>
        public bool HasFault => _reelController.LogicalState == ReelControllerState.Tilted && _checkHasFault;

        private int ReelsCount => _reelController.ConnectedReels.Count;

        private bool AllReelsIdle
        {
            get => _allReelsIdle;
            set
            {
                if (_allReelsIdle == value)
                {
                    return;
                }

                _allReelsIdle = value;
                RaisePropertyChanged(nameof(NudgeEnabled));
                RaisePropertyChanged(nameof(PlayCurveEnabled));
                RaisePropertyChanged(nameof(HomeEnabled));
            }
        }

        private bool AllReelsIdleUnknown
        {
            get => _allReelsIdleUnknown;
            set
            {
                if (_allReelsIdleUnknown == value)
                {
                    return;
                }

                _allReelsIdleUnknown = value;
                RaisePropertyChanged(nameof(HomeEnabled));
            }
        }

        /// <summary>
        ///     Updates the reel info and buttons on screen
        /// </summary>
        public void UpdateScreen()
        {
            RaisePropertyChanged(nameof(ReelInfo));

            AllReelsIdle = ReelInfo.All(x => x.State == Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Idle));
            AllReelsIdleUnknown = ReelInfo.All(x => x.State == Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReelController_IdleUnknown));

            RaisePropertyChanged(nameof(HomeEnabled));
            RaisePropertyChanged(nameof(NudgeEnabled));
            RaisePropertyChanged(nameof(PlayCurveEnabled));
        }

        private bool IsReelActive(int reel) => ReelInfo.Any(o => o.Id == reel);

        private AnimationFile GetSelectedStepperAnimation() =>
            _animationCapabilities.AnimationFiles.First(
                x => x.FriendlyName == SampleStepperCurves.ElementAt(SelectedStepperCurveIndex));

        private ReelInfoItem GetActiveReel(int reel) => ReelInfo.First(o => o.Id == reel);

        private async Task ReelHomeTest()
        {
            _checkHasFault = false;
            HomeEnabled = false;
            PlayCurveEnabled = false;
            NudgeEnabled = false;
            AllReelsIdle = false;
            AllReelsIdleUnknown = false;

            ClearReelsSteps();

            Dictionary<int, int> homeData = new();

            for (var i = 1; i <= ReelsCount; ++i)
            {
                if (!IsReelActive(i))
                {
                    continue;
                }

                var activeReel = GetActiveReel(i);
                homeData.Add(i, _reelController.ReelHomeSteps[i]);

                activeReel.IsHoming = true;
                activeReel.IsSpinning = false;
                activeReel.IsNudging = false;
            }

            await _reelController.HomeReels(homeData);

            HomeEnabled = true;
            NudgeEnabled = true;
            PlayCurveEnabled = true;
            _checkHasFault = true;

            _updateScreenCallback();
        }
        
        private async Task ReelNudgeTest()
        {
            var nudgeData = new List<NudgeReelData>();

            NudgeEnabled = false;

            for (var i = 1; i <= ReelsCount; ++i)
            {
                if (!IsReelActive(i))
                {
                    continue;
                }

                var activeReel = GetActiveReel(i);

                if (!activeReel.Enabled)
                {
                    continue;
                }

                var spinDirection = activeReel.DirectionToNudge ? SpinDirection.Forward : SpinDirection.Backwards;
                var currentStop = StepsToStop(_reelController.Steps[i]);
                var stepsToNudge = GetStepsToNextStop(currentStop, spinDirection);

                nudgeData.Add(new NudgeReelData(
                    i - 1,
                    spinDirection,
                    stepsToNudge,
                    DefaultRpm
                ));

                activeReel.IsHoming = false;
                activeReel.IsSpinning = false;
                activeReel.IsNudging = true;
            }

            await _animationCapabilities.PrepareNudgeReels(nudgeData);
            await _animationCapabilities.PlayAnimations();

            NudgeEnabled = true;
        }

        private async Task StepperCurveTest()
        {
            TestActive = true;
            var stepperCurveData = new List<ReelCurveData>();

            for (var i = 1; i <= ReelsCount; ++i)
            {
                if (!IsReelActive(i))
                {
                    continue;
                }

                var activeReel = GetActiveReel(i);

                if (!activeReel.Enabled)
                {
                    continue;
                }

                stepperCurveData.Add(new ReelCurveData
                (
                    (byte)(i - 1),
                    GetSelectedStepperAnimation().FriendlyName
                ));

                activeReel.IsHoming = false;
                activeReel.IsSpinning = true;
                activeReel.IsNudging = false;
            }

            UpdateScreen();

            await _animationCapabilities.PrepareAnimations(stepperCurveData);
            await _animationCapabilities.PlayAnimations();
        }

        private async Task PrepareStopReels()
        {
            var stopData = new List<ReelStopData>();

            for (var i = 1; i <= ReelsCount; ++i)
            {
                if (!IsReelActive(i))
                {
                    continue;
                }

                var activeReel = GetActiveReel(i);

                if (!activeReel.Enabled)
                {
                    continue;
                }

                stopData.Add(new ReelStopData ((byte)(i - 1), StopReelsMs, (short)StopToSteps(activeReel.StopIndex - 1)));

                activeReel.IsHoming = false;
                activeReel.IsSpinning = false;
                activeReel.IsNudging = false;
            }

            await _animationCapabilities.PrepareStopReels(stopData);
            await _animationCapabilities.PlayAnimations();

            TestActive = false;
        }

        /// <summary>
        ///     Cancels the light show test
        /// </summary>
        public void CancelTest()
        {
            if (!_reelController.Connected || _animationCapabilities is null || !TestActive)
            {
                return;
            }

            var stopData = new List<ReelStopData>();

            for (var i = 1; i <= ReelsCount; ++i)
            {
                stopData.Add(new ReelStopData((byte)(i - 1), StopReelsMs, 0));
            }

            _animationCapabilities.PrepareStopReels(stopData);
            _animationCapabilities.PlayAnimations();
            TestActive = false;
        }

        private static int StepsToStop(int step)
        {
            if (step >= MiddleStep)
            {
                step--;	
            }
		
            var stop = (step - 1) / StepsPerStop;
            return stop;
        }
	
        private static int StopToSteps(int stop)
        {
            var steps = (stop * StepsPerStop) + InitialOffSet;
		
            if (stop >= MiddleStop)
            {
                // We need to add 1 for this special case where there are 10 steps in between
                steps++;
            }
		
            return steps;
        }
	
        private static int GetNextStop(int currentStop, SpinDirection direction)
        {
            if (currentStop == MaxStop && direction == SpinDirection.Forward)
            {
                return MinStop;
            }
		
            if (currentStop == MinStop && direction == SpinDirection.Backwards)
            {
                return MaxStop;	
            }

            if (direction == SpinDirection.Forward)
            {
                currentStop++;
            }
            else
            {
                currentStop--;
            }

            return currentStop;
        }

        private static int GetStepsToNextStop(int currentStop, SpinDirection direction)
        {
            var newStop = GetNextStop(currentStop, direction);
		
            if ((newStop == MinStop && direction == SpinDirection.Forward) || (newStop == MaxStop && direction == SpinDirection.Backwards))
            {
                return WrapAroundSteps;
            }
		
            var currentStep = StopToSteps(currentStop);
            var newStep = StopToSteps(newStop);
            return Math.Abs(currentStep - newStep);
        }

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RaisePropertyChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                OnPropertyChanged(propertyName);
            }
        }

        private void ClearReelsSteps()
        {
            for (var i = 1; i <= ReelsCount; ++i)
            {
                if (!IsReelActive(i))
                {
                    continue;
                }

                var activeReel = GetActiveReel(i);
                if (activeReel.Enabled && activeReel.Connected)
                {
                    activeReel.Step = string.Empty;
                }
            }

            RaisePropertyChanged(nameof(ReelInfo));
        }
    }
}
