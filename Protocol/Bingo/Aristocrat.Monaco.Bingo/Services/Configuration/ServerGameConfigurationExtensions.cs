namespace Aristocrat.Monaco.Bingo.Services.Configuration
{
    using Application.Contracts.Extensions;
    using Common.Storage.Model;
    using Gaming.Contracts;

    public static class ServerGameConfigurationExtensions
    {
        public static BingoGameConfiguration ToGameConfiguration(
            this ServerGameConfiguration configuration,
            IGameDetail gameDetail)
        {
            return new BingoGameConfiguration
            {
                GameTitleId = configuration.GameTitleId,
                PaytableId = configuration.PaytableId,
                Bets = configuration.Bets,
                Denomination = configuration.Denomination.CentsToMillicents(),
                EvaluationTypePaytable = configuration.EvaluationTypePaytable,
                PlatformGameId = gameDetail.Id,
                QuickStopMode = configuration.QuickStopMode,
                ThemeSkinId = configuration.ThemeSkinId,
                HelpUrl = configuration.HelpUrl,
                CrossGameProgressiveEnabled = configuration.CrossGameProgressiveEnabled,
                SideBetGames = configuration.SideBetGames
            };
        }
    }
}