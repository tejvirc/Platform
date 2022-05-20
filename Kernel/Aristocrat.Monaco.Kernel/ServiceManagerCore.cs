namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using log4net;

    /// <summary>
    ///     ServiceManagerCore is the implementation of the IServiceManager interface.
    /// </summary>
    public sealed class ServiceManagerCore : IServiceManager, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<Type, IService> _serviceDirectory = new Dictionary<Type, IService>();
        private readonly Dictionary<IService, Thread> _threadDirectory = new Dictionary<IService, Thread>();

        private bool _disposed;

        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                Shutdown();

                if (_lock != null)
                {
                    _lock.Dispose();
                    _lock = null;
                }
            }
        }

        /// <inheritdoc />
        public void AddService(IService service)
        {
            AddService(service, false);
        }

        /// <inheritdoc />
        public void AddServiceAndInitialize(IService service)
        {
            AddService(service, true);
        }

        /// <inheritdoc />
        public bool IsServiceAvailable<T>()
        {
            _lock.EnterReadLock();
            try
            {
                return _serviceDirectory.ContainsKey(typeof(T));
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc />
        public T GetService<T>()
        {
            var service = TryGetService<T>();

            if (service != null)
            {
                return service;
            }

            throw CreateServiceNotFoundException($"GetService() failed to retrieve the service: unable to find {typeof(T)}", null);
        }

        /// <inheritdoc />
        public T TryGetService<T>()
        {
            _lock.EnterReadLock();
            try
            {
                if (!_serviceDirectory.TryGetValue(typeof(T), out var service))
                {
                    return default(T);
                }

                return (T)service;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc />
        public void RemoveService(IService service)
        {
            Logger.Debug($"Removing service {service.Name}...");

            _lock.EnterUpgradeableReadLock();

            try
            {
                if (_serviceDirectory.Values.Contains(service))
                {
                    var serviceAsRunnable = service as IRunnable;
                    if (serviceAsRunnable != null)
                    {
                        Logger.Debug($"Stopping service {service.Name}...");
                        serviceAsRunnable.Stop();
                    }

                    var keys = new List<Type>(_serviceDirectory.Keys);
                    foreach (var serviceType in keys)
                    {
                        if (_serviceDirectory[serviceType] == service)
                        {
                            RemoveServiceDictionaryEntry(serviceType);
                            PostEvent(new ServiceRemovedEvent(serviceType));
                        }
                    }

                    if (serviceAsRunnable != null)
                    {
                        Logger.Debug($"Joining thread for service {service.Name}...");
                        try
                        {
                            if (!_threadDirectory[service].Join(10000))
                            {
                                _threadDirectory[service].Abort();
                                Logger.Error($"Aborted service runnable: {serviceAsRunnable.GetType()}");
                                Debug.Assert(true, $"Aborted service runnable: {serviceAsRunnable.GetType()}");
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Debug($"Error stopping service runnable: {serviceAsRunnable.GetType()}", e);
                        }

                        Logger.Debug($"Service {service.Name} thread terminated");
                        RemoveThreadDictionaryEntry(service);
                    }

                    if (service is IDisposable serviceAsDisposable)
                    {
                        serviceAsDisposable.Dispose();
                    }
                }
                else
                {
                    throw CreateServiceNotFoundException($"RemoveService() failed to remove the service: {service.Name} -- no service to remove", null);
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        /// <inheritdoc />
        public void RemoveService(Type type)
        {
            Logger.Debug($"Removing service by type {type}...");

            _lock.EnterUpgradeableReadLock();
            try
            {
                if (!_serviceDirectory.TryGetValue(type, out var service))
                {
                    throw CreateServiceNotFoundException($"RemoveService() failed to remove the service by type: {type} -- no service to remove", null);
                }

                RemoveService(service);
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        /// <inheritdoc />
        public void Shutdown()
        {
            Logger.Info("Shutting down the ServiceManager...");

            _lock.EnterUpgradeableReadLock();

            try
            {
                var stoppedServices = new List<IRunnable>();
                foreach (var servicePair in _serviceDirectory)
                {
                    Logger.Debug($"Stopping service: {servicePair.Value.Name} of type: {servicePair.Key}");

                    var service = _serviceDirectory[servicePair.Key];
                    if (service is IRunnable serviceAsRunnable && !stoppedServices.Contains(serviceAsRunnable))
                    {
                        serviceAsRunnable.Stop();
                        stoppedServices.Add(serviceAsRunnable);
                    }
                }

                foreach (var threadPair in _threadDirectory)
                {
                    Logger.Debug($"Stopping the thread for service: {threadPair.Key.Name} with thread id: {threadPair.Value.ManagedThreadId}");

                    _threadDirectory[threadPair.Key].Join();

                    Logger.Debug($"Stopped the thread for service: {threadPair.Key.Name} with thread id: {threadPair.Value.ManagedThreadId}");
                }

                ClearThreadDictionary();

                foreach (var servicePair in _serviceDirectory)
                {
                    if (servicePair.Value is IDisposable serviceAsDisposable)
                    {
                        serviceAsDisposable.Dispose();
                    }
                }

                ClearServiceDictionary();
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }

            Logger.Info("ServiceManager shutdown complete");
        }

        private static ServiceException CreateServiceException(string message, Exception insideException)
        {
            Logger.Error(message);

            return insideException == null
                ? new ServiceException(message)
                : new ServiceException(message, insideException);
        }

        private static ServiceNotFoundException CreateServiceNotFoundException(string message, Exception inner)
        {
            Logger.Error(message);

            return new ServiceNotFoundException(message, inner);
        }

        private void RunService(IRunnable serviceAsRunnable)
        {
            var serviceThread = new Thread(serviceAsRunnable.Run)
            {
                Name = serviceAsRunnable.GetType().Name,
                CurrentCulture = new CultureInfo(Thread.CurrentThread.CurrentCulture.Name),
                CurrentUICulture = new CultureInfo(Thread.CurrentThread.CurrentUICulture.Name)
            };
            serviceThread.Start();

            _threadDirectory.Add((IService)serviceAsRunnable, serviceThread);
        }

        private void RemoveServiceDictionaryEntry(Type serviceType)
        {
            _lock.EnterWriteLock();

            try
            {
                _serviceDirectory.Remove(serviceType);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void RemoveThreadDictionaryEntry(IService service)
        {
            _lock.EnterWriteLock();

            try
            {
                _threadDirectory.Remove(service);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void ClearServiceDictionary()
        {
            _lock.EnterWriteLock();

            try
            {
                _serviceDirectory.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void ClearThreadDictionary()
        {
            _lock.EnterWriteLock();

            try
            {
                _threadDirectory.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void PostEvent<T>(T theEvent)
            where T : IEvent
        {
            var eventBusServiceType = typeof(IEventBus);
            if (_serviceDirectory.ContainsKey(eventBusServiceType))
            {
                var eventBus = (IEventBus)_serviceDirectory[eventBusServiceType];
                eventBus.Publish(theEvent);
            }
        }

        private void AddService(IService service, bool initialize)
        {
            try
            {
                _lock.EnterWriteLock();

                if (!_serviceDirectory.Values.Contains(service))
                {
                    if (initialize)
                    {
                        service.Initialize();
                    }

                    if (service is IRunnable serviceAsRunnable)
                    {
                        RunService(serviceAsRunnable);
                        Logger.Debug($"Started service: {service.Name}");
                    }

                    foreach (var serviceType in service.ServiceTypes)
                    {
                        if (_serviceDirectory.ContainsKey(serviceType))
                        {
                            _serviceDirectory.Remove(serviceType);
                        }

                        _serviceDirectory[serviceType] = service;
                        Logger.Debug($"Adding service: {service.Name} of type {serviceType}");
                        PostEvent(new ServiceAddedEvent(serviceType));
                    }
                }
                else
                {
                    throw CreateServiceException($"AddService() failed to add the service: {service.Name}, it already exists.", null);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
