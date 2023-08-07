namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Reflection;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Capabilities;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Command handler for the <see cref="PrepareStepperRuleCommandHandler" /> command.
    /// </summary>
    public class PrepareStepperRuleCommandHandler : ICommandHandler<PrepareStepperRule>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IStepperRuleCapabilities _stepperRuleCapabilities;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrepareStepperRuleCommandHandler" /> class.
        /// </summary>
        public PrepareStepperRuleCommandHandler()
        {
            var reelController = ServiceManager.GetInstance().TryGetService<IReelController>();

            if (reelController?.HasCapability<IStepperRuleCapabilities>() ?? false)
            {
                _stepperRuleCapabilities = reelController.GetCapability<IStepperRuleCapabilities>();
            }
        }

        /// <inheritdoc />
        public void Handle(PrepareStepperRule command)
        {
            Logger.Debug("Handle PrepareStepperRule command");

            if (_stepperRuleCapabilities is not null)
            {
                var result = _stepperRuleCapabilities.PrepareStepperRule(command.StepperRuleData);
                command.Success = result.Result;
                return;
            }

            command.Success = false;
        }
    }
}
