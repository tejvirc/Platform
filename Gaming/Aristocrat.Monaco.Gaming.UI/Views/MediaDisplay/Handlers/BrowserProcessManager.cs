namespace Aristocrat.Monaco.Gaming.UI.Views.MediaDisplay.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Management;
    using System.Reflection;
    using System.Threading;
    using System.Windows.Controls;
    using Application.Contracts;
    using Application.Contracts.Media;
    using CefSharp;
    using CefSharp.Wpf;
    using Contracts;
    using Kernel;
    using log4net;

    public class BrowserProcessManager : IBrowserProcessManager
    {
        private const string CefStartQuery =
            "select * from Win32_ProcessStartTrace where processname='CefSharp.BrowserSubprocess.exe'";

        private const string CefStopQuery =
            "select * from Win32_ProcessStopTrace where processname='CefSharp.BrowserSubprocess.exe'";

        private const string ProcessCategoryName = "Process";
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<int> _overCpuLimit = new List<int>();
        private readonly List<int> _overRamLimit = new List<int>();
        private readonly List<Process> _processes = new List<Process>();
        private readonly object _processLock = new object();

        private readonly IPropertiesManager _properties;
        private readonly IEventBus _eventBus;

        private double _maxCpuPerProcess;
        private double _maxCpuTotal;
        private double _maxMemoryPerProcess;

        private Timer _monitor;
        private bool _overTotalCpuLimit;

        private ManagementEventWatcher _startWatcher;
        private ManagementEventWatcher _stopWatcher;

        private bool _disposed;

        public BrowserProcessManager(IPropertiesManager properties, IEventBus eventBus)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public string Name => typeof(IBrowserProcessManager).ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(IBrowserProcessManager) };

        public void Initialize()
        {
            if (!_properties.GetValue(ApplicationConstants.MediaDisplayEnabled, false))
            {
                return;
            }

            _maxCpuPerProcess = _properties.GetValue(GamingConstants.BrowserMaxCpuPerProcess, 25.0); // %
            _maxCpuTotal = _properties.GetValue(GamingConstants.BrowserMaxCpuTotal, 50.0); // %
            _maxMemoryPerProcess = _properties.GetValue(GamingConstants.BrowserMaxMemoryPerProcess, 500.0); // MB

            _startWatcher = new ManagementEventWatcher(new WqlEventQuery(CefStartQuery));
            _startWatcher.EventArrived += StartWatcher_EventArrived;
            _startWatcher.Start();

            _stopWatcher = new ManagementEventWatcher(new WqlEventQuery(CefStopQuery));
            _stopWatcher.EventArrived += StopWatcher_EventArrived;
            _stopWatcher.Start();

            _monitor = new Timer(CheckProcessLimits, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(9900));
        }

        public ContentControl StartNewBrowser(IMediaPlayerViewModel player)
        {
            CefHelper.Initialize();

            var view = new MediaPlayerView(player, _eventBus);

            Logger.Info($"StartNewBrowser view initialized for ID {player.Id}");

            return view;
        }

        public void ReleaseBrowser(Control mediaPlayerView)
        {
            if (mediaPlayerView is ChromiumWebBrowser view)
            {
                if (view.DataContext is IMediaPlayerViewModel vm)
                {
                    Logger.Info($"ReleaseBrowser for ID {vm.Id}");
                }

                view.GetBrowserHost().CloseBrowser(true);
                view.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_startWatcher != null)
                {
                    _startWatcher.EventArrived -= StartWatcher_EventArrived;
                    _startWatcher.Dispose();
                }

                if (_stopWatcher != null)
                {
                    _stopWatcher.EventArrived -= StopWatcher_EventArrived;
                    _stopWatcher.Dispose();
                }

                if (_monitor != null)
                {
                    _monitor.Dispose();
                }

                CefHelper.Shutdown();
            }

            _monitor = null;
            _startWatcher = null;
            _stopWatcher = null;

            _disposed = true;
        }

        private static string GetProcessInstanceName(Process process)
        {
            // CEF Browser Sub-processes will all have the same name, so use the PID to get the instance name
            var category = new PerformanceCounterCategory(ProcessCategoryName);

            var instances = category.GetInstanceNames().Where(n => n.Contains(process.ProcessName));
            foreach (var instance in instances)
            {
                using (var counter = new PerformanceCounter(ProcessCategoryName, "ID Process", instance, true))
                {
                    var raw = (int)counter.RawValue;
                    if (raw == process.Id)
                    {
                        return instance;
                    }
                }
            }

            return null;
        }

        private static void CheckLimit(double currentUsage, double maxUsage, Process process, List<int> overLimitList)
        {
            if (currentUsage > maxUsage)
            {
                if (overLimitList.Contains(process.Id))
                {
                    // Usage has been over the limit for more than 10 seconds and we should kill it
                    Logger.Warn($"KILLING CEF PROCESS {process.Id} due to usage {currentUsage} greater than maximum {maxUsage}");

                    process.Kill();
                    overLimitList.Remove(process.Id);
                }
                else
                {
                    overLimitList.Add(process.Id);
                }
            }
            else
            {
                overLimitList.Remove(process.Id);
            }
        }

        private void CheckProcessLimits(object state)
        {
            if (!_processes.Any())
            {
                return;
            }

            var totalUsage = 0.0;

            var processesToRemove = new List<Process>();
            foreach (var process in _processes.ToArray())
            {
                try
                {
                    if (process.HasExited)
                    {
                        processesToRemove.Add(process);
                        continue;
                    }

                    var name = GetProcessInstanceName(process);
                    if (string.IsNullOrEmpty(name))
                    {
                        processesToRemove.Add(process);
                        continue;
                    }

                    using (var cpuUsageTotal = new PerformanceCounter(ProcessCategoryName, "% Processor Time", "_Total"))
                        using (var cpuUsageProcess = new PerformanceCounter(ProcessCategoryName, "% Processor Time", name))
                        {
                            cpuUsageProcess.NextValue();
                            cpuUsageTotal.NextValue();
                            Thread.Sleep(100); // necessary to retrieve correct next values

                            var cpuUsage = Math.Round(cpuUsageProcess.NextValue() / cpuUsageTotal.NextValue() * 100, 1);

                            if (double.IsNaN(cpuUsage))
                            {
                                cpuUsage = 0;
                            }

                            // CPU Usage
                            Logger.Debug($"Process {process.Id} CPU Usage: {cpuUsage}%");
                            totalUsage += cpuUsage;

                            // If CPU % is above threshold for over 10 seconds, kill process
                            CheckLimit(cpuUsage, _maxCpuPerProcess, process, _overCpuLimit);
                        }

                    // Process Memory Usage
                    var memUsage = process.PrivateMemorySize64 / 1048576;
                    Logger.Debug($"Process {process.Id} Memory Usage: {memUsage}MB");

                    // If process memory is above threshold for over 10 seconds, kill process
                    CheckLimit(memUsage, _maxMemoryPerProcess, process, _overRamLimit);
                }
                catch (Exception ex)
                {
                    var processId = process?.Id.ToString() ?? "NULL";
                    Logger.Error($"Checking process limits failed for: {processId}", ex);

                    if (ex is InvalidOperationException)
                    {
                        processesToRemove.Add(process);
                    }
                }
            }

            lock (_processLock)
            {
                foreach (var process in processesToRemove)
                {
                    // DO NOT call RemoveProcess from here because this entire loop needs to be within the lock
                    Logger.Debug($"RemoveProcess for ID {process.Id}");

                    process.ErrorDataReceived -= Process_ErrorDataReceived;
                    process.OutputDataReceived -= Process_OnOutputDataReceived;
                    process.Exited -= Process_OnExited;

                    _processes.Remove(process);
                }
            }

            // If Total CPU % is above threshold, kill highest usage process
            CheckTotalLimit(totalUsage);
        }

        private void CheckTotalLimit(double totalUsage)
        {
            // Overall Memory Usage
            Logger.Debug($"Total CEF Processes CPU Usage: {totalUsage}%");

            if (totalUsage > _maxCpuTotal)
            {
                if (_overTotalCpuLimit)
                {
                    // Total CPU Usage has been over the limit for more than 10 seconds and we should kill the worst offender
                    Process process;
                    lock (_processLock)
                    {
                        process = _processes.OrderByDescending(p => p.TotalProcessorTime).First();
                    }

                    Logger.Warn($"KILLING CEF PROCESS {process.Id} due to overall CPU usage");
                    process.Kill();
                    _overTotalCpuLimit = false;
                }
                else
                {
                    _overTotalCpuLimit = true;
                }
            }
        }

        private void StartWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            var processName = e.NewEvent.GetPropertyValue("ProcessName");
            var pid = (uint)e.NewEvent.GetPropertyValue("ProcessID");
            Logger.Debug($"Started process {processName} with ID {pid}");

            try
            {
                lock (_processLock)
                {
                    var process = Process.GetProcessById((int)pid);
                    if (!_processes.Contains(process))
                    {
                        _processes.Add(process);

                        // Monitor CefSharp.Browser Sub-process for errors
                        process.EnableRaisingEvents = true;
                        process.ErrorDataReceived += Process_ErrorDataReceived;
                        process.OutputDataReceived += Process_OnOutputDataReceived;
                        process.Exited += Process_OnExited;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Error configuring process ID {pid}", ex);
            }
        }

        private void StopWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            var processName = e.NewEvent.GetPropertyValue("ProcessName");
            var pid = (uint)e.NewEvent.GetPropertyValue("ProcessID");

            Logger.Debug($"Stopped process {processName} with ID {pid}");

            var process = Process.GetProcessById((int)pid);

            RemoveProcess(process);
        }

        private void RemoveProcess(Process process)
        {
            if (process == null)
            {
                return;
            }

            Logger.Debug($"RemoveProcess for ID {process.Id}");

            lock (_processLock)
            {
                process.ErrorDataReceived -= Process_ErrorDataReceived;
                process.OutputDataReceived -= Process_OnOutputDataReceived;
                process.Exited -= Process_OnExited;

                _processes.Remove(process);
            }
        }

        private static void Process_OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Logger.Debug($"Output Data Received for process: {e.Data}");
        }

        private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Logger.Warn($"Error Data Received for process: {e.Data}");
        }

        private void Process_OnExited(object sender, EventArgs e)
        {
            // The actual restarting of the process is initiated by RequestHandler, so here we just need to clean up locals
            if (sender is Process process)
            {
                RemoveProcess(process);

                Logger.Info($"Process_OnExited with exit code {process.ExitCode} occurred for process ID {process.Id}");
            }
        }
    }
}