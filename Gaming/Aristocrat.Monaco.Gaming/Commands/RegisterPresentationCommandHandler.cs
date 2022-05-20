namespace Aristocrat.Monaco.Gaming.Commands
{
    using Contracts;
    using log4net;
    using System;
    using System.Reflection;

    public class RegisterPresentationCommandHandler : ICommandHandler<RegisterPresentation>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IOverlayMessageStrategyController _overlayMessageStrategyController;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RegisterPresentationCommandHandler" /> class.
        /// </summary>
        public RegisterPresentationCommandHandler(IOverlayMessageStrategyController overlayMessageStrategyController)
        {
            _overlayMessageStrategyController = overlayMessageStrategyController ??
                                                throw new ArgumentNullException(
                                                    nameof(overlayMessageStrategyController));
        }
        
        /// <inheritdoc />
        public void Handle(RegisterPresentation command)
        {
            Logger.Debug("RegisterPresentation Handle entered");
            var result = _overlayMessageStrategyController.RegisterPresentation(command.Result, command.TypeData);

            command.Success = result.Result;
        }
    }
}
