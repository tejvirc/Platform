namespace Aristocrat.Monaco.G2S.Handlers.Meters
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using G2S.Meters;

    /// <summary>
    ///     Handles the v21.clearMeterSub G2S message
    /// </summary>
    public class ClearMeterSub : ICommandHandler<meters, clearMeterSub>
    {
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IMetersSubscriptionManager _metersSubscriptionManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ClearMeterSub" /> class.
        ///     Creates a new instance of the ClearMeterSub handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="metersSubscriptionManager">Meters subscription manager.</param>
        /// <param name="eventLift">Event lift.</param>
        public ClearMeterSub(IG2SEgm egm, IMetersSubscriptionManager metersSubscriptionManager, IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _metersSubscriptionManager = metersSubscriptionManager ??
                                         throw new ArgumentNullException(nameof(metersSubscriptionManager));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<meters, clearMeterSub> command)
        {
            return await Sanction.OwnerAndGuests<IMetersDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<meters, clearMeterSub> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var device = _egm.GetDevice<IMetersDevice>(command.IClass.deviceId);

                if (device == null)
                {
                    return;
                }

                //// TODO: Audit meters
                var type = command.Command.meterSubType == MeterInfoType.Periodic
                    ? MetersSubscriptionType.Periodic
                    : MetersSubscriptionType.EndOfDay;

                var meter = _metersSubscriptionManager.ClearSubscriptions(command.HostId, type);

                _eventLift.Report(device, EventCode.G2S_MTE101);

                var response = command.GenerateResponse<meterSubList>();

                response.Command.listStateDateTime = DateTime.UtcNow;
                response.Command.listStateDateTimeSpecified = true;
                response.Command.meterSubType = command.Command.meterSubType;
                if (meter != null)
                {
                    response.Command.onCoinDrop = meter.OnCoinDrop;
                    response.Command.onDoorOpen = meter.OnDoorOpen;
                    response.Command.onEOD = meter.OnEndOfDay;
                    response.Command.onNoteDrop = meter.OnNoteDrop;
                    response.Command.eodBase = meter.SubType == MetersSubscriptionType.EndOfDay ? meter.Base : 0;
                    response.Command.periodicBase = meter.SubType == MetersSubscriptionType.Periodic ? meter.Base : 0;
                    response.Command.periodicInterval = meter.PeriodInterval;
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
    }
}