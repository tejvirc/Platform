namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using AutoMapper;
    using Converters;
    using Data;
    using SimpleInjector;
    using Utility;

    /// <inheritdoc cref="IMessageFactory" />
    public class MessageFactory : IMessageFactory
    {
        private readonly Container _container;
        private readonly Dictionary<Type, object> _messageTypeProducers = new Dictionary<Type, object>();
        private readonly Dictionary<Command, Type> _responseTypes = new Dictionary<Command, Type>();

        /// <summary>
        ///     Constructor that takes the SimpleInjector Container in order to use it to find message converters
        /// </summary>
        /// <param name="container">The SimpleInjector root class where we may find converter implementations</param>
        /// <param name="assemblies"></param>
        public MessageFactory(Container container, Assembly[] assemblies = null)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            LoadResponseTypes(assemblies);
        }

        /// <inheritdoc cref="IMessageFactory.Serialize" />
        public byte[] Serialize(Request message)
        {
            var converter = GetInstanceProducer(typeof(IRequestConverter<>), message.GetType()) as dynamic;

            return converter.Convert(message as dynamic);
        }

        /// <inheritdoc cref="IMessageFactory.Deserialize" />
        public Response Deserialize(byte[] data)
        {
            var header = MessageUtility.ConvertByteArrayToMessage<MessageHeader>(data);
            if (!_responseTypes.TryGetValue((Command)header.Command, out var responseType))
            {
                return new EmptyResponse();
            }

            var converter = GetInstanceProducer(typeof(IResponseConverter<>), responseType) as dynamic;

            var response = (Response)converter.Convert(MessageUtility.ExtractMessageHeader(data) as dynamic);
            response.ReplyId = header.ReplyId;

            return response;
        }

        private object GetInstanceProducer(Type serviceType, Type type)
        {
            if (_messageTypeProducers.TryGetValue(type, out var instance))
            {
                return instance;
            }

            var messageConverter = _container.GetRegistration(serviceType.MakeGenericType(type));
            if (messageConverter == null)
            {
                throw new InvalidDataException($"Converter for Request type {type} not found.");
            }

            var implementationType = messageConverter.Registration.ImplementationType;

            _messageTypeProducers.Add(
                type,
                implementationType.GetConstructors()
                    .Any(x => x.GetParameters().Length == 0)
                    ? Activator.CreateInstance(implementationType)
                    : Activator.CreateInstance(
                        implementationType,
                        _container.GetInstance<IMapper>()));

            return _messageTypeProducers[type];
        }

        private void LoadResponseTypes(Assembly[] assemblies)
        {
            foreach (var response in AssemblyUtilities.LoadAllTypesImplementing<Response>(assemblies))
            {
                _responseTypes.Add(response.Command, response.GetType());
            }
        }
    }
}