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
    ///     Handles the <see cref="PrimaryGameEndedEvent" /> event.
    /// </summary>
    public class PrimaryGameEndedConsumer : GamePlayConsumerBase<PrimaryGameEndedEvent>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrimaryGameEndedConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance</param>
        /// <param name="gameMeters">An <see cref="IGameMeterManager" /> instance.</param>
        public PrimaryGameEndedConsumer(
            IG2SEgm egm,
            IEventLift eventLift,
            IGameMeterManager gameMeters)
            : base(egm, eventLift, gameMeters, EventCode.G2S_GPE105)
        {
        }

        /// <inheritdoc />
        protected override meterList GetMeters(IGamePlayDevice device, long denomination, string wagerCategory)
        {
            var gameMeters = InternalGetMeters(device, denomination, wagerCategory);

            return new meterList { meterInfo = gameMeters.ToArray() };
        }

        private IEnumerable<meterInfo> InternalGetMeters(IDevice device, long denomination, string wagerCategory)
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
                                        PerformanceMeterName.AveragePaybackPercent,
                                        PerformanceMeterName.TheoreticalPaybackAmount).ToArray()
                            },
                            new deviceMeters
                            {
                                deviceClass = device.PrefixedDeviceClass(),
                                deviceId = 0,
                                simpleMeter =
                                    GetSimpleMeters(
                                        PerformanceMeterName.AveragePaybackPercent,
                                        PerformanceMeterName.TheoreticalPaybackAmount).ToArray()
                            }
                        },
                    gameDenomMeters =
                        new[]
                        {
                            new gameDenomMeters
                            {
                                deviceClass = device.PrefixedDeviceClass(),
                                deviceId = device.Id,
                                denomId = denomination,
                                simpleMeter =
                                    GetSimpleMeters(
                                        device.Id,
                                        denomination,
                                        PerformanceMeterName.WageredAmount,
                                        PerformanceMeterName.AveragePaybackPercent,
                                        PerformanceMeterName.TheoreticalPaybackAmount).ToArray()
                            },
                            new gameDenomMeters
                            {
                                deviceClass = device.PrefixedDeviceClass(),
                                deviceId = 0,
                                denomId = denomination,
                                simpleMeter =
                                    GetSimpleMeters(
                                        denomination,
                                        PerformanceMeterName.WageredAmount,
                                        PerformanceMeterName.AveragePaybackPercent,
                                        PerformanceMeterName.TheoreticalPaybackAmount).ToArray()
                            }
                        },
                    wagerMeters =
                        new[]
                        {
                            new wagerMeters
                            {
                                deviceClass = device.PrefixedDeviceClass(),
                                deviceId = device.Id,
                                wagerCategory = wagerCategory,
                                simpleMeter =
                                    GetWagerCategoryMeters(
                                        device.Id,
                                        wagerCategory,
                                        WagerCategoryMeterName.WageredAmount).ToArray()
                            }
                        }
                }
            };
        }
    }
}