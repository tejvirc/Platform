namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.CryptoRng;
    using Contracts;
    using SimpleInjector;

    /// <summary>
    ///     PRNGFactory
    /// </summary>
    [CLSCompliant(false)]
    public class RandomFactory : IRandomFactory
    {
        private readonly Container _container;

        private readonly Dictionary<RandomType, InstanceProducer> _producers =
            new Dictionary<RandomType, InstanceProducer>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="RandomFactory" /> class.
        /// </summary>
        /// <param name="container">SimpleInjector container.</param>
        public RandomFactory(Container container)
        {
            _container = container;
        }

        /// <inheritdoc />
        public IRandom Create(RandomType type)
        {
            return (IRandom)_producers[type].GetInstance();
        }

        /// <summary>
        ///     Registers IPRNG of the specified type.
        /// </summary>
        /// <typeparam name="TImplementation">IPRNG implemenation.</typeparam>
        /// <param name="type">RandomType - Gaming or NonGaming.</param>
        /// <param name="lifestyle">SimpleInjector Lifestyle - singleton, transient, etc.</param>
        public void Register<TImplementation>(RandomType type, Lifestyle lifestyle)
            where TImplementation : class, IRandom
        {
            lifestyle = lifestyle ?? Lifestyle.Transient;

            var producer = lifestyle.CreateProducer<IRandom, TImplementation>(_container);
            _producers.Add(type, producer);
        }
    }
}