namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts;
    using Hardware.Contracts.ButtonDeck;
    using Kernel;

    /// <summary>
    ///     Command handler for the <see cref="UpdateButtonDeckImage" /> command.
    /// </summary>
    public class UpdateButtonDeckImageCommandHandler : ICommandHandler<UpdateButtonDeckImage>
    {
        //private readonly IButtonDeckDisplay _buttonDeckDisplay;
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly ISystemDisableManager _systemDisableManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UpdateButtonDeckImageCommandHandler" /> class.
        /// </summary>
        /// <param name="buttonDeckDisplay">An <see cref="IButtonDeckDisplay" /> instance.</param>
        /// <param name="systemDisableManager">An <see cref="ISystemDisableManager" /> instance.</param>
        /// <param name="gameDiagnostics">An <see cref="IGameDiagnostics" /> instance.</param>
        public UpdateButtonDeckImageCommandHandler(
            //IButtonDeckDisplay buttonDeckDisplay,
            ISystemDisableManager systemDisableManager,
            IGameDiagnostics gameDiagnostics)
        {
            //_buttonDeckDisplay = buttonDeckDisplay ?? throw new ArgumentNullException(nameof(buttonDeckDisplay));

            _systemDisableManager =
                systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));

            _gameDiagnostics = gameDiagnostics ?? throw new ArgumentNullException(nameof(gameDiagnostics));
        }

        /// <inheritdoc />
        public void Handle(UpdateButtonDeckImage command)
        {
            //// Don't draw game graphics when disabled.
            //if (!_systemDisableManager.IsDisabled || _gameDiagnostics.IsActive)
            //{
            //    _buttonDeckDisplay.DrawFromSharedMemory();
            //}
        }
    }
}
