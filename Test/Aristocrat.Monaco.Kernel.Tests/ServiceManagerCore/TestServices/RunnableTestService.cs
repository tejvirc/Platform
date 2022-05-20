namespace Aristocrat.Monaco.Kernel.Tests.ServiceManagerCore.TestServices
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    ///     RunnableTestService tests the ability of the ServiceManager to add and remove services.
    /// </summary>
    public sealed class RunnableTestService : BaseRunnable, IService
    {
        /// <summary>
        ///     Gets a value indicating whether or not the instance is running.
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether or not the instance has been disposed.
        /// </summary>
        public bool WasDisposed
        {
            get { return Disposed; }
        }

        /// <summary>
        ///     Gets the name from a service.
        /// </summary>
        /// <returns>The name of the service.</returns>
        public string Name
        {
            get { return ToString(); }
        }

        /// <summary>
        ///     Gets the type from a service.
        /// </summary>
        /// <returns>The type of the service.</returns>
        public ICollection<Type> ServiceTypes
        {
            get { return new List<Type> { typeof(RunnableTestService) }; }
        }

        /// <summary>
        ///     Called when runnable is initialized
        /// </summary>
        protected override void OnInitialize()
        {
        }

        /// <summary>
        ///     Called when runnable is run
        /// </summary>
        protected override void OnRun()
        {
            while (RunState == RunnableState.Running)
            {
                Thread.Sleep(50);
            }
        }

        /// <summary>
        ///     Called when runnable is stopped
        /// </summary>
        protected override void OnStop()
        {
        }
    }
}