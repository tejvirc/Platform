namespace Aristocrat.G2S.Client.Devices.v21
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Protocol.v21;

    /// <summary>
    /// </summary>
    public class PlayerDevice : ClientDeviceBase<player>, IPlayerDevice
    {
        /// <summary>
        ///     The message duration default
        /// </summary>
        public const int MessageDurationDefault = 30000;

        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerDevice" /> class.
        /// </summary>
        /// <param name="deviceStateObserver">An <see cref="IDeviceObserver" /> instance.</param>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="idReaderDevices">The id reader devices</param>
        /// <param name="eventLift">The event lift</param>
        public PlayerDevice(int deviceId, IDeviceObserver deviceStateObserver, IEnumerable<IIdReaderDevice> idReaderDevices, IEventLift eventLift)
            : base(deviceId, deviceStateObserver, false)
        {
            SetDefaults();

            var devices = idReaderDevices.ToList();

            IdReader = devices.FirstOrDefault()?.Id ?? 0;
            IdReaders = devices.Select(i => i.Id).ToList();

            _eventLift = eventLift;
        }

        /// <inheritdoc />
        public bool RestartStatus { get; private set; }

        /// <inheritdoc />
        public bool DisplayPresent { get; private set; }

        /// <inheritdoc />
        public int MessageDuration { get; private set; }

        /// <inheritdoc />
        public bool MeterDeltaSupported { get; private set; }

        /// <inheritdoc />
        public bool SendMeterDelta { get; private set; }

        /// <inheritdoc />
        public int IdReader { get; private set; }

        /// <inheritdoc />
        public bool UseMultipleIdDevices { get; private set; }

        /// <inheritdoc />
        public IEnumerable<int> IdReaders { get; private set; }

        /// <inheritdoc />
        public IEnumerable<meterDeltaHostSubscription> SubscribedMeters { get; private set; }

        /// <inheritdoc />
        public void SetMeterSubscription(IEnumerable<meterDeltaHostSubscription> subscription)
        {
            SubscribedMeters = subscription;

            _eventLift.Report(this, EventCode.G2S_PRE301);
        }

        /// <inheritdoc />
        public (bool timedOut, playerSessionStartAck response) StartSession(
            long transactionId,
            string idReaderType,
            string idNumber,
            string playerId,
            DateTime startDateTime)
        {
            var command = new playerSessionStart
            {
                transactionId = transactionId,
                idReaderType = idReaderType,
                idNumber = idNumber,
                playerId = playerId,
                startDateTime = startDateTime
            };

            var request = InternalCreateClass();
            request.Item = command;

            var session = SendRequest(request, 0, true);
            session.WaitForCompletion(); // wait for the session to complete

            if (session.SessionState == SessionStatus.Success && session.Responses.Count > 0)
            {
                if (session.Responses[0].IClass.Item is playerSessionStartAck ack)
                {
                    return (false, ack);
                }
            }

            return (session.SessionState == SessionStatus.TimedOut, null);
        }

        /// <inheritdoc />
        public long? EndSession<T>(T sessionEnd) where T : c_baseCommand
        {
            var request = InternalCreateClass();
            request.Item = sessionEnd;

            var session = SendRequest(request, 0, true);
            session.WaitForCompletion(); // wait for the session to complete

            if (session.SessionState == SessionStatus.Success && session.Responses.Count > 0)
            {
                if (session.Responses[0].IClass.Item is playerSessionEndAck ack)
                {
                    return ack.transactionId;
                }
            }

            return null;
        }

        /// <inheritdoc />
        public override void Close()
        {
        }

        /// <inheritdoc />
        public override void Open(IStartupContext context)
        {
        }

        /// <inheritdoc />
        public override void ApplyOptions(DeviceOptionConfigValues optionConfigValues)
        {
            base.ApplyOptions(optionConfigValues);

            SetDeviceValue(
                G2SParametersNames.RestartStatusParameterName,
                optionConfigValues,
                parameterId => { RestartStatus = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.UseDefaultConfigParameterName,
                optionConfigValues,
                parameterId => { UseDefaultConfig = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.RequiredForPlayParameterName,
                optionConfigValues,
                parameterId => { RequiredForPlay = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.PlayerDevice.MessageDurationParameterName,
                optionConfigValues,
                parameterId => { MessageDuration = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.PlayerDevice.SendMeterDeltaParameterName,
                optionConfigValues,
                parameterId => { SendMeterDelta = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.PlayerDevice.IdReaderIdParameterName,
                optionConfigValues,
                parameterId => { IdReader = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.PlayerDevice.MultipleValidationDevicesParameterName,
                optionConfigValues,
                parameterId => { UseMultipleIdDevices = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.PlayerDevice.PlayerValidationDeviceParameterName,
                optionConfigValues,
                parameterId =>
                {
                    var table = optionConfigValues.GetTableValue(
                        G2SParametersNames.PlayerDevice.PlayerValidationDeviceParameterName);

                    var devices = new List<int>();

                    foreach (var tableRow in table)
                    {
                        if (tableRow.GetDeviceOptionConfigValue(G2SParametersNames.PlayerDevice.IdReaderLinkedParameterName)
                            .BooleanValue())
                        {
                            devices.Add(tableRow
                                .GetDeviceOptionConfigValue(G2SParametersNames.PlayerDevice.IdReaderIdParameterName)
                                .Int32Value());
                        }
                    }

                    IdReaders = devices;
                });
        }

        /// <inheritdoc />
        public override void RegisterEvents()
        {
            var deviceClass = this.PrefixedDeviceClass();

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE001);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE002);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE004);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE005);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE006);

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE100);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE101); // TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE102); // TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE103);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE104);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE105); // TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE106); // TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE107); // TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE108); // TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE109); // TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE110); // TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE111); // TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE112);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE113); // TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE114);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE115); // TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE116); // TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE117); // TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE118); // TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE119); // TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE120); // TODO

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE300); // TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_PRE301); // TODO
        }

        private void SetDefaults()
        {
            RestartStatus = true;
            RequiredForPlay = false;

            DisplayPresent = false;
            IdReader = 0;

            MeterDeltaSupported = true;
            MessageDuration = MessageDurationDefault;
            SendMeterDelta = false;
            UseMultipleIdDevices = true;  
            IdReaders = new List<int>();
            SubscribedMeters = new List<meterDeltaHostSubscription>();
        }
    }
}
