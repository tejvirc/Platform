namespace Aristocrat.Monaco.Gaming.Runtime
{
    using Application.Contracts;
    using Contracts;
    using Contracts.Models;
    using Contracts.Process;
    using Kernel;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Hardware.Contracts.Display;

    /// <summary>
    ///     Service used to start a game process
    /// </summary>
    public class GameProcess : IGameProcess
    {
        private const string Logs = "/Logs";

        private readonly IPathMapper _pathMapper;
        private readonly IProcessManager _processManager;
        private readonly IPropertiesManager _properties;
        private readonly IDisplayService _display;

        private readonly object _lock = new object();

        private readonly string _gamesPath;
        private readonly string _runtimeRoot;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameProcess" /> class.
        /// </summary>
        /// <param name="properties">The property manager</param>
        /// <param name="processManager">The process manager</param>
        /// <param name="pathMapper">The path mapper</param>
        /// <param name="display">The display service</param>
        public GameProcess(IPropertiesManager properties, IProcessManager processManager, IPathMapper pathMapper, IDisplayService display)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _processManager = processManager ?? throw new ArgumentNullException(nameof(processManager));
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));
            _display = display ?? throw new ArgumentNullException(nameof(display));

            _gamesPath = pathMapper.GetDirectory(GamingConstants.GamesPath).FullName;
            _runtimeRoot = pathMapper.GetDirectory(GamingConstants.RuntimePath).FullName;
        }

        /// <inheritdoc />
        public int StartGameProcess(GameInitRequest request)
        {
            var game = _properties
                .GetValues<IGameDetail>(GamingConstants.Games)
                .FirstOrDefault(g => g.Id == request.GameId);

            if (game == null)
            {
                return -1;
            }

            var logPath = _pathMapper.GetDirectory(Logs);

            var args = new GameProcessArgs(
                game.VariationId,
                request.Denomination / GamingConstants.Millicents,
                request.GameBottomHwnd,
                request.GameTopHwnd,
                request.GameVirtualButtonDeckHwnd,
                request.GameTopperHwnd,
                game.GameDll,
                logPath.FullName,
                _properties.GetValue(ApplicationConstants.JurisdictionKey, string.Empty),
                game.CentralAllowed,
                _display.MaximumFrameRate,
                _properties.GetValue(GamingConstants.RuntimeArgs, string.Empty));

            var fullGamePath = Path.Combine(_gamesPath, game.Folder);
            var fullRuntimePath = Path.Combine(_runtimeRoot, game.TargetRuntime, GamingConstants.RuntimeHost);

            lock (_lock)
            {
                return _processManager.StartProcess(
                    new ProcessStartInfo
                    {
                        CreateNoWindow = false,
                        Arguments = args.Build(),
                        FileName = fullRuntimePath,
                        WorkingDirectory = fullGamePath,
                        UseShellExecute = false,
                        ErrorDialog = false
                    });
            }
        }

        public void CreateMiniDump()
        {
            lock (_lock)
            {
                var processes = _processManager.GetRunningProcesses();

                foreach (var process in processes)
                {
                    _processManager.CreateMiniDump(process);
                }
            }
        }

        /// <inheritdoc />
        public void EndGameProcess(bool notifyExited = true)
        {
            lock (_lock)
            {
                var processes = _processManager.GetRunningProcesses();

                foreach (var process in processes)
                {
                    _processManager.EndProcess(process, notifyExited);
                }
            }
        }

        /// <inheritdoc />
        public void EndGameProcess(int processId, bool notifyExited = true)
        {
            lock (_lock)
            {
                _processManager.EndProcess(processId, notifyExited);
            }
        }

        /// <inheritdoc />
        public bool IsRunning(int processId)
        {
            lock (_lock)
            {
                var processes = _processManager.GetRunningProcesses();

                return processes.Contains(processId);
            }
        }

        /// <inheritdoc />
        public void Exiting()
        {
            lock (_lock)
            {
                _processManager.ProcessExiting();
            }
        }
    }
}
