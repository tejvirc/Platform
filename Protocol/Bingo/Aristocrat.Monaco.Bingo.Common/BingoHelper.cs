namespace Aristocrat.Monaco.Bingo.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Gaming.Contracts;
    using Kernel;
    using Protocol.Common.Storage.Entity;
    using Storage.Model;

    public static class BingoHelper
    {
        private const int FallBackDynamicHelpPort = 7510;

        public static Uri GetHelpUri(this IUnitOfWorkFactory unitOfWorkFactory, IPropertiesManager propertiesManager)
        {
            var (gameDetail, denomination) = propertiesManager.GetActiveGame();
            var serverSettings = unitOfWorkFactory.Invoke(
                    x => x.Repository<BingoServerSettingsModel>().Queryable().SingleOrDefault())?.GamesConfigured
                ?.FirstOrDefault(
                    c => gameDetail is null || denomination is null ||
                         c.PlatformGameId == gameDetail.Id && c.Denomination == denomination.Value);
            return unitOfWorkFactory.GetHelpUri(serverSettings);
        }

        public static IEnumerable<Uri> GetHelpUris(this IUnitOfWorkFactory unitOfWorkFactory)
        {
            var gamesConfigured =
                unitOfWorkFactory.Invoke(x => x.Repository<BingoServerSettingsModel>().Queryable().SingleOrDefault())
                    ?.GamesConfigured ?? Enumerable.Empty<BingoGameConfiguration>();
            foreach (var game in gamesConfigured)
            {
                yield return unitOfWorkFactory.GetHelpUri(game);
            }
        }

        public static bool IsValidHelpUri(this string helpUrl)
        {
            return !string.IsNullOrEmpty(helpUrl) &&
                   Uri.IsWellFormedUriString(helpUrl, UriKind.RelativeOrAbsolute);
        }

        private static Uri GetHelpUri(this IUnitOfWorkFactory unitOfWorkFactory, BingoGameConfiguration serverSettings)
        {
            var helpUrl = serverSettings?.HelpUrl;
            return helpUrl.IsValidHelpUri() ? new Uri(helpUrl) : unitOfWorkFactory.GetFallbackUri(serverSettings);
        }

        private static Uri GetFallbackUri(this IUnitOfWorkFactory unitOfWorkFactory, BingoGameConfiguration serverSettings)
        {
            var bingoHost = unitOfWorkFactory.Invoke(x => x.Repository<Host>().Queryable().Single());
            var uriBuilder = new UriBuilder
            {
                Host = bingoHost.HostName, Port = FallBackDynamicHelpPort, Scheme = Uri.UriSchemeHttps,
            };

            if (serverSettings is not null)
            {
                uriBuilder.Path = string.Format(
                    BingoConstants.DefaultBingoHelpUriFormat,
                    serverSettings.GameTitleId,
                    serverSettings.PaytableId);
            }

            return uriBuilder.Uri;
        }
    }
}
