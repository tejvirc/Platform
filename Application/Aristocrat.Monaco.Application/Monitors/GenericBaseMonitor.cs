namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Contracts.Localization;
    using Hardware.Contracts;
    using Kernel;
    using log4net;
    using Kernel.MessageDisplay;
    using Kernel.Contracts.MessageDisplay;

    /// <summary>
    ///     Base class for common behavior for device monitors.
    /// </summary>
    public abstract class GenericBaseMonitor : IPropertyProvider, IDisposable
    {
        private const int DefaultDelayInterval = 2000;
        protected const string DisconnectedKey = "Disconnected";
        protected const string DisabledKey = "Disabled";
        protected const string DfuInprogressKey = "DfuInprogress";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMessageDisplay _messageDisplay;
        private readonly ISystemDisableManager _disableManager;

        // Map of all possible errors, whether active or not
        private readonly Dictionary<string, ErrorInfo> _faults = new();

        // Map of any error names whose Disable functionality needs to wait for an external impetus.
        private readonly List<ErrorInfo> _pendingFaults = new();

        private readonly Dictionary<string, object> _properties = new();

        protected GenericBaseMonitor()
            : this(
                ServiceManager.GetInstance().GetService<IMessageDisplay>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>())
        {
        }

        protected GenericBaseMonitor(IMessageDisplay message, ISystemDisableManager disable)
        {
            _messageDisplay = message ?? throw new ArgumentNullException(nameof(message));
            _disableManager = disable ?? throw new ArgumentNullException(nameof(disable));
        }

        /// <summary>
        ///     Get general device name
        /// </summary>
        public abstract string DeviceName { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public ICollection<KeyValuePair<string, object>> GetCollection =>
            new List<KeyValuePair<string, object>>(_properties);

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            if (_properties.TryGetValue(propertyName, out var value))
            {
                return value;
            }

            var errorMessage = "Unknown monitor property: " + propertyName;
            Logger.Error(errorMessage);
            throw new UnknownPropertyException(errorMessage);
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            // No external sets for this provider...
        }

        /// <summary>
        ///     Releases the unmanaged resources used by the Aristocrat.Monaco.Application.Monitors.GenericBaseMonitor and
        ///     optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     True to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        /// <summary>
        ///     Manage a binary condition by its name.
        ///     Property key will have the format "[DeviceName]_[condition]" .
        /// </summary>
        /// <param name="condition">Name of condition</param>
        /// <param name="classification">The <see cref="DisplayableMessageClassification" /> category.</param>
        /// <param name="priority">The <see cref="DisplayableMessagePriority" /> category.</param>
        /// <param name="id">The Guid for the <see cref="IDisplayableMessage" /></param>
        /// <param name="isLockup">Whether to disable the system while this condition goes into effect (default false).</param>
        protected void ManageBinaryCondition(
            string condition,
            DisplayableMessageClassification classification,
            DisplayableMessagePriority priority,
            Guid id,
            bool isLockup = false)
        {
            AddBinaryCondition(GetKey(condition), classification, priority, id, isLockup);
        }

        /// <summary>
        ///     During startup, set up <see cref="ErrorInfo" /> object based on the given enum.
        /// </summary>
        /// <param name="value">An enum</param>
        /// <param name="defaultClassification">
        ///     The default <see cref="DisplayableMessageClassification" /> category.
        /// </param>
        /// <param name="defaultPriority">
        ///     The default <see cref="DisplayableMessagePriority" /> category.
        /// </param>
        /// <param name="isLockup">Whether to disable the system while this condition goes into effect (default false).</param>
        /// <param name="ignoreThis">Name of an enumeration value to ignore, if desired; default "None".</param>
        protected void ManageErrorEnum(Enum value,
            DisplayableMessageClassification defaultClassification,
            DisplayableMessagePriority defaultPriority,
            bool isLockup = false,
            string ignoreThis = "None")
        {
            if (value.ToString() == ignoreThis)
            {
                return;
            }

            var id = EnumHelper.GetAttribute<ErrorGuidAttribute>(value).Id;
            AddBinaryCondition(GetKey(value), defaultClassification, defaultPriority, id, isLockup);
        }

        /// <summary>
        ///     During startup, set up <see cref="ErrorInfo" /> object based on the given enum.
        /// </summary>
        /// <param name="value">An enum</param>
        /// <param name="defaultPriority">
        ///     The default <see cref="DisplayableMessagePriority" /> category for all values in this
        ///     enum.
        /// </param>
        /// <param name="isLockup">Whether to disable the system while this condition goes into effect (default false).</param>
        /// <param name="ignoreThis">Name of an enumeration value to ignore, if desired; default "None".</param>
        protected void ManageErrorEnum(Enum value,
            DisplayableMessagePriority defaultPriority,
            bool isLockup = false,
            string ignoreThis = "None")
        {
            if (value.ToString() == ignoreThis)
            {
                return;
            }

            var classification = EnumHelper.GetAttribute<ErrorGuidAttribute>(value).Classification;
            var id = EnumHelper.GetAttribute<ErrorGuidAttribute>(value).Id;
            AddBinaryCondition(GetKey(value), classification, defaultPriority, id, isLockup);
        }

        /// <summary>
        ///     During startup, set up <see cref="ErrorInfo" /> objects based on an enum.
        ///     Behavioral defaults are <see cref="DisplayableMessageClassification.HardError" />
        ///     and <see cref="DisplayableMessagePriority.Immediate" /> for lockups, and
        ///     <see cref="DisplayableMessageClassification.SoftError" /> and
        ///     <see cref="DisplayableMessagePriority.Normal" /> for non-lockups,
        ///     with no associated icon.
        ///     Property keys will have the format [enumType]_[enumValue] .
        ///     After this call, any individual value of the enum can be tweaked with
        /// </summary>
        /// <typeparam name="TEnumType">An enum</typeparam>
        /// <param name="defaultClassification">
        ///     The default <see cref="DisplayableMessageClassification" /> category for all values
        ///     in this enum.
        /// </param>
        /// <param name="defaultPriority">
        ///     The default <see cref="DisplayableMessagePriority" /> category for all values in this
        ///     enum.
        /// </param>
        /// <param name="isLockup">Whether to disable the system while this condition goes into effect (default false).</param>
        /// <param name="ignoreThis">Name of an enumeration value to ignore, if desired; default "None".</param>
        protected void ManageErrorEnum<TEnumType>(
            DisplayableMessageClassification defaultClassification,
            DisplayableMessagePriority defaultPriority,
            bool isLockup = false,
            string ignoreThis = "None")
            where TEnumType : Enum
        {
            foreach (var value in Enum.GetValues(typeof(TEnumType)).Cast<TEnumType>())
            {
                ManageErrorEnum(value, defaultClassification, defaultPriority, isLockup, ignoreThis);
            }
        }

        /// <summary>
        ///     During startup, set up <see cref="ErrorInfo" /> objects based on an enum.
        ///     Behavioral defaults are <see cref="DisplayableMessageClassification.HardError" />
        ///     and <see cref="DisplayableMessagePriority.Immediate" /> for lockups, and
        ///     <see cref="DisplayableMessageClassification.SoftError" /> and
        ///     <see cref="DisplayableMessagePriority.Normal" /> for non-lockups,
        ///     with no associated icon.
        ///     Property keys will have the format [enumType]_[enumValue] .
        ///     After this call, any individual value of the enum can be tweaked with
        /// </summary>
        /// <typeparam name="TEnumType">An enum</typeparam>
        /// <param name="defaultPriority">
        ///     The default <see cref="DisplayableMessagePriority" /> category for all values in this
        ///     enum.
        /// </param>
        /// <param name="isLockup">Whether to disable the system while this condition goes into effect (default false).</param>
        /// <param name="ignoreThis">Name of an enumeration value to ignore, if desired; default "None".</param>
        protected void ManageErrorEnum<TEnumType>(
            DisplayableMessagePriority defaultPriority,
            bool isLockup = false,
            string ignoreThis = "None")
            where TEnumType : Enum
        {
            foreach (var value in Enum.GetValues(typeof(TEnumType)).Cast<TEnumType>())
            {
                ManageErrorEnum(value, defaultPriority, isLockup, ignoreThis);
            }
        }

        /// <summary>
        ///     A fault causes an EGM lockup, but a warning doesn't.
        ///     It's stored for later clearing in either case.
        /// </summary>
        /// <param name="value">A single enumerated value.</param>
        /// <param name="delayedDisableKey">
        ///     If non-null/non-empty, this error won't disable until some later condition is met
        ///     (default null).
        /// </param>
        protected void AddFault(Enum value, string delayedDisableKey = null)
        {
            if (_faults.TryGetValue(GetKey(value), out var error))
            {
                StartCondition(error, delayedDisableKey);
            }
        }

        /// <summary>
        ///     Clear a previously-reported fault.
        /// </summary>
        /// <param name="value">A single enumerated value.</param>
        protected void ClearFault(Enum value)
        {
            if (_faults.TryGetValue(GetKey(value), out var error))
            {
                StopCondition(error);
            }
        }

        /// <summary>
        ///     Handle on/off for a binary condition
        /// </summary>
        /// <param name="key">Condition key</param>
        /// <param name="yes">True when condition is "on"</param>
        /// <param name="delayedDisableKey">
        ///     If non-null/non-empty, this error won't disable until some later condition is met
        ///     (default null).
        /// </param>
        protected void SetBinary(string key, bool yes, string delayedDisableKey = null)
        {
            if (_faults.TryGetValue(GetKey(key), out var error))
            {
                if (yes)
                {
                    StartCondition(error, delayedDisableKey);
                }
                else
                {
                    StopCondition(error);
                }
            }
        }

        /// <summary>
        ///     Turn on a binary condition, then automatically turn it off after an interval.
        /// </summary>
        /// <param name="key">Condition key</param>
        /// <param name="delayedDisableKey">
        ///     If non-null/non-empty, this error won't disable until some later condition is met
        ///     (default null).
        /// </param>
        /// <param name="msDelay">Interval in milliseconds after which to turn off condition</param>
        protected void SetBinaryOnWithTimeout(
            string key,
            string delayedDisableKey = null,
            int msDelay = DefaultDelayInterval)
        {
            SetBinary(key, true, delayedDisableKey);
            Task.Delay(msDelay).ContinueWith(_ => { SetBinary(key, false); });
        }

        /// <summary>
        ///     For some pending disabling ErrorInfos, let them do the disable now.
        /// </summary>
        /// <param name="delayedDisableKey">Delayed-disable key that identifies which ones to release</param>
        protected void PerformDelayedDisables(string delayedDisableKey)
        {
            _pendingFaults.ForEach(
                e =>
                {
                    e.DelayedDisableKey = null;
                    StartCondition(e, null);
                });
            _pendingFaults.Clear();
        }

        /// <summary>
        ///     Get a unique key from an enumerated value
        /// </summary>
        /// <param name="value">Enumerated value</param>
        /// <returns>Unique key</returns>
        protected string GetKey(Enum value)
        {
            return $"{value.GetType().Name}_{value}";
        }

        /// <summary>
        ///     Get a unique key from an enumerated value
        /// </summary>
        /// <typeparam name="TEnumType">Enumeration type</typeparam>
        /// <param name="value">Enumerated value</param>
        /// <returns>Unique key</returns>
        protected string GetKey<TEnumType>(object value)
        {
            return $"{typeof(TEnumType).Name}_{value}";
        }

        /// <summary>
        ///     Get a unique key for a binary condition
        /// </summary>
        /// <param name="condition">Name of condition</param>
        /// <returns>Unique key</returns>
        protected string GetKey(string condition)
        {
            return $"{DeviceName}_{condition}";
        }

        /// <summary>
        ///     Query if 'value' is set.
        /// </summary>
        /// <param name="value">A single enumerated value.</param>
        /// <returns>True if set, false if not.</returns>
        protected bool IsSet(Enum value)
        {
            return _faults.TryGetValue(GetKey(value), out var error) && _disableManager.CurrentDisableKeys.Contains(error.Guid);
        }

        /// <summary>
        ///     Query if 'condition' is set.
        /// </summary>
        /// <param name="condition">Name of condition</param>
        /// <returns>True if set, false if not.</returns>
        protected bool IsSet(string condition)
        {
            return _faults.TryGetValue(GetKey(condition), out var error) && _disableManager.CurrentDisableKeys.Contains(error.Guid);
        }

        private void AddBinaryCondition(
            string key,
            DisplayableMessageClassification classification,
            DisplayableMessagePriority priority,
            Guid id,
            bool isLockup)
        {
            var info = new ErrorInfo(key, classification, priority, id, isLockup);
            _faults.Add(key, info);
            _properties.Add(key, info.DisplayableMessage.Message);
        }

        private void StartCondition(ErrorInfo error, string delayedDisableKey)
        {
            if (error.IsActive)
            {
                return;
            }

            if (string.IsNullOrEmpty(delayedDisableKey))
            {
                error.IsActive = true;
                if (error.IsLockup)
                {
                    _disableManager.Disable(error.Guid, error.DisablePriority, error.DisplayableMessage.MessageResourceKey, CultureProviderType.Operator);
                }
                else
                {
                    _messageDisplay.DisplayMessage(error.DisplayableMessage);
                }
            }
            else
            {
                _pendingFaults.RemoveAll(e => e.Guid == error.Guid);

                error.DelayedDisableKey = delayedDisableKey;
                _pendingFaults.Add(error);
            }
        }

        private void StopCondition(ErrorInfo error)
        {
            if (!error.IsActive)
            {
                return;
            }

            error.IsActive = false;

            var numPending = _pendingFaults.RemoveAll(e => e == error);
            if (numPending == 0 && error.IsLockup)
            {
                // I don't think this is right
                _disableManager.Enable(error.Guid);
            }
            else
            {
                _messageDisplay.RemoveMessage(error.DisplayableMessage);
            }
        }

        private class ErrorInfo
        {
            public ErrorInfo(
                string key,
                DisplayableMessageClassification classification,
                DisplayableMessagePriority priority,
                Guid id,
                bool isLockup)
            {

                DisplayableMessage = new DisplayableMessage(
                    //() => Localizer.For(CultureFor.Operator).GetString(key, _ => { }),
                    key, CultureProviderType.Operator, classification, priority, null, id, null,
                    _ => { });

                IsActive = false;
                IsLockup = isLockup;
                DelayedDisableKey = null;
            }

            public IDisplayableMessage DisplayableMessage { get; }

            public bool IsActive { get; set; }

            public SystemDisablePriority DisablePriority
            {
                get
                {
                    switch (DisplayableMessage.Priority)
                    {
                        case DisplayableMessagePriority.Immediate:
                            return SystemDisablePriority.Immediate;
                        default:
                            return SystemDisablePriority.Normal;
                    }
                }
            }

            public bool IsLockup { get; }

            public Guid Guid => DisplayableMessage.Id;

            public string DelayedDisableKey { get; set; }
        }
    }
}