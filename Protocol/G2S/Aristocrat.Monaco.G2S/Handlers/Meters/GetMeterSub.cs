namespace Aristocrat.Monaco.G2S.Handlers.Meters
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using G2S.Meters;

    /// <summary>
    ///     Handles the v21.getMeterSub G2S message
    /// </summary>
    public class GetMeterSub : ICommandHandler<meters, getMeterSub>
    {
        private readonly IG2SEgm _egm;
        private readonly IMetersSubscriptionManager _metersSubscriptionManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetMeterSub" /> class.
        ///     Creates a new instance of the GetMeterSub handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="metersSubscriptionManager">Meters subscription manager.</param>
        public GetMeterSub(IG2SEgm egm, IMetersSubscriptionManager metersSubscriptionManager)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _metersSubscriptionManager = metersSubscriptionManager ??
                                         throw new ArgumentNullException(nameof(metersSubscriptionManager));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<meters, getMeterSub> command)
        {
            return await Sanction.OwnerAndGuests<IMetersDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<meters, getMeterSub> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var response = command.GenerateResponse<meterSubList>();
                //// TODO: save listStateDateTime to DB
                //// response.Command.listStateDateTime = DateTime.UtcNow;
                //// response.Command.listStateDateTimeSpecified = true;
                response.Command.meterSubType = command.Command.meterSubType;

                //// TODO: Audit meters
                var type = command.Command.meterSubType == MeterInfoType.Periodic
                    ? MetersSubscriptionType.Periodic
                    : MetersSubscriptionType.EndOfDay;
                var metersSubs = _metersSubscriptionManager.GetMeterSub(command.HostId, type)?.ToList();

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

                    response.Command.getCurrencyMeters =
                        metersSubs.Where(item => item.MeterType == MeterType.Currency).Select(
                            s => new getCurrencyMeters
                            {
                                deviceId = s.DeviceId,
                                deviceClass = s.ClassName,
                                meterDefinitions = s.MeterDefinition
                            }).ToArray();

                    response.Command.getDeviceMeters =
                        metersSubs.Where(item => item.MeterType == MeterType.Device).Select(
                            s => new getDeviceMeters
                            {
                                deviceId = s.DeviceId,
                                deviceClass = s.ClassName,
                                meterDefinitions = s.MeterDefinition
                            }).ToArray();

                    response.Command.getGameDenomMeters =
                        metersSubs.Where(item => item.MeterType == MeterType.Game).Select(
                            s => new getGameDenomMeters
                            {
                                deviceId = s.DeviceId,
                                deviceClass = s.ClassName,
                                meterDefinitions = s.MeterDefinition
                            }).ToArray();

                    response.Command.getWagerMeters =
                        metersSubs.Where(item => item.MeterType == MeterType.Wage).Select(
                            s => new getWagerMeters
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
    }
}