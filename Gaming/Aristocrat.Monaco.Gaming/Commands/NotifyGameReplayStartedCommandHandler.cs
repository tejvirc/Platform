namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts;
    using Hardware.Contracts.Audio;

    /// <summary>
    ///     Command handler for the <see cref="NotifyGameReplayStarted" /> command.
    /// </summary>
    public class NotifyGameReplayStartedCommandHandler : ICommandHandler<NotifyGameReplayStarted>
    {
        private readonly IGameService _gameService;
        private readonly IAudio _audio;
        private readonly IGameDiagnostics _gameDiagnostics;
        
        public NotifyGameReplayStartedCommandHandler(
            IGameDiagnostics gameDiagnostics,
            IAudio audio,
            IGameService gameService)
        {
            _gameDiagnostics = gameDiagnostics ?? throw new ArgumentNullException(nameof(gameDiagnostics));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
        }

        public void Handle(NotifyGameReplayStarted command)
        {
            if (!_gameDiagnostics.IsActive)
            {
                return;
            }

            var volume = _gameService.GetVolumeControl();

            volume?.SetVolume(_audio.GetDefaultVolume());
        }
    }
}
