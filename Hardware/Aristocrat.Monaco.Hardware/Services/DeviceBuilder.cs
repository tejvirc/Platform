namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Contracts.Communicator;
    using Contracts.SerialPorts;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;
    using SimpleInjector;

    /// <summary>
    ///     Helper for device factory
    /// </summary>
    /// <typeparam name="TDeviceType"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    internal sealed class DeviceBuilder<TDeviceType, TImplementation>
        where TImplementation : class where TDeviceType : class
    {
        private ILog _logger;
        private readonly Container _container;
        private readonly IComConfiguration _comConfiguration;
        private Type _implementation;
        private Type _communicator;

        public DeviceBuilder(Container container, IComConfiguration comConfiguration)
        {
            _logger = LogManager.GetLogger(GetType());
            _container = container;
            _comConfiguration = comConfiguration;
        }

        public DeviceBuilder<TDeviceType, TImplementation> WithCommunicator(Type communicator)
        {
            _communicator = communicator;
            return this;
        }

        public DeviceBuilder<TDeviceType, TImplementation> WithImplementation(Type implementation)
        {
            _implementation = implementation;
            return this;
        }

        public TDeviceType Build()
        {
            return (TDeviceType)Build(typeof(TDeviceType));
        }

        private object Build(Type type)
        {
            if (type is null)
            {
                return null;
            }
            var args = type.GetConstructors().SingleOrDefault()?.GetParameters()?.Select(
                x =>
                {
                    if (typeof(ICommunicator).IsAssignableFrom(x.ParameterType))
                    {
                        var communicator = (ICommunicator)Build(_communicator);
                        if (communicator != null)
                        {
                            communicator.Device = new Device(
                                new DeviceConfiguration(_comConfiguration.Protocol,
                                    communicator.Manufacturer,
                                    communicator.Model,
                                    string.Empty,
                                    string.Empty,
                                    string.Empty,
                                    string.Empty,
                                    string.Empty,
                                    string.Empty,
                                    string.Empty,
                                    0),
                                _comConfiguration,
                                _container.GetInstance<ISerialPortsService>());

                            (communicator as IGdsCommunicator)?.Configure(_comConfiguration);
                        }

                        return communicator;
                    }

                    if (x.ParameterType == typeof(TImplementation))
                    {
                        return Build(_implementation);
                    }

                    return _container.GetInstance(x.ParameterType);
                }).ToArray() ?? Array.Empty<object>();

            return Activator.CreateInstance(type, args);
        }
    }

    /// <summary>
    ///     IDeviceFactory interface definition
    /// </summary>
    /// <typeparam name="T">The IDeviceAdapter interface this factory will build</typeparam>
    public interface IDeviceFactory<out T> where T : IDeviceAdapter
    {
        /// <summary>
        ///     Construct a hardware device implementation with the given configuration
        /// </summary>
        /// <param name="comConfiguration"></param>
        /// <param name="deviceConfig"></param>
        /// <returns></returns>
        public T CreateDevice(IComConfiguration comConfiguration, ConfigurationData deviceConfig);

        /// <summary>
        ///     Returns true if there is a registered device implementation for the specified protocol
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public bool DoesImplementationExist(string protocol);
    }

    /// <summary>
    ///     Factory to construct devices
    /// </summary>
    /// <typeparam name="TDevice"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    /// <typeparam name="TType"></typeparam>
    public class DeviceFactory<TDevice, TImplementation, TType> : IDeviceFactory<TDevice>, IService
        where TDevice : class, IDeviceAdapter where TImplementation : class where TType : class, TDevice
    {
        private ILog _logger;
        private readonly Container _container;

        private readonly Dictionary<string, Type> _implementations = new();
        private readonly Dictionary<string, Type> _communicators = new();

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="container">SimpleInjector container</param>
        public DeviceFactory(Container container)
        {
            _logger = LogManager.GetLogger(GetType());
            _container = container;
        }

        /// <summary>
        ///     Builds the specific device adapter with appropriate implementation and protocol
        /// </summary>
        /// <param name="comConfiguration"></param>
        /// <returns></returns>
        public TDevice CreateDevice(IComConfiguration comConfiguration, ConfigurationData configData)
        {
            if (comConfiguration == null)
            {
                return null;
            }
            _logger.Debug($"CreateDevice called with type={configData.DeviceType} and protocol={comConfiguration.Protocol}");

            var implementation =
                _implementations.TryGetValue(comConfiguration.Protocol, out var impl) ? impl : null;
            var communicator = _communicators.TryGetValue(comConfiguration.Protocol, out var result)
                ? result
                : null;
            var device = new DeviceBuilder<TType, TImplementation>(_container, comConfiguration)
                .WithImplementation(implementation)
                .WithCommunicator(communicator)
                .Build();

            _logger.Debug($"CreateDevice returning {device.GetType()} with impl={implementation}");
            return device;
        }

        /// <summary>
        ///     Register device implementation type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="protocol">Communication protocol string</param>
        public void RegisterImplementation<T>(string protocol) where T : class
        {
            RegisterImplementation(typeof(T), protocol);
        }

        /// <summary>
        ///     Register communication protocol type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="protocol">Communication protocol string</param>
        public void RegisterCommunicator<T>(string protocol) where T : class
        {
            RegisterCommunicator(typeof(T), protocol);
        }

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="protocol"></param>
        public void RegisterImplementation(Type type, string protocol)
        {
            _implementations[protocol] = type;
        }

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="protocol"></param>
        public void RegisterCommunicator(Type type, string protocol)
        {
            _communicators[protocol] = type;
        }

        public bool DoesImplementationExist(string protocol)
        {
            return _implementations.ContainsKey(protocol);
        }

        public string Name => nameof(DeviceFactory<TDevice, TImplementation, TType>);

        public ICollection<Type> ServiceTypes { get; } = new List<Type> { typeof(IDeviceFactory<TDevice>) };

        public void Initialize()
        {
        }
    }
}