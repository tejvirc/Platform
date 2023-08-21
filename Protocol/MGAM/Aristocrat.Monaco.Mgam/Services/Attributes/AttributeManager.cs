namespace Aristocrat.Monaco.Mgam.Services.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Attribute;
    using Aristocrat.Mgam.Client.Helpers;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Services.Registration;
    using Commands;
    using Common.Events;
    using Kernel;

    /// <summary>
    ///     Manages server attributes on the VLT.
    /// </summary>
    internal class AttributeManager : IAttributeManager, IService
    {
        private readonly ILogger _logger;
        private readonly IEventBus _eventBus;
        private readonly IEgm _egm;
        private readonly IAttributeCache _cache;
 
        /// <summary>
        ///     Initializes a new instance of the <see cref="AttributeManager"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger{TCategory}"/>.</param>
        /// <param name="eventBus"><see cref="IEventBus"/>.</param>
        /// <param name="egm"><see cref="IEgm"/>.</param>
        /// <param name="cache"><see cref="IAttributeCache"/>.</param>
        public AttributeManager(
            ILogger<AttributeManager> logger,
            IEventBus eventBus,
            IEgm egm,
            IAttributeCache cache)
        {
            _logger = logger;
            _eventBus = eventBus;
            _egm = egm;
            _cache = cache;
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IAttributeManager) };

        /// <inheritdoc />
        public void Initialize()
        {
            foreach (var attribute in SupportedAttributes.Get())
            {
                _logger.LogInfo($"Adding attribute {attribute.Name} with value {attribute.DefaultValue} of type {attribute.DefaultValue?.GetType()}");

                _cache.TryAddAttribute(attribute.Name, attribute.DefaultValue);
            }
        }

        /// <inheritdoc />
        public void Add(AttributeInfo attribute)
        {
            SupportedAttributes.Add(attribute);

            _logger.LogInfo($"Adding attribute {attribute.Name} with value {attribute.DefaultValue} of type {attribute.DefaultValue?.GetType()}");

            _cache.TryAddAttribute(attribute.Name, attribute.DefaultValue);
        }

        /// <inheritdoc />
        public bool Has(string name) => _cache.ContainsAttribute(name);

        /// <inheritdoc />
        public void Set(string name, object value, AttributeSyncBehavior behavior)
        {
            if (!Has(name))
            {
                throw new ArgumentOutOfRangeException(nameof(name));
            }

            _logger.LogInfo($"Updating attribute {name} with value {value} of type {value?.GetType()}");
            _cache[name] = value;

            if (behavior == AttributeSyncBehavior.LocalAndServer)
            {
                try
                {
                    _egm.GetService<IRegistration>().SetAttribute(name, value).Wait();
                }
                catch (AggregateException ex) when (ServerResponseError(ex))
                {
                    var responseCode = ex.InnerExceptions.OfType<ServerResponseException>().First().ResponseCode;
                    _eventBus.Publish(new SetAttributeFailedEvent(responseCode));
                }
                catch (ServerResponseException ex)
                {
                    _logger.LogError(ex, $"Set server attribute {name} to \"{value}\" failure");
                    _eventBus.Publish(new SetAttributeFailedEvent(ex.ResponseCode));
                }
            }

            _eventBus.Publish(new AttributeChangedEvent(name, behavior == AttributeSyncBehavior.ServerSource));
        }

        /// <inheritdoc />
        public TValue Get<TValue>(string name, TValue defaultValue)
        {
            if (!_cache.TryGetAttribute(name, out var value))
            {
                _logger.LogWarn($"No attribute found for attribute name: {name}, returning default value");
                return defaultValue;
            }

            if (!(value is TValue))
            {
                _logger.LogWarn($"Incorrect type found for attribute name: {name}, returning default value; Expected: {typeof(TValue)}, Actual: {value?.GetType()}");
                return defaultValue;
            }

            return (TValue)value;
        }

        /// <inheritdoc />
        public async Task Update()
        {
            var registration = _egm.GetService<IRegistration>();

            var result = await registration.GetAttributes();
            var attributes = result.attributes;

            if (attributes == null || attributes.Count == 0)
            {
                throw new RegistrationException("No attributes found", RegistrationFailureBehavior.Lock);
            }

            foreach (var a in attributes)
            {
                if (_cache.ContainsAttribute(a.Name))
                {
                    _logger.LogInfo(
                        $"Updating attribute {a.Name} with value {a.Value} of type {a.Value?.GetType()}");
                    _cache[a.Name] = AttributeHelper.ConvertValue(a.Name, a.Value);
                }
            }

            _eventBus.Publish(new AttributesUpdatedEvent());
        }

        private static bool ServerResponseError(AggregateException ex)
        {
            return ex?.InnerExceptions.Any(e => e.GetType() == typeof(ServerResponseException)) ?? false;
        }
    }
}
