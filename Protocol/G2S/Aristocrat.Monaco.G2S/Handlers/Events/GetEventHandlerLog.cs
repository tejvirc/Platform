namespace Aristocrat.Monaco.G2S.Handlers.Events
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.EventHandler;
    using Data.Model;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Handles the v21.getEventHandlerLog G2S message
    /// </summary>
    public class GetEventHandlerLog : ICommandHandler<eventHandler, getEventHandlerLog>
    {
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IG2SEgm _egm;
        private readonly IEventHandlerLogRepository _repository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetEventHandlerLog" /> class.
        ///     Creates a new instance of the GetEventHandlerLog handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="contextFactory">DB context factory</param>
        /// <param name="repository">Event Handler log repository</param>
        public GetEventHandlerLog(
            IG2SEgm egm,
            IMonacoContextFactory contextFactory,
            IEventHandlerLogRepository repository)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<eventHandler, getEventHandlerLog> command)
        {
            return await Sanction.OwnerAndGuests<IEventHandlerDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<eventHandler, getEventHandlerLog> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var eventHandlerDevice = _egm.GetDevice<IEventHandlerDevice>(command.IClass.deviceId);
                if (eventHandlerDevice == null)
                {
                    return;
                }

                var response = command.GenerateResponse<eventHandlerLogList>();

                using (var context = _contextFactory.Create())
                {
                    var logEntries = _repository.Get(context, l => l.HostId == command.HostId);
                    response.Command.eventHandlerLog = logEntries.AsEnumerable()
                        .TakeLogs(command.Command.lastSequence, command.Command.totalEntries)
                        .Select(ConvertEventHandlerLog)
                        .ToArray();
                }
            }

            await Task.CompletedTask;
        }

        private static eventHandlerLog ConvertEventHandlerLog(EventHandlerLog log)
        {
            var result = new eventHandlerLog
            {
                logSequence = log.Id,
                deviceClass = log.DeviceClass,
                deviceId = log.DeviceId,
                eventCode = log.EventCode,
                eventId = log.EventId,
                eventDateTime = log.EventDateTime,
                eventText = EventHandlerExtensions.GetEventText(log.EventCode),
                eventAck = log.EventAck,
                transactionId = log.TransactionId
            };

            var deviceList = !string.IsNullOrEmpty(log.DeviceList)
                ? EventHandlerExtensions.ParseXml<deviceList1>(log.DeviceList)
                : null;

            var deviceLen = deviceList?.statusInfo.Length;

            if (deviceLen != null)
            {
                result.affectedDeviceList = new affectedDeviceList { deviceSelect = new deviceSelect[deviceLen.Value] };

                for (var i = 0; i < deviceLen.Value; i++)
                {
                    var dSel = new deviceSelect
                    {
                        deviceClass = deviceList.statusInfo[i].deviceClass,
                        deviceId = deviceList.statusInfo[i].deviceId
                    };
                    result.affectedDeviceList.deviceSelect[i] = dSel;
                }
            }

            var meterList = result.affectedMeterList?.deviceSelect?.Length;
            if (meterList != null)
            {
                var logMeterList = !string.IsNullOrEmpty(log.MeterList)
                    ? EventHandlerExtensions.ParseXml<meterList>(log.MeterList)
                    : null;

                if (logMeterList != null)
                {
                    result.affectedMeterList =
                        new affectedMeterList { deviceSelect = new deviceSelect[meterList.Value] };

                    for (var i = 0; i < meterList.Value; i++)
                    {
                        var dSel = new deviceSelect
                        {
                            deviceClass =
                                logMeterList.meterInfo[i].currencyMeters.Length > 0
                                    ? logMeterList.meterInfo[i].currencyMeters[0].deviceClass
                                    : logMeterList.meterInfo[i].deviceMeters.Length > 0
                                        ? logMeterList.meterInfo[i].deviceMeters[0].deviceClass
                                        : logMeterList.meterInfo[i].gameDenomMeters.Length > 0
                                            ? logMeterList.meterInfo[i].gameDenomMeters[0].deviceClass
                                            : logMeterList.meterInfo[i].wagerMeters.Length > 0
                                                ? logMeterList.meterInfo[i].wagerMeters[0].deviceClass
                                                : string.Empty,
                            deviceId =
                                logMeterList.meterInfo[i].currencyMeters.Length > 0
                                    ? logMeterList.meterInfo[i].currencyMeters[0].deviceId
                                    : logMeterList.meterInfo[i].deviceMeters.Length > 0
                                        ? logMeterList.meterInfo[i].deviceMeters[0].deviceId
                                        : logMeterList.meterInfo[i].gameDenomMeters.Length > 0
                                            ? logMeterList.meterInfo[i].gameDenomMeters[0].deviceId
                                            : logMeterList.meterInfo[i].wagerMeters.Length > 0
                                                ? logMeterList.meterInfo[i].wagerMeters[0].deviceId
                                                : 0
                        };
                        result.affectedMeterList.deviceSelect[i] = dSel;
                    }
                }
            }

            var transactionList = !string.IsNullOrEmpty(log.TransactionList)
                ? EventHandlerExtensions.ParseXml<transactionList>(log.TransactionList)
                : null;

            var deviceTrans = transactionList?.transactionInfo?.Length;

            if (deviceTrans != null)
            {
                result.affectedTransactionList = new affectedTransactionList
                {
                    transactionSelect = new transactionSelect[deviceTrans.Value]
                };

                for (var i = 0; i < deviceTrans.Value; i++)
                {
                    var tSel = new transactionSelect
                    {
                        deviceClass = transactionList.transactionInfo[i].deviceClass,
                        deviceId = transactionList.transactionInfo[i].deviceId,
                        transactionId = log.TransactionId
                    };
                    result.affectedTransactionList.transactionSelect[i] = tSel;
                }
            }

            return result;
        }
    }
}