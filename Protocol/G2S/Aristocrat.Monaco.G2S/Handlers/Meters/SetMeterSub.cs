namespace Aristocrat.Monaco.G2S.Handlers.Meters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using G2S.Meters;

    /// <summary>
    ///     Handles the v21.setMeterSub G2S message
    /// </summary>
    public class SetMeterSub : ICommandHandler<meters, setMeterSub>
    {
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IMetersSubscriptionManager _metersSubscriptionManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetMeterSub" /> class.
        ///     Creates a new instance of the SetMeterSub handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="metersSubscriptionManager">Meters subscription manager.</param>
        /// <param name="eventLift">Event lift.</param>
        public SetMeterSub(IG2SEgm egm, IMetersSubscriptionManager metersSubscriptionManager, IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _metersSubscriptionManager = metersSubscriptionManager ??
                                         throw new ArgumentNullException(nameof(metersSubscriptionManager));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<meters, setMeterSub> command)
        {
            return await Sanction.OwnerAndGuests<IMetersDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<meters, setMeterSub> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                // Invalid meterSubType Specified
                if (command.Command.meterSubType != MeterSubscriptionType.EndOfDay &&
                    command.Command.meterSubType != MeterSubscriptionType.Periodic)
                {
                    command.Error.Code = ErrorCode.G2S_MTX001;
                    return;
                }

                var device = _egm.GetDevice<IMetersDevice>(command.IClass.deviceId);

                if (device == null)
                {
                    return;
                }

                var cmd = command.Command;
                var subscriptions = new List<MeterSubscription>();

                if (command.Command.getCurrencyMeters != null)
                {
                    foreach (var s in command.Command.getCurrencyMeters)
                    {
                        subscriptions.Add(GetMeterSub(cmd, command.HostId, s, MeterType.Currency));
                    }
                }

                if (command.Command.getGameDenomMeters != null)
                {
                    foreach (var s in command.Command.getGameDenomMeters)
                    {
                        subscriptions.Add(GetMeterSub(cmd, command.HostId, s, MeterType.Game));
                    }
                }

                if (command.Command.getDeviceMeters != null)
                {
                    foreach (var s in command.Command.getDeviceMeters)
                    {
                        subscriptions.Add(GetMeterSub(cmd, command.HostId, s, MeterType.Device));
                    }
                }

                if (command.Command.getWagerMeters != null)
                {
                    foreach (var s in command.Command.getWagerMeters)
                    {
                        subscriptions.Add(GetMeterSub(cmd, command.HostId, s, MeterType.Wage));
                    }
                }

                _metersSubscriptionManager.SetMetersSubscription(
                    command.HostId,
                    cmd.meterSubType == MeterInfoType.Periodic
                        ? MetersSubscriptionType.Periodic
                        : MetersSubscriptionType.EndOfDay,
                    subscriptions);

                var response = command.GenerateResponse<meterSubList>();
                //// TODO: save listStateDateTime to DB
                response.Command.listStateDateTime = DateTime.UtcNow;
                response.Command.listStateDateTimeSpecified = true;
                response.Command.meterSubType = command.Command.meterSubType;

                //// TODO: Audit meters
                var type = command.Command.meterSubType == MeterInfoType.Periodic
                    ? MetersSubscriptionType.Periodic
                    : MetersSubscriptionType.EndOfDay;
                var metersSubs = _metersSubscriptionManager.GetMeterSub(command.HostId, type);

                if (metersSubs != null && metersSubs.Any())
                {
                    var meter = metersSubs.First();

                    response.Command.onCoinDrop = meter.OnCoinDrop;
                    response.Command.onDoorOpen = meter.OnDoorOpen;
                    response.Command.onEOD = meter.OnEndOfDay;
                    response.Command.onNoteDrop = meter.OnNoteDrop;
                    response.Command.eodBase = meter.SubType == MetersSubscriptionType.EndOfDay ? meter.Base : 0;
                    response.Command.periodicBase = meter.SubType == MetersSubscriptionType.Periodic ? meter.Base : 0;
                    response.Command.periodicInterval = meter.PeriodInterval;

                    _eventLift.Report(device, EventCode.G2S_MTE100);

                    response.Command.getCurrencyMeters =
                        metersSubs.Where(item => item.MeterType == MeterType.Currency).Select(
                            s =>
                                new getCurrencyMeters
                                {
                                    deviceId = s.DeviceId,
                                    deviceClass = s.ClassName,
                                    meterDefinitions = s.MeterDefinition
                                }).ToArray();

                    response.Command.getDeviceMeters =
                        metersSubs.Where(item => item.MeterType == MeterType.Device).Select(
                            s =>
                                new getDeviceMeters
                                {
                                    deviceId = s.DeviceId,
                                    deviceClass = s.ClassName,
                                    meterDefinitions = s.MeterDefinition
                                }).ToArray();

                    response.Command.getGameDenomMeters =
                        metersSubs.Where(item => item.MeterType == MeterType.Game).Select(
                            s =>
                                new getGameDenomMeters
                                {
                                    deviceId = s.DeviceId,
                                    deviceClass = s.ClassName,
                                    meterDefinitions = s.MeterDefinition
                                }).ToArray();

                    response.Command.getWagerMeters =
                        metersSubs.Where(item => item.MeterType == MeterType.Wage).Select(
                            s =>
                                new getWagerMeters
                                {
                                    deviceId = s.DeviceId,
                                    deviceClass = s.ClassName,
                                    meterDefinitions = s.MeterDefinition
                                }).ToArray();
                }
                else
                {
                    response.Command.onCoinDrop = false;
                    response.Command.onDoorOpen = false;
                    response.Command.onEOD = false;
                    response.Command.onNoteDrop = false;
                    response.Command.eodBase = 0;
                    response.Command.periodicBase = 0;
                    response.Command.periodicInterval = 90000;
                }
            }

            await Task.CompletedTask;
        }

        private MeterSubscription GetMeterSub(setMeterSub cmd, int hostId, c_getMeters s, MeterType type)
        {
            return new MeterSubscription
            {
                Base = cmd.meterSubType == MeterInfoType.EndOfDay ? cmd.eodBase : cmd.periodicBase,
                ClassName = s.deviceClass,
                DeviceId = s.deviceId,
                HostId = hostId,
                MeterDefinition = s.meterDefinitions,
                MeterType = type,
                OnCoinDrop = cmd.onCoinDrop,
                OnDoorOpen = cmd.onDoorOpen,
                OnEndOfDay = cmd.meterSubType != MeterInfoType.Periodic && cmd.onEOD,
                OnNoteDrop = cmd.onNoteDrop,
                PeriodInterval = cmd.periodicInterval,
                LastAckedTime = DateTime.Now,
                SubType =
                    cmd.meterSubType == MeterInfoType.Periodic
                        ? MetersSubscriptionType.Periodic
                        : MetersSubscriptionType.EndOfDay
            };
        }
    }
}