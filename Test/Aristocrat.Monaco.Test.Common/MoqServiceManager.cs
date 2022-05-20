namespace Aristocrat.Monaco.Test.Common
{
    using System.Globalization;
    using System.Reflection;
    using Kernel;
    using Moq;

    /// <summary>
    ///     MoqServiceManager provides the ability to use Moq to mock the implementation of
    ///     Vgt.Client12.Kernel.IServiceManager used by the ServiceManager singleton, as well
    ///     as create and Moq implementations of a service interface and configure the mocked
    ///     service manager to return them from IServiceManager.GetService calls.
    /// </summary>
    public static class MoqServiceManager
    {
        /// <summary>
        ///     Gets the mocked IServiceManager instance, or null if CreateInstance()
        ///     has not been called.
        /// </summary>
        public static Mock<IServiceManager> Instance { get; private set; }

        /// <summary>
        ///     Creates a new, mocked IServiceManager and sets it as the
        ///     Vgt.Client12.Kernel.ServiceManager's IServiceManager reference.
        /// </summary>
        /// <param name="behavior">The behavior for the new mocked service manager.</param>
        /// <returns>A mocked IServiceManager registered with the ServiceManager singleton.</returns>
        public static Mock<IServiceManager> CreateInstance(MockBehavior behavior)
        {
            SetServiceManagerInstance(new Mock<IServiceManager>(behavior));

            return Instance;
        }

        /// <summary>
        ///     Sets the Vgt.Client12.Kernel.ServiceManager's instance to null, as well
        ///     as the mocked instance returned by the Instance property of this class.
        /// </summary>
        public static void RemoveInstance()
        {
            SetServiceManagerInstance(null);
        }

        /// <summary>
        ///     Mocks the mocked IServiceManager methods to return the passed-in service
        ///     and return that it is available for the service type specified in the
        ///     type parameter.
        /// </summary>
        /// <param name="service">The service to add.</param>
        /// <typeparam name="T">The service Type for which to map this service.</typeparam>
        public static void AddService<T>(IService service) where T : class
        {
            if (Instance == null)
            {
                CreateInstance(MockBehavior.Strict);
            }

            Instance.Setup(mock => mock.GetService<T>()).Returns((T)service);
            Instance.Setup(mock => mock.TryGetService<T>()).Returns((T)service);
            Instance.Setup(mock => mock.IsServiceAvailable<T>()).Returns(true);
        }

        /// <summary>
        ///     Mocks the mocked IServiceManager methods to return the passed-in mocked service
        ///     and return that it is available for the service type specified in the
        ///     type parameter.
        /// </summary>
        /// <param name="service">The service to add.</param>
        /// <typeparam name="T">The service Type for which to map this service.</typeparam>
        public static void AddService<T>(Mock<T> service) where T : class
        {
            if (Instance == null)
            {
                CreateInstance(MockBehavior.Strict);
            }

            Instance.Setup(mock => mock.GetService<T>()).Returns(service.Object);
            Instance.Setup(mock => mock.TryGetService<T>()).Returns(service.Object);
            Instance.Setup(mock => mock.IsServiceAvailable<T>()).Returns(true);
        }

        /// <summary>
        ///     Creates a mock of type T and mocks the mocked service manager's GetService
        ///     methods to return it.  CreateInstance() must be called beforehand, else a
        ///     NullReferenceException will be thrown.
        /// </summary>
        /// <remarks>
        ///     The mocked service will not implement the IService interface.
        /// </remarks>
        /// <typeparam name="T">The service Type for which to create a mock.</typeparam>
        /// <param name="behavior">The behavior for the new mocked service.</param>
        /// <returns>The mocked service implementation.</returns>
        public static Mock<T> CreateAndAddService<T>(MockBehavior behavior) where T : class
        {
            var service = CreateAndAddService<T>(behavior, false);
            return service;
        }

        /// <summary>
        ///     Creates a mock of type T and mocks the mocked service manager's GetService
        ///     methods to return it.  CreateInstance() must be called beforehand, else a
        ///     NullReferenceException will be thrown.
        /// </summary>
        /// <typeparam name="T">The service Type for which to create a mock.</typeparam>
        /// <param name="behavior">The behavior for the new mocked service.</param>
        /// <param name="addServiceInterface">Indicates whether or not the mocked service should also implement IService interface.</param>
        /// <returns>The mocked service implementation.</returns>
        public static Mock<T> CreateAndAddService<T>(MockBehavior behavior, bool addServiceInterface) where T : class
        {
            var service = new Mock<T>(behavior);

            if (addServiceInterface)
            {
                var service2 = service.As<IService>();
                service2.Setup(mock => mock.ServiceTypes).Returns(new[] { typeof(T) });
                service2.Setup(mock => mock.Name).Returns(
                    string.Format(CultureInfo.CurrentCulture, "mock {0} service", typeof(T)));
            }

            Instance.Setup(mock => mock.GetService<T>()).Returns(service.Object);
            Instance.Setup(mock => mock.TryGetService<T>()).Returns(service.Object);
            Instance.Setup(mock => mock.IsServiceAvailable<T>()).Returns(true);

            return service;
        }

        /// <summary>
        ///     Creates a mock of type T and mocks the mocked service manager's GetService
        ///     methods to return it.
        /// </summary>
        /// <typeparam name="T">The service type of the mocked object.</typeparam>
        public static void RemoveService<T>() where T : class
        {
            if (null == Instance)
            {
                return;
            }

            Instance.Setup(mock => mock.GetService<T>()).Returns((T)null);
            Instance.Setup(mock => mock.TryGetService<T>()).Returns((T)null);
            Instance.Setup(mock => mock.IsServiceAvailable<T>()).Returns(false);
        }

        /// <summary>
        ///     Helper method to use reflection to set the Vgt.Client12.Kernel.ServiceManager's
        ///     private instance and the local static instance.
        /// </summary>
        /// <param name="mockedInstance">The new mocked instance reference, or null.</param>
        private static void SetServiceManagerInstance(Mock<IServiceManager> mockedInstance)
        {
            Instance = mockedInstance;

            IServiceManager instance = null;
            if (null != mockedInstance)
            {
                instance = mockedInstance.Object;
            }

            var type = typeof(ServiceManager);
            var info = type.GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);
            info?.SetValue(null, instance);
        }
    }
}