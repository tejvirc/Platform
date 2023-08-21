namespace Aristocrat.Monaco.Mgam
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;

    /// <summary>
    ///     Responsible for starting the MGAM client.
    /// </summary>
    public class MgamEngine : IEngine
    {
        private const int StartTimeout = 30;

        private readonly ILogger _logger;
        private readonly IEgm _egm;

        /// <summary>
        ///     Instantiates a new instance of the <see cref="MgamEngine"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger{TCategory}"/>.</param>
        /// <param name="egm"><see cref="IEgm"/>.</param>
        public MgamEngine(
            ILogger<MgamEngine> logger,
            IEgm egm)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public Task Restart(IStartupContext context)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task Start(IStartupContext context, Action onReady = null)
        {
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(StartTimeout)))
            {
                try
                {
                    await _egm.Start(cts.Token);

                    onReady?.Invoke();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error starting client services", ex);
                }
            }
        }

        /// <inheritdoc />
        public async Task Stop()
        {
            try
            {
                await _egm.Stop(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error stopping client services", ex);
            }
        }
    }
}
