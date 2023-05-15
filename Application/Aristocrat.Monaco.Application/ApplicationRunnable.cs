namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Aristocrat.Monaco.Hardware.Contracts.SharedDevice;
    using Authentication;
    using Common;
    using Common.Storage;
    using Contracts;
    using Contracts.Authentication;
    using Contracts.Localization;
    using Contracts.Protocol;
    using Contracts.TiltLogger;
    using Drm;
    using EKey;
    using ErrorMessage;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    //using Hardware.Contracts.VHD;
    using Kernel;
    using Kernel.Contracts;
    using Kernel.Contracts.Components;
    using Kernel.Contracts.Events;
    using log4net;
    using Monaco.Localization.Properties;
    using Mono.Addins;
    using SerialGat;

    /// <summary>
    ///     The BootExtender is responsible for loading components and extensions in the Application layer.
    /// </summary>
    public class ApplicationRunnable : BaseRunnable
    {
        private const string PropertyProvidersExtensionPath = "/Application/PropertyProviders";
        private const string TimeServiceExtensionPath = "/Application/Time";
        private const string DisableByOperatorManagerServiceExtensionPath = "/Application/DisableByOperatorManager";
        private const string ConfigurationWizardExtensionPath = "/Application/ConfigurationWizard";
        private const string InspectionWizardExtensionPath = "/Application/InspectionWizard";
        private const string ConfigurationSettingsManagerExtensionPath = "/Application/Configuration/SettingsManager";
        private const string MeterManagerExtensionPath = "/Application/MeterManager";
        private const string PersistenceClearArbiterPathExtensionPath = "/Application/PersistenceClearArbiter";
        private const string OperatorMenuLauncherExtensionPath = "/Application/OperatorMenuLauncher";
        private const string TicketCreatorsExtensionPath = "/Application/TicketCreators";
        private const string ServicesExtensionPath = "/Application/Services";
        private const string RunnablesExtensionPath = "/Application/Runnables";
        private const string JurisdictionRunnablesExtensionPath = "/Application/{0}/Runnables";
        private const string ExtenderExtensionPath = "/Application/BootExtender";
        private const string PreConfigurationExtensionPath = "/Application/PreConfiguration";
        private const string ConfigurationExtensionPath = "/Application/Configuration";
        private const string NetworkServiceExtensionPath = "/Application/Network";
        private const string KeyboardServiceExtensionPath = "/Application/Keyboard";
        private const string PersistenceCriticalClearedBlockName = "PersistenceCriticalCleared";
        private const string PersistenceCriticalClearExecutedField = "JustExecuted";
        private const string PowerResetMeterName = "PowerReset";
        private const string MultiProtocolConfigurationProviderExtensionPath = "/Application/Protocol/MultiProtocolConfigurationProvider";

        // The int representing a verbose logging level for MonoLogger
        private const int VerboseMonoLogLevel = 2;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private IPathMapper _pathMapper;
        //private IVirtualDisk _virtualDisk;
        //private VirtualDiskHandle _jurisdictionMountHandle;
        private string _jurisdictionLinkPath;

        private readonly RunnablesManager _runnablesManager = new RunnablesManager();
        private readonly List<IService> _services = new List<IService>();
        private readonly object _thisLock = new object();

        private IRunnable _configurationWizard;
        private IRunnable _inspectionWizard;
        private IService _disableByOperatorManager;
        private IRunnable _extender;
        private bool _firstBoot;
        private IService _meterManager;
        private IService _network;
        private IService _networkLog;
        private IService _operatorMenuLauncher;
        private IService _persistenceClearArbiter;
        private IService _time;
        private IService _authentication;
        private IService _serialGat;
        private IService _liveAuthentication;
        private IService _digitalRights;
        private IService _multiProtocolConfigurationProvider;
        private IService _configurationUtilitiesProvider;
        private IService _protocolCapabilityAttributeProvider;
        private IService _ekeyService;
        private IService _keyboardService;

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            // Subscribe to all events.
            SubscribeToEvents();

            SetupJurisdiction();

            Logger.Debug("Initialized");
        }

        /// <inheritdoc />
        /// <exception cref="RunnableException">
        ///     Thrown when Run() is called more than once on an instance of this class.
        /// </exception>
        protected override void OnRun()
        {
            Logger.Debug("Run started");

            // Check to see if the layer should stop loading based on a
            // request to clear persistent storage from the operator
            if (RunState == RunnableState.Running)
            {
                LoadPropertyProviders();

                // The config wizard is skipped in Linux.  So, set hard-coded configuration
                ForceConfiguration();

                LoadTimeService();

                LoadNetworkService();

                LoadMultiProtocolConfigurationProvider();

                ServiceManager.GetInstance().AddService(new ConfigurationUtilitiesProvider());
                ServiceManager.GetInstance().AddService(new ProtocolCapabilityAttributeProvider());
                ServiceManager.GetInstance().AddService(new HardMeterMappingConfigurationProvider());

                LoadConfigurationSettingsManager();

                LoadDigitalRights();

                LoadEkeyService();

                LoadKeyboardService();

                CheckInitialConfiguration();

                Logger.Debug("ApplicationRunnable configuration wizard complete");
            }

            if (RunState == RunnableState.Running)
            {
                LoadConfiguration();

                LoadErrorMessageMapping();

                SetDemonstrationMode();

                LoadMeterManager();

                LoadPersistenceClearArbiter();

                LoadOperatorMenuLauncher();

                LoadServices();

                RegisterLogAdapters();
            }

            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var isInspection = (bool)propertiesManager.GetProperty(KernelConstants.IsInspectionOnly, false);
            if (isInspection)
            {
                CheckInspection();
            }
            else if (RunState == RunnableState.Running)
            {
                LoadDisableByOperatorManager();

                LoadRunnables();

                LoadAuthenticationServices();

                RemoveUnusedStartupEventListener();

                RunBootExtender();
            }

            UnLoadLayer();

            UnsubscribeFromEvents();

            Logger.Debug("Done unloading, giving GUI time to close");

            // allow time for all the GUI stuff to finish
            Thread.Sleep(500);
            Logger.Debug("Stopped");
        }

        /// <inheritdoc />
        protected override void OnStop()
        {
            Logger.Debug("Stopping");

            lock (_thisLock)
            {
                if (_configurationWizard != null)
                {
                    Logger.Debug("Disposing config wizard in Stop");
                    _configurationWizard.Stop();
                    ((IDisposable)_configurationWizard).Dispose();
                    _configurationWizard = null;
                }

                if (_inspectionWizard != null)
                {
                    Logger.Debug("Disposing inspection wizard in Stop");
                    _inspectionWizard.Stop();
                    ((IDisposable)_inspectionWizard).Dispose();
                    _inspectionWizard = null;
                }
            }

            if (_extender != null)
            {
                // In scenarios, where services being used by protocol layer were unloaded first and protocol layer try to access
                // those services caused crash. So, we unload protocol layer before other services here.
                UnLoadProtocolLayer();

                _extender.Stop();
                _extender = null;
            }
        }

        private void SetupJurisdiction()
        {
            _pathMapper = ServiceManager.GetInstance().GetService<IPathMapper>();
            _jurisdictionLinkPath = _pathMapper.GetDirectory(ApplicationConstants.JurisdictionsPath).FullName;

            if (Directory.Exists(_jurisdictionLinkPath))
            {
                // It's already valid.
                if (Directory.GetDirectories(_jurisdictionLinkPath).Length > 0)
                {
                    return;
                }

                // It's just a placeholder created by PathMapper.
                Directory.Delete(_jurisdictionLinkPath);
            }

            var packagesFolder = _pathMapper.GetDirectory(ApplicationConstants.ManifestPath).FullName;
            var jurIsoPackages = Directory.GetFiles(packagesFolder, ApplicationConstants.JurisdictionPackagePrefix + "*.iso", SearchOption.TopDirectoryOnly).ToList();
            if (jurIsoPackages.Count > 0)
            {
                //jurIsoPackages.Sort(new VersionComparer());
                //jurIsoPackages.ForEach(p => Logger.Debug($"found package {p}"));
                //var newestJurIsoPackagePath = Path.GetFullPath(jurIsoPackages.Last());

                // mount the newest version.
                //Directory.CreateDirectory(_jurisdictionLinkPath);

                //_virtualDisk = ServiceManager.GetInstance().GetService<IVirtualDisk>();
                //_jurisdictionMountHandle = _virtualDisk.AttachImage(newestJurIsoPackagePath, _jurisdictionLinkPath);
                //if (_jurisdictionMountHandle.IsInvalid)
                //{
                    //Logger.Warn($"invalid handle; couldn't mount {newestJurIsoPackagePath} at {_jurisdictionLinkPath}");
                    //_jurisdictionMountHandle.Close();
                    //return;
                //}
                //Logger.Debug($"Mounted {newestJurIsoPackagePath} at {_jurisdictionLinkPath}");
            }
            else
            {
                // Default behavior: link to the as-built data folder
                var jurisdictionSourcePath = Path.Combine(Directory.GetCurrentDirectory(), @"jurisdiction");
                Logger.Debug($"No jurisdiction ISO packages found; link '{jurisdictionSourcePath}' at '{_jurisdictionLinkPath}'");

                // Linux/Windows support
                string fileName = "cmd.exe";
                string arguments = $"/c mklink /j \"{_jurisdictionLinkPath}\" \"{jurisdictionSourcePath}\"";
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    fileName = "/bin/bash";
                    arguments = $"-c \"ln -s '{jurisdictionSourcePath}' '{_jurisdictionLinkPath}'\"";
                }

                // Hard link using "mklink"
                Process.Start(new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    CreateNoWindow = false
                })?.WaitForExit();
                Logger.Debug($"Attached {jurisdictionSourcePath} to {_jurisdictionLinkPath}");
            }

            // Update the add-ins registry
            AddinManager.Registry.Update(new MonoLogger(int.MaxValue));
        }

        private class VersionComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                var xVersion = GetVersion(x);
                var yVersion = GetVersion(y);

                if (xVersion != null && yVersion != null)
                {
                    return xVersion.CompareTo(yVersion);
                }

                return string.Compare(x, y, StringComparison.InvariantCulture);

                Version GetVersion(string text)
                {
                    return !Version.TryParse(text, out var version) ? null : version;
                }
            }
        }

        private static void WritePendingActionToMessageDisplay(string resourceStringName)
        {
            var display = ServiceManager.GetInstance().GetService<IMessageDisplay>();

            var localizer = Localizer.For(CultureFor.Operator);

            var displayMessage = localizer.GetString(resourceStringName, _ => display.DisplayStatus(resourceStringName));

            if (!string.IsNullOrWhiteSpace(displayMessage))
            {
                display.DisplayStatus(displayMessage);
            }

            var logMessage = localizer.GetString(CultureInfo.InvariantCulture, resourceStringName, _ => Logger.Info(resourceStringName));

            if (!string.IsNullOrWhiteSpace(logMessage))
            {
                Logger.Info(logMessage);
            }
        }

        private static void LoadPropertyProviders()
        {
            WritePendingActionToMessageDisplay(ResourceKeys.CreatingPropertyProviders);

            var manager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            foreach (var node in MonoAddinsHelper.GetSelectedNodes<TypeExtensionNode>(PropertyProvidersExtensionPath))
            {
                manager.AddPropertyProvider((IPropertyProvider)node.CreateInstance());
            }
        }

        private static void LoadConfigurationSettingsManager()
        {
            WritePendingActionToMessageDisplay(ResourceKeys.LoadingConfigurationSettingsManager);
            var node = MonoAddinsHelper.GetSingleSelectedExtensionNode<TypeExtensionNode>(ConfigurationSettingsManagerExtensionPath);
            var settingsManager = (IService)node.CreateInstance();
            settingsManager.Initialize();
            ServiceManager.GetInstance().AddService(settingsManager);
        }

        private static void LoadPreConfiguration()
        {
            WritePendingActionToMessageDisplay(ResourceKeys.LoadingPreConfiguration);
            foreach (var node in MonoAddinsHelper.GetSelectedNodes<TypeExtensionNode>(PreConfigurationExtensionPath))
            {
                node.CreateInstance();
            }
        }

        private static void LoadConfiguration()
        {
            WritePendingActionToMessageDisplay(ResourceKeys.LoadingConfiguration);
            foreach (var node in MonoAddinsHelper.GetSelectedNodes<TypeExtensionNode>(ConfigurationExtensionPath))
            {
                node.CreateInstance();
            }
        }

        private void LoadTimeService()
        {
            WritePendingActionToMessageDisplay(ResourceKeys.CreatingTimeService);
            var node = MonoAddinsHelper.GetSingleSelectedExtensionNode<TypeExtensionNode>(TimeServiceExtensionPath);
            _time = (IService)node.CreateInstance();
            _time.Initialize();
            ServiceManager.GetInstance().AddService(_time);
        }

        private void LoadAuthenticationServices()
        {
            WritePendingActionToMessageDisplay(ResourceKeys.CreatingAuthenticationService);
            var compReg = ServiceManager.GetInstance().GetService<IComponentRegistry>();
            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            _authentication = new AuthenticationService(compReg, eventBus, new FileSystemProvider());
            ServiceManager.GetInstance().AddServiceAndInitialize(_authentication);

            WritePendingActionToMessageDisplay(ResourceKeys.CreatingSerialGatService);
            var disableMgr = ServiceManager.GetInstance().GetService<ISystemDisableManager>();
            var storageMgr = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
            _serialGat = new SerialGatService(disableMgr, storageMgr, (IAuthenticationService)_authentication, compReg);
            ServiceManager.GetInstance().AddServiceAndInitialize(_serialGat);

            WritePendingActionToMessageDisplay(ResourceKeys.CreatingLiveAuthenticationManager);
            _liveAuthentication = new LiveAuthenticationManager();
            ServiceManager.GetInstance().AddServiceAndInitialize(_liveAuthentication);
        }

        private void LoadNetworkService()
        {
            WritePendingActionToMessageDisplay(ResourceKeys.CreatingNetworkService);
            var node = MonoAddinsHelper.GetSingleSelectedExtensionNode<TypeExtensionNode>(NetworkServiceExtensionPath);
            _network = (IService)node.CreateInstance();
            _network.Initialize();
            ServiceManager.GetInstance().AddService(_network);
        }

        private void LoadDigitalRights()
        {
            _digitalRights = new DigitalRights();
            ServiceManager.GetInstance().AddServiceAndInitialize(_digitalRights);
        }

        private void LoadEkeyService()
        {
            _ekeyService = new EKeyService();
            ServiceManager.GetInstance().AddServiceAndInitialize(_ekeyService);
        }

        private void LoadKeyboardService()
        {
            //WritePendingActionToMessageDisplay(ResourceKeys.CreatingKeyboardService);
            //var node = MonoAddinsHelper.GetSingleSelectedExtensionNode<TypeExtensionNode>(KeyboardServiceExtensionPath);
            //_keyboardService = (IService)node.CreateInstance();
            //_keyboardService.Initialize();
            //ServiceManager.GetInstance().AddService(_keyboardService);
        }

        private void LoadDisableByOperatorManager()
        {
            WritePendingActionToMessageDisplay(ResourceKeys.CreatingDisableByOperatorManager);
            var node =
                MonoAddinsHelper.GetSingleSelectedExtensionNode<TypeExtensionNode>(
                    DisableByOperatorManagerServiceExtensionPath);
            _disableByOperatorManager = (IService)node.CreateInstance();
            _disableByOperatorManager.Initialize();
            ServiceManager.GetInstance().AddService(_disableByOperatorManager);
        }

        private void LoadMultiProtocolConfigurationProvider()
        {
            WritePendingActionToMessageDisplay(ResourceKeys.CreatingMultiProtocolConfigurationProvider);
            var node =
                MonoAddinsHelper.GetSingleSelectedExtensionNode<TypeExtensionNode>(
                    MultiProtocolConfigurationProviderExtensionPath);
            _multiProtocolConfigurationProvider = (IService)node.CreateInstance();
            _multiProtocolConfigurationProvider.Initialize();
            ServiceManager.GetInstance().AddService(_multiProtocolConfigurationProvider);
        }

        private void LoadMeterManager()
        {
            WritePendingActionToMessageDisplay(ResourceKeys.CreatingMeterManager);
            var node = MonoAddinsHelper.GetSingleSelectedExtensionNode<TypeExtensionNode>(MeterManagerExtensionPath);
            _meterManager = (IService)node.CreateInstance();
            _meterManager.Initialize();
            ServiceManager.GetInstance().AddService(_meterManager);
        }

        private void LoadPersistenceClearArbiter()
        {
            WritePendingActionToMessageDisplay(ResourceKeys.CreatingPersistenceClearArbiter);
            var node = MonoAddinsHelper.GetSingleSelectedExtensionNode<TypeExtensionNode>(
                PersistenceClearArbiterPathExtensionPath);
            _persistenceClearArbiter = (IService)node.CreateInstance();
            _persistenceClearArbiter.Initialize();
            ServiceManager.GetInstance().AddService(_persistenceClearArbiter);
        }

        private void LoadOperatorMenuLauncher()
        {
            //WritePendingActionToMessageDisplay(ResourceKeys.CreatingOperatorMenuLauncher);
            //var node =
            //    MonoAddinsHelper.GetSingleSelectedExtensionNode<TypeExtensionNode>(OperatorMenuLauncherExtensionPath);
            //_operatorMenuLauncher = (IService)node.CreateInstance();
            //_operatorMenuLauncher.Initialize();
            //ServiceManager.GetInstance().AddService(_operatorMenuLauncher);
        }

        private void LoadTicketCreators()
        {
            WritePendingActionToMessageDisplay(ResourceKeys.LoadingTicketCreators);
            foreach (var node in MonoAddinsHelper.GetSelectedNodes<TypeExtensionNode>(TicketCreatorsExtensionPath))
            {
                var service = (IService)node.CreateInstance();
                service.Initialize();
                ServiceManager.GetInstance().AddService(service);
                _services.Add(service);
            }
        }

        private void LoadServices()
        {
            WritePendingActionToMessageDisplay(ResourceKeys.LoadingApplicationServices);
            foreach (var node in MonoAddinsHelper.GetSelectedNodes<TypeExtensionNode>(ServicesExtensionPath))
            {
                var service = (IService)node.CreateInstance();
                service.Initialize();
                ServiceManager.GetInstance().AddService(service);
                _services.Add(service);
            }
        }

        private void LoadRunnables()
        {
            // Load common runnables
            WritePendingActionToMessageDisplay(ResourceKeys.LoadingApplicationRunnables);
            _runnablesManager.StartRunnables(RunnablesExtensionPath);

            // Load jurisdiction specific runnables
            var jurisdiction = (string)ServiceManager.GetInstance().GetService<IPropertiesManager>()
                .GetProperty("System.Jurisdiction", null);
            var path = string.Format(JurisdictionRunnablesExtensionPath, jurisdiction);
            _runnablesManager.StartRunnables(path);
        }

        private void CheckInitialConfiguration()
        {
            WritePendingActionToMessageDisplay(ResourceKeys.CheckingInitialConfiguration);
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            var complete = propertiesManager.GetValue(ApplicationConstants.IsInitialConfigurationComplete, false);

            if (complete && ImportIncomplete(propertiesManager))
            {
                complete = false;
            }
            else if (!complete)
            {
                Logger.Debug("Initial configuration is incomplete");
            }

            if (!complete)
            {
                _firstBoot = true;

                if (RunState == RunnableState.Running)
                {
                    //var node =
                    //    MonoAddinsHelper.GetSingleSelectedExtensionNode<TypeExtensionNode>(
                    //        ConfigurationWizardExtensionPath);
                    //_configurationWizard = (IRunnable)node.CreateInstance();
                    //_configurationWizard.Initialize();
                    //WritePendingActionToMessageDisplay(ResourceKeys.RunningConfigurationWizard);
                    //_configurationWizard.Run();
                    //lock (_thisLock)
                    //{
                    //    // Stop could have set the wizard to null on another thread
                    //    if (_configurationWizard != null)
                    //    {
                    //        ((IDisposable)_configurationWizard).Dispose();
                    //        _configurationWizard = null;
                    //    }
                    //}
                }
            }
            else
            {
                Logger.Debug("Initial configuration is complete");
                _firstBoot = false;
                LoadPreConfiguration();
                LoadTicketCreators();
            }
        }

        private void CheckInspection()
        {
            WritePendingActionToMessageDisplay(ResourceKeys.CheckingInspection);

            if (RunState == RunnableState.Running)
            {
                var node =
                    MonoAddinsHelper.GetSingleSelectedExtensionNode<TypeExtensionNode>(
                        InspectionWizardExtensionPath);
                _inspectionWizard = (IRunnable)node.CreateInstance();
                _inspectionWizard.Initialize();
                WritePendingActionToMessageDisplay(ResourceKeys.RunningInspectionWizard);
                _inspectionWizard.Run();
                lock (_thisLock)
                {
                    // Stop could have set the wizard to null on another thread
                    if (_inspectionWizard != null)
                    {
                        ((IDisposable)_inspectionWizard).Dispose();
                        _inspectionWizard = null;
                    }
                }
            }
        }

        private bool ImportIncomplete(IPropertiesManager propertiesManager)
        {
            var machineSettingsImported = propertiesManager.GetValue(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None);

            var importIncomplete =  machineSettingsImported != ImportMachineSettings.None &&
                   (!machineSettingsImported.HasFlag(ImportMachineSettings.Imported) ||
                    !machineSettingsImported.HasFlag(ImportMachineSettings.ConfigWizardConfigurationPropertiesLoaded) ||
                    !machineSettingsImported.HasFlag(ImportMachineSettings.AccountingPropertiesLoaded) ||
                    !machineSettingsImported.HasFlag(ImportMachineSettings.HandpayPropertiesLoaded) ||
                    !machineSettingsImported.HasFlag(ImportMachineSettings.ApplicationConfigurationPropertiesLoaded) ||
                    !machineSettingsImported.HasFlag(ImportMachineSettings.CabinetFeaturesPropertiesLoaded) ||
                    !machineSettingsImported.HasFlag(ImportMachineSettings.GamingPropertiesLoaded)
                   );

            if (importIncomplete)
            {
                Logger.Debug($"Initial configuration is incomplete, EGM was rebooted with imported machine settings {machineSettingsImported}");
            }

            return importIncomplete;
        }

        private void RemoveUnusedStartupEventListener()
        {
            var serviceMan = ServiceManager.GetInstance();
            var selectedProtocols = serviceMan.GetService<IMultiProtocolConfigurationProvider>().MultiProtocolConfiguration.Select(x => x.Protocol);
            List<string> protocols = new List<string>(
                MonoAddinsHelper.GetSelectableConfigurationAddins(ApplicationConstants.Protocol));
            MonoAddinsHelper.GetSelectableConfigurationAddins(ApplicationConstants.Protocol);
            // removes the selected protocol from the list.
            protocols.RemoveAll(r => selectedProtocols.Select(s => EnumParser.ToName(s)).Contains(r)
            );

            var eventListenerNodes =
                AddinManager.GetExtensionNodes<StartupEventListenerImplementationExtensionNode>(
                    HardwareConstants.StartupEventListenerExtensionPoint);
            foreach (var node in eventListenerNodes)
            {
                // if the node's protocol name appears in the list, drop its associated service.
                if (protocols.Contains(node.ProtocolName))
                {
                    serviceMan.RemoveService(node.Type);
                }
            }
        }

        private void RunBootExtender()
        {
            var storageManager = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();

            var criticalBlock = storageManager.GetBlock(PersistenceCriticalClearedBlockName);
            var criticalMemoryCleared = (bool)criticalBlock[PersistenceCriticalClearExecutedField];

            // The platform is now considered to be booted
            ServiceManager.GetInstance()
                .GetService<IEventBus>()
                .Publish(new PlatformBootedEvent(DateTime.UtcNow, criticalMemoryCleared));

            // Create a soft error message for Power Reset
            var display = ServiceManager.GetInstance().GetService<IMessageDisplay>();
            var powerResetMessage = new DisplayableMessage(
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PowerResetText),
                DisplayableMessageClassification.Informative,
                DisplayableMessagePriority.Immediate,
                typeof(PlatformBootedEvent),
                ApplicationConstants.PlatformBootedKey);
            display.DisplayMessage(powerResetMessage);

            // The Power Reset meter cannot be incremented immediately after the persistence was partially cleared.
            if (!_firstBoot && !criticalMemoryCleared)
            {
                ServiceManager.GetInstance().GetService<IMeterManager>().GetMeter(PowerResetMeterName).Increment(1);
            }

            // Unflag it before running into the next layer so that the next time the Power Reset meter is updated.
            criticalBlock[PersistenceCriticalClearExecutedField] = false;

            var node = MonoAddinsHelper.GetSingleSelectedExtensionNode<TypeExtensionNode>(ExtenderExtensionPath);
            _extender = (IRunnable)node.CreateInstance();
            _extender.Initialize();
            if (RunState == RunnableState.Running)
            {
                Logger.Debug("Running boot extender...");
                _extender.Run();
            }

            _extender = null;
        }

        private void UnLoadProtocolLayer()
        {
            WritePendingActionToMessageDisplay(ResourceKeys.UnloadingApplicationRunnables);
            _runnablesManager.StopRunnables();
        }

        private void UnLoadLayer()
        {
            var serviceManager = ServiceManager.GetInstance();

            UnRegisterLogAdapters();

            WritePendingActionToMessageDisplay(ResourceKeys.UnloadingApplicationServices);
            foreach (var service in _services)
            {
                serviceManager.RemoveService(service);
            }

            _services.Clear();

            if (_operatorMenuLauncher != null)
            {
                WritePendingActionToMessageDisplay(ResourceKeys.UnloadingOperatorMenuLauncher);
                serviceManager.RemoveService(_operatorMenuLauncher);
                _operatorMenuLauncher = null;
            }

            if (_persistenceClearArbiter != null)
            {
                WritePendingActionToMessageDisplay(ResourceKeys.UnloadingPersistenceClearArbiter);
                serviceManager.RemoveService(_persistenceClearArbiter);
                _persistenceClearArbiter = null;
            }

            if (_meterManager != null)
            {
                WritePendingActionToMessageDisplay(ResourceKeys.UnloadingMeterManager);
                serviceManager.RemoveService(_meterManager);
                _meterManager = null;
            }

            if (_networkLog != null)
            {
                WritePendingActionToMessageDisplay(ResourceKeys.UnloadingNetworkLog);
                serviceManager.RemoveService(_networkLog);
                _networkLog = null;
            }

            if (_disableByOperatorManager != null)
            {
                WritePendingActionToMessageDisplay(ResourceKeys.UnloadingDisableByOperatorManager);
                serviceManager.RemoveService(_disableByOperatorManager);
                _disableByOperatorManager = null;
            }

            if (_time != null)
            {
                WritePendingActionToMessageDisplay(ResourceKeys.UnloadingTimeService);
                serviceManager.RemoveService(_time);
                _time = null;
            }

            if (_authentication != null)
            {
                WritePendingActionToMessageDisplay(ResourceKeys.UnloadingGATService);
                serviceManager.RemoveService(_authentication);
                _authentication = null;
            }

            if (_serialGat != null)
            {
                serviceManager.RemoveService(_serialGat);
                _serialGat = null;
            }

            if (_digitalRights != null)
            {
                serviceManager.RemoveService(_digitalRights);
                _digitalRights = null;
            }

            if(_ekeyService != null)
            {
                serviceManager.RemoveService(_ekeyService);
                _ekeyService = null;
            }

            if (_liveAuthentication != null)
            {
                WritePendingActionToMessageDisplay(ResourceKeys.UnloadingLiveAuthenticationManager);
                serviceManager.RemoveService(_liveAuthentication);
                _liveAuthentication = null;
            }

            if (_network != null)
            {
                serviceManager.RemoveService(_network);
                _network = null;
            }

            if (_protocolCapabilityAttributeProvider != null)
            {
                serviceManager.RemoveService(_protocolCapabilityAttributeProvider);
                _protocolCapabilityAttributeProvider = null;
            }

            if (_configurationUtilitiesProvider != null)
            {
                serviceManager.RemoveService(_configurationUtilitiesProvider);
                _configurationUtilitiesProvider = null;
            }

            if (_multiProtocolConfigurationProvider != null)
            {
                WritePendingActionToMessageDisplay(ResourceKeys.UnloadingMultiProtocolConfigurationProvider);
                serviceManager.RemoveService(_multiProtocolConfigurationProvider);
                _multiProtocolConfigurationProvider = null;
            }

            if(_keyboardService != null)
            {
                serviceManager.RemoveService(_keyboardService);
                _keyboardService = null;
            }
        }

        private void SubscribeToEvents()
        {
            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            eventBus.Subscribe<AddinConfigurationCompleteEvent>(this, Handle);
        }

        private void UnsubscribeFromEvents()
        {
            ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);
        }

        private void Handle(AddinConfigurationCompleteEvent theEvent)
        {
            lock (_thisLock)
            {
                if (RunState == RunnableState.Running)
                {
                    var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
                    eventBus.Unsubscribe<AddinConfigurationCompleteEvent>(this);
                    LoadPreConfiguration();
                    LoadTicketCreators();
                    eventBus.Publish(new PreConfigBootCompleteEvent());
                }
            }
        }

        private void SetDemonstrationMode()
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            var isDemonstrationModeEnabled = propertiesManager.GetValue(
                ApplicationConstants.DemonstrationModeEnabled,
                false); // From jurisdiction config

            if (isDemonstrationModeEnabled)
            {
                return;
            }

            propertiesManager.SetProperty(ApplicationConstants.DemonstrationMode, false);
        }

        private void LoadErrorMessageMapping()
        {
            var errorMessageMapping = new ErrorMessageMapping();
            if (errorMessageMapping.Initialize())
            {
                var messageDisplay = ServiceManager.GetInstance().GetService<IMessageDisplay>();
                messageDisplay.AddErrorMessageMapping(errorMessageMapping);
            }
        }

        private void RegisterLogAdapters()
        {
            var logAdapterService = ServiceManager.GetInstance().GetService<ILogAdaptersService>();
            logAdapterService.RegisterLogAdapter(new AlteredMediaEventLogAdapter());
        }

        private void UnRegisterLogAdapters()
        {
            var logAdapterService = ServiceManager.GetInstance().TryGetService<ILogAdaptersService>();
            logAdapterService?.UnRegisterLogAdapter(EventLogType.SoftwareChange.GetDescription(typeof(EventLogType)));
        }

        private void ForceConfiguration()
        {
            Console.WriteLine("Applying hard-coded config");

            var propsManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            const string JurisdictionString = "Nevada";

            // ConfiguredAddinsPropertiesProvider
            var selectedAddinsConfig = new Dictionary<string, string>();
            selectedAddinsConfig.Add("Jurisdiction", JurisdictionString);
            selectedAddinsConfig.Add("Protocol", "[\"Test\"]");
            propsManager.SetProperty(ApplicationConstants.SelectedConfigurationKey, selectedAddinsConfig);

            // LocalizationPropertiesProvider
            var localizationState = @"{""DefaultCulture"":""en-US"",""CurrentCulture"":""en-US"",""Providers"":{""Operator"":{""AvailableCultures"":[""en-US""],""DefaultCulture"":""en-US"",""ProviderName"":""Operator"",""CurrentCulture"":""en-US""},""OperatorTicket"":{""AvailableCultures"":[""en-US""],""ProviderName"":""OperatorTicket"",""CurrentCulture"":""en-US""},""Player"":{""AvailableCultures"":[""en-US""],""PrimaryCulture"":""en-US"",""ProviderName"":""Player"",""CurrentCulture"":""en-US""},""PlayerTicket"":{""AvailableCultures"":[""en-US""],""ProviderName"":""PlayerTicket"",""CurrentCulture"":""en-US""},""Currency"":{""AvailableCultures"":[""en-US""],""ProviderName"":""Currency"",""CurrentCulture"":""en-US""}}}";
            propsManager.SetProperty(ApplicationConstants.LocalizationState, localizationState);
            propsManager.SetProperty(ApplicationConstants.LocalizationCurrentCulture, "en-US");

            // HardwarePropertyProvider
            propsManager.SetProperty(HardwareConstants.HardMetersEnabledKey, false);

            // SystemPropertiesProvider
            propsManager.SetProperty(ApplicationConstants.IsInitialConfigurationComplete, true);
            propsManager.SetProperty(ApplicationConstants.NoteAcceptorEnabled, true);
            propsManager.SetProperty(ApplicationConstants.NoteAcceptorManufacturer, "Fake");
            propsManager.SetProperty(ApplicationConstants.PrinterEnabled, true);
            propsManager.SetProperty(ApplicationConstants.PrinterManufacturer, "Fake");
            propsManager.SetProperty(ApplicationConstants.SerialNumber, "1");
            propsManager.SetProperty(ApplicationConstants.MachineId, 1u);
            propsManager.SetProperty(ApplicationConstants.CurrencyId, "USD");
            propsManager.SetProperty(ApplicationConstants.CurrencyDescription, "US Dollar USD $1,000.00");
            propsManager.SetProperty(ApplicationConstants.Jurisdiction, JurisdictionString);
            propsManager.SetProperty(ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEnabled, true);
            propsManager.SetProperty(ApplicationConstants.HardMeterMapSelectionValue, "(1)Turnover,(2)Total Won,(4)Handpay,(5)Bills In,(7)Coins In,(8)Coins Out");
            propsManager.SetProperty(ApplicationConstants.LegalCopyrightAcceptedKey, true);

            // InitialSetupPropertiesProvider
            propsManager.SetProperty(ApplicationConstants.ConfigWizardLastPageViewedIndex, 5);
            propsManager.SetProperty(ApplicationConstants.ConfigWizardSelectionPagesDone, true);

            // Hardware config would normally be determined by the config wizard on first boot and
            // passed directly to the HardwareConfiguration service, which would persist it.
            // On subsequent boots, HardwareConfiguration service listens for a PropertyChangedEvent
            // for the "Mono.SelectedAddinConfigurationHashCode" property.  The event is posted when
            // the ConfiguredAddinsPropertiesProvider is loaded.  So, we need to set persistence and
            // re-post the event here.

            var storage = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();

            var level = PersistenceLevel.Static;
            var name = "Aristocrat.Monaco.Hardware.HardwareConfiguration";
            var size = Enum.GetValues(typeof(DeviceType)).Length;
            var accessor = storage.GetAccessor(level, name, size);

            using (var transaction = accessor.StartTransaction())
            {
                transaction[(int)DeviceType.NoteAcceptor, "DeviceType"] = DeviceType.NoteAcceptor;
                transaction[(int)DeviceType.NoteAcceptor, "Enabled"] = true;
                transaction[(int)DeviceType.NoteAcceptor, "Make"] = "Fake";
                transaction[(int)DeviceType.NoteAcceptor, "Protocol"] = "Fake";
                transaction[(int)DeviceType.NoteAcceptor, "Port"] = "Fake";

                transaction[(int)DeviceType.Printer, "DeviceType"] = DeviceType.Printer;
                transaction[(int)DeviceType.Printer, "Enabled"] = true;
                transaction[(int)DeviceType.Printer, "Make"] = "Fake";
                transaction[(int)DeviceType.Printer, "Protocol"] = "Fake";
                transaction[(int)DeviceType.Printer, "Port"] = "Fake";

                transaction[(int)DeviceType.IdReader, "DeviceType"] = DeviceType.IdReader;
                transaction[(int)DeviceType.IdReader, "Enabled"] = false;
                transaction[(int)DeviceType.IdReader, "Make"] = "UNIFORM USB GDS";
                transaction[(int)DeviceType.IdReader, "Protocol"] = "GDS";
                transaction[(int)DeviceType.IdReader, "Port"] = "USB";

                //transaction[(int)DeviceType.ReelController, "DeviceType"] = DeviceType.ReelController;
                //transaction[(int)DeviceType.ReelController, "Enabled"] = true;
                //transaction[(int)DeviceType.ReelController, "Make"] = "Fake";
                //transaction[(int)DeviceType.ReelController, "Protocol"] = "Fake";
                //transaction[(int)DeviceType.ReelController, "Port"] = "Fake";

                transaction.Commit();
            }

            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            eventBus.Publish(new PropertyChangedEvent() { PropertyName = "Mono.SelectedAddinConfigurationHashCode" });
        }
    }
}
