namespace Aristocrat.Monaco.Bingo.Services
{
    using System;
    using System.Collections.Generic;
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
    using Polly;
    using Protocol.Common.Storage.Entity;

    public sealed class DynamicHelpMonitor : IDisposable
    {
        private const int RetryCount = 3;
        private static readonly TimeSpan DynamicHelpMonitorTime = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(100);

        private readonly IBingoClientConnectionState _connectionState;
        private readonly ISystemDisableManager _disableManager;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly HttpClient _httpClient = new(new HttpClientHandler { AllowAutoRedirect = true });
        private readonly IAsyncPolicy<bool> _helpPolicy;

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
            _helpPolicy = Policy<bool>.Handle<HttpRequestException>()
                .OrResult(result => !result)
                .WaitAndRetryAsync(RetryCount, _ => RetryDelay);
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
                var results = await Task.WhenAll(helpUris.Select(x => GetValidResponseAsync(x, token)));
                HandleHelpResults(results);
                await Task.Delay(DynamicHelpMonitorTime, token);
            }
        }

        private void HandleHelpResults(IEnumerable<bool> results)
        {
            if (results.All(b => b))
            {
                _disableManager.Enable(BingoConstants.BingoHostHelpUrlInvalidKey);
            }
            else
            {
                _disableManager.Disable(
                    BingoConstants.BingoHostHelpUrlInvalidKey,
                    SystemDisablePriority.Normal,
                    () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BingoDynamicHelpInvalidConfiguration),
                    true,
                    () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BingoDynamicHelpInvalidConfigurationHelp));
            }
        }

        private async Task<bool> GetValidResponseAsync(Uri uri, CancellationToken token)
        {
            try
            {
                return await _helpPolicy.ExecuteAsync(GetHelpUrlResponse);
            }
            catch (Exception e) when (e is InvalidOperationException or HttpRequestException ||
                                      e.InnerException is InvalidOperationException or HttpRequestException)
            {
                return false;
            }

            async Task<bool> GetHelpUrlResponse()
            {
                var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseContentRead, token);
                return IsSuccessful(response);
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