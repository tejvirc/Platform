namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using PRNGLib;
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
        public IPRNG Create(RandomType type)
        {
            return (IPRNG)_producers[type].GetInstance();
        }

        /// <summary>
        ///     Registers IPRNG of the specified type.
        /// </summary>
        /// <typeparam name="TImplementation">IPRNG implemenation.</typeparam>
        /// <param name="type">RandomType - Gaming or NonGaming.</param>
        /// <param name="lifestyle">SimpleInjector Lifestyle - singleton, transient, etc.</param>
        public void Register<TImplementation>(RandomType type, Lifestyle lifestyle)
            where TImplementation : class, IPRNG
        {
            lifestyle = lifestyle ?? Lifestyle.Transient;

            var producer = lifestyle.CreateProducer<IPRNG, TImplementation>(_container);
            _producers.Add(type, producer);
        }
    }
}