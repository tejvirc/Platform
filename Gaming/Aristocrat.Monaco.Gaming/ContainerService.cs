namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using SimpleInjector;

    /// <inheritdoc />
    public class ContainerService : IContainerService
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ContainerService" /> class.
        /// </summary>
        /// <param name="container">The Container.</param>
        public ContainerService(Container container)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
        }

        /// <inheritdoc />
        public string Name => "Container Service";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes
        {
            get
            {
                return new[] { typeof(IContainerService) };
            }
        }

        /// <inheritdoc />
        public Container Container { get; }

        /// <inheritdoc />
        public void Initialize()
        {
        }
    }
}