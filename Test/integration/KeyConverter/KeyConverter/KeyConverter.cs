namespace Aristocrat.Monaco.Test.KeyConverter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;
    using System.Windows.Forms;
    using Accounting.Contracts;
    using Application.Contracts.EKey;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Gaming.Contracts;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Kernel.Contracts;
    using log4net;
    using Newtonsoft.Json;
    using RobotController.Contracts;
    using Vgt.Client12.Testing.Tools;

    /// <summary>
    ///     This class is responsible for capturing all keyboard input for all windows
    ///     owned/used by the process, and converting them to events, if the key is mapped
    ///     to an event based on configuration.  Generated events are posted to the event
    ///     bus.
    /// </summary>
    public sealed class KeyConverter : BaseRunnable, IService
    {
        private struct HookData
        {
            public HookData(IntPtr hook, NativeMethods.HookProc keyboardHookProcedure)
            {
                Hook = hook;
                _keyboardHookProcedure = keyboardHookProcedure;
            }

            public readonly IntPtr Hook;
            private NativeMethods.HookProc _keyboardHookProcedure;
        }

        private const int WhKeyboardDll = 2;
        private const int WhKeyboardDllLl = 13;
        private const int WmKeyDown = 0x100;
        private const int WmKeyUp = 0x101;
        private const int WmSysKeyDown = 0x0104;
        private const int WmSysKeyUp = 0x0105;
        private const int HandledInput = 1;
        private const string EventFile = "keyboardActions.json";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool _shiftModifier;

        private readonly IntPtr _consoleWindow;
        private readonly IList<int> _processIds = new List<int>();
        private readonly bool _lowLevelHook = false;
        private readonly Dictionary<IntPtr, HookData> _hookedWindows = new Dictionary<IntPtr, HookData>();
        private readonly Dictionary<Keys, bool> _lastToggleValue = new Dictionary<Keys, bool>();

        private KeyConverterConfiguration _configuration;
        private NativeMethods.HookProc _keyboardHookProcedure;
        private IntPtr _hook;
        private Keys _lastKey;
        private bool _lastKeyWasDown;
        private bool _enabled;

        /// <summary>
        ///     Initializes a new instance of the KeyConverter class.
        /// </summary>
        public KeyConverter()
        {
            Logger.Info("Constructing");

            _consoleWindow = NativeMethods.GetForegroundWindow();
            _processIds.Add(Process.GetCurrentProcess().Id);

            BuildKeyMapping();
        }

        /// <summary>
        ///     Finalizes an instance of the KeyConverter class.
        /// </summary>
        ~KeyConverter()
        {
            Logger.Info("Destructing");

            Dispose(false);
        }

        /// <inheritdoc />>
        public string Name => "Key To Event Converter";

        /// <inheritdoc />>
        public ICollection<Type> ServiceTypes => new[] { typeof(KeyConverter) };

        /// <inheritdoc />>
        protected override void OnInitialize()
        {
            Logger.Info("Initialized");
        }

        /// <inheritdoc />>
        protected override void OnRun()
        {
            Logger.Info("Run Started");

            if (_enabled && !IsKeyboardConnected())
            {
                _enabled = false;
            }

            if (_lowLevelHook)
            {
                SetHook();
                Application.Run();
            }
            else
            {
                while (RunState == RunnableState.Running)
                {
                    UpdateHooks();
                    Thread.Sleep(1000);
                    Application.DoEvents();
                }
            }

            Logger.Info("Run Stopped");
        }

        /// <inheritdoc />>
        protected override void OnStop()
        {
            Logger.Info("Stop called");

            ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);

            UnsetHook();
            if (_lowLevelHook)
            {
                Application.Exit();
            }
        }

        private static void PostEvent<T>(T theEvent)
            where T : IEvent
        {
            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            eventBus.Publish(theEvent.GetType(), theEvent);

            Logger.DebugFormat("Posted [{0}] to the EventBus", theEvent);
        }

        private static bool IsKeyboardConnected()
        {
            using (var searcher = new ManagementObjectSearcher("Select Name from Win32_Keyboard"))
            {
                var results = searcher.Get();

                return results.Count >= 1;
            }
        }

        [EnvironmentPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        private IntPtr LowLevelKeyboardProc(int code, IntPtr messageId, IntPtr hookStruct)
        {
            // As stated in the Win32 API documentation, if 'code' is less than zero,
            // the hook procedure must return the value returned by CallNextHookEx.
            //  If we no longer have a hook handle, we didn't want this message.
            //  If the input was not for any of our windows or processes, ignore it.
            if (code < 0 || (IntPtr)0 == _hook || !IsActiveProcess())
            {
                return NativeMethods.CallNextHookEx(_hook, code, messageId, hookStruct);
            }

            // Marshall the data from the callback.
            var param = messageId.ToInt32();
            var keyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(hookStruct, typeof(KeyboardHookStruct));
            var key = (Keys)keyboardHookStruct.KeyCode;

            Logger.Info($"Intercepted key press: code={code} key={key} extra info={keyboardHookStruct.ExtraInfo}");

            // If not the OnOff key and the KeyConverter is OFF, OR if this is a pass-through key, do not consume the key.
            if (_configuration.OnOffKey != key && !_enabled || _configuration.PassThroughKeys.Contains(key))
            {
                Logger.Info("Passing through key press " + key);
                return NativeMethods.CallNextHookEx(_hook, code, messageId, hookStruct);
            }

            HandleKey(key, param);

            return (IntPtr)HandledInput;
        }

        [EnvironmentPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        private IntPtr HighLevelKeyboardProc(int code, IntPtr wParam, IntPtr lParam)
        {
            // As stated in the Win32 API documentation, if 'code' is less than zero,
            // the hook procedure must return the value returned by CallNextHookEx.
            //  If we no longer have a hook handle, we didn't want this message.
            //  If the input was not for any of our windows or processes, ignore it.
            if (code < 0 || (IntPtr)0 == _hook)
            {
                return NativeMethods.CallNextHookEx(_hook, code, wParam, lParam);
            }

            // Marshall the data from the callback.
            var wparam = wParam.ToInt32();
            var lparam = lParam.ToInt64();

            var key = (Keys)wparam;

            _shiftModifier = Convert.ToBoolean(NativeMethods.GetKeyState(Keys.ShiftKey) & 0x8000);

            Logger.Info($"Intercepted key press: code={code} key={key} extra info={wparam}");

            // If not the OnOff key and the KeyConverter is OFF, OR if this is a pass-through key, do not consume the key.
            if (_configuration.OnOffKey != key && !_enabled || _configuration.PassThroughKeys.Contains(key))
            {
                Logger.Info("Passing through key press " + key);
                return NativeMethods.CallNextHookEx(_hook, code, wParam, lParam);
            }

            if ((lparam & (1 << 31)) > 0)
            {
                HandleKeyUp(key);
            }
            else
            {
                HandleKeyDown(key);
            }

            return (IntPtr)HandledInput;
        }

        private void HandleKey(Keys key, int param)
        {
            switch (param)
            {
                case WmKeyDown:
                case WmSysKeyDown:
                    {
                        HandleKeyDown(key);
                        _lastKeyWasDown = true;
                        break;
                    }

                case WmKeyUp:
                case WmSysKeyUp:
                    {
                        HandleKeyUp(key);
                        _lastKeyWasDown = false;
                        break;
                    }
            }

            _lastKey = key;
        }

        private void HandleKeyDown(Keys key)
        {
            if (!_configuration.KeyDownEvents.ContainsKey(key))
            {
                return;
            }

            if (key == _lastKey && _lastKeyWasDown)
            {
                return;
            }

            var value = Activator.CreateInstance(_configuration.KeyDownEvents[key].type, _configuration.KeyDownEvents[key].args.ToArray());

            if (value is ToggleInputEvent toggle)
            {
                if (!toggle.SpringLoaded)
                {
                    // Post the current key down event and return.
                    Logger.Debug($"Handling toggle key down: [{key}]");
                    bool newState;
                    if (_lastToggleValue.ContainsKey(key))
                    {
                        newState = !_lastToggleValue[key];
                    }
                    else
                    {
                        newState = true;
                    }

                    _lastToggleValue[key] = newState;

                    PostEvent(new InputEvent(toggle.Id, newState));
                }
                else
                {
                    Logger.Debug($"Handling spring-loaded behavior: [{key}]");
                    PostEvent(new InputEvent(toggle.Id, false));
                    PostEvent(new InputEvent(toggle.Id, true));
                }

                return;
            }

            if (value is IEvent theEvent)
            {
                var downEvent = theEvent as DownEvent;
                var upEvent = theEvent as UpEvent;

                if (downEvent != null || upEvent != null)
                {
                    var buttonService = ServiceManager.GetInstance().TryGetService<IButtonService>();
                    var logicalId = downEvent?.LogicalId ?? upEvent.LogicalId;

                    if (buttonService != null &&
                        buttonService.LogicalButtons.TryGetValue(logicalId, out var logicalButton))
                    { // convert to an InputEvent if logical button exists
                        theEvent = new InputEvent(logicalButton.PhysicalId, downEvent != null);
                    }
                    else if (_shiftModifier)
                    { // convert to system events of type Down or Up -Event
                        if (downEvent != null)
                        {
                            theEvent = new SystemDownEvent(logicalId);
                        }
                        else
                        {
                            theEvent = new SystemUpEvent(logicalId, false);
                        }
                    }
                }

                Logger.Debug($"Handling key down: [{key}]");
                PostEvent(theEvent);
            }
        }

        private void HandleKeyUp(Keys key)
        {
            // Check the On/Off key first
            if (_configuration.OnOffKey == key)
            {
                _enabled = !_enabled;
            }
            else if (_configuration.KeyUpEvents.ContainsKey(key))
            {
                Logger.Debug($"Handling key up: [{key}]");

                var evt = Activator.CreateInstance(_configuration.KeyUpEvents[key].type, _configuration.KeyUpEvents[key].args.ToArray()) as IEvent;

                PostEvent(evt);
            }
        }

        private void UpdateHooks()
        {
            if (_keyboardHookProcedure == null)
            {
                _keyboardHookProcedure = HighLevelKeyboardProc;
            }

            var functionPointer = Marshal.GetFunctionPointerForDelegate(_keyboardHookProcedure);

            foreach (var handle in NativeMethods.EnumerateProcessWindowHandles(Process.GetCurrentProcess().Id))
            {
                if (NativeMethods.IsWindowVisible(handle) && !_hookedWindows.ContainsKey(handle))
                {
                    _hook = NativeMethods.SetWindowsHookEx(
                        WhKeyboardDll,
                        functionPointer,
                        IntPtr.Zero,
                        NativeMethods.GetWindowThreadProcessId(handle, out _));
                    var lastError = Marshal.GetLastWin32Error();

                    if ((IntPtr)0 == _hook)
                    {
                        Logger.Error(
                            $"SetWindowsHookEx Failed:\n [{lastError.ToString(CultureInfo.InvariantCulture)}]");
                    }
                    else
                    {
                        _hookedWindows.Add(handle, new HookData(_hook, _keyboardHookProcedure));
                    }
                }
            }
        }

        private void SetHook()
        {
            if ((IntPtr)0 == _hook)
            {
                Logger.Info("Attempting to set hook to WinProc");

                _keyboardHookProcedure = LowLevelKeyboardProc;

                var functionPointer = Marshal.GetFunctionPointerForDelegate(_keyboardHookProcedure);

                var module = NativeMethods.GetModuleHandle(typeof(KeyConverter).Module.Name);

                _hook = NativeMethods.SetWindowsHookEx(WhKeyboardDllLl, functionPointer, module, 0);
                var lastError = Marshal.GetLastWin32Error();

                if ((IntPtr)0 == _hook)
                {
                    Logger.Error($"SetWindowsHookEx Failed:\n [{lastError.ToString(CultureInfo.InvariantCulture)}]");
                    throw new ApplicationException("Failed to set Windows keyboard hook");
                }
            }
        }

        private void UnsetHook()
        {
            if (_lowLevelHook)
            {
                if ((IntPtr)0 != _hook)
                {
                    Logger.Info("Attempting to unset hook to WinProc");

                    if (!NativeMethods.UnhookWindowsHookEx(_hook))
                    {
                        Logger.Error(
                            $"UnhookWindowsHookEx Failed:\n [{Marshal.GetLastWin32Error().ToString(CultureInfo.InvariantCulture)}]");
                    }
                }
            }
            else
            {
                foreach (var hookData in _hookedWindows.Values)
                {
                    if (!NativeMethods.UnhookWindowsHookEx(hookData.Hook))
                    {
                        Logger.Error(
                            $"UnhookWindowsHookEx Failed:\n [{Marshal.GetLastWin32Error().ToString(CultureInfo.InvariantCulture)}]");
                    }
                }

                _hookedWindows.Clear();
            }

            _hook = (IntPtr)0;
        }

        private bool IsActiveProcess()
        {
            var useKey = false;

            var currentWindowHandle = NativeMethods.GetForegroundWindow();

            NativeMethods.GetWindowThreadProcessId(currentWindowHandle, out var processId);

            if (_consoleWindow == currentWindowHandle)
            {
                useKey = true;
            }
            else if (_processIds.Contains(processId))
            {
                useKey = true;
            }

            return useKey;
        }

        private void BuildKeyMapping()
        {
            // *NOTE* InputEvents need to use the corresponding PhysicalIOId value as defined in the IO configuration file for 
            // the target hardware (IE. IO.Gen8.config.xml), logical events (IE. DownEvent) need to use the corresponding
            // LogicalIOId.

            var keyDownEvents = new Dictionary<Keys, (Type type, ICollection<object> args)>
            {
                { Keys.C, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Collect }) },
                { Keys.Q, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Play1 }) },
                { Keys.W, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Play2 }) },
                { Keys.E, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Play3 }) },
                { Keys.R, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Play4 }) },
                { Keys.T, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Play5 }) },
                { Keys.Y, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.MaxBet }) },
                { Keys.X, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.ExitToLobby }) },
                { Keys.A, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Bet1 }) },
                { Keys.S, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Bet2 }) },
                { Keys.D, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Bet3 }) },
                { Keys.F, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Bet4 }) },
                { Keys.G, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Bet5 }) },
                { Keys.Space, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Play }) },
                { Keys.F6, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Service }) },
                { Keys.B, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Barkeeper }) },
