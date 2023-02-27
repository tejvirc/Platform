namespace Aristocrat.Monaco.Bingo.Services.Configuration
{
    using System.Linq;
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
                BetInformationDetails =
                    configuration.BetInformationDetails.Select(ToBetInformationDetail).ToList().AsReadOnly(),
                Denomination = configuration.Denomination.CentsToMillicents(),
                EvaluationTypePaytable = configuration.EvaluationTypePaytable,
                PlatformGameId = gameDetail.Id,
                QuickStopMode = configuration.QuickStopMode,
                ThemeSkinId = configuration.ThemeSkinId,
                HelpUrl = configuration.HelpUrl
            };
        }

        public static BetInformationDetail ToBetInformationDetail(this ServerBetInformationDetail detail)
        {
            return new BetInformationDetail
            {
                Bet = detail.Bet,
                Rtp = detail.Rtp
            };
        }
    }
}