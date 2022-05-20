namespace Aristocrat.Monaco.Gaming.CompositionRoot
{
    using System;
    using System.Collections.Generic;
    using Commands.RuntimeEvents;
    using Runtime.Client;
    using SimpleInjector;

    /// <summary>
    ///     Provides a mechanism to resolve and register a jackpot strategy
    /// </summary>
    [CLSCompliant(false)]
    public class RuntimeEventHandlerFactory : IRuntimeEventHandlerFactory
    {
        private readonly Container _container;

        private readonly Dictionary<GameRoundEventState, InstanceProducer<IRuntimeEventHandler>> _producers =
            new Dictionary<GameRoundEventState, InstanceProducer<IRuntimeEventHandler>>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="RuntimeEventHandlerFactory" /> class.
        /// </summary>
        /// <param name="container">SimpleInjector container.</param>
        public RuntimeEventHandlerFactory(Container container)
        {
            _container = container;
        }

        /// <inheritdoc />
        public IRuntimeEventHandler Create(GameRoundEventState type)
        {
            return _producers.TryGetValue(type, out var handler) ? handler.GetInstance() : null;
        }

        /// <summary>
        ///     Registers <see cref="IRuntimeEventHandler"/> of the specified type.
        /// </summary>
        /// <typeparam name="TImplementation"><see cref="IRuntimeEventHandler"/> implementation.</typeparam>
        /// <param name="type">Progressive type.</param>
        /// <param name="lifestyle">SimpleInjector Lifestyle - singleton, transient, etc.</param>
        public void Register<TImplementation>(GameRoundEventState type, Lifestyle lifestyle = null)
            where TImplementation : class, IRuntimeEventHandler
        {
            var producer = (lifestyle ?? _container.Options.DefaultLifestyle)
                .CreateProducer<IRuntimeEventHandler, TImplementation>(_container);

            _producers.Add(type, producer);
        }

        /// <summary>
        ///     Registers <see cref="IRuntimeEventHandler"/> of the specified type.
        /// </summary>
        /// <typeparam name="TImplementation"><see cref="IRuntimeEventHandler"/> implementation.</typeparam>
        /// <param name="type">Progressive type.</param>
        /// <param name="instance">The jackpot strategy instance</param>
        public void RegisterSingleton<TImplementation>(GameRoundEventState type, TImplementation instance)
            where TImplementation : class, IRuntimeEventHandler
        {
            var producer = Lifestyle.Singleton.CreateProducer<IRuntimeEventHandler>(() => instance, _container);

            _producers.Add(type, producer);
        }
    }
}
