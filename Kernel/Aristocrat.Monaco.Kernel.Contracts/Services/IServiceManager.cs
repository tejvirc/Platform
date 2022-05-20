namespace Aristocrat.Monaco.Kernel
{
    using System;

    /// <summary>
    ///     IServiceManager provides an interface for adding, removing, and retrieving
    ///     a specific service from the ServiceManager and stopping the ServiceManager.
    /// </summary>
    public interface IServiceManager
    {
        /// <summary>
        ///     Adds a service to the ServiceManager.
        /// </summary>
        /// <param name="service">The service to be added to the ServiceManager.</param>
        /// <exception cref="ServiceException">
        ///     Thrown when ServiceManager fails to add the service.
        /// </exception>
        void AddService(IService service);

        /// <summary>
        ///     Adds a service to the ServiceManager and initializes it.
        /// </summary>
        /// <param name="service">The service to be added to the ServiceManager.</param>
        /// <exception cref="ServiceException">
        ///     Thrown when ServiceManager fails to add the service.
        /// </exception>
        void AddServiceAndInitialize(IService service);

        /// <summary>
        ///     Gets whether or not a service of the specified type is available.
        /// </summary>
        /// <returns>Whether or not a service of the specified type is available.</returns>
        /// <typeparam name="T">The type of service.</typeparam>
        bool IsServiceAvailable<T>();

        /// <summary>
        ///     Gets a service of the specified type from the ServiceManager.
        /// </summary>
        /// <returns>A reference of the specified service type.</returns>
        /// <exception cref="ServiceNotFoundException">
        ///     Thrown when ServiceManager fails to get the service.
        /// </exception>
        /// <typeparam name="T">The desired service type.</typeparam>
        T GetService<T>();

        /// <summary>
        ///     Gets a service of the specified type from the ServiceManager, if
        ///     one is available, else returns null.
        /// </summary>
        /// <returns>A reference of the specified service type, null if one is not available.</returns>
        /// <typeparam name="T">The desired service type.</typeparam>
        T TryGetService<T>();

        /// <summary>
        ///     Removes a service from the ServiceManager.
        /// </summary>
        /// <param name="service">The service to be removed from the ServiceManager.</param>
        /// <exception cref="ServiceException">
        ///     Thrown when ServiceManager fails to remove the service.
        /// </exception>
        void RemoveService(IService service);

        /// <summary>
        ///     Removes a service from the ServiceManager by type
        /// </summary>
        /// <param name="type">The type of service to be removed from the ServerManager.</param>
        /// <exception cref="ServiceException">
        ///     Thrown when ServiceManager fails to remove the service.
        /// </exception>
        void RemoveService(Type type);

        /// <summary>
        ///     Signals the ServiceManager to clean up services and their threads.
        /// </summary>
        /// <exception cref="ServiceException">
        ///     Thrown when ServiceManager fails to shutdown.
        /// </exception>
        void Shutdown();
    }
}
