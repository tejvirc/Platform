namespace Aristocrat.Monaco.Bingo.Common
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Kernel;
    using log4net;
    using Protocol.Common.Storage.Entity;
    using Storage.Model;

    public static class BingoHelper
    {
        private const int HelpPort = 7510; // TODO Update this to be the server setting once we get it in the configuration
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static Uri GetHelpUri(this IUnitOfWorkFactory unitOfWorkFactory, IPropertiesManager propertiesManager)
        {
            var helpUrl = propertiesManager.GetValue(BingoConstants.BingoHelpUri, string.Empty);
            if (!string.IsNullOrEmpty(helpUrl) &&
                Uri.CheckSchemeName(helpUrl) &&
                Uri.IsWellFormedUriString(helpUrl, UriKind.RelativeOrAbsolute))
            {
                return new Uri(helpUrl);
            }

            var serverSettings = unitOfWorkFactory.Invoke(
                x => x.Repository<BingoServerSettingsModel>().Queryable().SingleOrDefault());
            var bingoHost = unitOfWorkFactory.Invoke(x => x.Repository<Host>().Queryable().Single());
            var uriBuilder = new UriBuilder
            {
                Host = bingoHost.HostName,
                Port = HelpPort,
                Scheme = Uri.UriSchemeHttps,
            };

            if (serverSettings is not null)
            {
                uriBuilder.Path = $"/{serverSettings.GameTitles}/{serverSettings.PaytableIds}";
            }

            return uriBuilder.Uri;
        }

        public static async Task<bool> ValidateAddressAsync(this Uri address, CancellationToken token = default)
        {
            try
            {
                var request = WebRequest.Create(address);
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
    }
}