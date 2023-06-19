namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Events;
    using Kernel.Debugging;

    /// <summary>
    ///     Provides status on component initialization progress
    /// </summary>
    public class InitializationProvider : IInitializationProvider, IService
    {
        /// <inheritdoc />
        public void SystemInitializationCompleted()
        {
            IsSystemInitializationComplete = true;
            ServiceManager.GetInstance()?.GetService<IEventBus>()?.Publish(new InitializationCompletedEvent());
#if DEBUG
            ServiceManager.GetInstance().TryGetService<IDebuggerService>()?.AttachDebuggerIfRequestedForPoint(DebuggerAttachPoint.OnSystemInitialized);
#endif
        }

        /// <inheritdoc />
        public bool IsSystemInitializationComplete { get; private set; }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IInitializationProvider) };

        /// <inheritdoc />
        public void Initialize()
        {
        }
    }
}