namespace Aristocrat.Monaco.Hhr.Client.Extensions
{
    using System;
    using System.IO;
    using Communication;
    using Messages;
    using Messages.Converters;
    using SimpleInjector;
    using WorkFlow;

    /// <summary>
    ///     Extensions function for SimpleInjector container.
    /// </summary>
    public static class ContainerExtensions
    {
        /// <summary>
        ///     Registers Client services into Container
        /// </summary>
        /// <param name="container">Container where services needs to be registered.</param>
        public static void RegisterClientServices(this Container container)
        {
            container.Register(typeof(IRequestConverter<>), typeof(IRequestConverter<>).Assembly);
            container.Register(typeof(IResponseConverter<>), typeof(IResponseConverter<>).Assembly);
            container.Collection.Register(typeof(IRequestTimeout), typeof(IRequestTimeout).Assembly);

            container.RegisterSingleton<IMessageFactory>(() => new MessageFactory(container));
            container.RegisterSingleton<ICentralManager, CentralManager>();
            container.RegisterSingleton<ITcpConnection, TcpConnection>();
            container.RegisterSingleton<IConnectionOpener, TcpConnection>();
            container.RegisterSingleton<IConnectionReader, TcpConnection>();
            container.RegisterSingleton<IUdpConnection, UdpConnection>();
            container.RegisterSingleton<ICrcProvider, CrcProvider>();
            container.RegisterSingleton<IMessageFlow, MessageFlow>();
        }

        /// <summary>
        ///     Returns Instance of class implementing a generic interface of a given type.
        /// </summary>
        /// <param name="this">Container instance.</param>
        /// <param name="genericType">Generic base type of class.</param>
        /// <param name="type">GenericType as generic type this.</param>
        /// <param name="param">Parameter list to be used to construct the instance.</param>
        /// <returns>Object of type GenericType</returns>
        /// <exception cref="InvalidDataException"></exception>
        public static object GetGenericInstanceOfType(this Container @this, Type genericType, Type type, params object[] param)
        {
            var instance = @this.GetRegistration(genericType.MakeGenericType(type));
            if (instance == null)
            {
                throw new InvalidDataException($"Converter for Request type {type} not found.");
            }

            var obj = Activator.CreateInstance(instance.Registration.ImplementationType, param);

            return obj;
        }
    }
}