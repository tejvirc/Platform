namespace Aristocrat.Monaco.Kernel.Tests.ServiceManagerCore
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common.UnitTesting;
    using TestServices;
    using ServiceManagerCore = Kernel.ServiceManagerCore;

    /// <summary>
    ///     This is a test class for ServiceManagerTest and is intended
    ///     to contain all ServiceManagerTest Unit Tests
    /// </summary>
    [TestClass]
    public class ServiceManagerCoreTests
    {
        private Mock<IEventBus> _bus = new Mock<IEventBus>();
        private ServiceManagerCore _target;

        private TestContext testContext;

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            set { testContext = value; }
        }

        /// <summary>
        ///     Gets the service directory.
        /// </summary>
        /// <value>
        ///     The service directory.
        /// </value>
        private Dictionary<Type, IService> ServiceDirectory =>
            GetPrivateField("_serviceDirectory") as Dictionary<Type, IService>;

        /// <summary>
        ///     Gets the thread directory.
        /// </summary>
        /// <value>
        ///     The thread directory.
        /// </value>
        private Dictionary<IService, Thread> ThreadDirectory =>
            GetPrivateField("_threadDirectory") as Dictionary<IService, Thread>;

        /// <summary>Use TestInitialize to run code before running each test.</summary>
        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new ServiceManagerCore();

            _bus.As<IService>();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            if (_target != null)
            {
                _target.Dispose();
                _target = null;
            }
        }

        /// <summary>A test for the constructor.</summary>
        [TestMethod]
        public void ConstructorTest()
        {
            Assert.AreEqual(0, ServiceDirectory.Count);
            Assert.AreEqual(0, ThreadDirectory.Count);

            AssertLockAndDisposed();
        }

        /// <summary>Creates a ServiceException() that has an inner exception.</summary>
        [TestMethod]
        public void CreateServiceExceptionWithInnerExceptionTest()
        {
            var message = "outer exception";
            var innerException = new Exception("inner exception");

            var serviceException =
                InvokePrivateMethod(
                    "CreateServiceException",
                    new[] { typeof(string), typeof(Exception) },
                    new object[] { message, innerException },
                    true) as ServiceException;

            Assert.AreEqual(message, serviceException.Message);
            Assert.AreEqual(innerException, serviceException.InnerException);
            Assert.AreEqual(innerException.Message, serviceException.InnerException.Message);
        }

        /// <summary>A test for RunService().</summary>
        [TestMethod]
        public void RunServiceTest()
        {
            var service = new RunnableTestService();
            service.Initialize();
            ServiceDirectory[typeof(RunnableTestService)] = service;

            InvokePrivateMethod(
                "RunService",
                new[] { typeof(IRunnable) },
                new object[] { service },
                false);

            Thread.Sleep(1000);

            Assert.AreEqual(1, ServiceDirectory.Count);
            Assert.IsTrue(ThreadDirectory.ContainsKey(service));
            Assert.IsTrue(ThreadDirectory[service].IsAlive);

            AssertLockAndDisposed();
        }

        /// <summary>A test for RemoveServiceDictionaryEntry().</summary>
        [TestMethod]
        public void RemoveServiceDictionaryEntryTest()
        {
            var service = new NonRunnableTestService();
            ServiceDirectory[typeof(NonRunnableTestService)] = service;

            InvokePrivateMethod(
                "RemoveServiceDictionaryEntry",
                new[] { typeof(Type) },
                new object[] { typeof(NonRunnableTestService) },
                false);

            Assert.AreEqual(0, ServiceDirectory.Count);
            Assert.AreEqual(0, ThreadDirectory.Count);
            AssertLockAndDisposed();
        }

        /// <summary>A test for RemoveThreadDictionaryEntry().</summary>
        [TestMethod]
        public void RemoveThreadDictionaryEntryTest()
        {
            var service = new RunnableTestService();
            ServiceDirectory[typeof(RunnableTestService)] = service;

            var thread = new Thread(service.Run);
            ThreadDirectory[service] = thread;

            InvokePrivateMethod(
                "RemoveThreadDictionaryEntry",
                new[] { typeof(IService) },
                new object[] { service },
                false);

            Assert.AreEqual(1, ServiceDirectory.Count);
            Assert.AreEqual(0, ThreadDirectory.Count);
            AssertLockAndDisposed();
        }

        /// <summary>A test for ClearServiceDictionary().</summary>
        [TestMethod]
        public void ClearServiceDictionaryTest()
        {
            var service = new NonRunnableTestService();
            var runnableService = new RunnableTestService();
            ServiceDirectory[typeof(NonRunnableTestService)] = service;
            ServiceDirectory[typeof(RunnableTestService)] = runnableService;

            InvokePrivateMethod("ClearServiceDictionary", null, null, false);

            Assert.AreEqual(0, ServiceDirectory.Count);
            Assert.AreEqual(0, ThreadDirectory.Count);
            AssertLockAndDisposed();
        }

        /// <summary>A test for ClearThreadDictionary().</summary>
        [TestMethod]
        public void ClearThreadDictionaryTest()
        {
            var service1 = new RunnableTestService();
            var thread1 = new Thread(service1.Run);
            ThreadDirectory[service1] = thread1;

            var service2 = new RunnableTestService();
            var thread2 = new Thread(service2.Run);
            ThreadDirectory[service2] = thread2;

            InvokePrivateMethod("ClearThreadDictionary", null, null, false);

            Assert.AreEqual(0, ServiceDirectory.Count);
            Assert.AreEqual(0, ThreadDirectory.Count);
            AssertLockAndDisposed();
        }

        /// <summary>A test for AddService() where a service of that type already exists.</summary>
        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void AddServiceDuplicateTest()
        {
            var service = new NonRunnableTestService();
            ServiceDirectory[typeof(NonRunnableTestService)] = service;

            _target.AddService(service);
        }

        /// <summary>A test for AddService() for a service that is not runnable.</summary>
        [TestMethod]
        public void AddNonRunnableServiceTest()
        {
            ServiceDirectory[typeof(IEventBus)] = _bus.Object as IService;

            var service = new NonRunnableTestService();
            _target.AddService(service);

            Assert.AreEqual(2, ServiceDirectory.Count);
            Assert.AreEqual(service, ServiceDirectory[typeof(NonRunnableTestService)]);
            Assert.AreEqual(0, ThreadDirectory.Count);
            AssertLockAndDisposed();

            _bus.Verify(m => m.Publish(It.IsAny<ServiceAddedEvent>()));
        }

        /// <summary>A test for AddService() for a runnable service.</summary>
        [TestMethod]
        public void AddRunnableServiceTest()
        {
            ServiceDirectory[typeof(IEventBus)] = _bus.Object as IService;

            var service = new RunnableTestService();
            service.Initialize();
            _target.AddService(service);

            Assert.AreEqual(2, ServiceDirectory.Count);
            Assert.AreEqual(service, ServiceDirectory[typeof(RunnableTestService)]);
            Assert.AreEqual(1, ThreadDirectory.Count);
            Assert.IsTrue(ThreadDirectory.ContainsKey(service));
            Assert.IsTrue(ThreadDirectory[service].IsAlive);
            AssertLockAndDisposed();

            _bus.Verify(m => m.Publish(It.IsAny<ServiceAddedEvent>()));
        }

        /// <summary>A test for AddServiceAndInitialize() where a service of that type already exists.</summary>
        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void AddServiceAndInitializeDuplicateTest()
        {
            var service = new NonRunnableTestService();
            ServiceDirectory[typeof(NonRunnableTestService)] = service;

            _target.AddServiceAndInitialize(service);
        }

        /// <summary>A test for AddServiceAndInitialize() for a service that is not runnable.</summary>
        [TestMethod]
        public void AddAndInitializeNonRunnableServiceTest()
        {
            ServiceDirectory[typeof(IEventBus)] = _bus.Object as IService;

            var service = new NonRunnableTestService();
            _target.AddServiceAndInitialize(service);

            Assert.AreEqual(2, ServiceDirectory.Count);
            Assert.AreEqual(service, ServiceDirectory[typeof(NonRunnableTestService)]);
            var threadDirectory = GetPrivateField("_threadDirectory") as Dictionary<IService, Thread>;
            Assert.AreEqual(0, ThreadDirectory.Count);
            AssertLockAndDisposed();

            _bus.Verify(m => m.Publish(It.IsAny<ServiceAddedEvent>()));
        }

        /// <summary>A test for AddServiceAndInitialize() for a runnable service.</summary>
        [TestMethod]
        public void AddAndInitializeRunnableServiceTest()
        {
            ServiceDirectory[typeof(IEventBus)] = _bus.Object as IService;

            var service = new RunnableTestService();
            _target.AddServiceAndInitialize(service);

            Assert.AreEqual(2, ServiceDirectory.Count);
            Assert.AreEqual(service, ServiceDirectory[typeof(RunnableTestService)]);
            Assert.AreEqual(1, ThreadDirectory.Count);
            Assert.IsTrue(ThreadDirectory.ContainsKey(service));

            Assert.IsTrue(ThreadDirectory[service].IsAlive);
            AssertLockAndDisposed();

            _bus.Verify(m => m.Publish(It.IsAny<ServiceAddedEvent>()));
        }

        /// <summary>A test for IsServiceAvailable() where the service has not been added.</summary>
        [TestMethod]
        public void IsServiceAvailableTestForUnknownService()
        {
            var available = _target.IsServiceAvailable<NonRunnableTestService>();

            Assert.IsFalse(available);

            Assert.AreEqual(0, ServiceDirectory.Count);
            Assert.AreEqual(0, ThreadDirectory.Count);

            AssertLockAndDisposed();
        }

        /// <summary>A test for IsServiceAvailable() where the service is known.</summary>
        [TestMethod]
        public void IsServiceAvailableTestForKnownService()
        {
            var service = new NonRunnableTestService();
            ServiceDirectory[typeof(NonRunnableTestService)] = service;

            var available = _target.IsServiceAvailable<NonRunnableTestService>();

            Assert.IsTrue(available);

            Assert.AreEqual(1, ServiceDirectory.Count);
            Assert.AreEqual(0, ThreadDirectory.Count);

            AssertLockAndDisposed();
        }

        /// <summary>A test for GetService() where the service has not been added.</summary>
        [TestMethod]
        [ExpectedException(typeof(ServiceNotFoundException))]
        public void GetServiceTestForUnknownService()
        {
            _target.GetService<NonRunnableTestService>();
        }

        /// <summary>A test for GetService() where the service is known.</summary>
        [TestMethod]
        public void GetServiceTestForKnownService()
        {
            var expected = new NonRunnableTestService();
            ServiceDirectory[typeof(NonRunnableTestService)] = expected;

            var actual = _target.GetService<NonRunnableTestService>();

            Assert.AreEqual(expected, actual);

            Assert.AreEqual(1, ServiceDirectory.Count);
            Assert.AreEqual(0, ThreadDirectory.Count);
            AssertLockAndDisposed();
        }

        /// <summary>A test for TryGetService() where the service has not been added.</summary>
        [TestMethod]
        public void TryGetServiceTestForUnknownService()
        {
            var service = _target.TryGetService<NonRunnableTestService>();

            Assert.IsNull(service);

            Assert.AreEqual(0, ServiceDirectory.Count);
            Assert.AreEqual(0, ThreadDirectory.Count);
            AssertLockAndDisposed();
        }

        /// <summary>A test for TryGetService() where the service is known.</summary>
        [TestMethod]
        public void TryGetServiceTestForKnownService()
        {
            var expected = new NonRunnableTestService();
            ServiceDirectory[typeof(NonRunnableTestService)] = expected;

            var actual = _target.TryGetService<NonRunnableTestService>();

            Assert.AreEqual(expected, actual);

            Assert.AreEqual(1, ServiceDirectory.Count);
            Assert.AreEqual(0, ThreadDirectory.Count);
            AssertLockAndDisposed();
        }

        /// <summary>A test for RemoveService() where the service does not exist.</summary>
        [TestMethod]
        [ExpectedException(typeof(ServiceNotFoundException))]
        public void RemoveServiceTestForUnknownService()
        {
            var service = new NonRunnableTestService();
            _target.RemoveService(service);
        }

        /// <summary>
        ///     A test for RemoveService() where the passed-in service's type is a registered service
        ///     type, but the loaded service instance does not match the provided one.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ServiceNotFoundException))]
        public void RemoveServiceTestForWrongInstance()
        {
            var service1 = new NonRunnableTestService();
            ServiceDirectory[typeof(NonRunnableTestService)] = service1;

            var service2 = new NonRunnableTestService();
            _target.RemoveService(service2);
        }

        /// <summary>A test for RemoveService() for a known, non-runnable service.</summary>
        [TestMethod]
        public void RemoveServiceTestForNonRunnableService()
        {
            ServiceDirectory[typeof(IEventBus)] = _bus.Object as IService;

            var service = new NonRunnableTestService();
            ServiceDirectory[typeof(NonRunnableTestService)] = service;

            _target.RemoveService(service);

            Assert.AreEqual(1, ServiceDirectory.Count);
            Assert.AreEqual(0, ThreadDirectory.Count);
            AssertLockAndDisposed();

            _bus.Verify(m => m.Publish(It.IsAny<ServiceRemovedEvent>()));
        }

        /// <summary>A test for RemoveService() for a known, runnable, and disposable service.</summary>
        [TestMethod]
        public void RemoveServiceTestForRunnableAndDisposableService()
        {
            ServiceDirectory[typeof(IEventBus)] = _bus.Object as IService;

            var service = new RunnableTestService();
            ServiceDirectory[typeof(RunnableTestService)] = service;

            var thread = new Thread(service.Run);
            thread.Start();
            ThreadDirectory[service] = thread;

            Thread.Sleep(200);

            _target.RemoveService(service);

            Assert.AreEqual(1, ServiceDirectory.Count);
            Assert.AreEqual(0, ThreadDirectory.Count);
            AssertLockAndDisposed();

            _bus.Verify(m => m.Publish(It.IsAny<ServiceRemovedEvent>()));
            Assert.AreNotEqual(RunnableState.Running, service.RunState);
            Assert.IsTrue(service.WasDisposed);
        }

        /// <summary>A test for Shutdown() with a runnable service running that is also disposable.</summary>
        [TestMethod]
        public void ShutdownTest()
        {
            var service = new RunnableTestService();
            ServiceDirectory[typeof(RunnableTestService)] = service;

            var thread = new Thread(service.Run);
            thread.Start();
            ThreadDirectory[service] = thread;

            Thread.Sleep(200);

            _target.Shutdown();

            Assert.AreEqual(0, ServiceDirectory.Count);
            Assert.AreEqual(0, ThreadDirectory.Count);
            AssertLockAndDisposed();

            Assert.AreNotEqual(RunnableState.Running, service.RunState);
            Assert.IsTrue(service.WasDisposed);
        }

        /// <summary>
        ///     A test for Dispose() with a runnable service running that is also disposable.  Call it
        ///     twice, as the MSDN documentation says it should be callable twice or more without error.
        /// </summary>
        [TestMethod]
        public void DisposeTest()
        {
            var service = new RunnableTestService();
            ServiceDirectory[typeof(RunnableTestService)] = service;

            var thread = new Thread(service.Run);
            thread.Start();
            ThreadDirectory[service] = thread;

            Thread.Sleep(200);

            _target.Dispose();

            Assert.AreEqual(0, ServiceDirectory.Count);
            Assert.AreEqual(0, ThreadDirectory.Count);
            Assert.IsNull(GetPrivateField("_lock") as ReaderWriterLockSlim);
            Assert.IsTrue((bool)GetPrivateField("_disposed"));
            Assert.AreNotEqual(RunnableState.Running, service.RunState);
            Assert.IsTrue(service.WasDisposed);

            _target.Dispose();

            Assert.AreEqual(0, ServiceDirectory.Count);
            Assert.AreEqual(0, ThreadDirectory.Count);
            Assert.IsNull(GetPrivateField("_lock") as ReaderWriterLockSlim);
            Assert.IsTrue((bool)GetPrivateField("_disposed"));
            Assert.AreNotEqual(RunnableState.Running, service.RunState);
            Assert.IsTrue(service.WasDisposed);
        }

        /// <summary>
        ///     A test for AddService() when there are multiple services of different types to be added.
        /// </summary>
        [TestMethod]
        public void AddRunnableServiceWithMultipleServiceTypes()
        {
            ServiceDirectory[typeof(IEventBus)] = _bus.Object as IService;

            var service = new RunnableTestServiceWithMultipleServices();
            service.Initialize();
            _target.AddService(service);

            Thread.Sleep(1000);

            Assert.AreEqual(3, ServiceDirectory.Count);

            foreach (var serviceType in service.ServiceTypes)
            {
                Assert.IsTrue(ServiceDirectory.ContainsKey(serviceType));
            }

            Assert.AreEqual(1, ThreadDirectory.Count);
            Assert.IsTrue(ThreadDirectory.ContainsKey(service));
            Assert.IsTrue(ThreadDirectory[service].IsAlive);
            AssertLockAndDisposed();

            _bus.Verify(m => m.Publish(It.IsAny<ServiceAddedEvent>()), Times.Exactly(2));
        }

        /// <summary>
        ///     A test for AddService() when there are multiple non runnable services of different types to be added.
        /// </summary>
        [TestMethod]
        public void AddNonRunnableServiceWithMultipleServiceTypes()
        {
            ServiceDirectory[typeof(IEventBus)] = _bus.Object as IService;

            var service = new NonRunnableTestServiceWithMultipleServices();
            _target.AddService(service);

            Thread.Sleep(1000);

            Assert.AreEqual(3, ServiceDirectory.Count);

            foreach (var serviceType in service.ServiceTypes)
            {
                Assert.IsTrue(ServiceDirectory.ContainsKey(serviceType));
            }

            Assert.AreEqual(0, ThreadDirectory.Count);
            AssertLockAndDisposed();

            _bus.Verify(m => m.Publish(It.IsAny<ServiceAddedEvent>()), Times.Exactly(2));
        }

        /// <summary>A test for RemoveService() for a known, runnable service with multiple services.</summary>
        [TestMethod]
        public void RemoveServiceTestForRunnableWithMultipleServices()
        {
            ServiceDirectory[typeof(IEventBus)] = _bus.Object as IService;

            var service = new RunnableTestServiceWithMultipleServices();
            foreach (var serviceType in service.ServiceTypes)
            {
                ServiceDirectory.Add(serviceType, service);
            }

            var thread = new Thread(service.Run);
            thread.Start();
            ThreadDirectory[service] = thread;

            Thread.Sleep(200);

            _target.RemoveService(service);

            Assert.AreEqual(1, ServiceDirectory.Count);
            Assert.AreEqual(0, ThreadDirectory.Count);
            AssertLockAndDisposed();

            _bus.Verify(m => m.Publish(It.IsAny<ServiceRemovedEvent>()), Times.Exactly(2));
            Assert.AreNotEqual(RunnableState.Running, service.RunState);
        }

        /// <summary>A test for RemoveService() for a known, nonrunnable service with multiple services.</summary>
        [TestMethod]
        public void RemoveServiceTestForNonRunnableWithMultipleServices()
        {
            ServiceDirectory[typeof(IEventBus)] = _bus.Object as IService;

            var service = new NonRunnableTestServiceWithMultipleServices();
            foreach (var serviceType in service.ServiceTypes)
            {
                ServiceDirectory.Add(serviceType, service);
            }

            _target.RemoveService(service);

            Assert.AreEqual(1, ServiceDirectory.Count);
            Assert.AreEqual(0, ThreadDirectory.Count);
            AssertLockAndDisposed();

            _bus.Verify(m => m.Publish(It.IsAny<ServiceRemovedEvent>()), Times.Exactly(2));
        }

        private void AssertLockAndDisposed()
        {
            var @lock = GetPrivateField("_lock") as ReaderWriterLockSlim;
            Assert.IsNotNull(@lock);
            var disposed = (bool)GetPrivateField("_disposed");
            Assert.IsFalse(disposed);
        }

        /// <summary>
        ///     Invokes the private static method.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>result</returns>
        private object InvokePrivateMethod(string methodName, Type[] parameterTypes, object[] arguments, bool @static)
        {
            var privateStaticFlags = BindingFlags.Static | BindingFlags.NonPublic;
            var privateFlags = BindingFlags.Instance | BindingFlags.NonPublic;

            BindingFlags currentFlags;
            if (@static)
            {
                currentFlags = privateStaticFlags;
            }
            else
            {
                currentFlags = privateFlags;
            }

            var privateObject = new PrivateObject(_target);

            var result = privateObject.Invoke(
                methodName,
                currentFlags,
                parameterTypes,
                arguments);

            return result;
        }

        private object GetPrivateField(string fieldName)
        {
            var privateObject = new PrivateObject(_target);

            var fieldValue = privateObject.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            return fieldValue;
        }
    }
}