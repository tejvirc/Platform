namespace Aristocrat.Monaco.Asp.Client.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Contracts;

    public partial class ParameterProcessor : IParameterProcessor
    {
        private readonly Dictionary<int, DataChangeHandler> _eventHandlers = new Dictionary<int, DataChangeHandler>();

        public ParameterProcessor(IParameterFactory factory)
        {
            ParameterFactory = factory;
            SetAlwaysReportEventHandlers();
        }

        private IParameterFactory ParameterFactory { get; }

        public event EventHandler<IParameter> ParameterEvent;

        public bool ClearEvent(IParameter parameter)
        {
            if (parameter.EventAccessType != EventAccessType.OnRequest)
            {
                return false;
            }

            SetEventHandler(parameter.Prototype, false);
            return true;
        }

        public bool SetEvent(IParameter parameter)
        {
            if (parameter.EventAccessType != EventAccessType.OnRequest)
            {
                return false;
            }

            SetEventHandler(parameter.Prototype);
            return true;
        }

        public IParameter GetParameter(IParameter parameter)
        {
            parameter.Load();
            return parameter;
        }

        public bool SetParameter(IParameter parameter)
        {
            if (parameter.MciAccessType != AccessType.ReadWrite)
            {
                return false;
            }

            try
            {
                parameter.Save();
                return true;
            }
            catch
            {
                // ignored
            }

            return false;
        }

        private static int IdToKey(IParameterPrototype parameter)
        {
            return (parameter.ClassId.Id << 16) | (parameter.TypeId.Id << 8) | parameter.Id;
        }

        private void SetAlwaysReportEventHandlers()
        {
            var parameters =
                ParameterFactory.SelectParameterPrototypes(
                    p => p.EventAccessType == EventAccessType.Always);

            foreach (var param in parameters)
            {
                SetEventHandler(param);
            }
        }

        private void SetEventHandler(IParameterPrototype parameter, bool bSet = true)
        {
            Debug.Assert(
                parameter.EventAccessType == EventAccessType.Always ||
                parameter.EventAccessType == EventAccessType.OnRequest);
            var uniqueDataSources = parameter.FieldsPrototype.Select(x => x.DataSource).Distinct();
            var handler = GetHandler(parameter);
            if (bSet)
            {
                handler.Subscribe(uniqueDataSources);
            }
            else
            {
                handler.UnSubscribe(uniqueDataSources);
            }
        }

        private DataChangeHandler GetHandler(IParameterPrototype parameter)
        {
            var key = IdToKey(parameter);
            DataChangeHandler handler;

            lock (_eventHandlers)
            {
                if (_eventHandlers.TryGetValue(key, out handler))
                {
                    return handler;
                }

                handler = new DataChangeHandler { ParameterPrototype = parameter, EventAction = OnEvent };
                _eventHandlers[key] = handler;
            }

            return handler;
        }

        private void OnEvent((DataChangeHandler handler, Dictionary<string, object> members) evt)
        {
            var param = ParameterFactory.Create(evt.handler.ClassId, evt.handler.TypeId, evt.handler.ParamId);
            param.SetFields(evt.members);
            ParameterEvent?.Invoke(this, param);
        }
    }
}