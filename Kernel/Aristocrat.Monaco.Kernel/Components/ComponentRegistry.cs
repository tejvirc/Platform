namespace Aristocrat.Monaco.Kernel.Components
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Contracts.Components;

    /// <summary>
    ///     Registry for software components
    /// </summary>
    public class ComponentRegistry : IService, IComponentRegistry, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly ConcurrentDictionary<string, Component> _components = new ConcurrentDictionary<string, Component>();

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ComponentRegistry" /> class.
        /// </summary>
        public ComponentRegistry()
            : this(ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ComponentRegistry" /> class.
        /// </summary>
        /// <param name="bus">An <see cref="IEventBus" /> instance</param>
        public ComponentRegistry(IEventBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc />
        public IEnumerable<Component> Components => _components.Values;

        /// <inheritdoc />
        public void Register(Component component, bool cycling)
        {
            UnRegister(component.ComponentId, cycling);

            _components[component.ComponentId] = component;

            if (!cycling)
            {
                _bus.Publish(new ComponentAddedEvent(component));
            }
        }

        /// <inheritdoc />
        public bool UnRegister(string id, bool cycling)
        {
            if (!cycling && _components.TryRemove(id, out var component))
            {
                _bus.Publish(new ComponentRemovedEvent(component));
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public Component Get(string id)
        {
            _components.TryGetValue(id, out var component);

            return component;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IComponentRegistry) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _components.Clear();
            }

            _disposed = true;
        }
    }
}