namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;
    using Handlers;
    using Kernel;
    using Meters;

    /// <summary>
    ///     Handles the <see cref="PrimaryGameStartedEvent" /> event.
    /// </summary>
    public class PrimaryGameStartedConsumer : GamePlayConsumerBase<PrimaryGameStartedEvent>
    {
        private readonly IMeterAggregator<ICabinetDevice> _cabinetMeters;
        private readonly ICommandBuilder<ICabinetDevice, cabinetStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IGameHistory _history;
        private readonly IGameProvider _provider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrimaryGameStartedConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}" /> instance.</param>
        /// <param name="cabinetMeters">An <see cref="IMeterAggregator&lt;ICabinetDevice&gt;" /> instance.</param>
        /// <param name="provider">An <see cref="IGameProvider" /> instance.</param>
        /// <param name="history">An <see cref="IGameHistory" /> instance.</param>
        /// <param name="gameMeters">An <see cref="IMeterManager" /> instance.</param>
        /// <param name="eventLift">An <see cref="IEvent" /> instance.</param>
        public PrimaryGameStartedConsumer(
            IG2SEgm egm,
            ICommandBuilder<ICabinetDevice, cabinetStatus> commandBuilder,
            IMeterAggregator<ICabinetDevice> cabinetMeters,
            IGameProvider provider,
            IGameHistory history,
            IGameMeterManager gameMeters,
            IEventLift eventLift)
            : base(egm, eventLift, gameMeters, EventCode.G2S_GPE103)
        {
            _egm = egm;
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _cabinetMeters = cabinetMeters ?? throw new ArgumentNullException(nameof(cabinetMeters));
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _history = history ?? throw new ArgumentNullException(nameof(history));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public override void Consume(PrimaryGameStartedEvent theEvent)
        {
            var device = _egm.GetDevice<ICabinetDevice>();

            var game = _provider.GetGame(theEvent.GameId);

            var currentLog = _history.GetByIndex(theEvent.Log.StorageIndex - 1);

            if (game?.Id != currentLog?.GameId || theEvent.Denomination != currentLog?.DenomId)
            {
                var status = new cabinetStatus();

                _commandBuilder.Build(device, status);

                _eventLift.Report(device, EventCode.G2S_CBE314, device.DeviceList(status));
            }

            base.Consume(theEvent);
        }

        /// <inheritdoc />
        protected override meterList GetMeters(IGamePlayDevice device, long denomination, string wagerCategory)
        {
            var cabinet = _egm.GetDevice<ICabinetDevice>();

            var cabinetMeters = _cabinetMeters.GetMeters(
                cabinet,
                CabinetMeterName.PlayerCashableAmount,
                CabinetMeterName.PlayerPromoAmount,
                CabinetMeterName.PlayerNonCashableAmount,
                CabinetMeterName.WageredCashableAmount,
                CabinetMeterName.WageredPromoAmount,
                CabinetMeterName.WageredNonCashableAmount);

            var gameMeters = InternalGetMeters(device, denomination, wagerCategory);

            return new meterList { meterInfo = cabinetMeters.Concat(gameMeters).ToArray() };
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
                                        PerformanceMeterName.WageredAmount,
                                        PerformanceMeterName.AveragePaybackPercent,
                                        PerformanceMeterName.TheoreticalPaybackAmount).ToArray()
                            },
                            new deviceMeters
                            {
                                deviceClass = device.PrefixedDeviceClass(),
                                deviceId = 0,
                                simpleMeter =
                                    GetSimpleMeters(
                                        PerformanceMeterName.WageredAmount,
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