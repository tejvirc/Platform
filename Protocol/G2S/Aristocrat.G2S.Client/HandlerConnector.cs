namespace Aristocrat.G2S.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Devices;

    /// <summary>
    ///     Defines an instance of an IHandlerConnector
    /// </summary>
    public class HandlerConnector : IHandlerConnector
    {
        private readonly Dictionary<Tuple<string, Type>, ICommandHandler> _handlerCache =
            new Dictionary<Tuple<string, Type>, ICommandHandler>();

        /// <inheritdoc />
        public void RegisterHandler(ICommandHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (!IsImplementationOf(handler.GetType(), typeof(ICommandHandler<,>)))
            {
                throw new ArgumentException(@"Invalid type specified.", nameof(handler));
            }

            var supportedTypes =
                handler.GetType()
                    .GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>));

            foreach (var supportedType in supportedTypes)
            {
                var args = supportedType.GetGenericArguments();

                var classType = args[0];
                var commandType = args[1];

                _handlerCache.Add(Tuple.Create(classType.Name, commandType), handler);
            }
        }

        /// <inheritdoc />
        public bool IsClassSupported(ClassCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            return _handlerCache.Keys.Any(key => key.Item1 == command.ClassName);
        }

        /// <inheritdoc />
        public void Clear()
        {
            _handlerCache.Clear();
        }

        /// <inheritdoc />
        public ICommandHandler GetHandler(ClassCommand command)
        {
            // TODO: Something better
            var args = command.GetType().GetGenericArguments();
            if (args.Length < 2)
            {
                return null;
            }

            return _handlerCache.TryGetValue(Tuple.Create(command.ClassName, args[1]), out var handler)
                ? handler
                : null;
        }

        private static bool IsImplementationOf(Type type, Type interfaceType)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (interfaceType == null)
            {
                throw new ArgumentNullException(nameof(interfaceType));
            }

            return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
        }
    }
}