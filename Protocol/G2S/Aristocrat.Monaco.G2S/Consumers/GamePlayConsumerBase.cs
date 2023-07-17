namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;
    using Handlers.GamePlay;

    /// <summary>
    ///     Base class for stacker related events
    /// </summary>
    /// <typeparam name="T">The event type</typeparam>
    public abstract class GamePlayConsumerBase<T> : Consumes<T>
        where T : BaseGameEvent
    {
        private readonly IG2SEgm _egm;
        private readonly string _eventCode;
        private readonly IEventLift _eventLift;
        private readonly IGameMeterManager _gameMeters;

        private readonly Dictionary<string, string> _simpleMeters = new Dictionary<string, string>
        {
            { PerformanceMeterName.WageredAmount, GamingMeters.WageredAmount },
            { PerformanceMeterName.AveragePaybackPercent, GamingMeters.AveragePayback },
            { PerformanceMeterName.TheoreticalPaybackAmount, GamingMeters.TheoPayback },
            { PerformanceMeterName.EgmPaidGameWonAmount, GamingMeters.TotalEgmPaidGameWonAmount },
            { PerformanceMeterName.HandPaidGameWonAmount, GamingMeters.TotalHandPaidGameWonAmount },
            { PerformanceMeterName.EgmPaidProgWonAmount, GamingMeters.EgmPaidProgWonAmount },
            { PerformanceMeterName.HandPaidProgWonAmount, GamingMeters.HandPaidProgWonAmount },
            { PerformanceMeterName.SecondaryFailedCount, GamingMeters.SecondaryFailedCount },
            { PerformanceMeterName.SecondaryLostCount, GamingMeters.SecondaryLostCount },
            { PerformanceMeterName.SecondaryTiedCount, GamingMeters.SecondaryTiedCount },
            { PerformanceMeterName.SecondaryWonCount, GamingMeters.SecondaryWonCount },
            { PerformanceMeterName.SecondaryWageredAmount, GamingMeters.SecondaryWageredAmount },
            { PerformanceMeterName.SecondaryWonAmount, GamingMeters.SecondaryWonAmount },
            { PerformanceMeterName.FailedCount, GamingMeters.FailedCount },
            { PerformanceMeterName.WonCount, GamingMeters.WonCount },
            { PerformanceMeterName.LostCount, GamingMeters.LostCount },
            { PerformanceMeterName.TiedCount, GamingMeters.TiedCount },
            { GameDenomMeterName.PlayedCount, GamingMeters.PlayedCount }
        };

        /// <summary>
        ///     Initializes a new instance of the <see cref="GamePlayConsumerBase{T}" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance</param>
        /// <param name="gameMeters">An <see cref="IGameMeterManager" /> instance</param>
        /// <param name="eventCode">The G2S Event code</param>
        protected GamePlayConsumerBase(
            IG2SEgm egm,
            IEventLift eventLift,
            IGameMeterManager gameMeters,
            string eventCode)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _gameMeters = gameMeters ?? throw new ArgumentNullException(nameof(gameMeters));
            _eventCode = eventCode;
        }

        /// <inheritdoc />
        public override void Consume(T theEvent)
        {
            var device = _egm.GetDevice<IGamePlayDevice>(theEvent.GameId);
            if (device == null)
            {
                return;
            }

            _eventLift.Report(
                device,
                _eventCode,
                theEvent.Log.TransactionId,
                device.TransactionList(theEvent.Log.ToRecallLog()),
                GetMeters(device, theEvent.Denomination, theEvent.WagerCategory),
                theEvent);
        }

        /// <summary>
        ///     Used to gather meters related to the event
        /// </summary>
        /// <param name="device">The game play device</param>
        /// <param name="denomination">The played denomination</param>
        /// <param name="wagerCategory">The played wager category</param>
        /// <returns>An optional meter list</returns>
        protected virtual meterList GetMeters(IGamePlayDevice device, long denomination, string wagerCategory)
        {
            return null;
        }

        /// <summary>
        ///     Used to gather meters
        /// </summary>
        /// <param name="includeMeters">The meters to include</param>
        /// <returns>A collection of simple meters</returns>
        protected IEnumerable<simpleMeter> GetSimpleMeters(params string[] includeMeters)
        {
            return _simpleMeters.Where(m => { return includeMeters != null && includeMeters.Any(i => i == m.Key); })
                .Select(
                    meter => new simpleMeter
                    {
                        meterName = meter.Key.StartsWith("G2S_", StringComparison.InvariantCultureIgnoreCase)
                            ? meter.Key
                            : $"G2S_{meter.Key}",
                        meterValue = _gameMeters.IsMeterProvided(meter.Value)
                            ? _gameMeters.GetMeter(meter.Value).Lifetime
                            : 0
                    });
        }

        /// <summary>
        ///     Used to gather meters
        /// </summary>
        /// <param name="deviceId">The device Id</param>
        /// <param name="includeMeters">The meters to include</param>
        /// <returns>A collection of simple meters</returns>
        protected IEnumerable<simpleMeter> GetSimpleMeters(int deviceId, params string[] includeMeters)
        {
            return _simpleMeters.Where(m => { return includeMeters != null && includeMeters.Any(i => i == m.Key); })
                .Select(
                    meter => new simpleMeter
                    {
                        meterName = meter.Key.StartsWith("G2S_", StringComparison.InvariantCultureIgnoreCase)
                            ? meter.Key
                            : $"G2S_{meter.Key}",
                        meterValue = _gameMeters.IsMeterProvided(deviceId, meter.Value)
                            ? _gameMeters.GetMeter(deviceId, meter.Value).Lifetime
                            : 0
                    });
        }

        /// <summary>
        ///     Used to gather meters
        /// </summary>
        /// <param name="denomination">The denomination</param>
        /// <param name="includeMeters">The meters to include</param>
        /// <returns>A collection of simple meters</returns>
        protected IEnumerable<simpleMeter> GetSimpleMeters(long denomination, params string[] includeMeters)
        {
            return _simpleMeters.Where(m => { return includeMeters != null && includeMeters.Any(i => i == m.Key); })
                .Select(
                    meter => new simpleMeter
                    {
                        meterName = meter.Key.StartsWith("G2S_", StringComparison.InvariantCultureIgnoreCase)
                            ? meter.Key
                            : $"G2S_{meter.Key}",
                        meterValue = _gameMeters.IsMeterProvided(denomination, meter.Value)
                            ? _gameMeters.GetMeter(denomination, meter.Value).Lifetime
                            : 0
                    });
        }

        /// <summary>
        ///     Used to gather meters
        /// </summary>
        /// <param name="deviceId">The device Id</param>
        /// <param name="denomination">The denomination</param>
        /// <param name="includeMeters">The meters to include</param>
        /// <returns>A collection of simple meters</returns>
        protected IEnumerable<simpleMeter> GetSimpleMeters(
            int deviceId,
            long denomination,
            params string[] includeMeters)
        {
            return _simpleMeters.Where(m => { return includeMeters != null && includeMeters.Any(i => i == m.Key); })
                .Select(
                    meter => new simpleMeter
                    {
                        meterName = meter.Key.StartsWith("G2S_", StringComparison.InvariantCultureIgnoreCase)
                            ? meter.Key
                            : $"G2S_{meter.Key}",
                        meterValue = _gameMeters.IsMeterProvided(deviceId, denomination, meter.Value)
                            ? _gameMeters.GetMeter(deviceId, denomination, meter.Value).Lifetime
                            : 0
                    });
        }

        /// <summary>
        ///     Used to gather meters
        /// </summary>
        /// <param name="deviceId">The device Id</param>
        /// <param name="wagerCategory">The wager category</param>
        /// <param name="includeMeters">The meters to include</param>
        /// <returns>A collection of simple meters</returns>
        protected IEnumerable<simpleMeter> GetWagerCategoryMeters(
            int deviceId,
            string wagerCategory,
            params string[] includeMeters)
        {
            // Had to split these out to avoid the name collision with the performance meters
            var wagerCategoryMeters = new Dictionary<string, string>
            {
                { WagerCategoryMeterName.WageredAmount, GamingMeters.WagerCategoryWageredAmount },
                { WagerCategoryMeterName.PlayedCount, GamingMeters.WagerCategoryPlayedCount }
            };

            return wagerCategoryMeters
                .Where(m => { return includeMeters != null && includeMeters.Any(i => i == m.Key); })
                .Select(
                    meter => new simpleMeter
                    {
                        meterName = meter.Key.StartsWith("G2S_", StringComparison.InvariantCultureIgnoreCase)
                            ? meter.Key
                            : $"G2S_{meter.Key}",
                        meterValue = _gameMeters.IsMeterProvided(deviceId, wagerCategory, meter.Value)
                            ? _gameMeters.GetMeter(deviceId, wagerCategory, meter.Value).Lifetime
                            : 0
                    });
        }
    }
}