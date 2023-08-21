namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     IService provides an interface to get a service name and type from a service.
    /// </summary>
    public interface IService
    {
        /// <summary>
        ///     Gets the name from a service.
        /// </summary>
        /// <returns>The name of the service.</returns>
        string Name { get; }

        /// <summary>
        ///     Gets the type from a service.
        /// </summary>
        /// <returns>The type of the service.</returns>
        ICollection<Type> ServiceTypes { get; }

        /// <summary>
        ///     Initializes the service
        /// </summary>
        void Initialize();
    }
}