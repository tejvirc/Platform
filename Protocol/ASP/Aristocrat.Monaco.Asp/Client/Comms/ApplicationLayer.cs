namespace Aristocrat.Monaco.Asp.Client.Comms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Localization;
    using Contracts;
    using Events;
    using Kernel;
    using Kernel.Contracts.MessageDisplay;
    using Localization.Properties;
    using Utilities;

    /// <summary>
    ///     Asp protocol application layer. Responsible for processing/sending application messages.
    /// </summary>
    public class ApplicationLayer : TransportLayer
    {
        private static readonly Guid ProgressiveLinkDownGuid = new Guid("{A9ED9439-946E-4FCC-97D0-121EC418A771}");
        private readonly Dictionary<AppCommandTypes, Action<IParameter>> _commandProcessor;
        private readonly IParameterFactory _factory;
        private readonly ApplicationMessage _message = new ApplicationMessage();
        private readonly IParameterProcessor _processor;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly IEventBus _eventBus;
        private readonly IReportableEventsManager _reportableEventsManager;

        public ApplicationLayer(
            ICommPort port,
            IParameterProcessor processor,
            IParameterFactory factory,
            ISystemDisableManager systemDisableManager,
            IEventBus eventBus,
            IReportableEventsManager reportableEventsManager)
            : base(port)
        {
            _processor = processor;
            _factory = factory;
            _systemDisableManager = systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _reportableEventsManager = reportableEventsManager ?? throw new ArgumentNullException(nameof(reportableEventsManager));
            _processor.ParameterEvent += _processor_ParameterEvent;
            _commandProcessor = new Dictionary<AppCommandTypes, Action<IParameter>>
            {
                { AppCommandTypes.SetParameter, SetParameter },
                { AppCommandTypes.GetParameter, GetParameter },
                { AppCommandTypes.SetEvent, SetEvent },
                { AppCommandTypes.ClearEvent, ClearEvent }
            };

            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            ApplySetEvents();
        }

        private void ApplySetEvents()
        {
            var setEvents = _reportableEventsManager.GetAll()
                .Select(s => CreateParameter(s.@class, s.type, s.parameter))
                .ToList();

            if (setEvents.Any()) SetEvents(setEvents, null);
        }

        public uint CurrentTime => DateTimeHelper.GetNumberOfSecondsSince1990();

        protected override void OnLinkStatusChanged()
        {
            if (IsLinkUp)
            {
                _systemDisableManager.Enable(
                    ProgressiveLinkDownGuid);
            }
            else
            {
                _systemDisableManager.Disable(
                    ProgressiveLinkDownGuid,
                    SystemDisablePriority.Normal,
                    ResourceKeys.ProgressiveDisconnectText,
                    CultureProviderType.Player);
            }

            _eventBus.Publish(new LinkStatusChangedEvent(IsLinkUp));
            base.OnLinkStatusChanged();
        }

        private void _processor_ParameterEvent(object source, IParameter parameter)
        {
            var eventReport = new AppEventResponse
            {
                Class = (byte)parameter.ClassId.Id,
                Type = (byte)parameter.TypeId.Id,
                Param = (byte)parameter.Id,
                Command = AppResponseTypes.EventReport,
                TimeStamp = CurrentTime,
                DataSize = parameter.SizeInBytes
            };
            parameter.WriteBytes(eventReport.DataWriter);
            QueueEvent(eventReport);
        }

        private IParameter CreateParameter(ApplicationMessage message)
        {
            return _factory.Create(message.Class, message.Type, message.Param);
        }

        private IParameter CreateParameter(byte @class, byte type, byte parameter)
        {
            return _factory.Create(@class, type, parameter);
        }

        private bool IsValidCommandType(AppCommandTypes type)
        {
            return type == AppCommandTypes.ClearEvent || type == AppCommandTypes.GetParameter
                                                      || type == AppCommandTypes.SetEvent ||
                                                      type == AppCommandTypes.SetParameter;
        }

        private static AppResponseTypes ToResponseType(AppCommandTypes type)
        {
            switch (type)
            {
                case AppCommandTypes.SetParameter:
                    return AppResponseTypes.SetParameterAck;
                case AppCommandTypes.GetParameter:
                    return AppResponseTypes.GetParameterAck;
                case AppCommandTypes.SetEvent:
                    return AppResponseTypes.SetEventAck;
                case AppCommandTypes.ClearEvent:
                    return AppResponseTypes.ClearEventAck;
                default:
                    return (AppResponseTypes)type;
            }
        }

        private static T CreateResponse<T>(
            ApplicationMessage msg,
            AppResponseStatus status = AppResponseStatus.ValidResponse)
            where T : AppResponse, new()
        {
            return new T
            {
                Class = msg.Class,
                Type = msg.Type,
                Param = msg.Param,
                Command = ToResponseType(msg.Command),
                ResponseStatus = status
            };
        }

        private void EnqueueResponse(ApplicationMessage msg, AppResponseStatus status = AppResponseStatus.ValidResponse)
        {
            QueueResponse(CreateResponse<AppResponse>(msg, status));
        }

        private void ClearEvent(IParameter parameter)
        {
            if (_processor.ClearEvent(parameter))
            {
                QueueResponse(CreateResponse<AppResponse>(_message));

                _reportableEventsManager.Clear(_message.Class, _message.Type, _message.Param);
            }
            else
            {
                EnqueueResponse(_message, AppResponseStatus.UnsupportedParameter);
            }
        }

        private void ClearEvents(IEnumerable<IParameter> parameters, IParameter parameter)
        {
            var updated = new List<(byte @class, byte type, byte parameter)>();

            foreach (var m in parameters)
            {
                var result = _processor.ClearEvent(m);
                if (result) updated.Add(((byte)m.ClassId.Id, (byte)m.TypeId.Id, (byte)m.Id));

                if (m == parameter)
                {
                    if (result) QueueResponse(CreateResponse<AppResponse>(_message));
                    else EnqueueResponse(_message, AppResponseStatus.UnsupportedParameter);
                }
            }

            if (updated.Any()) _reportableEventsManager.ClearBatch(updated);
        }

        private void SetEvent(IParameter parameter)
        {
            if (_processor.SetEvent(parameter))
            {
                QueueResponse(CreateResponse<AppResponse>(_message));

                _reportableEventsManager.Set(_message.Class, _message.Type, _message.Param);
            }
            else
            {
                EnqueueResponse(_message, AppResponseStatus.UnsupportedParameter);
            }
        }

        private void SetEvents(IEnumerable<IParameter> parameters, IParameter parameter)
        {
            var updated = new List<(byte @class, byte type, byte parameter)>();

            foreach (var m in parameters)
            {
                var result = _processor.SetEvent(m);
                if (result) updated.Add(((byte)m.ClassId.Id, (byte)m.TypeId.Id, (byte)m.Id));

                if (m == parameter)
                {
                    if (result) QueueResponse(CreateResponse<AppResponse>(_message));
                    else EnqueueResponse(_message, AppResponseStatus.UnsupportedParameter);
                }
            }

            if (updated.Any()) _reportableEventsManager.SetBatch(updated);
        }

        private void GetParameter(IParameter parameter)
        {
            var response = _processor.GetParameter(parameter);
            var resp = CreateResponse<AppDataResponse>(_message);
            resp.DataSize = response.SizeInBytes;
            response.WriteBytes(resp.DataWriter);
            QueueResponse(resp);
        }

        private void SetParameter(IParameter parameter)
        {
            parameter.ReadBytes(_message.Reader);
            if (_processor.SetParameter(parameter))
            {
                EnqueueResponse(_message);
            }
            else
            {
                EnqueueResponse(_message, AppResponseStatus.InvalidParameterData);
            }
        }

        private AppResponseStatus CheckMessageValidity(ApplicationMessage msg)
        {
            if (!IsValidCommandType(msg.Command))
            {
                return AppResponseStatus.UnsupportedCommand;
            }

            var (classExists, typeExists, paramExists) = _factory.Exists(msg.Class, msg.Type, msg.Param);
            if (!classExists)
            {
                return AppResponseStatus.UnsupportedClass;
            }

            if (!typeExists)
            {
                return AppResponseStatus.UnsupportedType;
            }

            return paramExists ? AppResponseStatus.ValidResponse : AppResponseStatus.UnsupportedParameter;
        }

        protected override void ProcessPacket(TransportMessage packet)
        {
            _message.Reset(packet);
            var status = CheckMessageValidity(_message);
            if (status != AppResponseStatus.ValidResponse)
            {
                EnqueueResponse(_message, status);
            }
            else
            {
                var configParameter = CreateParameter(_message);
                var classId = configParameter.Prototype.ClassId;
                var deviceClass = _factory.DeviceDefinition.Classes.Find(m => m.Id == classId.Id);

                if ((_message.Command == AppCommandTypes.SetEvent || _message.Command == AppCommandTypes.ClearEvent) &&
                    deviceClass.SharedDeviceTypeEventReport)
                {
                    var parameters = deviceClass.DeviceTypes
                        .Select(m => CreateParameter((byte)classId.Id, (byte)m.Id, _message.Param))
                        .ToList();

                    var notifyParameter = parameters.Single(m => m.TypeId.Id == _message.Type);

                    switch (_message.Command)
                    {
                        case AppCommandTypes.SetEvent:
                            SetEvents(parameters, notifyParameter);
                            break;
                        case AppCommandTypes.ClearEvent:
                            ClearEvents(parameters, notifyParameter);
                            break;
                    }
                }
                else
                {
                    _commandProcessor[_message.Command](configParameter);
                }
            }
        }
    }
}