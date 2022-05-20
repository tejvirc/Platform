namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Events;

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