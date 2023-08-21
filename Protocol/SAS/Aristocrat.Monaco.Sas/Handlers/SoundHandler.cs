namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts;
    using Hardware.Contracts.Audio;

    /// <summary>
    ///     Handles turning sound on/off
    /// </summary>
    public class SoundHandler : ISasLongPollHandler<LongPollResponse, LongPollSingleValueData<SoundActions>>
    {
        private readonly IAudio _audio;
        private readonly IGameService _gameService;

        /// <summary>
        ///     Creates the SoundHandler instance
        /// </summary>
        /// <param name="audio">An <see cref="IAudio" /> instance</param>
        /// <param name="gameService">An <see cref="IGameService"/> instance</param>
        public SoundHandler(IAudio audio, IGameService gameService)
        {
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands { get; } =
            new List<LongPoll> { LongPoll.SoundOff, LongPoll.SoundOn, LongPoll.GameSoundsDisable };

        /// <inheritdoc />
        public LongPollResponse Handle(LongPollSingleValueData<SoundActions> data)
        {
            Task.Run(
                () =>
                {
                    switch (data.Value)
                    {
                        case SoundActions.AllOff:
                            _audio.SetSystemMuted(true);
                            break;
                        case SoundActions.AllOn:
                            _audio.SetSystemMuted(false);
                            _gameService.GetVolumeControl()?.SetMuted(false);
                            break;
                        case SoundActions.GameOff:
                            var volume = _gameService.GetVolumeControl();

                            // NOTE:  This is per instance and will only be muted while the process is running
                            volume?.SetMuted(true);
                            break;
                    }
                });

            return null;
        }
    }
}