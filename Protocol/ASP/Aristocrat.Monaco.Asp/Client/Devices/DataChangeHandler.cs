namespace Aristocrat.Monaco.Asp.Client.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    public partial class ParameterProcessor
    {
        private class DataChangeHandler
        {
            public int ClassId => ParameterPrototype.ClassId.Id;
            public int TypeId => ParameterPrototype.TypeId.Id;
            public int ParamId => ParameterPrototype.Id;
            public IParameterPrototype ParameterPrototype { get; set; }
            public Action<(DataChangeHandler, Dictionary<string, object>)> EventAction { get; set; }
            private bool EventRegistered { get; set; }

            private void MemberValueChanged(object source, Dictionary<string, object> members)
            {
                if (ParameterPrototype.FieldsPrototype.FirstOrDefault(x => members.ContainsKey(x.DataMemberName)) != null)
                {
                    EventAction?.Invoke((this, members));
                }
            }

            public void Subscribe(IEnumerable<IDataSource> dataSources)
            {
                if (EventRegistered)
                {
                    return;
                }

                foreach (var dataSource in dataSources)
                {
                    dataSource.MemberValueChanged += MemberValueChanged;
                }

                EventRegistered = true;
            }

            public void UnSubscribe(IEnumerable<IDataSource> dataSources)
            {
                if (!EventRegistered)
                {
                    return;
                }

                foreach (var dataSource in dataSources)
                {
                    dataSource.MemberValueChanged -= MemberValueChanged;
                }

                EventRegistered = false;
            }
        }
    }
}