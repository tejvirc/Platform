namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts.Button;
    using Contracts.IO;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;
    using DisabledEvent = Contracts.Button.DisabledEvent;
    using EnabledEvent = Contracts.Button.EnabledEvent;

    /// <summary>
    ///     Provides access to button services. Implements IButton.
    ///     ButtonService is responsible for mapping and handling of physical
    ///     input events from one or more IO services and posting the associated logical button events to the system.
    ///     This component provides an operator menu plug-in interface for managing buttons.
    /// </summary>
    public class ButtonService : IDeviceService, IButtonService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _bus;
        private readonly IIO _io;

        private readonly ConcurrentDictionary<int, LogicalButton> _logicalButtons =
            new ConcurrentDictionary<int, LogicalButton>();

        private bool _disposed;

        public ButtonService()
            : this(
                ServiceManager.GetInstance().GetService<IIO>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        public ButtonService(IIO io, IEventBus bus)
        {
            _io = io ?? throw new ArgumentNullException(nameof(io));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));

            Enabled = false;
        }

        private static ButtonLogicalState LastEnabledLogicalState { get; set; }

        /// <inheritdoc />
        public IReadOnlyDictionary<int, LogicalButton> LogicalButtons => _logicalButtons;

        /// <inheritdoc />
        public ButtonLogicalState LogicalState { get; private set; }

        /// <inheritdoc />
        public bool IsTestModeActive { get; private set; }

        /// <inheritdoc />
        public void Disable(IEnumerable<int> buttonIdList)
        {
            foreach (var buttonId in buttonIdList)
            {
                if (_logicalButtons.TryGetValue(buttonId, out var button))
                {
                    if (button.State == ButtonState.Enabled)
                    {
                        _logicalButtons[buttonId].State = ButtonState.Disabled;
                        Logger.Debug(
                            $"Logical Button {buttonId} {_logicalButtons[buttonId].Name} {_logicalButtons[buttonId].State}");
                    }
                }
                else
                {
                    Logger.Warn($"Cannot disable logical Button {buttonId}, need to add first");
                }
            }
        }

        /// <inheritdoc />
        public void Enable(IEnumerable<int> buttonIdList)
        {
            foreach (var buttonId in buttonIdList)
            {
                if (_logicalButtons.TryGetValue(buttonId, out var button))
                {
                    if (button.State == ButtonState.Disabled)
                    {
                        _logicalButtons[buttonId].State = ButtonState.Enabled;
                        Logger.Debug(
                            $"Logical Button {buttonId} {_logicalButtons[buttonId].Name} {_logicalButtons[buttonId].State}");
                    }
                }
                else
                {
                    Logger.Warn($"Cannot enable logical Button {buttonId}, need to add first");
                }
            }
        }

        /// <inheritdoc />
        public ButtonAction GetButtonAction(int buttonId)
        {
            return _logicalButtons.TryGetValue(buttonId, out var button) ? button.Action : ButtonAction.Up;
        }

        /// <inheritdoc />
        public string GetButtonName(int buttonId)
        {
            return _logicalButtons.TryGetValue(buttonId, out var button) ? button.Name : string.Empty;
        }

        /// <inheritdoc />
        public int GetButtonPhysicalId(int buttonId)
        {
            return _logicalButtons.TryGetValue(buttonId, out var button) ? button.PhysicalId : -1;
        }

        /// <inheritdoc />
        public ButtonState GetButtonState(int buttonId)
        {
            return _logicalButtons.TryGetValue(buttonId, out var button) ? button.State : ButtonState.Uninitialized;
        }

        /// <inheritdoc />
        public string GetLocalizedButtonName(int buttonId)
        {
            return _logicalButtons.TryGetValue(buttonId, out var button) ? button.LocalizedName : string.Empty;
        }

        /// <inheritdoc />
        public int GetButtonLampBit(int buttonId)
        {
            return _logicalButtons.TryGetValue(buttonId, out var button) ? button.LampBit : -1;
        }

        public void EnterButtonTestMode()
        {
            IsTestModeActive = true;
        }

        public void ExitButtonTestMode()
        {
            IsTestModeActive = false;
        }

        /// <inheritdoc />
        public bool Enabled { get; private set; }

        /// <inheritdoc />
        public bool Initialized { get; private set; }

        /// <inheritdoc />
        public string LastError { get; private set; }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public DisabledReasons ReasonDisabled { get; private set; }

        /// <inheritdoc />
        public string ServiceProtocol
        {
            get => string.Empty;
            set { }
        }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public void Disable(DisabledReasons reason)
        {
            Logger.Debug(Name + " disabled by " + reason);
            ReasonDisabled |= reason;
            Enabled = false;
            if (LogicalState != ButtonLogicalState.Disabled)
            {
                Logger.Debug("Last enabled logical state " + LastEnabledLogicalState + " set to " + LogicalState);
                LastEnabledLogicalState = LogicalState;
            }

            LogicalState = ButtonLogicalState.Disabled;
            _bus?.Publish(new DisabledEvent(ReasonDisabled));
        }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public bool Enable(EnabledReasons reason)
        {
            if (Enabled)
            {
                Logger.Debug(Name + " enabled by " + reason + " logical state " + LogicalState);
                _bus.Publish(new EnabledEvent(reason));
            }
            else if (Initialized)
            {
                if (((ReasonDisabled & DisabledReasons.Error) > 0 ||
                     (ReasonDisabled & DisabledReasons.FirmwareUpdate) > 0) &&
                    (reason == EnabledReasons.Reset || reason == EnabledReasons.Operator))
                {
                    if ((ReasonDisabled & DisabledReasons.Error) > 0)
                    {
                        Logger.Debug("Removed disabled reason " + DisabledReasons.Error);
                        ReasonDisabled &= ~DisabledReasons.Error;
                    }

                    if ((ReasonDisabled & DisabledReasons.FirmwareUpdate) > 0)
                    {
                        Logger.Debug("Removed disabled reason " + DisabledReasons.FirmwareUpdate);
                        ReasonDisabled &= ~DisabledReasons.FirmwareUpdate;
                    }
                }
                else if ((ReasonDisabled & DisabledReasons.Operator) > 0 && reason == EnabledReasons.Operator)
                {
                    Logger.Debug("Removed disabled reason " + DisabledReasons.Operator);
                    ReasonDisabled &= ~DisabledReasons.Operator;
                }
                else if ((ReasonDisabled & DisabledReasons.Service) > 0 && reason == EnabledReasons.Service)
                {
                    Logger.Debug("Removed disabled reason " + DisabledReasons.Service);
                    ReasonDisabled &= ~DisabledReasons.Service;
                }
                else if ((ReasonDisabled & DisabledReasons.System) > 0 && reason == EnabledReasons.System)
                {
                    Logger.Debug("Removed disabled reason " + DisabledReasons.System);
                    ReasonDisabled &= ~DisabledReasons.System;
                }
                else if ((ReasonDisabled & DisabledReasons.Configuration) > 0 &&
                         reason == EnabledReasons.Configuration)
                {
                    Logger.Debug("Removed disabled reason " + DisabledReasons.Configuration);
                    ReasonDisabled &= ~DisabledReasons.Configuration;
                }

                // Set enabled if we no longer have any disabled reasons.
                Enabled = ReasonDisabled == 0;
                if (Enabled)
                {
                    Logger.Debug(Name + " enabled by " + reason + " logical state " + LogicalState);
                    if (LastEnabledLogicalState != ButtonLogicalState.Uninitialized &&
                        LastEnabledLogicalState != ButtonLogicalState.Idle)
                    {
                        Logger.Debug("Logical state " + LogicalState + " reset to " + ButtonLogicalState.Idle);
                        LogicalState = ButtonLogicalState.Idle;
                    }
                    else
                    {
                        Logger.Debug("Logical state " + LogicalState + " reset to " + LastEnabledLogicalState);
                        LogicalState = LastEnabledLogicalState;
                    }

                    _bus.Publish(new EnabledEvent(reason));
                }
                else
                {
                    Logger.Debug(
                        Name + " can not be enabled by " + reason + " because disabled by " + ReasonDisabled);
                    _bus.Publish(new DisabledEvent(ReasonDisabled));
                }
            }
            else
            {
                Logger.Debug(Name + " can not be enabled by " + reason + " because service is not initialized");
                _bus.Publish(new DisabledEvent(ReasonDisabled));
            }

            return Enabled;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public string Name => "Button Service";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IButtonService) };

        public void Initialize()
        {
            LastError = string.Empty;

            _bus.Subscribe<InputEvent>(this, HandleEvent);

            var config = _io.GetConfiguration();

            foreach (var item in config.Buttons)
            {
                var button = new LogicalButton(item.PhysicalId, item.Name, item.Name, item.LampBit);
                if (_logicalButtons.TryAdd(item.LogicalId, button))
                {
                    button.State = ButtonState.Enabled;
                }
            }

            Initialized = true;

            Enable(EnabledReasons.Service);

            Logger.Info(Name + " initialized");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Disable(DisabledReasons.Service);

                _bus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void HandleEvent(InputEvent theEvent)
        {
            var item = _logicalButtons.FirstOrDefault(i => i.Value.PhysicalId == theEvent.Id);

            var button = item.Value;

            if (button == null)
            {
                return;
            }

            var buttonId = item.Key;

            Logger.Debug($"buttonId: {buttonId}");

            button.Action = theEvent.Action ? ButtonAction.Down : ButtonAction.Up;

            if (theEvent.Action)
            {
                _bus.Publish(new SystemDownEvent(buttonId, button.State == ButtonState.Enabled || IsTestModeActive));
            }
            else
            {
                _bus.Publish(new SystemUpEvent(buttonId, button.State == ButtonState.Enabled || IsTestModeActive));
            }

            if (button.State == ButtonState.Enabled || IsTestModeActive)
            {
                if (theEvent.Action)
                {
                    _bus.Publish(new DownEvent(buttonId));
                }
                else
                {
                    _bus.Publish(new UpEvent(buttonId));
                }

                Logger.Debug(
                    $"Physical Button {theEvent.Id} {1}, logical Button {button.Action} {buttonId} {button.Name} event posted");
            }
            else
            {
                Logger.Debug(
                    $"Physical Button {theEvent.Id} {1}, logical Button {button.Action} {buttonId} {button.Name} disabled");
            }
        }
    }
}