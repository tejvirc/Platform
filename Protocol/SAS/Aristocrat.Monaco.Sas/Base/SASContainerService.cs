namespace Aristocrat.Monaco.Sas.Base
{
    using System;
    using System.Collections.Generic;
    using SimpleInjector;

    /// <inheritdoc />
    public class SasContainerService : Contracts.Intercomponent.ISasContainerService
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SasContainerService" /> class.
        /// </summary>
        /// <param name="container">The Container.</param>
        public SasContainerService(Container container)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
        }

        /// <inheritdoc />
        public string Name => "Sas Container Service";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(Contracts.Intercomponent.ISasContainerService) };

        /// <inheritdoc />
        public Container Container { get; }

        /// <inheritdoc />
        public void Initialize()
        {
        }
    }
}
