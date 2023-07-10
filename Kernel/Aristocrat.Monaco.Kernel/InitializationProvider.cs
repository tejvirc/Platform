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
        private readonly IEventBus _eventBus;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InitializationProvider" /> class.
        /// </summary>
        public InitializationProvider()
            : this(ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InitializationProvider" /> class.
        /// </summary>
        /// <param name="eventBus">An <see cref="IEventBus" /> instance</param>
        public InitializationProvider(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        /// <inheritdoc />
        public void SystemInitializationCompleted()
        {
            IsSystemInitializationComplete = true;
            _eventBus?.Publish(new InitializationCompletedEvent());
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