#if GEN8_SPRING_LOADED_KEYSWITCH
                { Keys.F1, (type: typeof(ToggleInputEvent), args: new List<object> { 2, true }) }, // Audit Switch
#else
                { Keys.F1, (type: typeof(ToggleInputEvent), args: new List<object> { 2, false }) }, // Audit Switch
#endif
            };

            // Key up events
            var keyUpEvents = new Dictionary<Keys, (Type type, ICollection<object> args)>
            {
                { Keys.C, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Collect }) },
                { Keys.Q, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Play1 }) },
                { Keys.W, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Play2 }) },
                { Keys.E, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Play3 }) },
                { Keys.R, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Play4 }) },
                { Keys.T, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Play5 }) },
                { Keys.Y, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.MaxBet }) },
                { Keys.X, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.ExitToLobby }) },
                { Keys.A, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Bet1 }) },
                { Keys.S, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Bet2 }) },
                { Keys.D, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Bet3 }) },
                { Keys.F, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Bet4 }) },
                { Keys.G, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Bet5 }) },
                { Keys.Space, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Play }) },
                { Keys.F2, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Button30 }) },
                { Keys.F5, (type: typeof(DebugNoteEvent), args: new List<object> { 0 }) },
                { Keys.F6, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Service }) },
                { Keys.D1, (type: typeof(DebugNoteEvent), args: new List<object> { 1 }) },
                { Keys.D2, (type: typeof(DebugNoteEvent), args: new List<object> { 2 }) },
                { Keys.D3, (type: typeof(DebugNoteEvent), args: new List<object> { 5 }) },
                { Keys.D4, (type: typeof(DebugNoteEvent), args: new List<object> { 10 }) },
                { Keys.D5, (type: typeof(DebugNoteEvent), args: new List<object> { 20 }) },
                { Keys.D6, (type: typeof(DebugNoteEvent), args: new List<object> { 50 }) },
                { Keys.D7, (type: typeof(DebugNoteEvent), args: new List<object> { 100 }) },
                { Keys.NumPad1, (type: typeof(DebugCoinEvent), args: new List<object> { 1000 }) },
                { Keys.NumPad2, (type: typeof(DebugCoinEvent), args: new List<object> { 5000 }) },
                { Keys.NumPad3, (type: typeof(DebugCoinEvent), args: new List<object> { 10000 }) },
                { Keys.NumPad4, (type: typeof(DebugCoinEvent), args: new List<object> { 25000 }) },
                { Keys.NumPad5, (type: typeof(DebugCoinEvent), args: new List<object> { 50000 }) },
                { Keys.NumPad6, (type: typeof(VoucherEscrowedEvent), args: new List<object> { "123456789012345678" }) },
                { Keys.N, (type: typeof(FakeCardReaderEvent), args: new List<object> { 1, "1124936", true }) },
                { Keys.M, (type: typeof(FakeCardReaderEvent), args: new List<object> { 1, "1124936", false }) },
                { Keys.H, (type: typeof(DebugEKeyEvent), args: new List<object> { true, "D:\\" }) },
                { Keys.J, (type: typeof(DebugEKeyEvent), args: new List<object> { false, null }) },

                { Keys.Oemcomma, (type: typeof(InputEvent), args: new List<object> { 48, true }) },
                { Keys.OemPeriod, (type: typeof(InputEvent), args: new List<object> { 48, false }) },
                { Keys.D0, (type: typeof(InputEvent), args: new List<object> { 51, true }) },
                { Keys.OemMinus, (type: typeof(InputEvent), args: new List<object> { 51, false }) },
                { Keys.D8, (type: typeof(InputEvent), args: new List<object> { 49, true }) },
                { Keys.D9, (type: typeof(InputEvent), args: new List<object> { 49, false }) },
                { Keys.O, (type: typeof(InputEvent), args: new List<object> { 50, true }) },
                { Keys.P, (type: typeof(InputEvent), args: new List<object> { 50, false }) },
                { Keys.K, (type: typeof(InputEvent), args: new List<object> { 45, true }) },
                { Keys.L, (type: typeof(InputEvent), args: new List<object> { 45, false }) },
                { Keys.NumPad8, (type: typeof(InputEvent), args: new List<object> { 3, true }) },
                { Keys.NumPad9, (type: typeof(InputEvent), args: new List<object> { 3, false }) },
                { Keys.Divide, (type: typeof(InputEvent), args: new List<object> { 4, true }) },
                { Keys.Multiply, (type: typeof(InputEvent), args: new List<object> { 4, false }) },
                { Keys.OemSemicolon, (type: typeof(InputEvent), args: new List<object> { 46, true }) },
                { Keys.OemQuotes, (type: typeof(InputEvent), args: new List<object> { 46, false }) },
                { Keys.OemPipe, (type: typeof(HardwareFaultEvent), args: new List<object> { NoteAcceptorFaultTypes.StackerDisconnected }) },
                { Keys.OemQuestion, (type: typeof(HardwareFaultClearEvent), args: new List<object> { NoteAcceptorFaultTypes.StackerDisconnected }) },

                { Keys.U, (type: typeof(DisableCountdownTimerEvent), args: new List<object> { true, TimeSpan.FromSeconds(5) }) },
                { Keys.I, (type: typeof(DisableCountdownTimerEvent), args: new List<object> {false }) },
                { Keys.Z, (type: typeof(RobotControllerEnableEvent), args: new List<object>()) },
                { Keys.Escape, (type: typeof(ExitRequestedEvent), args: new List<object> { ExitAction.ShutDown }) },
                { Keys.F11, (type: typeof(TerminateGameProcessEvent), args: new List<object> { true }) },
                { Keys.F12, (type: typeof(TerminateGameProcessEvent), args: new List<object> { false }) },
                { Keys.Add, (type: typeof(DebugAnyCreditEvent), args: new List<object> {20, AccountType.Promo}) },
                { Keys.Subtract, (type: typeof(DebugAnyCreditEvent), args: new List<object> {20, AccountType.NonCash}) },

                { Keys.OemOpenBrackets, (type: typeof(FakeCardReaderEvent), args: new List<object>{ 0, "EMP123", true }) }, // Technician employee card
                { Keys.OemCloseBrackets, (type: typeof(FakeCardReaderEvent), args: new List<object>{ 0, "EMP321", true }) }, // Operator employee card
                { Keys.Oemplus, (type: typeof(FakeCardReaderEvent), args: new List<object>{ 0, string.Empty, false }) } // Remove card
            };

            if (File.Exists(EventFile))
            {
                var events = JsonConvert.DeserializeObject<Dictionary<Keys, (Type type, ICollection<object> args)>>(File.ReadAllText(EventFile));
                foreach (var eventMap in events)
                {
                    var parsedEvent = ParseJsonDefinedEvent(eventMap.Value);
                    if (null == parsedEvent)
                        continue;

                    if (keyUpEvents.ContainsKey(eventMap.Key))
                    {
                        keyUpEvents[eventMap.Key] = parsedEvent.Value;
                    }
                    else
                    {
                        keyUpEvents.Add(eventMap.Key, parsedEvent.Value);
                    }
                }
            }

            _configuration = new KeyConverterConfiguration(Keys.F9, keyDownEvents, keyUpEvents)
            {
                PassThroughKeys = new List<Keys>(
                    new[]
                    {
                        Keys.Down, Keys.Up, Keys.Left, Keys.Right, Keys.Tab, Keys.LShiftKey
                    })
            };
        }

        private static (Type type, ICollection<object> args)? ParseJsonDefinedEvent((Type type, ICollection<object> args) eventValueTuple)
        {
            var eventType = eventValueTuple.type;
            var ctorInfos = eventType.GetConstructors().Where(ctorInfo => ctorInfo.GetParameters().Length == eventValueTuple.args.Count);

            foreach (var ctorInfo in ctorInfos)
            {
                try
                {
                    var parsedParameters = new List<object>();

                    var ctorDefinedParameters = ctorInfo.GetParameters();

                    var jsonDefinedParameters = eventValueTuple.args.ToArray();

                    for (var i = 0; i < ctorDefinedParameters.Length; i++)
                    {
                        if (ctorDefinedParameters[i].ParameterType.IsEnum)
                        {
                            var enumName = Convert.ToString(jsonDefinedParameters[i]).Split('.').Last();
                            parsedParameters.Add(Enum.Parse(ctorDefinedParameters[i].ParameterType, enumName));
                        }
                        else
                        {
                            parsedParameters.Add(Convert.ChangeType(jsonDefinedParameters[i], ctorDefinedParameters[i].ParameterType));
                        }
                    }

                    return (eventValueTuple.type, parsedParameters);
                }
                catch (Exception e)
                {
                    Logger.Debug($"Parse Json-defined event failure: [{nameof(eventValueTuple.type)}][Exception:{e}]");
                }
            }

            return null;
        }
    }
}