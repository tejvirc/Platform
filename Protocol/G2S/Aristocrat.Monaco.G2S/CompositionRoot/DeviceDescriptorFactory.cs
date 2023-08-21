namespace Aristocrat.Monaco.G2S.CompositionRoot
{
    using System;
    using Aristocrat.G2S.Client.Devices;
    using SimpleInjector;

    public class DeviceDescriptorFactory : IDeviceDescriptorFactory
    {
        private readonly Container _container;

        public DeviceDescriptorFactory(Container container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public object Create<TDevice>(TDevice device) where TDevice : IDevice
        {
            var genericConsumer = typeof(IDeviceDescriptor<>);

            var interfaces = device.GetType().GetInterfaces();

            foreach (var interfaceType in interfaces)
            {
                Type[] typeArgs = { interfaceType };

                try
                {
                    var consumer = genericConsumer.MakeGenericType(typeArgs);

                    return _container.GetInstance(consumer);
                }
                catch (ArgumentException)
                {
                }
                catch (ActivationException)
                {
                }
            }

            return null;
        }
    }
}