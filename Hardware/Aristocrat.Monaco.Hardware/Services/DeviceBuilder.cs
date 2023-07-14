namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts.Communicator;
    using Contracts.SharedDevice;
    using SimpleInjector;

    /// <summary>
    ///     Helper for device factory
    /// </summary>
    /// <typeparam name="TDeviceType"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    internal sealed class DeviceBuilder<TDeviceType, TImplementation>
        where TImplementation : class where TDeviceType : class
    {
        private readonly Container _container;
        private Type _implementation;
        private Type _communicator;

        public DeviceBuilder(Container container)
        {
            _container = container;
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
            var args = type.GetConstructors().SingleOrDefault()?.GetParameters()?.Select(
                x =>
                {
                    if (x.ParameterType == typeof(ICommunicator))
                    {
                        return Build(_communicator);
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
        public T CreateDevice(IComConfiguration comConfiguration);
    }

    /// <summary>
    ///     Factory to construct devices
    /// </summary>
    /// <typeparam name="TDevice"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    /// <typeparam name="TType"></typeparam>
    public class DeviceFactory<TDevice, TImplementation, TType> : IDeviceFactory<TDevice>
        where TDevice : class, IDeviceAdapter where TImplementation : class where TType : class, TDevice
    {
        private readonly Container _container;

        private readonly Dictionary<string, Type> _implementations = new();
        private readonly Dictionary<string, Type> _communicators = new();

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="container">SimpleInjector container</param>
        public DeviceFactory(Container container)
        {
            _container = container;
        }

        /// <summary>
        ///     Builds the specific device adapter with appropriate implementation and protocol
        /// </summary>
        /// <param name="comConfiguration"></param>
        /// <returns></returns>
        public TDevice CreateDevice(IComConfiguration comConfiguration)
        {
            var implementation =
                _implementations.TryGetValue(comConfiguration.Protocol, out var impl) ? impl : null;
            var communicator = _communicators.TryGetValue(comConfiguration.Protocol, out var result)
                ? result
                : null;
            var device = new DeviceBuilder<TType, TImplementation>(_container)
                .WithImplementation(implementation)
                .WithCommunicator(communicator)
                .Build();
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
    }
}