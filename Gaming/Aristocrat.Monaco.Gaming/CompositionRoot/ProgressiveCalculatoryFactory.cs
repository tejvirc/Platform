namespace Aristocrat.Monaco.Gaming.CompositionRoot
{
    using System.Collections.Generic;
    using Contracts.Progressives;
    using SimpleInjector;

    internal class ProgressiveCalculatorFactory : IProgressiveCalculatorFactory
    {
        private readonly Container _container;

        private readonly Dictionary<SapFundingType, InstanceProducer<ICalculatorStrategy>> _producers =
            new Dictionary<SapFundingType, InstanceProducer<ICalculatorStrategy>>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveCalculatorFactory" /> class.
        /// </summary>
        /// <param name="container">SimpleInjector container.</param>
        public ProgressiveCalculatorFactory(Container container)
        {
            _container = container;
        }

        /// <inheritdoc />
        public ICalculatorStrategy Create(SapFundingType type)
        {
            return _producers.TryGetValue(type, out var handler) ? handler.GetInstance() : null;
        }

        /// <summary>
        ///     Registers <see cref="ICalculatorStrategy" /> of the specified type.
        /// </summary>
        /// <typeparam name="TImplementation"><see cref="ICalculatorStrategy" /> implementation.</typeparam>
        /// <param name="type">Funding type.</param>
        /// <param name="lifestyle">SimpleInjector Lifestyle - singleton, transient, etc.</param>
        public void Register<TImplementation>(SapFundingType type, Lifestyle lifestyle = null)
            where TImplementation : class, ICalculatorStrategy
        {
            var producer = (lifestyle ?? _container.Options.DefaultLifestyle)
                .CreateProducer<ICalculatorStrategy, TImplementation>(_container);

            _producers.Add(type, producer);
        }
    }
}