namespace Aristocrat.G2S.Emdi.Consumers
{
    using Host;
    using log4net;
    using Meters;
    using Monaco.Gaming.Contracts.Session;
    using Protocol.v21ext1b1;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Consumes the <see cref="PointBalanceChangedEvent"/> event
    /// </summary>
    public class PointBalanceChangedConsumer : Consumes<PointBalanceChangedEvent>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IReporter _reporter;
        private readonly IPlayerService _player;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointBalanceChangedConsumer"/> class.
        /// </summary>
        /// <param name="player">IPlayerService instance</param>
        /// <param name="reporter">IEmdiReporter instance</param>
        public PointBalanceChangedConsumer(
            IPlayerService player,
            IReporter reporter)
        {
            _player = player;
            _reporter = reporter;
        }

        /// <inheritdoc />
        protected override async Task ConsumeAsync(PointBalanceChangedEvent theEvent)
        {
            try
            {
                Logger.Debug("EMDI: Received PointBalanceChangedEvent event");

                await _reporter.ReportAsync(GetMeters(), MeterNames.PlayerPointBalance, MeterNames.PlayerPointCountdown, MeterNames.PlayerSessionPoints);
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending cabinet meter report", ex);
            }
        }

        private IEnumerable<c_meterInfo> GetMeters()
        {
            yield return new c_meterInfo
            {
                meterName = MeterNames.PlayerPointBalance,
                meterType = t_meterTypes.IGT_count,
                meterValue = _player.ActiveSession?.PointBalance ?? 0
            };

            yield return new c_meterInfo
            {
                meterName = MeterNames.PlayerPointCountdown,
                meterType = t_meterTypes.IGT_count,
                meterValue = _player.ActiveSession?.PointCountdown ?? 0
            };

            yield return new c_meterInfo
            {
                meterName = MeterNames.PlayerSessionPoints,
                meterType = t_meterTypes.IGT_count,
                meterValue = _player.ActiveSession?.SessionPoints ?? 0
            };
        }
    }
}
