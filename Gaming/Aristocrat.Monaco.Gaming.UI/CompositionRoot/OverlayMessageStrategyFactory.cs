namespace Aristocrat.Monaco.Gaming.UI.CompositionRoot
{
    using System.Collections.Generic;
    using Contracts;
    using SimpleInjector;

    public class OverlayMessageStrategyFactory : IOverlayMessageStrategyFactory
    {
        private readonly Container _container;

        private readonly Dictionary<OverlayMessageStrategyOptions, InstanceProducer<IOverlayMessageStrategy>> _producers =
            new Dictionary<OverlayMessageStrategyOptions, InstanceProducer<IOverlayMessageStrategy>>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="OverlayMessageStrategyFactory" /> class.
        /// </summary>
        /// <param name="container">SimpleInjector container.</param>
        public OverlayMessageStrategyFactory(Container container)
        {
            _container = container;
        }

        /// <inheritdoc />
        public IOverlayMessageStrategy Create(OverlayMessageStrategyOptions mode)
        {
            return _producers.TryGetValue(mode, out var handler) ? handler.GetInstance() : null;
        }

        /// <summary>
        ///     Registers <see cref="IOverlayMessageStrategy" /> of the specified type.
        /// </summary>
        /// <typeparam name="TImplementation"><see cref="IOverlayMessageStrategy" /> implementation.</typeparam>
        /// <param name="type">Strategy type.</param>
        /// <param name="lifestyle">SimpleInjector Lifestyle - singleton, transient, etc.</param>
        public void Register<TImplementation>(OverlayMessageStrategyOptions type, Lifestyle lifestyle = null)
            where TImplementation : class, IOverlayMessageStrategy
        {
            var producer = (lifestyle ?? _container.Options.DefaultLifestyle)
                .CreateProducer<IOverlayMessageStrategy, TImplementation>(_container);

            _producers.Add(type, producer);
        }

        /// <summary>
        ///     Registers <see cref="IOverlayMessageStrategy" /> of the specified type.
        /// </summary>
        /// <typeparam name="TImplementation"><see cref="IOverlayMessageStrategy" /> implementation.</typeparam>
        /// <param name="mode">overlay message type.</param>
        /// <param name="instance">The overlay message instance</param>
        public void RegisterSingleton<TImplementation>(OverlayMessageStrategyOptions mode, TImplementation instance)
            where TImplementation : class, IOverlayMessageStrategy
        {
            var producer = Lifestyle.Singleton.CreateProducer<IOverlayMessageStrategy>(() => instance, _container);

            _producers.Add(mode, producer);
        }
    }
}
