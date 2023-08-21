namespace Aristocrat.Monaco.TestController
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;

    public class ProcessMonitor : IDisposable
    {
        private readonly Timer _monitorTimer;
        private readonly Process _process;

        private TimeSpan _lastTotalProcessorTime = TimeSpan.Zero;
        private DateTime _lastTime = DateTime.Now;
        private double _cpuUsage;

        private bool _disposed;

        public ProcessMonitor(string processName)
        {
            _monitorTimer = new Timer(MonitorTick, null, 1000, 1000);

            var pp = Process.GetProcessesByName(processName);

            if (pp.Length > 0)
            {
                _process = pp[0];
                _lastTime = DateTime.Now;
                _lastTotalProcessorTime = _process.TotalProcessorTime;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void GetMetrics(Dictionary<string, string> metrics)
        {
            if (_process == null)
            {
                return;
            }

            metrics["PrivateBytes"] = _process.PrivateMemorySize64.ToString();
            metrics["HandleCount"] = _process.HandleCount.ToString();
            metrics["ThreadCount"] = _process.Threads.Count.ToString();
            metrics["CpuUsage"] = _cpuUsage.ToString(CultureInfo.InvariantCulture);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _monitorTimer?.Dispose();
            }

            _disposed = true;
        }

        private void MonitorTick(object sender)
        {
            if (_process == null)
            {
                return;
            }

            _process.Refresh();

            var curTime = DateTime.Now;
            var curTotalProcessorTime = _process.TotalProcessorTime;

            _cpuUsage = (curTotalProcessorTime.TotalMilliseconds - _lastTotalProcessorTime.TotalMilliseconds) /
                        curTime.Subtract(_lastTime).TotalMilliseconds / Convert.ToDouble(Environment.ProcessorCount);

            _lastTime = curTime;
            _lastTotalProcessorTime = curTotalProcessorTime;
        }
    }
}