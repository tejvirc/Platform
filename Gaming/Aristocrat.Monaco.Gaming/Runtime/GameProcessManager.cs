namespace Aristocrat.Monaco.Gaming.Runtime
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Client;
    using Contracts;
    using Contracts.Process;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Starts and ends game processes
    /// </summary>
    public class GameProcessManager : IProcessManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IClientEndpointProvider<IRuntime> _serviceProvider;

        private bool _notifyProcessExited;

        private bool _expectProcessExit;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameProcessManager" /> class.
        /// </summary>
        /// <param name="eventBus">The event bus</param>
        /// <param name="serviceProvider">The IRuntime end point</param>
        public GameProcessManager(
            IEventBus eventBus,
            IClientEndpointProvider<IRuntime> serviceProvider)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public int StartProcess(ProcessStartInfo processInfo)
        {
            if (processInfo == null)
            {
                throw new ArgumentNullException(nameof(processInfo));
            }

            var process = Process.Start(processInfo);

            if (process == null)
            {
                Logger.Error($"Failed to start game: {processInfo.FileName} {processInfo.Arguments}");
                return -1;
            }

            _expectProcessExit = false;
            _notifyProcessExited = true;

            process.EnableRaisingEvents = true;
            process.Exited += ProcessExited;

            Logger.Info($"Game process started ({process.Id}): {processInfo.FileName} {processInfo.Arguments}");

            return process.Id;
        }

        public void CreateMiniDump(int processId)
        {
            var processToKill = GetProcessFromId(processId);

            if (processToKill.HasExited)
            {
                Logger.Debug($"Process Id ({processId}) has already exited.");
                return;
            }

            MiniDumpCreator.Create(processToKill);
        }

        /// <inheritdoc />
        public void EndProcess(int processId, bool notifyExited = true, bool terminateExpected = true)
        {
            var processToKill = GetProcessFromId(processId);

            if (processToKill.HasExited)
            {
                if (!notifyExited)
                {
                    processToKill.Exited -= ProcessExited;
                }

                Logger.Debug($"Process Id ({processId}) has already exited.");
                return;
            }

            try
            {
                _expectProcessExit = terminateExpected;
                _notifyProcessExited = notifyExited;

                processToKill.Kill();

                if (!notifyExited)
                {
                    processToKill.WaitForExit();
                }

                Logger.Info($"Terminated game process: {processId}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to terminate process Id: {processId}", ex);
            }
        }

        /// <inheritdoc />
        public void ProcessExiting()
        {
            _expectProcessExit = true;
        }

        /// <inheritdoc />
        public IEnumerable<int> GetRunningProcesses()
        {
            return Process.GetProcesses()
                .Where(p => p.ProcessName.Contains(GamingConstants.RuntimeHostName) && !p.HasExited)
                .Select(x => x.Id);
        }

        private static Process GetProcessFromId(int processId)
        {
            var process = Process.GetProcesses().FirstOrDefault(p => p.Id == processId);
            if (process == null ||
                !process.ProcessName.Contains(GamingConstants.RuntimeHostName))
            {
                throw new InvalidOperationException($"The specified process id ({processId}) is not valid.");
            }

            return process;
        }

        private void ProcessExited(object sender, EventArgs e)
        {
            var process = sender as Process;

            // Clear the RPC Client instance held, as once the Game/Runtime process is killed, it will become stale
            _serviceProvider.Clear();

            if (!_expectProcessExit)
            {
                Logger.Error(
                    $"Unexpected game process exit ({process?.Id}) : {process?.StartInfo.FileName} {process?.StartInfo.Arguments}",
                    new GameExitedException("The game process ended unexpectedly."));

                if (_notifyProcessExited)
                {
                    _eventBus.Publish(new GameProcessExitedEvent(process?.Id ?? -1, true));
                }
            }
            else
            {
                Logger.Info($"Game process exited ({process?.Id}): {process?.StartInfo.FileName} {process?.StartInfo.Arguments}");

                if (_notifyProcessExited)
                {
                    _eventBus.Publish(new GameProcessExitedEvent(process?.Id ?? -1));
                }
            }

            if (process != null)
            {
                process.Exited -= ProcessExited;
            }

            _expectProcessExit = false;
            _notifyProcessExited = true;
        }
    }
}