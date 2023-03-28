namespace Aristocrat.Monaco.Bingo.Services
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Common;
    using Kernel;
    using log4net;
    using Monaco.Common;

    public sealed class ClientActivityService : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IBingoClientConnectionState _connectionState;
        private readonly IPropertiesManager _properties;
        private readonly IActivityReportService _activityReportService;
        private CancellationTokenSource _tokenSource;
        private bool _disposed;

        /// <summary>
        ///     Creates an instance of <see cref="ClientActivityService"/>
        /// </summary>
        /// <param name="connectionState">An instance of <see cref="IBingoClientConnectionState"/></param>
        /// <param name="properties">An instance of <see cref="IPropertiesManager"/></param>
        /// <param name="activityReportService">An instance of <see cref="IActivityReportService"/></param>
        /// <exception cref="ArgumentNullException">Thrown when any of the constructor arguments are null</exception>
        public ClientActivityService(
            IBingoClientConnectionState connectionState,
            IPropertiesManager properties,
            IActivityReportService activityReportService)
        {
            _connectionState = connectionState ?? throw new ArgumentNullException(nameof(connectionState));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _activityReportService =
                activityReportService ?? throw new ArgumentNullException(nameof(activityReportService));
            _connectionState.ClientConnected += ConnectionStateOnClientConnected;
            _connectionState.ClientDisconnected += ConnectionStateOnClientDisconnected;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _connectionState.ClientConnected -= ConnectionStateOnClientConnected;
            _connectionState.ClientDisconnected -= ConnectionStateOnClientDisconnected;
            _tokenSource?.Cancel(true);
            if (_tokenSource is not null)
            {
                _tokenSource.Dispose();
            }

            _tokenSource = null;

            _disposed = true;
        }

        private async Task ProcessEgmActivityTime(TimeSpan activityReportTime, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await ReportActivity(token);
                await Task.Delay(activityReportTime, token);
            }
        }

        private async Task ReportActivity(CancellationToken token)
        {
            try
            {
                var machineSerial = _properties.GetValue(ApplicationConstants.SerialNumber, string.Empty);
                await _activityReportService.ReportActivity(new ActivityReportMessage(DateTime.UtcNow, machineSerial), token);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to process EGM activity", e);
            }
        }

        private void ConnectionStateOnClientDisconnected(object sender, EventArgs e)
        {
            _tokenSource?.Cancel();
        }

        private void ConnectionStateOnClientConnected(object sender, EventArgs e)
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();
            var activityReportTime = _properties.GetValue(
                BingoConstants.ActivityReportingTime,
                string.Empty);

            var time = TimeSpan.TryParse(activityReportTime, out var expectedTime)
                ? expectedTime
                : BingoConstants.DefaultActivityReportingTime;
            ProcessEgmActivityTime(time, _tokenSource.Token).FireAndForget();
        }
    }
}
