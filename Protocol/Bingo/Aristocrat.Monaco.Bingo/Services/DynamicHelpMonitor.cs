namespace Aristocrat.Monaco.Bingo.Services
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts.Localization;
    using Common;
    using Kernel;
    using Localization.Properties;
    using Monaco.Common;
    using Protocol.Common.Storage.Entity;

    public sealed class DynamicHelpMonitor : IDisposable
    {
        private static readonly TimeSpan DynamicHelpMonitorTime = TimeSpan.FromMinutes(1);

        private readonly IBingoClientConnectionState _connectionState;
        private readonly ISystemDisableManager _disableManager;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly HttpClient _httpClient = new(new HttpClientHandler { AllowAutoRedirect = true });

        private CancellationTokenSource _tokenSource;
        private bool _disposed;

        public DynamicHelpMonitor(
            IBingoClientConnectionState connectionState,
            ISystemDisableManager disableManager,
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            _connectionState = connectionState ?? throw new ArgumentNullException(nameof(connectionState));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

            _connectionState.ClientConnected += OnClientConnected;
            _connectionState.ClientDisconnected += OnClientDisconnected;
            _httpClient.DefaultRequestHeaders.Clear();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _connectionState.ClientConnected -= OnClientConnected;
            _connectionState.ClientDisconnected -= OnClientDisconnected;
            _httpClient.Dispose();
            ClearTokenSource();

            _disposed = true;
        }

        private void ClearTokenSource()
        {
            if (_tokenSource is not null)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
            }

            _tokenSource = null;
        }

        private async Task MonitorDynamicHelpAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var helpUris = _unitOfWorkFactory.GetHelpUris();
                await Task.WhenAll(helpUris.Select(x => ValidateUriAsync(x, token)));
                await Task.Delay(DynamicHelpMonitorTime, token);
            }
        }

        private async Task ValidateUriAsync(Uri uri, CancellationToken token)
        {
            if (await GetValidResponseAsync(uri, token))
            {
                _disableManager.Enable(BingoConstants.BingoHostHelpUrlInvalidKey);
            }
            else
            {
                _disableManager.Disable(
                    BingoConstants.BingoHostHelpUrlInvalidKey,
                    SystemDisablePriority.Immediate,
                    () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BingoDynamicHelpInvalidConfiguration),
                    true,
                    () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BingoDynamicHelpInvalidConfigurationHelp));
            }
        }

        private async Task<bool> GetValidResponseAsync(Uri uri, CancellationToken token)
        {
            try
            {
                var httpResponseMessage = await _httpClient.GetAsync(
                    uri,
                    HttpCompletionOption.ResponseContentRead,
                    token);
                return IsSuccessful(httpResponseMessage);
            }
            catch (Exception e) when (e is InvalidOperationException or HttpRequestException)
            {
                return false;
            }
        }

        private static bool IsSuccessful(HttpResponseMessage httpResponseMessage)
        {
            return (httpResponseMessage.IsSuccessStatusCode ||
                    httpResponseMessage is
                    {
                        StatusCode: HttpStatusCode.Redirect or
                        HttpStatusCode.MovedPermanently or
                        HttpStatusCode.Moved
                    }) && httpResponseMessage.Content is not null;
        }

        private void OnClientDisconnected(object sender, EventArgs e)
        {
            ClearTokenSource();
        }

        private void OnClientConnected(object sender, EventArgs e)
        {
            ClearTokenSource();
            _tokenSource = new CancellationTokenSource();
            MonitorDynamicHelpAsync(_tokenSource.Token).FireAndForget();
        }
    }
}