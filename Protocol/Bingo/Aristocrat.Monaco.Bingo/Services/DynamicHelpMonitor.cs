namespace Aristocrat.Monaco.Bingo.Services
{
    using System;
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
        private static readonly TimeSpan DynamicHelpMonitorTime = TimeSpan.FromMinutes(5);

        private readonly IBingoClientConnectionState _connectionState;
        private readonly ISystemDisableManager _disableManager;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IPropertiesManager _propertiesManager;

        private CancellationTokenSource _tokenSource;
        private bool _disposed;

        public DynamicHelpMonitor(
            IBingoClientConnectionState connectionState,
            ISystemDisableManager disableManager,
            IUnitOfWorkFactory unitOfWorkFactory,
            IPropertiesManager propertiesManager)
        {
            _connectionState = connectionState ?? throw new ArgumentNullException(nameof(connectionState));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));

            _connectionState.ClientConnected += OnClientConnected;
            _connectionState.ClientDisconnected += OnClientDisconnected;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _connectionState.ClientConnected -= OnClientConnected;
            _connectionState.ClientDisconnected -= OnClientDisconnected;
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
                foreach (var helpUri in helpUris)
                {
                    await ValidateUriAsync(helpUri, token);
                }

                await Task.Delay(DynamicHelpMonitorTime, token);
            }
        }

        private async Task ValidateUriAsync(Uri uri, CancellationToken token)
        {
            if (await uri.ValidateAddressAsync(token))
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