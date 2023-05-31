namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using log4net;

    /// <summary>
    ///     The ServiceWaiter allows the user to register types of services they need to be available before moving forward,
    ///     then wait for all of the services to be available.  The ServiceWaiter uses events and must be disposed after use to
    ///     clean up subscriptions.
    /// </summary>
    public sealed class ServiceWaiter : IDisposable
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(nameof(ServiceWaiter));

        private readonly IEventBus _eventBus;

        private readonly object _lock = new object();

        private AutoResetEvent _allServicesReady = new AutoResetEvent(false);

        private bool _disposed;

        private readonly List<Type> _services = new List<Type>();

        private bool _waiting;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceWaiter" /> class.
        /// </summary>
        /// <param name="eventBus">the event bus</param>
        public ServiceWaiter(IEventBus eventBus)
        {
            _eventBus = eventBus;

            //zhg**: ServiceManagerCore sends ServiceAddedEvent once its AddService is called
            _eventBus.Subscribe<ServiceAddedEvent>(this, HandleServiceAdded);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="ServiceWaiter" /> class.
        /// </summary>
        ~ServiceWaiter()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Adds service to wait for if it's not already running.
        /// </summary>
        /// <typeparam name="T">Type of service to add.</typeparam>
        public void AddServiceToWaitFor<T>()
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    return;
                }

                var serviceType = typeof(T);
                if (!_services.Contains(serviceType))
                {
                    if (!ServiceManager.GetInstance().IsServiceAvailable<T>())
                    {
                        Logger.DebugFormat("Adding service {0}", serviceType);
                        _services.Add(serviceType);
                    }
                    else
                    {
                        Logger.DebugFormat("Service {0} is already available.", serviceType);
                    }
                }
            }
        }

        /// <summary>
        ///     Waits for services specified by calls to AddService(). If no services are specified, or if all services are ready,
        ///     returns immediately.
        /// </summary>
        /// <returns>A value indicating if all the services are available.</returns>
        public bool WaitForServices()
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    return false;
                }

                if (_services.Count < 1)
                {
                    Logger.Debug("All services ready.");
                    return true;
                }

                _waiting = true;
                Logger.Debug("Waiting for services");
            }

            _allServicesReady.WaitOne();

            lock (_lock)
            {
                _waiting = false;
                if (_disposed || _services.Count != 0)
                {
                    Logger.Info("WaitForServices cancelled");
                    return false;
                }

                Logger.Debug("All services ready");
                return true;
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            lock (_lock)
            {
                _eventBus.UnsubscribeAll(this);

                if (disposing)
                {
                    if (_allServicesReady != null)
                    {
                        _allServicesReady.Set();
                        _allServicesReady.Close();
                    }
                }

                _services.Clear();
                _allServicesReady = null;
                _disposed = true;
            }
        }

        private void HandleServiceAdded(ServiceAddedEvent data)
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    return;
                }

                if (data == null)
                {
                    return;
                }

                if (_services.Contains(data.ServiceType))
                {
                    Logger.Debug($"Service {data.ServiceType} is ready");
                    _services.Remove(data.ServiceType);

                    if (_waiting && _services.Count == 0)
                    {
                        _allServicesReady.Set();
                    }
                }
            }
        }
    }
}