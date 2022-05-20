namespace Executor.VMs
{
    using Common.Models;
    using Common.Utils;
    using log4net;
    using log4net.Appender;
    using log4net.Core;
    using log4net.Layout;
    using log4net.Repository;
    using log4net.Repository.Hierarchy;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;
    using Utils;

    public class ExecutorVM : NotifyPropertyChanged
    {
        public bool ExecutionCompleted = false;
        public bool ExecutionSucceeded = false;
        public bool RestartOnCompletion = false;
        public readonly string ScriptDriveVariable = "[USBDRIVE]";
        public bool SkipLogicDoorCheck = false;
        public string RsaPublicKeyPathFullPath = "";

        private ObservableCollection<Command> _commands;
        private string _cornerMessage = "";
        private string _currentDetails = "";
        private string _currentStatus = "Executing... Do not remove the USB drive from the EGM. Please wait.";
        private string _fullLog = "";
        private bool _isDebug = false;
        private readonly string _loggingEnvVarName = "YOZZO"; // Closest thing to seeing my name in lights, is putting it in an environment variable in a slot machine. I've made the big time baby.
        private LogWatcher _logWatcha;
        private bool _pollingEnvVar = false;
        private bool _showGUI = false;
        private RsaService _rsaService;
        private string _statusTextColor = "White";
        private Stopwatch _sw;
        private readonly string _timeoutEnvVarName = "MonacoKeyExecutorScriptTimeout";
        private int _timeoutThreshold = 0;
        private string _title;
        private USBKey _usbKey = null;

        public ExecutorVM(string title)
        {
            Title = title;
            _logWatcha = new LogWatcher();
            _logWatcha.Updated += LogWatcha_Updated;

            #if DEBUG
            IsDebug = true; // This is what enable/disables the debug console & debug buttons on the UI.
            #endif

            // Setup Debug section of UI, hookup debug console log
            _sw = new Stopwatch();
            _sw.Start();
            DispatcherTimer dt = new DispatcherTimer();
            dt.Tick += Dt_Tick;
            dt.Interval = new TimeSpan(0, 0, 0, 0, 250);
            dt.Start();
        }

        public string CornerMessage
        {
            get
            {
                return _cornerMessage;
            }
            set
            {
                _cornerMessage = value;
                OnPropertyChanged(nameof(CornerMessage));
            }
        }
        public ObservableCollection<Command> Commands
        {
            get
            {
                return _commands;
            }
            set
            {
                _commands = value;
                OnPropertyChanged(nameof(Commands));
            }
        }
        public string CurrentDetails
        {
            get
            {
                return _currentDetails;
            }
            set
            {
                _currentDetails = value;
                OnPropertyChanged(nameof(CurrentDetails));
            }
        }
        public string CurrentStatus
        {
            get
            {
                return _currentStatus;
            }
            set
            {
                _currentStatus = value;
                OnPropertyChanged(nameof(CurrentStatus));
            }
        }
        public string FullLog
        {
            get
            {
                return _fullLog;
            }
            set
            {
                _fullLog = value;
                OnPropertyChanged(nameof(FullLog));
            }
        }
        public bool IsDebug
        {
            get
            {
                return _isDebug;
            }
            set
            {
                _isDebug = value;
                OnPropertyChanged(nameof(IsDebug));
            }
        }
        public string StatusTextColor
        {
            get
            {
                return _statusTextColor;
            }
            set
            {
                _statusTextColor = value;
                OnPropertyChanged(nameof(StatusTextColor));
            }
        }
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }
        public USBKey USBKey
        {
            get
            {
                return _usbKey;
            }
            set
            {
                _usbKey = value;
                OnPropertyChanged(nameof(USBKey));
            }
        }

        public void FlushLogBuffersAndShutdown()
        {
            ILoggerRepository rep = LogManager.GetRepository();
            foreach (IAppender appender in rep.GetAppenders())
                if (appender is BufferingAppenderSkeleton buffered)
                    buffered.Flush();

            LogManager.Shutdown();
        }
        public void HandleUSBRemoved()
        {
            if (ExecutionCompleted && RestartOnCompletion)
            {
                App.Log.Info("USB removed... execution is complete, and restart on completion is true. Rebooting...");
                WindowsUtilities.Reboot();
            }
            else
            {
                if (!ExecutionCompleted)
                {
                    App.Log.Info("USB removed... execution is not complete. Shutting down the Executor App now.");
                    Application.Current.Shutdown();
                }
                else
                {
                    App.Log.Info("USB removed... execution is complete, and restart on completion is false. Thus, not rebooting. Shutting down the Executor App now.");
                    Application.Current.Shutdown();
                }
            }
        }
        public void InitializeUSBLogFile(USBKey key)
        {
            if (key.LogFilePath == null || key.LogFilePath == "")
                return; // lord, you're screwed... I don't even know what the heck I could do at this point to resolve anything...

            File.WriteAllText(key.LogFilePath, FullLog);

            Thread.Sleep(3000); // I got some strange results from log4net without sleeps around this code
            PatternLayout patternLayout = new PatternLayout
            {
                ConversionPattern = "%date{HH:mm:ss.f} %level:      %message%newline"
            };
            patternLayout.ActivateOptions();

            RollingFileAppender fileAppender = new RollingFileAppender
            {
                AppendToFile = true,
                File = key.LogFilePath,
                Layout = patternLayout,
                MaxSizeRollBackups = 5,
                MaximumFileSize = "50MB",
                RollingStyle = RollingFileAppender.RollingMode.Size,
                StaticLogFileName = true,
                Name = "FileAppender" + key.DiskDeviceID
            };
            fileAppender.ActivateOptions();

            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Root.AddAppender(fileAppender);
            Thread.Sleep(3000);
        }
        public void LogDetailMessage(string message)
        {
            if (message == null)
                message = "";

            if (CurrentDetails != message)
            {
                CurrentDetails = message;
                App.Log.Info("User Facing Details: " + message);
            }
        }
        public void LogStatusMessage(string message, string color = "White")
        {
            StatusTextColor = color;

            if (message == null)
                message = "";

            if (CurrentStatus != message)
            {
                CurrentStatus = message;
                App.Log.Info("User Facing Status: " + message);
            }
        }
        public bool ShowGUI
        {
            get
            {
                return _showGUI;
            }
            set
            {
                _showGUI = value;
                OnPropertyChanged(nameof(ShowGUI));
            }
        }
        public void Start()
        {
            Task.Run(() => {
                // Catch Win32_VolumeChangeEvents 
                // https://ravichaganti.com/blog/monitoring-volume-change-events-in-powershell-using-wmi/
                ManagementEventWatcher watcher = new ManagementEventWatcher();
                WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 3"); // Volume Removed Events
                watcher.EventArrived += new EventArrivedEventHandler(VolumeRemovedEvent);
                watcher.Query = query;
                watcher.Start();

                // Kicks off the state machine, not on the UI thread
                ExecuteState(ExecutorState.Started);
            });
        }

        private void CheckCommands()
        {
            Commands = new ObservableCollection<Command>(Detector.ParseCommands());
            if (Commands == null || Commands.Count == 0)
            {
                App.Log.Error("Failed to detect any commands in the embedded resources. This should never happen. Exiting.");
                ExecuteState(ExecutorState.ExitImmediately);
                return;
            }
            else
            {
                ExecuteState(ExecutorState.PassedCommandCheck);
                return; 
            }
        }
        private void CheckLogicDoor()
        {
            if (!SkipLogicDoorCheck)
            {
                App.Log.Info("Checking logic door. It must be open to continue.");

                if (!LogicDoorUtil.DoorOpen())
                {
                    App.Log.Info("Logic door is closed.");
                    ExecuteState(ExecutorState.ExitImmediately);
                    return;
                }
                else
                {
                    App.Log.Info("Logic door is open.");
                }
            }
            else
            {
                App.Log.Info("Skipping logic door check.");
            }

            ExecuteState(ExecutorState.PassedLogicDoorCheck);
            return;
        }
        private void CheckRsaKeys()
        {
            string keyPairName = "arbitraryKeyName";
            _rsaService = new RsaService(App.Log, RsaPublicKeyPathFullPath, keyPairName);

            if (_rsaService.SelectedKeyPair == null)
            {
                App.Log.Error("Failed to find RSA keys.");
                ExecuteState(ExecutorState.ExitImmediately);
                return;
            }
            else
            {
                App.Log.Info("RSA keys loaded.");
                ExecuteState(ExecutorState.PassedRSAKeyCheck);
                return;
            }
        }
        private void CheckUsbs()
        {
            USBKey = null;
            List<USBKey> usbs = Detector.DetectAndValidateUsbs(Commands.ToList(), _rsaService);

            if(usbs.Count == 0)
            {
                App.Log.Debug("Did not detect any usbs. Exiting.");
                ExecuteState(ExecutorState.ExitImmediately);
                return;
            }

            bool productionKeyExists = false;
            foreach (USBKey usb in usbs)
                productionKeyExists |= usb.ProductionReady;

            if (productionKeyExists)
            {
                // use first detected production usb
                foreach (USBKey usb in usbs)
                {
                    if (usb.ProductionReady)
                    {
                        App.Log.Debug($"Found a validly signed and encrypted usb drive, mapped to drive: {usb.Partitions[0].DriveLetter}");
                        USBKey = usb;
                        InitializeUSBLogFile(USBKey);
                        ExecuteState(ExecutorState.PassedUSBCheck);
                        return;
                    }
                }
            }
            else
            {
                // use first key that was intended to be valid
                foreach (USBKey usb in usbs)
                {
                    if (usb.IntendedKey)
                    {
                        USBKey = usb;
                        InitializeUSBLogFile(USBKey);
                        LogDetailMessage(USBKey.ValidityFailureMessage);
                        ExecuteState(ExecutorState.ExecutionFailed);
                        return;
                    }
                }
            }

            // no valid keys, no intended valid keys, so bail
            ExecuteState(ExecutorState.ExitImmediately);
            return;
        }
        private void Dt_Tick(object sender, EventArgs e)
        {
            CornerMessage = "Runtime " + _sw.Elapsed.ToString(@"hh\:mm\:ss");
        }
        private void ExecuteCommandKey()
        {
            ShowGUI = true;

            Task.Run(() =>
            {
                App.Log.Debug("Execution thread started. Executing the command key...");
                bool scriptSucceeded = false;

                // setup dynamic environment var logging
                try
                {
                    StartEnvVarPolling();
                }
                catch(Exception e)
                {
                    App.Log.Error(e.Message);
                    App.Log.Error(e.StackTrace);
                    App.Log.Error("Failed to execute command. Exception thrown during setup.");
                    LogDetailMessage("Failed to execute. Check results.txt file on USB for more info.");
                    ExecuteState(ExecutorState.ExecutionFailed);
                    return;
                }

                // Edit & write script to a ps1 file on the USB, then execute, then delete the ps1 file
                try
                {
                    string drive = USBKey.Partitions[0].DriveLetter;
                    ScriptRunner runner = new ScriptRunner(App.Log);

                    // Edit script as necessary
                    Command com = USBKey.Command;
                    com.Script = com.Script.Replace(ScriptDriveVariable, drive + ":");

                    // Write script to a file on the USB
                    string scriptPath = drive + @":/tempPSscript.ps1";
                    File.Delete(scriptPath);
                    File.WriteAllText(scriptPath, com.Script);
                    File.SetAttributes(scriptPath, FileAttributes.Hidden);

                    // https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Core/About/about_PowerShell_exe?view=powershell-5.1
                    // If the value of File is a file path, the script runs in the local scope("dot-sourced"), so that the functions and variables that the
                    // script creates are available in the current session.Enter the script file path and any parameters. File must be the last parameter
                    // in the command. All values typed after the File parameter are interpreted as the script file path and parameters passed to that script.
                    string args = "-executionpolicy unrestricted -File " + scriptPath + " " + com.Parameters;
                    ScriptCommand sc = new ScriptCommand(ScriptRunner.Powershell64bitPath, null, args);

                    // Execute script
                    scriptSucceeded = runner.RunCommand(sc, Level.Debug);

                    // Delete the script file
                    File.Delete(scriptPath);

                    StopEnvVarPolling();

                    ExecutionCompleted = true;
                    RestartOnCompletion = com.RestartEGM;
                }
                catch (Exception e)
                {
                    App.Log.Error(e.Message);
                    App.Log.Error(e.StackTrace);
                    App.Log.Error("Failed to execute command. Exception thrown during execution.");
                    LogDetailMessage("Failed to execute. Check results.txt file on USB for more info.");
                    ExecuteState(ExecutorState.ExecutionFailed);
                    return;
                }

                if (scriptSucceeded)
                {
                    ExecutionSucceeded = true;
                    ExecuteState(ExecutorState.ExecutionSucceeded);
                    return;
                }
                else
                {
                    App.Log.Error("Command execution failed.");
                    ExecuteState(ExecutorState.ExecutionFailed);
                    return;
                }
            });
        }
        private void ExecuteState(ExecutorState state)
        {
            switch (state)
            {
                case ExecutorState.Started:
                    ParseCommandLineParams();
                    break;
                case ExecutorState.PassedCommandLineParametersCheck:
                    CheckCommands();
                    break;
                case ExecutorState.PassedCommandCheck:
                    CheckRsaKeys();
                    break;
                case ExecutorState.PassedRSAKeyCheck:
                    CheckUsbs();
                    break;
                case ExecutorState.PassedUSBCheck:
                    CheckLogicDoor();
                    break;
                case ExecutorState.PassedLogicDoorCheck:
                    ExecuteCommandKey();
                    break;
                case ExecutorState.ExecutionFailed:
                    HandleExecutionFailure();
                    break;
                case ExecutorState.ExecutionSucceeded:
                    HandleExecutionSuccess();
                    break;
                case ExecutorState.ExitImmediately:
                    ExitImmediately();
                    break;
            }
        }
        private void ExitImmediately()
        {
            Action shutdownAction = new Action(() =>
            {
                App.Log.Info("Shutting down early...");
                Application.Current.Shutdown(); // this only works if called from the main thread (thread that spawned the app)
            });

            ThreadUtil.ExecuteOnUI(shutdownAction);
        }
        private void HandleExecutionFailure()
        {
            ShowGUI = true;
            _sw.Stop();
            LogStatusMessage("Execution failed. Please remove the USB to continue.", "Orange");
            FlushLogBuffersAndShutdown();
        }
        private void HandleExecutionSuccess()
        {
            ShowGUI = true;
            _sw.Stop();
            if (RestartOnCompletion)
            {
                LogStatusMessage("Execution succeeded. Remove the USB to continue. The EGM will be automatically restarted when the USB is removed.", "LightGreen");
            }
            else
            {
                LogStatusMessage("Execution succeeded. Remove the USB to continue.", "LightGreen");
            }
            FlushLogBuffersAndShutdown();
        }
        private void HandleTimeoutCheck()
        {
            // todo I'm not sure how to properly Abort the thread which is executing (which presumably is hanging if we timeout)
            if (int.TryParse(Environment.GetEnvironmentVariable(_timeoutEnvVarName, EnvironmentVariableTarget.Machine), out int computedTimeoutThreshold))
                _timeoutThreshold = computedTimeoutThreshold;

            if (_timeoutThreshold <= 0)
                return; // timeout is disabled

            int elapsedSeconds = Convert.ToInt32(_sw.Elapsed.TotalSeconds);
            if (elapsedSeconds > _timeoutThreshold)
            {
                // todo force Executor into a Failed state, with a timeout message
                // not sure how to properly Abort the thread dynamically
            }
        }
        private void LogWatcha_Updated(object sender, EventArgs e)
        {
            FullLog = _logWatcha.LogContent;
        }
        private void ParseCommandLineParams()
        {
            string[] args = App.CommandLineParams;
            bool validArgs = true;

            HashSet<string> skipDoorKeys = new HashSet<string> { "-s", "--skipdoor" };
            HashSet<string> rsaKeyDirectoryPath = new HashSet<string> { "-k", "--key" };

            // first arg must be a key
            if (args.Length > 0)
                if (args[0][0] != '-')
                    validArgs = false;

            if (validArgs)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];
                    if (skipDoorKeys.Contains(arg))
                    {
                        SkipLogicDoorCheck = true;
                        continue;
                    }

                    if (rsaKeyDirectoryPath.Contains(arg))
                    {
                        if (i + 1 == args.Length)
                        {
                            validArgs = false;
                            continue;
                        }

                        RsaPublicKeyPathFullPath = args[i + 1].ToString();
                        i++;
                        continue;
                    }

                    // Either 2 values in a row, or a key that isn't defined.
                    validArgs = false;
                    break;
                }
            }

            // -k is a required command line parameter
            if (RsaPublicKeyPathFullPath == null || RsaPublicKeyPathFullPath == "")
                validArgs = false;

            if (validArgs)
            {
                App.Log.Debug("Passed command line parameters check.");
                ExecuteState(ExecutorState.PassedCommandLineParametersCheck);
                return;
            }
            else
            {
                App.Log.Debug("Failed command line parameter check.");
                ExecuteState(ExecutorState.ExitImmediately);
                return;
            }
        }
        private void StartEnvVarPolling()
        {
            Environment.SetEnvironmentVariable(_loggingEnvVarName, "", EnvironmentVariableTarget.Machine); // clear to prevent stale data

            _pollingEnvVar = true;
            Task.Run(() =>
            {
                while (_pollingEnvVar)
                {
                    LogDetailMessage(Environment.GetEnvironmentVariable(_loggingEnvVarName, EnvironmentVariableTarget.Machine));
                    // HandleTimeoutCheck(); // todo I think this will go here, if/when it is implemented
                    
                    Thread.Sleep(1000);
                }

                LogDetailMessage(Environment.GetEnvironmentVariable(_loggingEnvVarName, EnvironmentVariableTarget.Machine));
            });
        }
        private void StopEnvVarPolling()
        {
            _pollingEnvVar = false;
        }
        private void VolumeRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    string driveColon = (string)e.NewEvent.Properties["DriveName"].Value;
                    string drive = driveColon.Replace(":", "");
                    App.Log.Info("DRIVE REMOVED EVENT: Drive " + drive + " removed");

                    string usbDrive = USBKey?.Partitions[0]?.DriveLetter ?? "";

                    if (drive == usbDrive)
                    {
                        App.Log.Info("Removed drive matches the USB Key's drive. Handling USB removal.");
                        HandleUSBRemoved();
                    }
                    else
                    {
                        App.Log.Info("Removed drive does not match the USB Key's drive which is: " + usbDrive);
                    }

                }
                catch (Exception exp)
                {
                    App.Log.Info("Exception in Volume Remove Event code: " + exp.Message + " " + exp.StackTrace);
                }
            }));
        }
    }
}
