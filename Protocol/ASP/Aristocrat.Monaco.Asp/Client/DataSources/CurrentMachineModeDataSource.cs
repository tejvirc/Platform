namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using Contracts;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using log4net;

    /// <summary>
    /// Monitors system events to provide current machine state
    ///
    /// Notes:
    /// Demo mode is not implemented on Linux or Windows
    /// </summary>
    public class CurrentMachineModeDataSource : IDisposableDataSource
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private const string DataMemberName = "Current_Machine_Mode";

        private bool _disposed;
        private readonly ICurrentMachineModeStateManager _currentMachineModeState;

        public string Name { get; } = "Current_Machine_Mode";

        public IReadOnlyList<string> Members => new List<string> { DataMemberName };

        public event EventHandler<Dictionary<string, object>> MemberValueChanged;

        public CurrentMachineModeDataSource(ICurrentMachineModeStateManager currentMachineModeState)
        {
            _currentMachineModeState = currentMachineModeState ?? throw new ArgumentNullException(nameof(currentMachineModeState));
            _currentMachineModeState.MachineModeChanged += (_, machineMode) =>
                MemberValueChanged?.Invoke(this, new Dictionary<string, object> { {DataMemberName, machineMode} });
            Log.Debug($"DataSource {Name} initialised");
        }

        public object GetMemberValue(string member)
        {
            return _currentMachineModeState.GetCurrentMode();
        }

        public void SetMemberValue(string member, object value) => Log.Debug($"Attempt to set {member} to {value} was ignored");

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _currentMachineModeState.Dispose();
            }

            _disposed = true;
        }
    }
}