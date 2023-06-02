namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Linq;
    using System.Reflection;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Capabilities;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Command handler for the <see cref="PrepareLightShowAnimations" /> command.
    /// </summary>
    public class PrepareLightShowAnimationsCommandHandler : ICommandHandler<PrepareLightShowAnimations>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IReelAnimationCapabilities _animationCapabilities;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SpinReelsCommandHandler" /> class.
        /// </summary>
        public PrepareLightShowAnimationsCommandHandler()
        {
            var reelController = ServiceManager.GetInstance().TryGetService<IReelController>();

            if (reelController?.HasCapability<IReelAnimationCapabilities>() ?? false)
            {
                _animationCapabilities = reelController.GetCapability<IReelAnimationCapabilities>();
            }
        }

        /// <inheritdoc />
        public void Handle(PrepareLightShowAnimations command)
        {
            Logger.Debug("Handle PrepareLightShowAnimations command");

            if (_animationCapabilities is not null)
            {
                foreach (var showData in command.LightShowData)
                {
                    showData.Id = _animationCapabilities.AnimationFiles.First(x => x.Name == showData.AnimationName).AnimationId;
                }

                var result = _animationCapabilities.PrepareControllerAnimations(command.LightShowData, default);
                command.Success = result.Result;
                return;
            }

            command.Success = false;
        }
    }
}