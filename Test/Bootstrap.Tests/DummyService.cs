namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Definition of the DummyService class.
    /// </summary>
    /// <remarks>All members of this class purposefully throw a NotImplementedException.</remarks>
    public sealed class DummyService : IDummyService, IService
    {
        /// <summary>
        ///     Not implemented.
        /// </summary>
        /// <exception cref="System.NotImplementedException">Every time</exception>
        public void DummyServiceMethod()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Gets the name of this service.
        /// </summary>
        public string Name => "Service Test Dummy";

        /// <summary>
        ///     Gets the service types supported by this service.
        /// </summary>
        public ICollection<Type> ServiceTypes => new[] { typeof(IDummyService) };

        /// <summary>
        ///     Not implemented.
        /// </summary>
        /// <exception cref="System.NotImplementedException">Every time</exception>
        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}