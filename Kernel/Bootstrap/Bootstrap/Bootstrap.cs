namespace Aristocrat.Monaco.Bootstrap
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading.Tasks;
    using CrashHandlerDotNet;
    using Kernel;
    using Kernel.Contracts;
    using Kernel.Debugging;
    using log4net;
    using log4net.Config;
    using Mono.Addins;
    using NativeOS.Services.OS;

    public sealed class Bootstrap : MarshalByRefObject
    {
        // The paths for addin extensions
        private const string EventBusExtensionPath = "/Kernel/EventBus";
        private const string PropertiesManagerExtensionPath = "/Kernel/PropertiesManager";
        private const string LocalizerExtensionPath = "/Kernel/Localizer";
        private const string SystemDisableManagerExtensionPath = "/Kernel/SystemDisableManager";
        private const string ServicesExtensionPath = "/Kernel/Services";
        private const string RunnablesExtensionPath = "/Kernel/Runnables";
        private const string ExtenderExtensionPath = "/Kernel/BootExtender";
        private const string PathMapperExtensionPath = "/Kernel/PathMapper";
        private const string WindowLauncherExtensionPath = "/Kernel/WindowLauncher";
        private const string MessageDisplayExtensionPath = "/Kernel/MessageDisplay";
        private const string PlatformDisplayExtensionPath = "/Kernel/PlatformDisplay";
        private const string AddinHelperExtensionPath = "/Kernel/AddinHelper";
        private const string ComponentRegistryExtensionPath = "/Kernel/ComponentRegistry";
        private const string InitializationProviderExtensionPath = "/Kernel/InitializationProvider";
        private const string ProcessNodePath = "/Kernel/Processes";
        private const string GenerateCrashDumpArg = "GenerateCrashDump";

        private const string HardBootTime = "System.HardBoot.Time";
        private const string SoftBootTime = "System.SoftBoot.Time";

        // The int representing a verbose logging level for MonoLogger
        private const int VerboseMonoLogLevel = 2;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly bool CrashDumpRegistered =
            Environment.GetCommandLineArgs().Any(x => x.Equals(GenerateCrashDumpArg)) ||
            Environment.GetEnvironmentVariable(GenerateCrashDumpArg) != null;

        private readonly List<IService> _optionalServices = new List<IService>();
        private readonly Dictionary<string, object> _pendingProperties = new Dictionary<string, object>();
        private readonly Stack<IService> _requiredServices = new Stack<IService>();
        private readonly RunnablesManager _runnablesManager = new RunnablesManager();

        private IRunnable _extender;
        private IService _assemblyResolver;

        private DateTime _hardBootTime;
        private DateTime _softBootTime;
        private ExitAction _exitAction;

        private Bootstrap()
        {
        }

        public static int Main(string[] args)
        {
            return CrashDumpRegistered ? InternalStart(args) : StartWithExceptionHandling(args);
        }

        public void SetWorkingDirectory()
        {
            var executableFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(executableFolder))
            {
                Directory.SetCurrentDirectory(executableFolder);
            }
        }

        public int Start(string[] args)
        {
            SetWorkingDirectory();

            ConfigureLogging();

            SetUnhandledExceptionHandler();
            SetUnobservedTaskExceptionHandler();
            BootstrapProvider.SetErrorMode(
                BootstrapProvider.ErrorModes.SemFailCriticalErrors | BootstrapProvider.ErrorModes.SemNoGpFaultErrorBox);
            if (!CrashDumpRegistered)
            {
                CrashHandler.SetCrashHandlerLogger(Logger.Fatal);
                CrashHandler.RegisterCrashHandler("Monaco", @"..\logs");
            }

            Logger.Debug($"Command line arguments: {string.Join(" ", args)}");

            InitializeAddinManager();

            TerminateProcesses();

            if (ParseCommandLineArguments(args))
            {
                return (int)AppExitCode.Ok;
            }

            BootstrapProvider.DisableProcessWindowsGhosting();

            foreach (var item in _pendingProperties)
            {
                Logger.Debug($"_pendingProperties key: {item.Key} , _pendingProperties value: {item.Value}");
            }

            if (_pendingProperties.ContainsKey("powerUp"))
            {
                if (!DateTime.TryParse(_pendingProperties["powerUp"].ToString(), out _hardBootTime))
                {
                    _hardBootTime = DateTime.UtcNow;
                }

                _hardBootTime = _hardBootTime.ToUniversalTime();
                _softBootTime = DateTime.UtcNow;
            }
            else
            {
                _hardBootTime = DateTime.UtcNow;
                _softBootTime = _hardBootTime;
            }

            Logger.Debug($"_softBootTime: {_softBootTime} kind:{_softBootTime.Kind}");
            Logger.Debug($"_hardBootTime: {_hardBootTime} kind:{_softBootTime.Kind}");

            LoadKernel();
            RunBootExtender();
            UnloadKernel(false);

            Logger.Info($"Shutting down ({_exitAction})...");

            ShutdownAddinManager();

            Logger.Info($"Application version {GetVersion()} exiting...");

            if (!CrashDumpRegistered)
            {
                CrashHandler.UnRegisterCrashHandler();
            }

            LogManager.Shutdown();

            switch (_exitAction)
            {
                case ExitAction.ShutDown:
                    return (int)AppExitCode.Shutdown;
                case ExitAction.Reboot:
                    return (int)AppExitCode.Reboot;
            }

            return (int)AppExitCode.Ok;
        }

        private static void ConfigureLogging()
        {
            var loggingConfig = File.Exists(@"..\logs\logging.config")
                ? new FileInfo(@"..\logs\logging.config")
                : new FileInfo(@"logging.config");

            GlobalContext.Properties["AssemblyInfo.Version"] = GetVersion();
            GlobalContext.Properties["Runtime.Version"] = RuntimeInformation.FrameworkDescription;

            XmlConfigurator.Configure(loggingConfig);
        }

        private static int InternalStart(string[] args)
        {
            var bootstrap = new Bootstrap();

            return bootstrap.Start(args);
        }

        private static int StartWithExceptionHandling(string[] args)
        {
            try
            {
                return InternalStart(args);
            }
            catch (Exception ex)
            {
                Crash(ex);
                return (int)AppExitCode.Error;
            }
        }

        private static Version GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        private static void InitializeAddinManager()
        {
            Logger.Info("Initializing Addin Manager...");

            var currentDirectory = Directory.GetCurrentDirectory();

            // The add-in cache can't live in the platform folder since it will be launched from a read-only, mounted folder.
            //  Placing it in the local data folder will result in it being removed at boot because of the secure boot chain.
            //  The Delete below is simply for developer convenience since clearing it would require finding it...
            var cachePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Aristocrat Platform");
            if (Directory.Exists(cachePath))
            {
                Directory.Delete(cachePath, true);
            }

            // Previous Jurisdiction package mount, if present, is inaccessible now, so remove it
            var jurPath = $"{currentDirectory}/../jurisdiction";
            if (Directory.Exists(jurPath))
            {
                Directory.Delete(jurPath, true);
            }

            AddinManager.Initialize(currentDirectory, currentDirectory, cachePath);
            AddinManager.Registry.Update(new MonoLogger(VerboseMonoLogLevel));
            AddinManager.AddinLoadError += AddinManager_AddinLoadError;

            Logger.Info("Done.");
        }

        private static void AddinManager_AddinLoadError(object sender, AddinErrorEventArgs args)
        {
            Logger.Error($"{args.Message} for {args.AddinId}", args.Exception);
        }

        private static void TerminateProcesses()
        {
            var processNodes = AddinManager.GetExtensionNodes<ProcessNode>(ProcessNodePath);
            var processes = Process.GetProcesses();

            foreach (var processNode in processNodes)
            {
                var running = processes.Where(p => p.ProcessName == processNode.Name);
                foreach (var process in running)
                {
                    if (!process.HasExited)
                    {
                        Logger.Info($"Terminating running process: {process.ProcessName}({process.Id})");

                        process.Kill();
                    }
                }
            }
        }

        private static void ShutdownAddinManager()
        {
            Logger.Info("Shutting down the addin manager...");
            AddinManager.Shutdown();
            Logger.Info("Done.");
        }

        private static void OutputStatus(string message)
        {
            Logger.Info(message);
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Crash(args.ExceptionObject);
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        private static void Crash(object exception)
        {
            Logger.Fatal("UNHANDLED EXCEPTION CAUGHT");
            Logger.Fatal(exception);

            Console.WriteLine(exception);
            if (!CrashDumpRegistered)
            {
                Environment.Exit((int)AppExitCode.Error);
            }
        }

        private static void UnobservedTaskExceptionHandler(object sender, UnobservedTaskExceptionEventArgs args)
        {
            foreach (var exception in args.Exception.Flatten().InnerExceptions)
            {
                if (exception is HttpListenerException ex && ex.NativeErrorCode == 1229)
                {
                    args.SetObserved();
                    Logger.ErrorFormat("UNOBSERVED TASK EXCEPTION CAUGHT\n{0}", args.Exception);

                    Console.WriteLine(args.Exception);

                    break;
                }
            }
        }

        private void LoadRequiredService(string name, string serviceExtensionPath)
        {
            OutputStatus("Creating " + name);

            var typeExtensionNode = MonoAddinsHelper.GetSingleTypeExtensionNode(serviceExtensionPath);
            var service = (IService)typeExtensionNode.CreateInstance();
            ServiceManager.GetInstance().AddServiceAndInitialize(service);
            _requiredServices.Push(service);

            Logger.Info("Done.");
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        private void SetUnhandledExceptionHandler()
        {
            Logger.Info("Setting Unhandled Exception Handler...");
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += UnhandledExceptionHandler;
            Logger.Info("Done.");
        }

        private void SetUnobservedTaskExceptionHandler()
        {
            Logger.Info("Setting Unobserved Task Exception Handler...");
            TaskScheduler.UnobservedTaskException += UnobservedTaskExceptionHandler;
            Logger.Info("Done.");
        }

        private bool ParseCommandLineArguments(string[] args)
        {
            // if there weren't any arguments just return
            if (args.Length == 0)
            {
                Logger.Debug("No arguments found");
                return false;
            }

            // check to see if we want a list of all the available command line arguments
            if (args[0].Equals("/?"))
            {
                CommandLineHelp.DiscoverAndDisplayCommandLineArguments();
                return true;
            }

            foreach (var arg in args)
            {
                var tokens = arg.Split(new[] { '=' }, 2);

                if (tokens.Length == 2 && !string.IsNullOrEmpty(tokens[1]))
                {
                    _pendingProperties.Add(tokens[0], tokens[1]);
                }
                else if (tokens.Length == 1)
                {
                    // treat single token args as boolean properties with default value of true
                    _pendingProperties.Add(tokens[0], true);
                }
            }

            return false;
        }

        private void LoadKernel()
        {
            Logger.Info("Creating Service Manager...");
            var serviceManager = ServiceManager.GetInstance();
            Logger.Info("Done.");

            LoadRequiredService("Addin Helper", AddinHelperExtensionPath);
            LoadRequiredService("Event Bus", EventBusExtensionPath);
            LoadRequiredService("Properties Manager", PropertiesManagerExtensionPath);

            // Set initial properties
            var propertiesManager = serviceManager.GetService<IPropertiesManager>();
            propertiesManager.SetProperty(KernelConstants.SystemVersion, GetVersion().ToString());
            propertiesManager.SetProperty(HardBootTime, _hardBootTime);
            propertiesManager.SetProperty(SoftBootTime, _softBootTime);
            foreach (var item in _pendingProperties)
            {
                propertiesManager.SetProperty(item.Key, item.Value);
            }

            var typeExtensionNode =
                MonoAddinsHelper.GetSingleSelectedExtensionNode<TypeExtensionNode>("/Kernel/AssemblyResolver");
            _assemblyResolver = (IService)typeExtensionNode.CreateInstance();
            _assemblyResolver.Initialize();

            LoadRequiredService("Localizer", LocalizerExtensionPath);
            LoadRequiredService("Message Display", MessageDisplayExtensionPath);
            LoadRequiredService("Path Mapper", PathMapperExtensionPath);
            LoadRequiredService("Window Launcher", WindowLauncherExtensionPath);
            LoadRequiredService("Platform Display", PlatformDisplayExtensionPath);
            LoadRequiredService("System Disable Manager", SystemDisableManagerExtensionPath);
            LoadRequiredService("Component Registry", ComponentRegistryExtensionPath);
            LoadRequiredService("Initialization Provider", InitializationProviderExtensionPath);

            var eventBus = serviceManager.GetService<IEventBus>();
            eventBus.Subscribe<ExitRequestedEvent>(this, HandleEvent);

            OutputStatus("Loading Kernel Services");
            foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes(ServicesExtensionPath))
            {
                var service = (IService)node.CreateInstance();
                serviceManager.AddServiceAndInitialize(service);
                _optionalServices.Add(service);
            }

#if DEBUG
            // Register the DebuggerService when using the Debug build configuration.
            // This allows for auto-attaching the debugger at key points in the app.
            // See the IDebuggerService's xml docs for more info.
            ServiceManager.GetInstance().AddServiceAndInitialize(new DebuggerService());
#endif

            Logger.Info("Done.");

            OutputStatus("Loading Kernel Runnables");
            _runnablesManager.StartRunnables(RunnablesExtensionPath);
            Logger.Info("Done.");
        }

        private void UnloadKernel(bool softReboot)
        {
            Logger.Info("Unloading Kernel...");
            var serviceManager = ServiceManager.GetInstance();
            serviceManager.GetService<IEventBus>().UnsubscribeAll(this);

            OutputStatus("Unloading Kernel Runnables");
            _runnablesManager.StopRunnables();
            Logger.Info("Done.");

            OutputStatus("Unloading Kernel Services");
            foreach (var service in _optionalServices)
            {
                serviceManager.RemoveService(service);
            }

            Logger.Info("Done.");

            _optionalServices.Clear();

            (_assemblyResolver as IDisposable)?.Dispose();

            serviceManager.TryGetService<IPlatformDisplay>()?.Shutdown(!softReboot);
            foreach (var service in _requiredServices)
            {
                OutputStatus("Unloading " + service.Name);
                serviceManager.RemoveService(service);
                Logger.Info("Done.");
            }

            _requiredServices.Clear();

            Logger.Info("Shutting down the service manager...");
            serviceManager.Shutdown();

            Logger.Info("Kernel unloaded");
        }

        private void HandleEvent(ExitRequestedEvent @event)
        {
            Logger.Debug($"Exit requested with action: {@event.ExitAction}");

            _exitAction = @event.ExitAction;

            _extender?.Stop();
        }

        private void RunBootExtender()
        {
            var typeExtensionNode =
                MonoAddinsHelper.GetSingleTypeExtensionNode(ExtenderExtensionPath);
            _extender = (IRunnable)typeExtensionNode.CreateInstance();
            _extender.Initialize();

            Logger.Debug("Running boot extender...");
            _extender.Run();
            Logger.Debug("Boot extender exited");

            _extender = null;
        }

        private enum AppExitCode
        {
            Reboot = -2,
            Shutdown = -1,
            Ok,
            Error = 1
        }
    }
}