namespace Aristocrat.Monaco.Gaming.CompositionRoot
{
    using System.Collections.Generic;
    using Bonus;
    using Contracts.Bonus;
    using SimpleInjector;

    public class BonusStrategyFactory : IBonusStrategyFactory
    {
        private readonly Container _container;

        private readonly Dictionary<BonusMode, InstanceProducer<IBonusStrategy>> _producers =
            new Dictionary<BonusMode, InstanceProducer<IBonusStrategy>>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="RuntimeEventHandlerFactory" /> class.
        /// </summary>
        /// <param name="container">SimpleInjector container.</param>
        public BonusStrategyFactory(Container container)
        {
            _container = container;
        }

        /// <inheritdoc />
        public IBonusStrategy Create(BonusMode mode)
        {
            return _producers.TryGetValue(mode, out var handler) ? handler.GetInstance() : null;
        }

        /// <summary>
        ///     Registers <see cref="IBonusStrategy" /> of the specified type.
        /// </summary>
        /// <typeparam name="TImplementation"><see cref="IBonusStrategy" /> implementation.</typeparam>
        /// <param name="type">Progressive type.</param>
        /// <param name="lifestyle">SimpleInjector Lifestyle - singleton, transient, etc.</param>
        public void Register<TImplementation>(BonusMode type, Lifestyle lifestyle = null)
            where TImplementation : class, IBonusStrategy
        {
            var producer = (lifestyle ?? _container.Options.DefaultLifestyle)
                .CreateProducer<IBonusStrategy, TImplementation>(_container);

            _producers.Add(type, producer);
        }

        /// <summary>
        ///     Registers <see cref="IBonusStrategy" /> of the specified type.
        /// </summary>
        /// <typeparam name="TImplementation"><see cref="IBonusStrategy" /> implementation.</typeparam>
        /// <param name="mode">Bonus mode.</param>
        /// <param name="instance">The bonus strategy instance</param>
        public void RegisterSingleton<TImplementation>(BonusMode mode, TImplementation instance)
            where TImplementation : class, IBonusStrategy
        {
            var producer = Lifestyle.Singleton.CreateProducer<IBonusStrategy>(() => instance, _container);

            _producers.Add(mode, producer);
        }
    }
}