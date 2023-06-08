namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using Aristocrat.MVVM.Command;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Capabilities;
    using Hardware.Contracts.Reel.ControlData;
    using Monaco.Common;
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    ///     The MechanicalReelsAnimationTestViewModel class
    /// </summary>
    public class MechanicalReelsAnimationTestViewModel : INotifyPropertyChanged
    {
        private readonly string stepperCurveName = "SampleStepperCurve";

        private readonly IReelAnimationCapabilities _animationCapabilities;
        private readonly IReelController _reelController;

        /// <summary>
        ///     Instantiates a new instance of the MechanicalReelsLightAnimationTestViewModel class
        /// </summary>
        /// <param name="reelController">The reel controller</param>
        public MechanicalReelsAnimationTestViewModel(IReelController reelController)
        {
            _reelController =
                reelController ?? throw new ArgumentNullException(nameof(reelController));

            if (_reelController.HasCapability<IReelAnimationCapabilities>())
            {
                _animationCapabilities = _reelController.GetCapability<IReelAnimationCapabilities>();
            }

            HomeTest = new ActionCommand<object>(_ => ReelHomeTest().FireAndForget());
            NudgeTest = new ActionCommand<object>(_ => ReelNudgeTest().FireAndForget());
            StepperTest = new ActionCommand<object>(_ => StepperCurveTest().FireAndForget());
            StopReelsTest = new ActionCommand<object>(_ => PrepareStopReels().FireAndForget());
        }

        public ICommand NudgeTest { get; }
        public ICommand HomeTest { get; }
        public ICommand StepperTest { get; }
        public ICommand StopReelsTest { get; }
#pragma warning disable 67
        /// <summary>
        ///     Occurs when a property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67

        private async Task ReelHomeTest()
        {
            await _reelController.HomeReels();
        }

        private async Task ReelNudgeTest()
        {
            await _animationCapabilities.PrepareControllerNudgeReels(new NudgeReelData[]
            {
                new ( 0, SpinDirection.Forward, 4, 20),
                new ( 1, SpinDirection.Forward, 4, 20),
                new ( 2, SpinDirection.Forward, 4, 20),
                new ( 3, SpinDirection.Forward, 4, 20),
                new ( 4, SpinDirection.Forward, 4, 20)
            });
            await _animationCapabilities.PlayAnimations();
        }

        private async Task StepperCurveTest()
        {
            var stepperCurveData = new ReelCurveData()
            {
                AnimationName = stepperCurveName,
                ReelIndex = 0
            };

            await _animationCapabilities.PrepareControllerAnimation(stepperCurveData, default);

            await _animationCapabilities.PlayAnimations();
        }

        private async Task PrepareStopReels()
        {
            await _animationCapabilities.PrepareStopReels(
                new[]
                {
                    new ReelStopData{ReelIndex = 0, Duration = 100, Step = 0},
                    new ReelStopData{ReelIndex = 1, Duration = 100, Step = 0},
                    new ReelStopData{ReelIndex = 2, Duration = 100, Step = 0},
                    new ReelStopData{ReelIndex = 3, Duration = 100, Step = 0},
                    new ReelStopData{ReelIndex = 4, Duration = 100, Step = 0}
                }, default);

            await _animationCapabilities.PlayAnimations();
        }
    }
}
