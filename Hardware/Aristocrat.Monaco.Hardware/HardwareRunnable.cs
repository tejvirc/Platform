namespace Aristocrat.Monaco.Hardware
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Contracts;
    using Contracts.Discovery;
    using Contracts.IO;
    using Contracts.Persistence;
    using Contracts.SharedDevice;
    using EdgeLight.Device;
    using EdgeLight.Services;
    using Kernel;
    using Kernel.Contracts;
    using Kernel.Contracts.Events;
    using log4net;
    using Mono.Addins;
    using Properties;

    /// <summary>
    ///     <para>
    ///         The BootExtender is one of the primary core component of the Hardware layer.
    ///         The Hardware Bootstrap component controls the initialization of the Hardware layer.
    ///         The BootExtender is responsible for loading components and extensions in the Hardware layer.
    ///         Basically this is an extender which extends from the Bootstrap of the Kernel layer.
    ///         So this acts as an entry point in the Hardware layer.
    ///     </para>
    ///     <para>The major functions of the Boot Extender are</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <para>
    ///                     Boot extender loads the Hardware Discoverer and makes it run in the background in a separate
    ///                     thread.
    ///                     This is implemented as add-in Architectural model.
    ///                 </para>
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <para>
    ///                     It waits till persisted storage (NVRAM) is been discovered.
    ///                 </para>
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <para>
    ///                     Searches the provided path for Service add-ins, instantiates those add-ins and
    ///                     passes them to the ServiceManager to run and maintain.
    ///                 </para>
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <para>
    ///                     Checks if the security dongle is attached to this terminal. It is mandatory to have Security Dongle
    ///                     in place else
    ///                     the boot extender will simply exit the process by stopping the runnables and unloading the
    ///                     services.
    ///                 </para>
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <para>
    ///                     Then it searches runnable add-ins, instantiates those add-ins and runs them in background.
    ///                     The add-in descriptions can be represented using an XML manifest (HardwareBootstrap.Addin.xml).
    ///                 </para>
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <para>
    ///                     It subscribes to Persistent Storage Clear StartedEvent which signals that the hardware layer should
    ///                     shut down all its higher layers, perform persisted storage clear and restart.
    ///                 </para>
    ///             </description>
    ///         </item>
    ///     </list>
    /// </summary>
    public class HardwareRunnable : BaseRunnable
    {
        private const string StoragePath = "/Hardware/PersistentStorageService";
        private const string SecondaryStoragePath = "/Hardware/SecondaryStorageService";
        private const string StorageExtensionPath = "/Hardware/Persistence";
        private const string ServicesPath = "/Hardware/Services";
        private const string RunnablesPath = "/Hardware/Runnables";
        private const string ExtenderPath = "/Hardware/BootExtender";
        private const string PropertyProvidersPath = "/Hardware/PropertyProviders";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly RunnablesManager _runnablesManager = new RunnablesManager();
        private readonly List<IService> _services = new List<IService>();

        private bool _clearingPersistentStorage;
        private AutoResetEvent _clearingPersistentStorageResetEvent = new AutoResetEvent(false);
        private IRunnable _extender;
        private PersistenceLevel _persistenceLevelToClear = PersistenceLevel.Static;
        private bool _waitingOnPersistentStorageClearedEvent;

        protected override void OnInitialize()
        {
            Logger.Info("Initialized");
        }

        protected override void OnRun()
        {
            Logger.Info("Run started");

            LoadStorage();

            if (RunState == RunnableState.Running)
            {
                LoadPropertyProviders();

                LoadServices();

                LoadRunnables();

                ServiceManager.GetInstance().GetService<IEventBus>()
                    .Subscribe<PersistentStorageClearStartedEvent>(this, HandleClearStartedEvent);

                ServiceManager.GetInstance().GetService<IEventBus>()
                    .Subscribe<ServiceRemovedEvent>(this,
                        a =>
                        {
                            if(_services.Any(service => service.GetType() == a.ServiceType))
                            {
                                _services.RemoveAll(service => service.GetType() == a.ServiceType);
                            }
                        });

                if (RunState == RunnableState.Running)
                {
                    RunBootExtender();
                    Logger.Debug($"Boot extender exited, RunState: {RunState}");
                }

                // If we are going to clear persistent storage then we don't want to exit.
                // Instead, we want to wait for the signal then restart our Run loop.
                if (_clearingPersistentStorage)
                {
                    PrepareForPersistenceClear();
                    Logger.Debug("Waiting for persistent storage clear");
                    _clearingPersistentStorageResetEvent.WaitOne();
                    Logger.Debug("Stopped for persistent storage clear / soft reboot");
                }
            }

            UnLoadLayer();

            Logger.Info("Stopped");
        }

        protected override void OnStop()
        {
            Logger.Info("Stopping");

            if (_clearingPersistentStorageResetEvent != null)
            {
                Logger.Debug("Releasing clearing persistent storage AutoResetEvent");
                _clearingPersistentStorageResetEvent.Set();
            }

            if (_extender != null)
            {
                Logger.Debug("Stopping HardwareBootstrap's boot extender");
                _extender.Stop();
            }

            UnsubscribeAll();
        }

        protected override void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    _clearingPersistentStorageResetEvent.Close();
                    _clearingPersistentStorageResetEvent = null;
                }
            }

            base.Dispose(disposing);
        }

        private static void WritePendingActionToMessageDisplay(string resourceStringName)
        {
            var invariantString = Resources.ResourceManager.GetString(resourceStringName, CultureInfo.InvariantCulture);
            var localizedString = Resources.ResourceManager.GetString(resourceStringName);

            var display = ServiceManager.GetInstance().GetService<IMessageDisplay>();

            // due to the way resource lookup occurs, if the invariant version is valid the
            // localized version will also be non-null since it can fall back to the invariant version.
            // If the localized version is null it means the invariant version will also be null.
            if (!string.IsNullOrEmpty(localizedString))
            {
                display.DisplayStatus(localizedString);
                Logger.Info(invariantString);
            }
            else
            {
                Logger.Warn($"Localized string name \"{resourceStringName}\" not found");
                display.DisplayStatus(resourceStringName);
            }
        }

        private void LoadStorage()
        {
            Logger.Debug("Creating Persistent Storage");

            var baseSecondaryStorageNode = MonoAddinsHelper.GetSingleTypeExtensionNode(SecondaryStoragePath);
            var baseSecondaryStorage = (IService)baseSecondaryStorageNode.CreateInstance();

            ServiceManager.GetInstance().AddServiceAndInitialize(baseSecondaryStorage);
            _services.Add(baseSecondaryStorage); 

            var baseStorageNode = MonoAddinsHelper.GetSingleTypeExtensionNode(StoragePath);
            var baseStorage = (IService)baseStorageNode.CreateInstance();

            ServiceManager.GetInstance().AddServiceAndInitialize(baseStorage);
            _services.Add(baseStorage);

            var storageExtension = MonoAddinsHelper.GetSingleTypeExtensionNode(StorageExtensionPath);
            var extension = (IService)storageExtension.CreateInstance();

            ServiceManager.GetInstance().AddServiceAndInitialize(extension);
            _services.Add(extension);

            ServiceManager.GetInstance().GetService<IEventBus>().Publish(new PersistenceReadyEvent());
        }

        private void HandleClearStartedEvent(PersistentStorageClearStartedEvent incomingEvent)
        {
            if (_waitingOnPersistentStorageClearedEvent)
            {
                Logger.Info("Already waiting for PersistentStorageClearedEvent");
                return;
            }

            ServiceManager.GetInstance().GetService<IEventBus>().Unsubscribe<PersistentStorageClearStartedEvent>(this);

            _waitingOnPersistentStorageClearedEvent = true;
            _clearingPersistentStorage = true;
            _persistenceLevelToClear = incomingEvent.Level;

            // Stop the boot extender and proceed once its Run() method returns.
            _extender.Stop();
        }

        private void HandleClearedEvent(PersistentStorageClearedEvent incomingEvent)
        {
            ServiceManager.GetInstance().GetService<IEventBus>().Unsubscribe<PersistentStorageClearedEvent>(this);

            if (_waitingOnPersistentStorageClearedEvent)
            {
                Logger.Debug("Posting SoftRebootEvent");
                _waitingOnPersistentStorageClearedEvent = false;
                ServiceManager.GetInstance().GetService<IEventBus>().Publish(new ExitRequestedEvent(ExitAction.Restart));
            }
            else
            {
                // This is bound to be a bad situation right?
                Logger.Error("Received PersistentStorageClearedEvent and was not expecting or waiting on it");
            }
        }

        private void UnsubscribeAll()
        {
            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            eventBus.UnsubscribeAll(this);
        }

        private void LoadServices()
        {
            WritePendingActionToMessageDisplay("LoadingHardwareServices");
            foreach (var eventListenerNode in AddinManager.GetExtensionNodes<StartupEventListenerImplementationExtensionNode>(
                HardwareConstants.StartupEventListenerExtensionPoint))
            {
                var listenerBase = (StartupEventListenerBase)eventListenerNode.CreateInstance();
                listenerBase.EventBus = ServiceManager.GetInstance().GetService<IEventBus>();
                InitializeService(listenerBase);
            }

            foreach (TypeExtensionNode serviceNode in AddinManager.GetExtensionNodes<TypeExtensionNode>(ServicesPath))
            {
                InitializeService((IService)serviceNode.CreateInstance());
            }

#if !(RETAIL)
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var simulatedEdgeLightCabinet = propertiesManager.GetValue(
                HardwareConstants.SimulateEdgeLighting,
                false);

            if (!simulatedEdgeLightCabinet)
            {
                InitializeService(new EdgeLightDeviceFactory());
            }
            else
            {
                Logger.Debug("Initializing simulated edge lighting services");
                InitializeService(new SimEdgeLightDeviceFactory());
            }
#else
            InitializeService(new EdgeLightDeviceFactory());
#endif

            InitializeService(new EdgeLightingControllerService());

            if (ServiceManager.GetInstance().GetService<IIO>() is IDeviceService service)
            {
                service.Enable(EnabledReasons.Configuration);
            }
        }

        private void InitializeService(IService service)
        {
            service.Initialize();
            ServiceManager.GetInstance().AddService(service);
            _services.Add(service);
        }

        private void LoadRunnables()
        {
            WritePendingActionToMessageDisplay("LoadingHardwareRunnables");
            _runnablesManager.StartRunnables(RunnablesPath);
        }

        private static void LoadPropertyProviders()
        {
            var manager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            foreach (var node in MonoAddinsHelper.GetSelectedNodes<TypeExtensionNode>(PropertyProvidersPath))
            {
                manager.AddPropertyProvider((IPropertyProvider)node.CreateInstance());
            }
        }

        private void RunBootExtender()
        {
            var typeExtensionNode = MonoAddinsHelper.GetSingleTypeExtensionNode(ExtenderPath);
            _extender = (IRunnable)typeExtensionNode.CreateInstance();
            _extender.Initialize();
            if (RunState == RunnableState.Running)
            {
                Logger.Info("Running boot extender...");
                _extender.Run();
            }

            _extender = null;
        }

        private void PrepareForPersistenceClear()
        {
            var bus = ServiceManager.GetInstance().GetService<IEventBus>();

            bus.Subscribe<PersistentStorageClearedEvent>(this, HandleClearedEvent);

            ServiceManager.GetInstance().GetService<IEventBus>().Publish(
                new PersistentStorageClearReadyEvent(_persistenceLevelToClear));
        }

        private void StopRunnables()
        {
            WritePendingActionToMessageDisplay("UnloadingHardwareRunnables");
            _runnablesManager.StopRunnables();
        }

        private void UnloadServices()
        {
            WritePendingActionToMessageDisplay("UnloadingHardwareServices");
            foreach (var service in _services.ToArray().Reverse())
            {
                ServiceManager.GetInstance().RemoveService(service);
            }

            _services.Clear();
        }

        private void UnLoadLayer()
        {
            Logger.Info("Unloading layer");

            StopRunnables();

            UnloadServices();

            Logger.Info("Layer unloaded");
        }
    }
}
