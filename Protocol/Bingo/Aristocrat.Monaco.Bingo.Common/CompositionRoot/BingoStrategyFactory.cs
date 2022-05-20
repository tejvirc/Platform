namespace Aristocrat.Monaco.Bingo.Common.CompositionRoot
{
    using System;
    using System.Collections.Generic;
    using SimpleInjector;

    public class BingoStrategyFactory<TStrategy, TStrategyType> : IBingoStrategyFactory<TStrategy, TStrategyType>
        where TStrategy : class
    {
        private readonly Container _container;

        private readonly Dictionary<TStrategyType, InstanceProducer<TStrategy>> _producers = new();

        public BingoStrategyFactory(Container container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public TStrategy Create(TStrategyType strategyType)
        {
            return _producers.TryGetValue(strategyType, out var handler) ? handler.GetInstance() : null;
        }

        public void Register<TImplementation>(TStrategyType type, Lifestyle lifestyle = null)
            where TImplementation : class, TStrategy
        {
            var producer = (lifestyle ?? _container.Options.DefaultLifestyle)
                .CreateProducer<TStrategy, TImplementation>(_container);

            _producers.Add(type, producer);
        }
    }
}