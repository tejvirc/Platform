namespace Aristocrat.Monaco.Test.KeyConverter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Accounting.Contracts;
    using Application.Contracts.EKey;
    using Application.Contracts.Robot;
    using Gaming.Contracts;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Kernel.Contracts;
    using log4net;
    using NativeKeyboard;
    using Newtonsoft.Json;
    using Vgt.Client12.Testing.Tools;

    /// <summary>
    ///     This class is responsible for capturing all keyboard input for all windows
    ///     owned/used by the process, and converting them to events, if the key is mapped
    ///     to an event based on configuration.  Generated events are posted to the event
    ///     bus.
    /// </summary>
    public sealed class KeyConverter : BaseRunnable, IService
    {
        private const string EventFile = "keyboardActions.json";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly CancellationTokenSource _source = new();
        private readonly IKeyboardService _keyboardService;
        private readonly KeyConverterConfiguration _configuration;
        private readonly Dictionary<UserKeys, bool> _lastToggleValue = new();

        public KeyConverter()
        {
            Logger.Info("Constructing");
            _configuration = BuildKeyMapping();
            _keyboardService = KeyboardServiceFactory.CreateKeyboardService(_configuration);
            _keyboardService.KeyChanged += KeyboardServiceOnKeyChanged;
        }

        /// <inheritdoc />>
        public string Name => "Key To Event Converter";

        /// <inheritdoc />>
        public ICollection<Type> ServiceTypes => new[] { typeof(KeyConverter) };

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            _keyboardService.Dispose();
            _keyboardService.KeyChanged -= KeyboardServiceOnKeyChanged;
            base.Dispose(disposing);
        }

        /// <inheritdoc />>
        protected override void OnInitialize()
        {
            Logger.Info("Initialized");
        }

        /// <inheritdoc />
        protected override void OnRun()
        {
            _keyboardService.Run(_source.Token);
        }

        private void KeyboardServiceOnKeyChanged(object sender, KeyChangedEventArgs e)
        {
            switch (e.State)
            {
                case KeyState.Pressed:
                    HandleKeyDown(e.Keys);
                    break;
                case KeyState.Release:
                    HandleKeyUp(e.Keys);
                    break;
            }
        }

        /// <inheritdoc />
        protected override void OnStop()
        {
            _source.Cancel();
        }

        private void HandleKeyDown(UserKeys key)
        {
            if (!_configuration.KeyDownEvents.ContainsKey(key))
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
                    if (_lastToggleValue.TryGetValue(key, out var value1))
                    {
                        newState = !value1;
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

            if (value is not IEvent theEvent)
            {
                return;
            }

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
            }

            Logger.Debug($"Handling key down: [{key}]");
            PostEvent(theEvent);
        }

        private void HandleKeyUp(UserKeys key)
        {
            if (!_configuration.KeyUpEvents.ContainsKey(key))
            {
                return;
            }

            Logger.Debug($"Handling key up: [{key}]");

            var evt = Activator.CreateInstance(_configuration.KeyUpEvents[key].type, _configuration.KeyUpEvents[key].args.ToArray()) as IEvent;

            PostEvent(evt);
        }

        private static void PostEvent<T>(T theEvent)
            where T : IEvent
        {
            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            eventBus.Publish(theEvent.GetType(), theEvent);

            Logger.DebugFormat("Posted [{0}] to the EventBus", theEvent);
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

        private KeyConverterConfiguration BuildKeyMapping()
        {
            // *NOTE* InputEvents need to use the corresponding PhysicalIOId value as defined in the IO configuration file for
            // the target hardware (IE. IO.Gen8.config.xml), logical events (IE. DownEvent) need to use the corresponding
            // LogicalIOId.

            var keyDownEvents = new Dictionary<UserKeys, (Type type, ICollection<object> args)>
            {
                { UserKeys.C, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Collect }) },
                { UserKeys.Q, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Play1 }) },
                { UserKeys.W, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Play2 }) },
                { UserKeys.E, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Play3 }) },
                { UserKeys.R, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Play4 }) },
                { UserKeys.T, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Play5 }) },
                { UserKeys.Y, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.MaxBet }) },
                { UserKeys.X, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.ExitToLobby }) },
                { UserKeys.A, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Bet1 }) },
                { UserKeys.S, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Bet2 }) },
                { UserKeys.D, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Bet3 }) },
                { UserKeys.F, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Bet4 }) },
                { UserKeys.G, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Bet5 }) },
                { UserKeys.Space, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Play }) },
                { UserKeys.F6, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Service }) },
                { UserKeys.B, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Barkeeper }) },
