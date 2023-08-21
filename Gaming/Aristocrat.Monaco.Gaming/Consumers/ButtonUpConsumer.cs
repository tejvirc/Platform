namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts;
    using Hardware.Contracts.Button;
    using Kernel;
    using Runtime;
#if (RETAIL)
    using Application.Contracts;
#endif

    /// <summary>
    ///     Handles the UpEvent.
    /// </summary>
    public class ButtonUpConsumer : Consumes<UpEvent>
    {
        private readonly IGameService _gameService;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IRuntime _runtime;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ButtonUpConsumer" /> class.
        /// </summary>
        /// <param name="runtime">The runtime service</param>
        /// <param name="gameService">The game service</param>
        /// <param name="properties">The properties manager</param>
        public ButtonUpConsumer(IRuntime runtime, IGameService gameService, IPropertiesManager properties)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _propertiesManager = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <inheritdoc />
        public override void Consume(UpEvent theEvent)
        {
            // TODO: Some check that a game is active.  These virtual keys should only be enabled if a game is launched.

            // TEMP: For now, filter the keys we want to pass on to the game.
            if (!_gameService.Running)
            {
                return;
            }

#if (RETAIL) // Filter out the jackpot key, since it launches the show program (Let the jackpot key through if we are in Show Mode)
            if (theEvent.LogicalId == (int)ButtonLogicalId.Button30)
            {
                if (!(bool)_propertiesManager.GetProperty(ApplicationConstants.ShowMode, false))
                {
                    return;
                }
            }
#endif

            if (theEvent.LogicalId > (int)ButtonLogicalId.ButtonBase &&
                theEvent.LogicalId <= (int)ButtonLogicalId.MaxButtonId)
            {
                // Set the button state to released
                _runtime.InvokeButton((uint)theEvent.LogicalId - (int)ButtonLogicalId.ButtonBase, 0);
            }
        }
    }
}
