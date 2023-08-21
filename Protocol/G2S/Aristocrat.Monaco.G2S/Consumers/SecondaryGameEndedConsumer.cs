namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;

    /// <summary>
    ///     Handles the <see cref="SecondaryGameEndedEvent" /> event.
    /// </summary>
    public class SecondaryGameEndedConsumer : GamePlayConsumerBase<SecondaryGameEndedEvent>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SecondaryGameEndedConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance</param>
        /// <param name="gameMeters">An <see cref="IGameMeterManager" /> instance.</param>
        public SecondaryGameEndedConsumer(IG2SEgm egm, IEventLift eventLift, IGameMeterManager gameMeters)
            : base(egm, eventLift, gameMeters, EventCode.G2S_GPE110)
        {
        }

        protected override meterList GetMeters(IGamePlayDevice device, long denomination, string wagerCategory)
        {
            var gameMeters = InternalGetMeters(device);

            return new meterList { meterInfo = gameMeters.ToArray() };
        }

        private IEnumerable<meterInfo> InternalGetMeters(IDevice device)
        {
            return new List<meterInfo>
            {
                new meterInfo
                {
                    meterDateTime = DateTime.UtcNow,
                    meterInfoType = MeterInfoType.Event,
                    deviceMeters =
                        new[]
                        {
                            new deviceMeters
                            {
                                deviceClass = device.PrefixedDeviceClass(),
                                deviceId = device.Id,
                                simpleMeter =
                                    GetSimpleMeters(
                                        device.Id,
                                        PerformanceMeterName.SecondaryWonAmount,
                                        PerformanceMeterName.SecondaryWonCount,
                                        PerformanceMeterName.SecondaryLostCount,
                                        PerformanceMeterName.SecondaryTiedCount).ToArray()
                            },
                            new deviceMeters
                            {
                                deviceClass = device.PrefixedDeviceClass(),
                                deviceId = 0,
                                simpleMeter =
                                    GetSimpleMeters(
                                        PerformanceMeterName.SecondaryWonAmount,
                                        PerformanceMeterName.SecondaryWonCount,
                                        PerformanceMeterName.SecondaryLostCount,
                                        PerformanceMeterName.SecondaryTiedCount).ToArray()
                            }
                        }
                }
            };
        }
    }
}