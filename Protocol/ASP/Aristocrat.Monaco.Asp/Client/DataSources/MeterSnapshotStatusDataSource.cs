namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using HandlerMap =
        System.Collections.Generic.Dictionary<string, (System.Func<object> Getter, System.Func<object, bool> Setter)>;

    /// <summary>
    ///     This class provides the snapshot status as per the definition in Class 2 Type 3 Parameter 2 in ASP5000 protocol document.
    ///     It also communicates with MeterSnapshotProvider to update the snapshot status.
    /// </summary>
    public class MeterSnapshotStatusDataSource : IDataSource
    {
        private readonly HandlerMap _handlers;
        private readonly IMeterSnapshotProvider _meterSnapshotProvider;

        public MeterSnapshotStatusDataSource(IMeterSnapshotProvider meterSnapshotProvider)
        {
            _meterSnapshotProvider =
                meterSnapshotProvider ?? throw new ArgumentException(nameof(meterSnapshotProvider));
            _handlers = GetMemberMap();
        }

        public IReadOnlyList<string> Members => _handlers.Keys.ToList();

        public string Name => "MeterSnapShotUpdate";

        public event EventHandler<Dictionary<string, object>> MemberValueChanged = (sender, s) => { };

        public object GetMemberValue(string member)
        {
            return _handlers[member].Getter();
        }

        public void SetMemberValue(string member, object value)
        {
            _handlers[member].Setter(value);
        }

        private HandlerMap GetMemberMap()
        {
            return new HandlerMap { { "Audit_Update", (() => _meterSnapshotProvider.SnapshotStatus, SetSnapshotStatus) } };
        }

        private bool SetSnapshotStatus(object value)
        {
            if (value == null) return false;

            var parsed = Enum.TryParse<MeterSnapshotStatus>(value.ToString(), out var status);
            if (!parsed) return false;

            _meterSnapshotProvider.SnapshotStatus = status;
            return true;
        }
    }
}