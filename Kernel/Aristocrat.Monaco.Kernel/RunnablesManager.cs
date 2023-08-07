namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;
    using log4net;
    using Mono.Addins;

    /// <summary>
    ///     Manages the loading, running and stopping of IRunnable implementations.
    /// </summary>
    public class RunnablesManager
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly Stack<RunnableData> _runnables = new Stack<RunnableData>();

        /// <summary>
        ///     Searches the provided path for runnable add-ins, instantiates those add-ins
        ///     and starts them in their own thread.
        /// </summary>
        /// <param name="extensionPath">The add-in extension path to search for runnables</param>
        public void StartRunnables(string extensionPath)
        {
            var nodes =
                MonoAddinsHelper.GetSelectedNodes<TypeExtensionNode>(extensionPath);
            StartRunnables(nodes);
        }

        /// <summary>
        ///     Instantiates each runnable add-in and starts them in their own thread.
        /// </summary>
        /// <param name="nodes">A collection of runnable add-ins</param>
        [CLSCompliant(false)]
        public void StartRunnables(ICollection<TypeExtensionNode> nodes)
        {
            foreach (var node in nodes)
            {
                StartRunnable(node);
            }
        }

        /// <summary>
        ///     Instantiates the add-in and starts it in its own thread
        /// </summary>
        /// <param name="node">The add-in</param>
        [CLSCompliant(false)]
        public void StartRunnable(InstanceExtensionNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            var runnable = (IRunnable)node.CreateInstance();
            StartRunnable(runnable);
        }

        /// <summary>
        ///     Initializes the runnable in its own thread
        /// </summary>
        /// <param name="runnable">The runnable instance</param>
        public void StartRunnable(IRunnable runnable)
        {
            if (runnable == null)
            {
                throw new ArgumentNullException(nameof(runnable));
            }

            runnable.Initialize();

            var thread = new Thread(runnable.Run)
            {
                Name = runnable.GetType().Name,
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            };
            thread.Start();

            _runnables.Push(new RunnableData(runnable, thread));

            Log.Info("Loaded runnable: " + runnable.GetType());
        }

        /// <summary>
        ///     Stops all managed runnables.
        /// </summary>
        public void StopRunnables()
        {
            while (_runnables.Count > 0)
            {
                var runnableStruct = _runnables.Pop();

                Log.Info($"Stopping runnable: {runnableStruct.Runnable.GetType()}");

                runnableStruct.Runnable.Stop();
                try
                {
                    if (!runnableStruct.Thread.Join(runnableStruct.Runnable.Timeout))
                    {
                        runnableStruct.Thread.Interrupt();
                        Log.Error($"Interrupt runnable: {runnableStruct.Runnable.GetType()}");
                        Debug.Assert(true, $"Interrupt runnable: {runnableStruct.Runnable.GetType()}");
                    }
                }
                catch (Exception e)
                {
                    Log.Debug($"Error stopping runnable: {runnableStruct.Runnable.GetType()}", e);
                }

                if (runnableStruct.Runnable is IDisposable disposable)
                {
                    Log.InfoFormat("Disposing runnable: {0}", runnableStruct.Runnable.GetType());
                    disposable.Dispose();
                }

                Log.Info($"Stopped runnable: {runnableStruct.Runnable.GetType()}");
            }
        }
    }
}