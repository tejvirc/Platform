namespace Aristocrat.G2S.Client
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Communications;
    using Configuration;
    using Devices;

    /// <summary>
    ///     Implementation of a G2S Egm
    /// </summary>
    public class G2SEgm : IG2SEgm, IDisposable
    {
        private const int HandledMessageQueueDepth = 500;

        private readonly IDeviceConnector _deviceConnector;
        private readonly ConcurrentQueue<ClassCommand> _handledMessages = new ConcurrentQueue<ClassCommand>();
        private readonly IHandlerConnector _handlerConnector;
        private readonly IHostConnector _hostConnector;
        private readonly List<IObserver<ClassCommand>> _observers = new List<IObserver<ClassCommand>>();
        private readonly object _syncLock = new object();

        private bool _disposed;

        private IReceiveEndpoint _receiveEndpoint;

        /// <summary>
        ///     Initializes a new instance of the <see cref="G2SEgm" /> class.
        /// </summary>
        /// <param name="egmId">The unique egm identifier</param>
        /// <param name="hostConnector">An instance of a IHostConnector</param>
        /// <param name="deviceConnector">An instance of a IDeviceConnector</param>
        /// <param name="handlerConnector">An instance of a IHandlerConnector</param>
        /// <param name="receiveEndpoint">An instance of a IReceiveEndpoint.</param>
        public G2SEgm(
            string egmId,
            IHostConnector hostConnector,
            IDeviceConnector deviceConnector,
            IHandlerConnector handlerConnector,
            IReceiveEndpoint receiveEndpoint)
        {
            if (string.IsNullOrEmpty(egmId))
            {
                throw new ArgumentNullException(nameof(egmId));
            }

            Id = egmId;
            _hostConnector = hostConnector ?? throw new ArgumentNullException(nameof(hostConnector));
            _deviceConnector = deviceConnector ?? throw new ArgumentNullException(nameof(deviceConnector));
            _handlerConnector = handlerConnector ?? throw new ArgumentNullException(nameof(handlerConnector));
            _receiveEndpoint = receiveEndpoint ?? throw new ArgumentNullException(nameof(receiveEndpoint));

            // Host identifier 0 (zero) MUST identify the EGM itself and MUST always be contained in the list of all host indexes.
            RegisterHost(Constants.EgmHostId, null, false, Constants.EgmHostIndex);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <inheritdoc />
        public bool Running { get; private set; }

        /// <inheritdoc />
        public IEnumerable<IDevice> Devices => _deviceConnector.Devices;

        /// <inheritdoc />
        public Uri Address
        {
            get
            {
                lock (_syncLock)
                {
                    return _receiveEndpoint.Address;
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<IHostControl> Hosts => _hostConnector.Hosts;

        /// <inheritdoc />
        public IHostControl GetHostById(int hostId)
        {
            return _hostConnector.GetHostById(hostId);
        }

        /// <inheritdoc />
        public IHostControl RegisterHost(int hostId, Uri hostUri, bool requiredForPlay, int index)
        {
            return _hostConnector.RegisterHost(hostId, hostUri, requiredForPlay, index);
        }

        /// <inheritdoc />
        public IHost UnregisterHost(int hostId, IEgmStateManager egmStateManager)
        {
            if (hostId == Constants.EgmHostId)
            {
                throw new ArgumentException($@"Host Id {hostId} is the EGM and cannot be removed.", nameof(hostId));
            }

            var host = Hosts.FirstOrDefault(h => h.Id == hostId);
            if (host == null)
            {
                throw new ArgumentOutOfRangeException(nameof(hostId), $@"Host Id {hostId} is not a registered host.");
            }

            var cabinet = GetDevice<ICabinetDevice>();

            var devices = Devices.Where(d => d.IsMember(host.Id)).Cast<ClientDeviceBase>().ToList();
            foreach (var device in devices)
            {
                if (device.IsHostOriented())
                {
                    device.Close();
                    RemoveDevice(device);
                    egmStateManager.Enable(device, EgmState.EgmDisabled);
                    egmStateManager.Enable(device, EgmState.HostLocked);
                    egmStateManager.Enable(device, EgmState.HostDisabled);
                    egmStateManager.Enable(device, EgmState.TransportDisabled);
                    cabinet?.RemoveCondition(device, EgmState.EgmDisabled);
                    cabinet?.RemoveCondition(device, EgmState.HostLocked);
                    cabinet?.RemoveCondition(device, EgmState.HostDisabled);
                    cabinet?.RemoveCondition(device, EgmState.TransportDisabled);
                }
                else if (device.IsOwner(host.Id))
                {
                    device.HasOwner(Constants.EgmHostId, device.Active);
                }
                else if (device.IsConfigurator(host.Id))
                {
                    device.HasConfigurator(Constants.EgmHostId);
                }
                else if (device.IsGuest(host.Id))
                {
                    device.RemoveGuest(host.Id);
                }
            }

            host.Queue.MessageReceived -= QueueMessageHandled;
            host.Queue.MessageSent -= QueueMessageHandled;

            return _hostConnector.UnregisterHost(host.Id);
        }

        /// <inheritdoc />
        public void Start(IEnumerable<IStartupContext> contexts)
        {
            lock (_syncLock)
            {
                if (Running)
                {
                    return;
                }

                _receiveEndpoint.Open();

                var startupContexts = contexts as IList<IStartupContext> ?? contexts.ToList();

                var tasks = new List<Task>();

                foreach (var host in Hosts.OrderBy(h => h.Id))
                {
                    var context = startupContexts.FirstOrDefault(c => c.HostId == host.Id) ?? new StartupContext();

                    host.Queue.MessageReceived += QueueMessageHandled;
                    host.Queue.MessageSent += QueueMessageHandled;

                    tasks.Add(Task.Factory.StartNew(() => StartHost(host, context)));
                }

                Task.WaitAll(tasks.ToArray());

                Running = true;
            }
        }

        /// <inheritdoc />
        public void Stop()
        {
            lock (_syncLock)
            {
                if (!Running)
                {
                    return;
                }

                foreach (var device in Devices)
                {
                    device.Close();
                }

                foreach (var host in Hosts)
                {
                    host.Queue.MessageReceived -= QueueMessageHandled;
                    host.Queue.MessageSent -= QueueMessageHandled;

                    host.Stop();
                }

                _receiveEndpoint.Close();

                Running = false;
            }
        }

        /// <inheritdoc />
        public void Restart(IEnumerable<IStartupContext> contexts)
        {
            lock (_syncLock)
            {
                var tasks = new List<Task>();

                foreach (var context in contexts)
                {
                    var host = Hosts.Single(h => h.Id == context.HostId);

                    foreach (var device in Devices.Where(d => d.Owner == host.Id))
                    {
                        device.Close();
                    }

                    host.Stop();

                    tasks.Add(Task.Factory.StartNew(() => StartHost(host, context)));
                }

                Task.WaitAll(tasks.ToArray());
            }
        }

        /// <inheritdoc />
        public IDevice GetDevice(string deviceClass, int deviceId)
        {
            return _deviceConnector.GetDevice(deviceClass, deviceId);
        }

        /// <inheritdoc />
        public TDevice GetDevice<TDevice>()
            where TDevice : IDevice
        {
            return _deviceConnector.GetDevice<TDevice>();
        }

        /// <inheritdoc />
        public IEnumerable<TDevice> GetDevices<TDevice>() where TDevice : IDevice
        {
            return _deviceConnector.GetDevices<TDevice>();
        }

        /// <inheritdoc />
        public TDevice GetDevice<TDevice>(int deviceId)
            where TDevice : IDevice
        {
            return _deviceConnector.GetDevice<TDevice>(deviceId);
        }

        /// <inheritdoc />
        public IDevice AddDevice(IDevice device)
        {
            if (Hosts.All(h => h.Id != device.Owner))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(device),
                    $@"There is no host registered for owner id {device.Owner}");
            }

            if (device is ClientDeviceBase clientDevice)
            {
                clientDevice.Queue = Hosts.Single(h => h.Id == device.Owner).Queue;
            }

            return _deviceConnector.AddDevice(device);
        }

        /// <inheritdoc />
        public IDevice RemoveDevice(IDevice device)
        {
            return _deviceConnector.RemoveDevice(device);
        }

        /// <inheritdoc />
        public ICommandHandler GetHandler(ClassCommand command)
        {
            return _handlerConnector.GetHandler(command);
        }

        /// <inheritdoc />
        public void RegisterHandler(ICommandHandler handler)
        {
            _handlerConnector.RegisterHandler(handler);
        }

        /// <inheritdoc />
        public bool IsClassSupported(ClassCommand command)
        {
            return _handlerConnector.IsClassSupported(command);
        }

        /// <inheritdoc />
        public void Clear()
        {
            _handlerConnector.Clear();
        }

        /// <inheritdoc />
        public IEnumerable<IDevice> ApplyHostPermissions(
            int hostId,
            IEnumerable<OwnedDevice> owned,
            IEnumerable<IDevice> config,
            IEnumerable<IDevice> guest)
        {
            var host = Hosts.FirstOrDefault(h => h.Id == hostId);
            if (host == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(hostId),
                    $@"There is no host registered for id {hostId}");
            }

            var affectedDevices = new List<IDevice>();

            affectedDevices.AddRange(host.Owns(Devices, owned));

            affectedDevices.AddRange(host.Configures(Devices, config));

            affectedDevices.AddRange(host.GuestOf(Devices, guest));

            return affectedDevices.Distinct();
        }

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<ClassCommand> observer)
        {
            if (!_observers.Contains(observer))
            {
                var messages = _handledMessages.ToList();
                messages.ForEach(observer.OnNext);

                _observers.Add(observer);
            }

            return new Unsubscriber(_observers, observer);
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Stop();

                foreach (var host in _hostConnector.Hosts)
                {
                    host.Queue.MessageReceived -= QueueMessageHandled;
                    host.Queue.MessageSent -= QueueMessageHandled;

                    _hostConnector.UnregisterHost(host.Id);
                }

                _handlerConnector.Clear();

                _observers.Clear();

                lock (_syncLock)
                {
                    _receiveEndpoint?.Dispose();
                    _receiveEndpoint = null;
                }
            }

            _disposed = true;
        }

        private void StartHost(IHostControl host, IStartupContext context)
        {
            host.Start();

            foreach (var device in Devices.Where(d => d.Owner == host.Id))
            {
                device.Open(context);
            }
        }

        private void QueueMessageHandled(object sender, MessageHandledEventArgs e)
        {
            _handledMessages.Enqueue(e.Command);
            if (_handledMessages.Count > HandledMessageQueueDepth)
            {
                _handledMessages.TryDequeue(out var _);
            }

            foreach (var observer in _observers)
            {
                observer.OnNext(e.Command);
            }
        }

        private class Unsubscriber : IDisposable
        {
            private readonly IObserver<ClassCommand> _observer;
            private readonly List<IObserver<ClassCommand>> _observers;

            public Unsubscriber(List<IObserver<ClassCommand>> observers, IObserver<ClassCommand> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                {
                    _observers.Remove(_observer);
                }
            }
        }
    }
}