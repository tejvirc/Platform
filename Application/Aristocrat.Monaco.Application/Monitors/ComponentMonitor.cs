namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts.Components;
    using Kernel.Contracts.Events;

    /// <summary>
    ///     Watches for component changes and persists them for
    ///     later comparision to determine if there has been a change
    /// </summary>
    public class ComponentMonitor : IService, IComponentMonitor, IDisposable
    {
        private List<string> ComponentNames { get; set; } = new List<string>();
        private bool _disposed;
        private bool _startingUp = true;
        private bool _firstBoot;
        private bool _componentsChanged;
        private readonly List<string> _callers = new List<string>();

        private readonly IEventBus _eventBus;
        private readonly IComponentRegistry _componentRegistry;
        private readonly IPersistentStorageAccessor _accessor;
        private const string PersistenceBlockName = "Aristocrat.Monaco.Application.ComponentMonitor";

        /// <summary>
        ///     Constructs a new instance of the ComponentMonitor class.
        ///     Default constructor used to create the service
        /// </summary>
        public ComponentMonitor() :
            this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IComponentRegistry>())
        { }

        /// <summary>
        ///     Constructs a new instance of the ComponentMonitor class.
        ///     Used by unit tests.
        /// </summary>
        /// <param name="eventBus">Reference to the event bus</param>
        /// <param name="storageManager">Reference to the persistent storage manager</param>
        /// <param name="componentRegistry">Reference to the component registry</param>
        public ComponentMonitor(
            IEventBus eventBus,
            IPersistentStorageManager storageManager,
            IComponentRegistry componentRegistry)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            var storage = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
            _componentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));

            _eventBus.Subscribe<ComponentAddedEvent>(this, _ => UpdateComponentData());
            _eventBus.Subscribe<ComponentRemovedEvent>(this, _ => UpdateComponentData());
            _eventBus.Subscribe<InitializationCompletedEvent>(this, _ => SafeToCheckForComponentChanges());
            _accessor = storage.GetAccessor(PersistenceLevel.Transient, PersistenceBlockName);
        }

        /// <inheritdoc />
        public string Name => typeof(ComponentMonitor).Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IComponentMonitor) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public bool HaveComponentsChangedWhilePoweredOff(string identifier)
        {
            if (_callers.Contains(identifier) || _startingUp)
            {
                return false;
            }

            _callers.Add(identifier);
            return _componentsChanged && !_firstBoot;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Dispose of resources
        /// </summary>
        /// <param name="disposing">True if disposing the first time</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void SafeToCheckForComponentChanges()
        {
            // this will be an empty list on first boot after a persistence clear
            ComponentNames = StorageUtilities.GetListFromByteArray<string>((byte[])_accessor["Components"]).ToList();
            var components = _componentRegistry.Components.Select(id => id.ComponentId).ToArray();

            _componentsChanged = components.Length != ComponentNames.Count || components.Except(ComponentNames).Any();
            _firstBoot = ComponentNames.Count == 0;
            _startingUp = false;

            if (_componentsChanged)
            {
                // the first boot after persistence clear will not have any components persisted
                UpdateComponentData();
            }
        }

        private void UpdateComponentData()
        {
            // Ignore ComponentAdded/Removed events until after startup is finished.
            if (_startingUp)
            {
                return;
            }

            // get the current list of components from the component registry
            ComponentNames = _componentRegistry.Components.Select(id => id.ComponentId).ToList();

            // write them to persistence
            using (var transaction = _accessor.StartTransaction())
            {
                transaction["Components"] = StorageUtilities.ToByteArray(ComponentNames);
                transaction.Commit();
            }
        }
    }
}