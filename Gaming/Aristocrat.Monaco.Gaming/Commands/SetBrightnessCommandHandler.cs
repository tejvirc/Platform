namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Reflection;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Aristocrat.Monaco.Hardware.Contracts.Reel.Capabilities;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Command handler for the <see cref="SetBrightness" /> command.
    /// </summary>
    public class SetBrightnessCommandHandler : ICommandHandler<SetBrightness>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private const float MaxBrightness = 100;

        private readonly IReelBrightnessCapabilities _brightnessCapabilities;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetBrightnessCommandHandler" /> class.
        /// </summary>
        public SetBrightnessCommandHandler()
        {
            var reelController = ServiceManager.GetInstance().TryGetService<IReelController>();

            if (reelController?.HasCapability<IReelBrightnessCapabilities>() ?? false)
            {
                _brightnessCapabilities = reelController.GetCapability<IReelBrightnessCapabilities>();
            }
        }

        /// <inheritdoc />
        public void Handle(SetBrightness command)
        {
            Logger.Debug("Handle SetBrightness command");

            if (_brightnessCapabilities is not null)
            {
                var gameBrightness = command.Brightness / MaxBrightness;
                var result = _brightnessCapabilities.SetBrightness((int)(gameBrightness * _brightnessCapabilities.DefaultReelBrightness));
                command.Success = result.Result;
                return;
            }

            command.Success = false;
        }
    }
}
