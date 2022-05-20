namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using Contracts;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MciPropertyDataSource : IDataSource
    {
        private readonly Dictionary<string, object> _mciData;


        public string Name { get; } = "MciProperty";
        public IReadOnlyList<string> Members => _mciData.Keys.ToList();
        public event EventHandler<Dictionary<string, object>> MemberValueChanged = (sender, s) => { };
        public MciPropertyDataSource()
        {
            _mciData = new Dictionary<string, object>
            {
                { "Mci_Asp_Ver", null },
                { "Mci_Model",  0 },
                { "Mci_Firmware_Id", null},
                { "Mci_Firmware_Ver_No", null },
                { "Mci_Floor_No", null },
            };
        }

        public object GetMemberValue(string member)
        {
            return _mciData[member];
        }

        public void SetMemberValue(string member, object value)
        {
            _mciData[member] = value;
        }
    }
}