namespace Aristocrat.Monaco.Hardware
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Cabinet.Contracts;
    using Contracts;
    using Contracts.Audio;
    using Contracts.Battery;
    using Contracts.Bell;
    using Contracts.Button;
    using Contracts.ButtonDeck;
    using Contracts.Cabinet;
    using Contracts.Dfu;
    using Contracts.Discovery;
    using Contracts.Display;
    using Contracts.Door;
    using Contracts.EdgeLighting;
    using Contracts.HardMeter;
    using Contracts.IO;
    using Contracts.KeySwitch;
    using Contracts.NoteAcceptor;
    using Contracts.Persistence;
    using Contracts.SerialPorts;
    using Contracts.SharedDevice;
    using Contracts.Touch;
    using Contracts.TowerLight;
    using DFU;
    using EdgeLight.Contracts;
    using EdgeLight.Device;
    using EdgeLight.Manager;
    using EdgeLight.SequenceLib;
    using EdgeLight.Services;
    using EdgeLight.Strips;
    using Kernel;
    using Kernel.Contracts;
    using Kernel.Contracts.Components;
    using Kernel.Contracts.Events;
    using log4net;
    using Mono.Addins;
    using NativeDisk;
    using NativeOS.Services.IO;
    using NativeOS.Services.OS;
    using NativeTouch;
    using NativeUsb.DeviceWatcher;
    using NativeUsb.Hid;
    using Properties;
    using SerialTouch;
    using Services;
    using SimpleInjector;
    using StorageAdapters;
    using VHD;
    using IVirtualDisk = Contracts.VHD.IVirtualDisk;

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
        private const string ServicesPath = "/Hardware/Services";
        private const string RunnablesPath = "/Hardware/Runnables";
        private const string ExtenderPath = "/Hardware/BootExtender";
        private const string PropertyProvidersPath = "/Hardware/PropertyProviders";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly RunnablesManager _runnablesManager = new();
        private readonly List<IService> _services = new();

        private bool _clearingPersistentStorage;
        private AutoResetEvent _clearingPersistentStorageResetEvent = new(false);
        private IRunnable _extender;
        private PersistenceLevel _persistenceLevelToClear = PersistenceLevel.Static;
        private bool _waitingOnPersistentStorageClearedEvent;
        private Container _container;

        protected override void OnInitialize()
        {
            _container = new Container();
            ConfigureContainer(_container);
            _container.Verify();
            Logger.Info("Initialized");
        }

        protected override void OnRun()
        {
            Logger.Info("Run started");

            LoadStorageFromContainer();

            if (RunState == RunnableState.Running)
            {
                LoadPropertyProviders();

                LoadServices();

                LoadRunnables();

                ServiceManager.GetInstance().GetService<IEventBus>()
                    .Subscribe<PersistentStorageClearStartedEvent>(this, HandleClearStartedEvent);

                ServiceManager.GetInstance().GetService<IEventBus>()
                    .Subscribe<ServiceRemovedEvent>(
                        this,
                        a =>
                        {
                            if (_services.Any(service => service.GetType() == a.ServiceType))
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
                    _container?.Dispose();
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

        private static void LoadPropertyProviders()
        {
            var manager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            foreach (var node in MonoAddinsHelper.GetSelectedNodes<TypeExtensionNode>(PropertyProvidersPath))
            {
                manager.AddPropertyProvider((IPropertyProvider)node.CreateInstance());
            }
        }

        private void LoadStorageFromContainer()
        {
            Logger.Debug("Creating Persistent Storage");
            var serviceManager = ServiceManager.GetInstance();

            serviceManager.AddServiceAndInitialize(_container.GetInstance<ISecondaryStorageManager>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IPersistentStorageManager>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IPersistenceProvider>() as IService);

            serviceManager.GetService<IEventBus>().Publish(new PersistenceReadyEvent());
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
                ServiceManager.GetInstance().GetService<IEventBus>()
                    .Publish(new ExitRequestedEvent(ExitAction.Restart));
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
            var serviceManager = ServiceManager.GetInstance();

            serviceManager.AddServiceAndInitialize(_container.GetInstance<DeviceWatcher>());
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IDeviceRegistryService>());
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IHardwareConfiguration>());
            serviceManager.AddServiceAndInitialize(_container.GetInstance<ICabinetDetectionService>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<ISerialPortsService>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IIO>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IKeySwitch>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IDoorService>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IHardMeter>());
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IButtonService>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IButtonDeckDisplay>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IDisabledNotesService>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IBell>());
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IBeagleBoneController>());
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IOSService>());
            serviceManager.AddServiceAndInitialize(_container.GetInstance<InstrumentationService>());
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IDisplayService>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IAudio>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<ISharedMemoryManager>());
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IVirtualDisk>());
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IDfuProvider>());
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IPrinterFirmwareInstaller>());
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IOSInstaller>());
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IBattery>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<ITowerLight>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<ISerialDeviceSearcher>());
            serviceManager.AddServiceAndInitialize(_container.GetInstance<ISerialTouchService>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IEdgeLightDeviceFactory>());
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IEdgeLightingController>() as IService);

            // get services not yet portable to container
            foreach (var eventListenerNode in AddinManager
                         .GetExtensionNodes<StartupEventListenerImplementationExtensionNode>(
                             HardwareConstants.StartupEventListenerExtensionPoint))
            {
                var listenerBase = (StartupEventListenerBase)eventListenerNode.CreateInstance();
                listenerBase.EventBus = serviceManager.GetService<IEventBus>();
                InitializeService(listenerBase);
            }

            foreach (TypeExtensionNode serviceNode in AddinManager.GetExtensionNodes(ServicesPath))
            {
                InitializeService((IService)serviceNode.CreateInstance());
            }

            if (serviceManager.GetService<IIO>() is IDeviceService service)
            {
                service.Enable(EnabledReasons.Configuration);
            }
        }

        private void InitializeService(IService service)
        {
            ServiceManager.GetInstance().AddServiceAndInitialize(service);
            _services.Add(service);
        }

        private void LoadRunnables()
        {
            WritePendingActionToMessageDisplay("LoadingHardwareRunnables");
            _runnablesManager.StartRunnables(RunnablesPath);
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
            var serviceManager = ServiceManager.GetInstance();

            foreach (var service in _services.ToArray().Reverse())
            {
                serviceManager.RemoveService(service);
            }

            _services.Clear();

            serviceManager.RemoveService(_container.GetInstance<IEdgeLightingController>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IEdgeLightDeviceFactory>());
            serviceManager.RemoveService(_container.GetInstance<ISerialTouchService>() as IService);
            serviceManager.RemoveService(_container.GetInstance<ISerialDeviceSearcher>());
            serviceManager.RemoveService(_container.GetInstance<ITowerLight>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IBattery>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IOSInstaller>());
            serviceManager.RemoveService(_container.GetInstance<IPrinterFirmwareInstaller>());
            serviceManager.RemoveService(_container.GetInstance<IDfuProvider>());
            serviceManager.RemoveService(_container.GetInstance<IVirtualDisk>());
            serviceManager.RemoveService(_container.GetInstance<ISharedMemoryManager>());
            serviceManager.RemoveService(_container.GetInstance<IAudio>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IDisplayService>() as IService);
            serviceManager.RemoveService(_container.GetInstance<InstrumentationService>());
            serviceManager.RemoveService(_container.GetInstance<IOSService>());
            serviceManager.RemoveService(_container.GetInstance<IBeagleBoneController>());
            serviceManager.RemoveService(_container.GetInstance<IBell>());
            serviceManager.RemoveService(_container.GetInstance<IDisabledNotesService>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IButtonDeckDisplay>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IButtonService>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IHardMeter>());
            serviceManager.RemoveService(_container.GetInstance<IDoorService>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IKeySwitch>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IIO>() as IService);
            serviceManager.RemoveService(_container.GetInstance<ISerialPortsService>() as IService);
            serviceManager.RemoveService(_container.GetInstance<ICabinetDetectionService>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IHardwareConfiguration>());
            serviceManager.RemoveService(_container.GetInstance<IDeviceRegistryService>());
            serviceManager.RemoveService(_container.GetInstance<DeviceWatcher>());

            serviceManager.RemoveService(_container.GetInstance<IPersistenceProvider>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IPersistentStorageManager>() as IService);
            serviceManager.RemoveService(_container.GetInstance<ISecondaryStorageManager>() as IService);
        }

        private void UnLoadLayer()
        {
            Logger.Info("Unloading layer");

            StopRunnables();

            UnloadServices();

            Logger.Info("Layer unloaded");
        }

        private void ConfigureContainer(Container container)
        {
            AddExternalServices(container);
            AddInternalServices(container);
            container.AddScannableHardwareDeviceTypes();
        }

        private void AddExternalServices(Container container)
        {
            var serviceManager = ServiceManager.GetInstance();
            container.RegisterInstance<IServiceManager>(serviceManager);

            container.RegisterInstance(serviceManager.GetService<IComponentRegistry>());
            container.RegisterInstance(serviceManager.GetService<IPropertiesManager>());
            container.RegisterInstance(serviceManager.GetService<IEventBus>());
            container.RegisterInstance(serviceManager.GetService<IMessageDisplay>());
            container.RegisterInstance(serviceManager.GetService<ISystemDisableManager>());
            container.RegisterInstance(serviceManager.GetService<IPathMapper>());
            container.RegisterSingleton<IVirtualPartition>(
                () => VirtualPartitionProviderFactory.CreateVirtualPartitionProvider());
            container.RegisterSingleton<IIOProvider>(
                () => IOFactory.CreateIOProvider());
            container.RegisterSingleton<NativeDisk.IVirtualDisk>(
                () => VirtualDiskFactory.CreateVirtualDisk());
            container.RegisterSingleton<IDeviceWatcher>(
                () => DeviceWatcherFactory.CreateDeviceWatcher());
            container.RegisterSingleton<INativeTouch>(
                () => NativeTouchFactory.CreateNativeTouch());
        }

        private void AddInternalServices(Container container)
        {
            container.RegisterSingleton<ISqliteStorageInformation, SqliteStorageInformation>();
            container.RegisterSingleton<ISecondaryStorageManager, SqlSecondaryStorageManager>();
            container.RegisterSingleton<IPersistentStorageManager, SqlPersistentStorageManager>();
            container.RegisterSingleton<IPersistenceProvider, PersistenceProviderFacade>();

            container.Register<DeviceWatcher>(Lifestyle.Singleton);
            container.RegisterSingleton<IDeviceRegistryService, DeviceRegistryService>();
            container.RegisterSingleton<IHardwareConfiguration, HardwareConfiguration>();
            container.RegisterSingleton<ICabinetManager>(
                () => Cabinet.Container.Instance.GetInstance<ICabinetManager>());
            container.RegisterSingleton<ICabinetDisplaySettings>(
                () => Cabinet.Container.Instance.GetInstance<ICabinetDisplaySettings>());
            container.RegisterSingleton<ICabinetDetectionService, CabinetDetectionService>();
            container.Register<SerialPortEnumerator>(Lifestyle.Singleton);
            container.RegisterSingleton<ISerialPortsService, SerialPortsService>();
            container.RegisterSingleton<IIO, IOService>();
            container.RegisterSingleton<IKeySwitch, KeySwitchService>();
            container.RegisterSingleton<IDoorService, DoorService>();
            container.RegisterSingleton<IHardMeter, HardMeterService>();
            container.RegisterSingleton<IButtonService, ButtonService>();
            container.RegisterSingleton<HardwarePropertyProvider>();
            container.RegisterSingleton<IButtonDeckDisplay, ButtonDeckDisplayService>();
            container.RegisterSingleton<IDisabledNotesService, DisabledNotesService>();
            container.RegisterSingleton<IBell, BellService>();
            container.RegisterSingleton<IBeagleBoneController, BeagleBoneControllerService>();
            container.RegisterSingleton<IOSService, OSService>();
            container.Register<InstrumentationService>(Lifestyle.Singleton);
            container.RegisterSingleton<IDisplayService, DisplayService>();
            container.RegisterSingleton<IAudio, AudioService>();
            container.RegisterSingleton<ISharedMemoryInformation, SharedMemoryInformation>();
            container.RegisterSingleton<ISharedMemoryManager, SharedMemoryManager>();
            container.RegisterSingleton<IVirtualDisk, VirtualDisk>();
            container.RegisterSingleton<IDfuFactory, DfuFactory>();
            container.RegisterSingleton<IDfuProvider, DfuProvider>();
            container.RegisterSingleton<IPrinterFirmwareInstaller, PrinterFirmwareInstaller>();
            container.RegisterSingleton<IOSInstaller, OSInstaller>();
            container.RegisterSingleton<IBattery, BatteryService>();
            container.RegisterSingleton<ITowerLight, TowerLightService>();
            container.RegisterSingleton<ISerialDeviceSearcher, SerialDeviceSearcher>();
            container.RegisterSingleton<ISerialPortController, SerialPortController>();
            container.RegisterSingleton<ISerialTouchService, SerialTouchService>();

            var reelRegistration = Lifestyle.Singleton.CreateRegistration<ReelLightDevice>(container);
            var beagleBoneRegistration = Lifestyle.Singleton.CreateRegistration<BeagleBoneControllerService>(container);
            container.Collection.Register<IEdgeLightDevice>(new[] { reelRegistration, beagleBoneRegistration });

#if !(RETAIL)
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var simulatedEdgeLightCabinet = propertiesManager.GetValue(
                HardwareConstants.SimulateEdgeLighting,
                false);

            if (!simulatedEdgeLightCabinet)
            {
                container.RegisterSingleton<IEdgeLightDeviceFactory, EdgeLightDeviceFactory>();
            }
            else
            {
                Logger.Debug("Registering simulated edge lighting services");
                container.RegisterSingleton<IEdgeLightDeviceFactory, SimEdgeLightDeviceFactory>();
            }
#else
            container.RegisterSingleton<IEdgeLightDeviceFactory, EdgeLightDeviceFactory>();
#endif

            var edgeLightRendererRegistration =
                Lifestyle.Singleton.CreateRegistration<EdgeLightDataRenderer>(container);
            container.Collection.Register<IEdgeLightRenderer>(new[] { edgeLightRendererRegistration });

            container.Collection.Register(
                HidDeviceFactory.Enumerate(EdgeLightConstants.VendorId, EdgeLightConstants.ProductId)
                    .ToArray<IHidDevice>());

            container.RegisterSingleton<ILogicalStripInformation, LogicalStripInformation>();
            container.Register<PriorityComparer>(Lifestyle.Singleton);
            container.Register<StripDataRenderer>(Lifestyle.Singleton);
            container.RegisterSingleton<ILogicalStripFactory, LogicalStripFactory>();
            container.RegisterSingleton<IEdgeLightDevice, DeviceMultiplexer>();
            container.RegisterSingleton<IEdgeLightRendererFactory, RendererFactory>();
            container.Register<EdgeLightData>(Lifestyle.Singleton);
            container.RegisterSingleton<IEdgeLightManager, EdgeLightManager>();
            container.RegisterSingleton<IEdgeLightingController, EdgeLightingControllerService>();
        }
    }
}