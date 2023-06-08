namespace Aristocrat.Monaco.Gaming.Commands
{
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Aristocrat.Monaco.Hardware.Contracts.Reel.Capabilities;
    using Aristocrat.Monaco.Kernel;

    /// <summary>
    ///     Command handler for the <see cref="PrepareStepperCurves" /> command.
    /// </summary>
    public class PrepareStepperCurvesCommandHandler : ICommandHandler<PrepareStepperCurves>
    {
        private readonly IReelAnimationCapabilities _animationCapabilities;
        
        public PrepareStepperCurvesCommandHandler()
        {
            var reelController = ServiceManager.GetInstance().TryGetService<IReelController>();

            if (reelController?.HasCapability<IReelAnimationCapabilities>() ?? false)
            {
                _animationCapabilities = reelController.GetCapability<IReelAnimationCapabilities>();
            }
        }

        /// <inheritdoc />
        public void Handle(PrepareStepperCurves command)
        {
            var success = _animationCapabilities.PrepareControllerAnimations(command.StepperCurvesData).Result;

            command.Success = success;
        }
    }
}
