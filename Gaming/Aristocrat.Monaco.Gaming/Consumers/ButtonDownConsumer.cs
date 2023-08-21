namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
#if (RETAIL)
    using Application.Contracts;
#endif
    using Contracts;
    using Contracts.Barkeeper;
    using Hardware.Contracts.Button;
    using Kernel;
    using Runtime;

    /// <summary>
    ///     Handles the DownEvent.
    /// </summary>
    public class ButtonDownConsumer : Consumes<DownEvent>
    {
        private readonly IGameService _gameService;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IBarkeeperHandler _barkeeperHandler;
        private readonly IRuntime _runtime;

        public ButtonDownConsumer(
            IRuntime runtimeService,
            IGameService gameService,
            IPropertiesManager properties,
            IBarkeeperHandler barkeeperHandler)
        {
            _runtime = runtimeService ?? throw new ArgumentNullException(nameof(runtimeService));
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _propertiesManager = properties ?? throw new ArgumentNullException(nameof(properties));
            _barkeeperHandler = barkeeperHandler ?? throw new ArgumentNullException(nameof(barkeeperHandler));
        }

        public override void Consume(DownEvent theEvent)
        {
            // TODO: Some check that a game is active.  These virtual keys should only be enabled if a game is launched.
            if (theEvent.LogicalId == (int)ButtonLogicalId.Barkeeper)
            {
                _barkeeperHandler.BarkeeperButtonPressed();
            }

            // TEMP: For now, filter the keys we want to pass on to the game.
            if (!_gameService.Running || !_runtime.Connected)
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

            if (theEvent.LogicalId <= (int)ButtonLogicalId.ButtonBase ||
                theEvent.LogicalId > (int)ButtonLogicalId.MaxButtonId)
            {
                return;
            }

            // Set the state to pressed
            _runtime.InvokeButton((uint)theEvent.LogicalId - (int)ButtonLogicalId.ButtonBase, 1);
        }
    }
}