namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Contracts;
    using Extensions;
    using Gaming.Contracts;
    using Gaming.Contracts.Events.OperatorMenu;
    using Gaming.Contracts.Meters;
    using Gaming.Contracts.Rtp;
    using Kernel;

    // ReSharper disable once UnusedMember.Global
    public class GameCategoryDataSource : IDataSource, IDisposable
    {
        private readonly IGameMeterManager _gameMeterManager;
        private readonly Dictionary<string, byte> _gameNumber;
        private readonly IAspGameProvider _aspGameProvider;
        private readonly Dictionary<string, Func<(IGameDetail, IDenomination)?, object>> _handlers;
        private readonly IEventBus _eventBus;
        private IReadOnlyList<(IGameDetail game, IDenomination denom)> _gameProfile;

        private bool _disposed;
        private const byte MaxGamesSupported = 64;

        public GameCategoryDataSource(
            IAspGameProvider aspGameProvider,
            IGameMeterManager gameMeterManager,
            IEventBus eventBus)
        {
            _aspGameProvider = aspGameProvider ?? throw new ArgumentNullException(nameof(aspGameProvider));
            _gameMeterManager = gameMeterManager ?? throw new ArgumentNullException(nameof(gameMeterManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _handlers = GetMembersMap();
            _gameNumber = GetGameNumbersMap();
            _gameProfile = GetGameProfile();
            _eventBus.Subscribe<GameConfigurationSaveCompleteEvent>(this, OnGameConfigurationSaveComplete);

            foreach (var (game, denom) in _gameProfile)
            {
                _gameMeterManager.GetMeter(game.Id, denom.Value, GamingMeters.PlayedCount).MeterChangedEvent += OnMeterValueChanged;
            }
        }

        public string Name { get; } = "GameCategory";

        public IReadOnlyList<string> Members => _handlers.Keys.ToList();

        public event EventHandler<Dictionary<string, object>> MemberValueChanged;

        public object GetMemberValue(string member)
        {
            var gameNumber = _gameNumber[member];

            var gameProfile = gameNumber <= _gameProfile.Count
                ? _gameProfile[gameNumber - 1]
                : ((IGameDetail, IDenomination)?)null;

            return _handlers[member](gameProfile);
        }

        public void SetMemberValue(string member, object value)
        {
        }

        private static object GetCreditDenom((IGameDetail game, IDenomination denom)? gameProfile)
        {
            return gameProfile?.denom.Value.MillicentsToCents() ?? 0;
        }

        private static string GetGameId((IGameDetail game, IDenomination denom)? gameProfile)
        {
            //a unit of dacom game ID is made of by a concatenate of gameId + DemomId in string format
            return gameProfile?.game.Id.ToString("D04") + gameProfile?.denom.Id.ToString("D04");
        }

        private static string GetGameVersion((IGameDetail game, IDenomination denom)? gameProfile)
        {
            var version = gameProfile?.game.Version ?? string.Empty;
            var charsToRemove = new List<char> { '_', '-', '.', ' ' };
            return charsToRemove.Aggregate(version, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        private static string GetGameName((IGameDetail game, IDenomination denom)? gameProfile)
        {
            return gameProfile?.game.ThemeName ?? string.Empty;
        }

        private static string GetGameRtp((IGameDetail game, IDenomination denom)? gameProfile)
        {
            var theoreticalRtp = gameProfile?.game.MinimumPaybackPercent ?? 0;
            return theoreticalRtp.ToRtpString();
        }

        private static object GetGameMaxBetInCents((IGameDetail game, IDenomination denom)? gameProfile)
        {
            var maxBet = gameProfile?.denom.MaximumWagerCredits ?? 0;
            var denom = gameProfile?.denom.Value.MillicentsToCents() ?? 0;
            return maxBet * denom;
        }

        private static Dictionary<string, byte> GetGameNumbersMap()
        {
            var map = new Dictionary<string, byte>();

            for (byte gameNumber = 1; gameNumber <= MaxGamesSupported; gameNumber++)
            {
                map.Add($"Credit_Denomination_{gameNumber}", gameNumber);
                map.Add($"Game_ID_{gameNumber}", gameNumber);
                map.Add($"Version_Number_{gameNumber}", gameNumber);
                map.Add($"Game_Name_{gameNumber}", gameNumber);
                map.Add($"Theoretical_Rtn_{gameNumber}", gameNumber);
                map.Add($"Total_Games_Completed_{gameNumber}", gameNumber);
                map.Add($"Max_Bet_In_Cents_{gameNumber}", gameNumber);
            }

            return map;
        }

        private void OnGameConfigurationSaveComplete(GameConfigurationSaveCompleteEvent obj)
        {
            _gameProfile = GetGameProfile();

            foreach (var (game, denom) in _gameProfile)
            {
                _gameMeterManager.GetMeter(game.Id, denom.Value, GamingMeters.PlayedCount).MeterChangedEvent += OnMeterValueChanged;
            }
        }

        private object GetGamesPlayed((IGameDetail game, IDenomination denom)? gameProfile)
        {
            var meterId = gameProfile?.game.Id;
            return !meterId.HasValue
                ? 0
                : _gameMeterManager.GetMeter(
                    meterId.Value,
                    gameProfile.Value.denom.Value,
                    GamingMeters.PlayedCount).Lifetime;
        }

        private Dictionary<string, Func<(IGameDetail, IDenomination)?, object>> GetMembersMap()
        {
            var map = new Dictionary<string, Func<(IGameDetail, IDenomination)?, object>>();

            for (byte gameNumber = 1; gameNumber <= MaxGamesSupported; gameNumber++)
            {
                map.Add($"Credit_Denomination_{gameNumber}", GetCreditDenom);
                map.Add($"Game_ID_{gameNumber}", GetGameId);
                map.Add($"Version_Number_{gameNumber}", GetGameVersion);
                map.Add($"Game_Name_{gameNumber}", GetGameName);
                map.Add($"Theoretical_Rtn_{gameNumber}", GetGameRtp);
                map.Add($"Total_Games_Completed_{gameNumber}", GetGamesPlayed);
                map.Add($"Max_Bet_In_Cents_{gameNumber}", GetGameMaxBetInCents);
            }

            return map;
        }

        private IReadOnlyList<(IGameDetail game, IDenomination denom)> GetGameProfile()
        {
            return _aspGameProvider.GetEnabledGames();
        }

        private void OnMeterValueChanged(object sender, MeterChangedEventArgs e)
        {
            if (!(sender is IMeter meter))
            {
                return;
            }

            var gameList = _gameProfile.Aggregate(
                ImmutableList<(IGameDetail game, IDenomination denom)>.Empty,
                (current, element) => current.Add(element));
            var gameNumberList = (from (IGameDetail game, IDenomination denom) gameProfile in gameList
                where meter == _gameMeterManager.GetMeter(
                    gameProfile.game.Id,
                    gameProfile.denom.Value,
                    GamingMeters.PlayedCount)
                select gameList.IndexOf(gameProfile) + 1).ToList();
            gameNumberList.ForEach(
                gameNumber => { MemberValueChanged?.Invoke(this, this.GetMemberSnapshot($"Total_Games_Completed_{gameNumber}")); }
            );
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);

                foreach (var (game, denom) in _gameProfile)
                {
                    _gameMeterManager.GetMeter(game.Id, denom.Value, GamingMeters.PlayedCount).MeterChangedEvent -= OnMeterValueChanged;
                }
            }

            _disposed = true;
        }
    }
}