#if GEN8_SPRING_LOADED_UserKeysWITCH
                { UserKeys.F1, (type: typeof(ToggleInputEvent), args: new List<object> { 2, true }) }, // Audit Switch
#else
                { UserKeys.F1, (type: typeof(ToggleInputEvent), args: new List<object> { 2, false }) }, // Audit Switch
#endif
            };

            // Key up events
            var keyUpEvents = new Dictionary<UserKeys, (Type type, ICollection<object> args)>
            {
                { UserKeys.C, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Collect }) },
                { UserKeys.Q, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Play1 }) },
                { UserKeys.W, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Play2 }) },
                { UserKeys.E, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Play3 }) },
                { UserKeys.R, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Play4 }) },
                { UserKeys.T, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Play5 }) },
                { UserKeys.Y, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.MaxBet }) },
                { UserKeys.X, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.ExitToLobby }) },
                { UserKeys.A, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Bet1 }) },
                { UserKeys.S, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Bet2 }) },
                { UserKeys.D, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Bet3 }) },
                { UserKeys.F, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Bet4 }) },
                { UserKeys.G, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Bet5 }) },
                { UserKeys.Space, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Play }) },
                { UserKeys.F2, (type: typeof(DownEvent), args: new List<object> { (int)ButtonLogicalId.Button30 }) },
                { UserKeys.F5, (type: typeof(DebugNoteEvent), args: new List<object> { 0 }) },
                { UserKeys.F6, (type: typeof(UpEvent), args: new List<object> { (int)ButtonLogicalId.Service }) },
                { UserKeys.D1, (type: typeof(DebugNoteEvent), args: new List<object> { 1 }) },
                { UserKeys.D2, (type: typeof(DebugNoteEvent), args: new List<object> { 2 }) },
                { UserKeys.D3, (type: typeof(DebugNoteEvent), args: new List<object> { 5 }) },
                { UserKeys.D4, (type: typeof(DebugNoteEvent), args: new List<object> { 10 }) },
                { UserKeys.D5, (type: typeof(DebugNoteEvent), args: new List<object> { 20 }) },
                { UserKeys.D6, (type: typeof(DebugNoteEvent), args: new List<object> { 50 }) },
                { UserKeys.D7, (type: typeof(DebugNoteEvent), args: new List<object> { 100 }) },
                { UserKeys.NumPad1, (type: typeof(DebugCoinEvent), args: new List<object> { 1000 }) },
                { UserKeys.NumPad2, (type: typeof(DebugCoinEvent), args: new List<object> { 5000 }) },
                { UserKeys.NumPad3, (type: typeof(DebugCoinEvent), args: new List<object> { 10000 }) },
                { UserKeys.NumPad4, (type: typeof(DebugCoinEvent), args: new List<object> { 25000 }) },
                { UserKeys.NumPad5, (type: typeof(DebugCoinEvent), args: new List<object> { 50000 }) },
                { UserKeys.NumPad6, (type: typeof(VoucherEscrowedEvent), args: new List<object> { "123456789012345678" }) },
                { UserKeys.N, (type: typeof(FakeCardReaderEvent), args: new List<object> { 1, "1124936", true }) },
                { UserKeys.M, (type: typeof(FakeCardReaderEvent), args: new List<object> { 1, "1124936", false }) },
                { UserKeys.H, (type: typeof(DebugEKeyEvent), args: new List<object> { true, "D:\\" }) },
                { UserKeys.J, (type: typeof(DebugEKeyEvent), args: new List<object> { false, null }) },

                { UserKeys.Oemcomma, (type: typeof(InputEvent), args: new List<object> { 48, true }) },
                { UserKeys.OemPeriod, (type: typeof(InputEvent), args: new List<object> { 48, false }) },
                { UserKeys.D0, (type: typeof(InputEvent), args: new List<object> { 51, true }) },
                { UserKeys.OemMinus, (type: typeof(InputEvent), args: new List<object> { 51, false }) },
                { UserKeys.D8, (type: typeof(InputEvent), args: new List<object> { 49, true }) },
                { UserKeys.D9, (type: typeof(InputEvent), args: new List<object> { 49, false }) },
                { UserKeys.O, (type: typeof(InputEvent), args: new List<object> { 50, true }) },
                { UserKeys.P, (type: typeof(InputEvent), args: new List<object> { 50, false }) },
                { UserKeys.K, (type: typeof(InputEvent), args: new List<object> { 45, true }) },
                { UserKeys.L, (type: typeof(InputEvent), args: new List<object> { 45, false }) },
                { UserKeys.NumPad8, (type: typeof(InputEvent), args: new List<object> { 3, true }) },
                { UserKeys.NumPad9, (type: typeof(InputEvent), args: new List<object> { 3, false }) },
                { UserKeys.Divide, (type: typeof(InputEvent), args: new List<object> { 4, true }) },
                { UserKeys.Multiply, (type: typeof(InputEvent), args: new List<object> { 4, false }) },
                { UserKeys.OemSemicolon, (type: typeof(InputEvent), args: new List<object> { 46, true }) },
                { UserKeys.OemQuotes, (type: typeof(InputEvent), args: new List<object> { 46, false }) },
                { UserKeys.OemPipe, (type: typeof(HardwareFaultEvent), args: new List<object> { NoteAcceptorFaultTypes.StackerDisconnected }) },
                { UserKeys.OemQuestion, (type: typeof(HardwareFaultClearEvent), args: new List<object> { NoteAcceptorFaultTypes.StackerDisconnected }) },

                { UserKeys.U, (type: typeof(DisableCountdownTimerEvent), args: new List<object> { true, TimeSpan.FromSeconds(5) }) },
                { UserKeys.I, (type: typeof(DisableCountdownTimerEvent), args: new List<object> {false }) },
                { UserKeys.Z, (type: typeof(RobotControllerEnableEvent), args: new List<object>()) },
                { UserKeys.Escape, (type: typeof(ExitRequestedEvent), args: new List<object> { ExitAction.ShutDown }) },
                { UserKeys.F11, (type: typeof(TerminateGameProcessEvent), args: new List<object> { true }) },
                { UserKeys.F12, (type: typeof(TerminateGameProcessEvent), args: new List<object> { false }) },
                { UserKeys.Add, (type: typeof(DebugAnyCreditEvent), args: new List<object> {20, AccountType.Promo}) },
                { UserKeys.Subtract, (type: typeof(DebugAnyCreditEvent), args: new List<object> {20, AccountType.NonCash}) },

                { UserKeys.OemOpenBrackets, (type: typeof(FakeCardReaderEvent), args: new List<object>{ 0, "EMP123", true }) }, // Technician employee card
                { UserKeys.OemCloseBrackets, (type: typeof(FakeCardReaderEvent), args: new List<object>{ 0, "EMP321", true }) }, // Operator employee card
                { UserKeys.Oemplus, (type: typeof(FakeCardReaderEvent), args: new List<object>{ 0, string.Empty, false }) } // Remove card
            };

            if (File.Exists(EventFile))
            {
                var events = JsonConvert.DeserializeObject<Dictionary<UserKeys, (Type type, ICollection<object> args)>>(File.ReadAllText(EventFile));
                foreach (var eventMap in events)
                {
                    var parsedEvent = ParseJsonDefinedEvent(eventMap.Value);
                    if (null == parsedEvent)
                    {
                        continue;
                    }

                    keyUpEvents[eventMap.Key] = parsedEvent.Value;
                }
            }

            return new KeyConverterConfiguration(
                UserKeys.F9,
                keyDownEvents,
                keyUpEvents,
                new[] { UserKeys.Down, UserKeys.Up, UserKeys.Left, UserKeys.Right, UserKeys.Tab, UserKeys.LShiftKey });
        }
    }
}