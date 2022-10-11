namespace Aristocrat.Monaco.Bingo.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Gaming.Contracts;
    using Kernel;
    using log4net;
    using Protocol.Common.Storage.Entity;
    using Storage.Model;
    using GameConfiguration = Gaming.Contracts.GameConfiguration;

    public static class BingoHelper
    {
        private const int HelpPort = 7510; // TODO Update this to be the server setting once we get it in the configuration
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        public static Uri GetHelpUri(this IUnitOfWorkFactory unitOfWorkFactory, IPropertiesManager propertiesManager)
        {
            var helpUrl = propertiesManager.GetValue(BingoConstants.BingoHelpUri, string.Empty);
            if (!string.IsNullOrEmpty(helpUrl) &&
                Uri.CheckSchemeName(helpUrl) &&
                Uri.IsWellFormedUriString(helpUrl, UriKind.RelativeOrAbsolute))
            {
                return new Uri(helpUrl);
            }

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

        public static async Task<bool> ValidateAddressAsync(this Uri address, CancellationToken token = default)
        {
            try
            {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
                var request = WebRequest.Create(address);
#pragma warning restore SYSLIB0014 // Type or member is obsolete
                using var register = token.Register(() => request.Abort());
                using var response = await request.GetResponseAsync();
                return response.ContentLength > 0;
            }
            catch (Exception ex)
            {
                Logger.Error($"Caught exception {ex} - requesting {address}");
                return false;
            }
        }

        private static Uri GetHelpUri(this IUnitOfWorkFactory unitOfWorkFactory, BingoGameConfiguration serverSettings)
        {
            var bingoHost = unitOfWorkFactory.Invoke(x => x.Repository<Host>().Queryable().Single());
            var uriBuilder = new UriBuilder
            {
                Host = bingoHost.HostName,
                Port = HelpPort,
                Scheme = Uri.UriSchemeHttps,
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