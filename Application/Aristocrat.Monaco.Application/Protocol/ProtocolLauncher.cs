namespace Aristocrat.Monaco.Application.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using Contracts;
    using Contracts.Protocol;
    using Kernel;
    using Mono.Addins;

    /// <summary>
    ///     Handles the launching of the configured backend protocol based upon the operator or server
    ///     configuration.
    /// </summary>
    public class ProtocolLauncher : BaseRunnable
    {
        private const string ProtocolExtensionPath = "/Protocol/Runnables";

        private static readonly TimeSpan ProtocolTimeout = TimeSpan.FromSeconds(90);

        private readonly Dictionary<string, IRunnable> _runningProtocols = new();

        private readonly List<Task> _runningProtocolTasks = new();
        private readonly List<ProtocolTypeExtensionNode> _protocolTypeExtensionNodes = new();
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
                    var extentionNodes =
                        AddinManager.GetExtensionNodes<ProtocolTypeExtensionNode>(ProtocolExtensionPath);

                    var extensionNode = extentionNodes.SingleOrDefault(
                        node => string.Equals(
                            node.ProtocolId,
                            Enum.GetName(typeof(CommsProtocol), protocol),
                            StringComparison.InvariantCultureIgnoreCase));

                    if (extensionNode != null)
                    {
                        _protocolTypeExtensionNodes.Add(extensionNode);
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

        private async Task<(string, IRunnable)> CreateProtocol(ProtocolTypeExtensionNode extensionNode)
        {
            var protocol = (IRunnable)extensionNode.CreateInstance();
            _runningProtocols.Add(extensionNode.ProtocolId, protocol);
            return await Task.Run(
                () =>
                {
                    protocol.Initialize();
                    return (extensionNode.ProtocolId, protocol);
                }).ConfigureAwait(false);
        }

        private async Task InitializeAsync(IEnumerable<Task<(string, IRunnable)>> initializationTasks)
        {
            var runnables = await Task.WhenAll(initializationTasks).ConfigureAwait(false);
            _eventBus.Publish(new ProtocolLoadedEvent(runnables.Select(x => x.Item1).ToArray()));
        }

        private static async Task RunProtocol(Task<(string name, IRunnable runable)> initializeTask)
        {
            var (_, runnable) = await initializeTask.ConfigureAwait(false);
            await Task.Run(
                () =>
                {
                    runnable.Run();
                });
        }

        private void RunProtocols()
        {
            var runningProtocols = _protocolTypeExtensionNodes
                .Where(x => !_runningProtocols.ContainsKey(x.ProtocolId))
                .DistinctBy(x => x.ProtocolId)
                .Select(CreateProtocol)
                .ToList();
            var initialization = InitializeAsync(runningProtocols);
            _runningProtocolTasks.AddRange(runningProtocols.Select(RunProtocol).ToArray());
            initialization.WaitForCompletion();
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