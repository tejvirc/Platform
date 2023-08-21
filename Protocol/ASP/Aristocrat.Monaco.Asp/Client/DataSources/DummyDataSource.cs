namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Extensions;

    public class DummyDataSource : IDataSource
    {
        public Dictionary<string, object> MemberValues { get; set; } = new Dictionary<string, object>();

        public string Name { get; set; } = "DummyDataSource";

        public IReadOnlyList<string> Members => MemberValues.Keys.ToList();

        public event EventHandler<Dictionary<string, object>> MemberValueChanged;

        public object GetMemberValue(string member)
        {
            MemberValues.TryGetValue(member, out var value);
            return value;
        }

        public void SetMemberValue(string member, object value)
        {
            if (MemberValues.ContainsKey(member))
            {
                MemberValues[member] = value;
            }
            else
            {
                MemberValues.Add(member, value);
            }
        }

        public void ChangeDataMember(string member, object value)
        {
            SetMemberValue(member, value);
            MemberValueChanged?.Invoke(this, this.GetMemberSnapshot(member));
        }
    }
}