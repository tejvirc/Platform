namespace Aristocrat.Monaco.Application.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Protocol;
    using Kernel;
    using log4net;
    using Mono.Addins;

    /// <summary>
    ///     Handles the launching of the configured backend protocol based upon the operator or server
    ///     configuration.
    /// </summary>
    public class ProtocolLauncher : BaseRunnable
    {
        private const string ProtocolExtensionPath = "/Protocol/Runnables";
        private const string DemonstrationExtPath = "/Protocol/Demonstration/Runnables";

        private static readonly TimeSpan ProtocolTimeout = TimeSpan.FromSeconds(90);
        private static readonly ILog Logger = LogManager.GetLogger(nameof(ProtocolLauncher));

        private readonly Dictionary<string, IRunnable> _runningProtocols = new Dictionary<string, IRunnable>();

        private readonly List<Task> _runningProtocolTasks = new List<Task>();
        private readonly List<ProtocolTypeExtensionNode> _protocolTypeExtensionNodes = new List<ProtocolTypeExtensionNode>();
        private IEventBus _eventBus;
        private readonly List<CommsProtocol> _protocols;

        public ProtocolLauncher()
        {
            _protocols = ServiceManager.GetInstance()
                .GetService<IMultiProtocolConfigurationProvider>().MultiProtocolConfiguration.Select(x => x.Protocol)
                .ToList();
        }

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            Timeout = ProtocolTimeout;
            _protocols.ForEach(
                protocol =>
                {
                    if (protocol == CommsProtocol.DemonstrationMode)
                    {
                        _protocolTypeExtensionNodes.Add(MonoAddinsHelper.GetSingleSelectedExtensionNode<ProtocolTypeExtensionNode>(DemonstrationExtPath));
                    }
                    else
                    {
                        var extentionNodes =
                            AddinManager.GetExtensionNodes<ProtocolTypeExtensionNode>(ProtocolExtensionPath);

                        var extensionNode = extentionNodes.SingleOrDefault(
                            node => string.Equals(
                                node.ProtocolId,
                                Enum.GetName(typeof(CommsProtocol), protocol),
                                StringComparison.InvariantCultureIgnoreCase));
                        _protocolTypeExtensionNodes.Add(extensionNode);

                        if (extensionNode != null)
                        {
                            _protocolTypeExtensionNodes.Add(extensionNode);
                        }
                    }
                }
            );
        }

        /// <inheritdoc />
        protected override void OnRun()
        {
            while (RunState == RunnableState.Running)
            {
                // This will block until the protocol exits and reload if the platform isn't exiting giving the protocol the opportunity to reload itself
                RunProtocols();
            }
        }

        /// <inheritdoc />
        protected override void OnStop()
        {
            var protocolsToStop = _runningProtocols.Values.ToList();
            foreach (var protocol in protocolsToStop)
            {
                protocol.Stop();
            }
        }

        private void RunProtocols()
        {
            var protocolsLaunched = new List<string>();
            _protocolTypeExtensionNodes.ForEach(
                x =>
                {
                    if (_runningProtocols.ContainsKey(x.ProtocolId))
                    {
                        return;
                    }

                    var runningProtocol = (IRunnable)x.CreateInstance();
                    _runningProtocols[x.ProtocolId] = runningProtocol;

                    _runningProtocolTasks.Add(Task.Run(() =>
                    {
                        Logger.Debug($"Starting protocol: {x.ProtocolId}");
                        runningProtocol.Initialize();

                        protocolsLaunched.Add(x.ProtocolId);

                        runningProtocol.Run();
                    }));
                }
            );

            _eventBus.Publish(new ProtocolLoadedEvent(protocolsLaunched));

            Task.WaitAny(_runningProtocolTasks.ToArray());
            foreach (var completedTask in _runningProtocolTasks.Where(x => x.IsCompleted))
            {
                completedTask.Dispose();
            }

            _runningProtocolTasks.RemoveAll(x => x.IsCompleted);

            var protocolsDisposed = new List<string>();
            foreach (var runningProtocol in _runningProtocols.Where(
                runningProtocol => runningProtocol.Value.RunState == RunnableState.Stopped))
            {
                runningProtocol.Value.Stop();
                (runningProtocol.Value as IDisposable)?.Dispose();
                protocolsDisposed.Add(runningProtocol.Key);
            }

            protocolsDisposed.ForEach(x => _runningProtocols.Remove(x));
        }
    }
}