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
    using Meters;

    /// <summary>
    ///     Handles the <see cref="GameEndedEvent" /> event.
    /// </summary>
    public class GameEndedConsumer : GamePlayConsumerBase<GameEndedEvent>
    {
        private readonly IMeterAggregator<ICabinetDevice> _cabinetMeters;
        private readonly IG2SEgm _egm;
        private readonly IMeterAggregator<IVoucherDevice> _voucherMeters;
        private readonly IMeterAggregator<IHandpayDevice> _handpayMeters;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameEndedConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance</param>
        /// <param name="cabinetMeters">An <see cref="IMeterAggregator&lt;ICabinetDevice&gt;" /> instance.</param>
        /// <param name="voucherMeters">An <see cref="IMeterAggregator{IVoucherDevice}" /> instance.</param>
        /// <param name="handpayMeters">An <see cref="IMeterAggregator{IHandpayDevice}" /> instance.</param>
        /// <param name="gameMeters">An <see cref="IGameMeterManager" /> instance.</param>
        public GameEndedConsumer(
            IG2SEgm egm,
            IEventLift eventLift,
            IMeterAggregator<ICabinetDevice> cabinetMeters,
            IMeterAggregator<IVoucherDevice> voucherMeters,
            IMeterAggregator<IHandpayDevice> handpayMeters,
            IGameMeterManager gameMeters)
            : base(egm, eventLift, gameMeters, EventCode.G2S_GPE112)
        {
            _egm = egm;
            _cabinetMeters = cabinetMeters ?? throw new ArgumentNullException(nameof(cabinetMeters));
            _voucherMeters = voucherMeters ?? throw new ArgumentNullException(nameof(voucherMeters));
            _handpayMeters = handpayMeters ?? throw new ArgumentNullException(nameof(handpayMeters));
        }

        /// <inheritdoc />
        protected override meterList GetMeters(IGamePlayDevice device, long denomination, string wagerCategory)
        {
            var cabinetMeters = new List<meterInfo>(
                _cabinetMeters.GetMeters(
                    _egm.GetDevice<ICabinetDevice>(),
                    CabinetMeterName.PlayerCashableAmount,
                    CabinetMeterName.EgmPaidGameWonAmount,
                    CabinetMeterName.HandPaidGameWonAmount,
                    CabinetMeterName.EgmPaidProgWonAmount,
                    CabinetMeterName.HandPaidProgWonAmount,
                    CabinetMeterName.GamesSinceInitCount,
                    CabinetMeterName.GamesSincePowerResetCount,
                    CabinetMeterName.GamesSinceDoorClosedCount,
                    CabinetMeterName.HandPaidGameWonAmount));

            var voucherMeters = _voucherMeters.GetMeters(
                _egm.GetDevice<IVoucherDevice>(),
                VoucherMeterName.CashableOutAmount,
                VoucherMeterName.CashableOutCount);

            var handpayMeters = _handpayMeters.GetMeters(
                _egm.GetDevice<IHandpayDevice>(),
                TransferMeterName.CashableOutAmount,
                TransferMeterName.TransferOutCount);

            var gameMeters = InternalGetMeters(device, denomination, wagerCategory);

            return new meterList
            {
                meterInfo = cabinetMeters
                    .Concat(gameMeters)
                    .Concat(voucherMeters)
                    .Concat(handpayMeters)
                    .ToArray()
            };
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
                                        PerformanceMeterName.EgmPaidGameWonAmount,
                                        PerformanceMeterName.HandPaidGameWonAmount,
                                        PerformanceMeterName.EgmPaidProgWonAmount,
                                        PerformanceMeterName.HandPaidProgWonAmount,
                                        PerformanceMeterName.WonCount,
                                        PerformanceMeterName.LostCount,
                                        PerformanceMeterName.TiedCount).ToArray()
                            },
                            new deviceMeters
                            {
                                deviceClass = device.PrefixedDeviceClass(),
                                deviceId = 0,
                                simpleMeter =
                                    GetSimpleMeters(
                                        PerformanceMeterName.EgmPaidGameWonAmount,
                                        PerformanceMeterName.HandPaidGameWonAmount,
                                        PerformanceMeterName.EgmPaidProgWonAmount,
                                        PerformanceMeterName.HandPaidProgWonAmount,
                                        PerformanceMeterName.WonCount,
                                        PerformanceMeterName.LostCount,
                                        PerformanceMeterName.TiedCount).ToArray()
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
                                        GameDenomMeterName.PlayedCount).ToArray()
                            },
                            new gameDenomMeters
                            {
                                deviceClass = device.PrefixedDeviceClass(),
                                deviceId = 0,
                                denomId = denomination,
                                simpleMeter =
                                    GetSimpleMeters(
                                        denomination,
                                        GameDenomMeterName.PlayedCount).ToArray()
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
                                        WagerCategoryMeterName.PlayedCount).ToArray()
                            }
                        }
                }
            };
        }
    }
}