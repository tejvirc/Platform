namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.MessageDisplay;
    using log4net;
    using MessageDisplay;
    using Mono.Addins.Localization;

    /// <summary>
    ///     Definition of the SystemDisableManager class. This class handles matching system disable/enable commands.
    /// </summary>
    public sealed class SystemDisableManager : IService, ISystemDisableManager, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<Guid, (SystemDisablePriority priority, Func<string> reason, bool affectsIdle, CancellationTokenSource cts)> _systemDisables
                = new Dictionary<Guid, (SystemDisablePriority priority, Func<string> reason, bool affectsIdle, CancellationTokenSource cts)>();

        private ReaderWriterLockSlim _stateLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private IMessageDisplay _messageDisplay;
        private IEventBus _eventBus;

        private bool _disposed;

        /// <summary>
        ///     Finalizes an instance of the <see cref="SystemDisableManager" /> class.
        /// </summary>
        ~SystemDisableManager()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public string Name => nameof(SystemDisableManager);

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ISystemDisableManager) };

        /// <inheritdoc />
        public void Initialize()
        {
            _messageDisplay = ServiceManager.GetInstance().GetService<IMessageDisplay>();
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            Logger.Debug("Initialized");
        }

        /// <inheritdoc />
        public bool IsDisabled
        {
            get
            {
                _stateLock.EnterReadLock();
                try
                {
                    return _systemDisables.Count != 0;
                }
                finally
                {
                    _stateLock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc />
        public bool IsIdleStateAffected
        {
            get
            {
                _stateLock.EnterReadLock();
                try
                {
                    return _systemDisables.Any(d => d.Value.affectsIdle);
                }
                finally
                {
                    _stateLock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<Guid> CurrentDisableKeys
        {
            get
            {
                _stateLock.EnterReadLock();
                try
                {
                    return _systemDisables.Keys.ToList();
                }
                finally
                {
                    _stateLock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<Guid> CurrentImmediateDisableKeys
        {
            get
            {
                _stateLock.EnterReadLock();
                try
                {
                    return _systemDisables
                        .Where(d => d.Value.priority == SystemDisablePriority.Immediate).Select(d => d.Key).ToList();
                }
                finally
                {
                    _stateLock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc />
        public bool DisableImmediately => CurrentImmediateDisableKeys.Any();

        // Done except following classes, not being to change as messages are either too complicated or from protocol/device
        // LegitimacyLockUpMonitor has hardcoded messages,
        // SerialGatService
        // SASBase
        // ReserverOverlayViewModel
        // InformedPlayerService
        // SetCabinetState
        // BingoDisableProvider
        // HorseAnimationLauncher
        /// <inheritdoc />
        public void Disable(Guid enableKey, SystemDisablePriority priority, Func<string> disableReason, Type type = null)
        {
            Disable(enableKey, priority, disableReason, TimeSpan.Zero, type);
        }

        //Done, remaining are tests
        /// <inheritdoc />
        public void Disable(Guid enableKey, SystemDisablePriority priority, Func<string> disableReason, bool affectsIdleState, Type type = null)
        {
            Disable(enableKey, priority, disableReason, TimeSpan.Zero, affectsIdleState, type);
        }

        /// <inheritdoc />
        public void Disable(Guid enableKey, SystemDisablePriority priority, string disableReasonResourceKey, CultureProviderType providerType, params object[] msgParams)
        {
            Disable(enableKey, priority, disableReasonResourceKey, providerType, true, null, null, msgParams);
        }

        /// <inheritdoc />
        public void Disable(Guid enableKey, SystemDisablePriority priority, string disableReasonResourceKey, CultureProviderType providerType, bool affectsIdleState=true, Func<string> helpText = null, Type type = null, params object[] msgParams)
        {
            Disable(enableKey, priority, null, TimeSpan.Zero, affectsIdleState, type, helpText, disableReasonResourceKey, providerType, msgParams);
        }

        //Done, remaining are tests
        /// <inheritdoc />
        public void Disable(Guid enableKey, SystemDisablePriority priority, Func<string> disableReason, TimeSpan duration, Type type = null)
        {
            Disable(enableKey, priority, disableReason, duration, true, type);
        }

        //TODO EgmState
        // HorseAnimationLauncher won't change, too complicated with nested resource string
        // The rest are tests
        /// <inheritdoc />
        public void Disable(Guid enableKey, SystemDisablePriority priority, Func<string> disableReason, bool affectsIdleState, Func<string> helpText, Type type = null)
        {
            Disable(enableKey, priority, disableReason, TimeSpan.Zero, affectsIdleState, type, helpText);
        }

        // Done
        /// <inheritdoc />
        public void Disable(Guid enableKey, SystemDisablePriority priority, Func<string> disableReason, TimeSpan duration, bool affectsIdleState, Type type = null, Func<string> helpText = null, string messageResourceKey = null, CultureProviderType? providerType=null, params object[] msgParams)
        {
            _stateLock.EnterWriteLock();

            DisplayableMessage message = null;
            string disableReasonText = disableReason?.Invoke();
            var messagePriority = priority == SystemDisablePriority.Immediate
                ? DisplayableMessagePriority.Immediate
                : DisplayableMessagePriority.Normal;
            if (!string.IsNullOrEmpty(disableReasonText))
            {
                message = new DisplayableMessage(
                    disableReason,
                    DisplayableMessageClassification.HardError,
                    messagePriority,
                    type,
                    enableKey,
                    helpText);
            }
            else if (!string.IsNullOrEmpty(messageResourceKey))
            {
                message = new DisplayableMessage(
                    messageResourceKey,
                    providerType ?? CultureProviderType.Operator,
                    DisplayableMessageClassification.HardError,
                    messagePriority,
                    type,
                    enableKey);
                if (msgParams != null)
                {
                    message.Params = msgParams;
                }
                disableReasonText = message.Message;
            }

            try
            {
                var disabling = !IsDisabled || !DisableImmediately && priority == SystemDisablePriority.Immediate;

                if (_systemDisables.TryGetValue(enableKey, out var current))
                {
                    SafeDispose(current.cts);

                    var systemIdleStateAffected = IsIdleStateAffected || affectsIdleState;

                    _systemDisables[enableKey] = CreateDisableItem(
                        enableKey,
                        priority,
                        () => disableReasonText,
                        affectsIdleState,
                        duration);

                    PostEvent(
                        new SystemDisableUpdatedEvent(
                            priority,
                            enableKey,
                            disableReason?.Invoke(),
                            systemIdleStateAffected));
                }
                else
                {
                    var systemIdleStateAffected = IsIdleStateAffected || affectsIdleState;

                    _systemDisables.Add(
                        enableKey,
                        CreateDisableItem(enableKey, priority, () => disableReasonText, affectsIdleState, duration));

                    PostEvent(
                        new SystemDisableAddedEvent(
                            priority,
                            enableKey,
                            disableReason?.Invoke(),
                            systemIdleStateAffected));
                }

                if (disabling)
                {
                    PostEvent(new SystemDisabledEvent(priority));
                }

                Logger.Debug(
                    $"Disabled: key={enableKey}, priority={priority}, disabling={disabling}, affectsIdleState={affectsIdleState}, reason={disableReason?.Invoke()}, helpText={helpText?.Invoke()}");
                LogCurrentLockups();
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }

            if (!string.IsNullOrEmpty(disableReasonText))
            {
                _messageDisplay.DisplayMessage(message);
            }

        }

        /// <inheritdoc />
        public void Disable(Guid guid, SystemDisablePriority disablePriority, IDisplayableMessage displayableMessage)
        {
            if (displayableMessage.MessageResourceKey != null)
            {
                Disable(guid, disablePriority, displayableMessage.MessageResourceKey, displayableMessage.CultureProvider);
            }
            else if (displayableMessage.MessageCallback != null)
            {
                Disable(guid, disablePriority, displayableMessage.MessageCallback);
            }
            else
            {
                throw new Exception("Invalid displayable message.");
            }
        }

        /// <inheritdoc />
        public void Enable(Guid enableKey)
        {
            Logger.Debug($"Enabling: key={enableKey}");

            _stateLock.EnterWriteLock();
            try
            {
                if (!_systemDisables.TryGetValue(enableKey, out var deleted) || !_systemDisables.Remove(enableKey))
                {
                    return;
                }

                SafeDispose(deleted.cts);

                PostEvent(
                    new SystemDisableRemovedEvent(
                        deleted.priority,
                        enableKey,
                        deleted.reason?.Invoke(),
                        IsDisabled,
                        IsIdleStateAffected));

                if (!IsDisabled)
                {
                    PostEvent(new SystemEnabledEvent());
                }
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }

            _messageDisplay.RemoveMessage(enableKey);

            Logger.Debug($"Enabled: key={enableKey}");
            LogCurrentLockups();
        }

        private void PostEvent<T>(T eventToPost)
            where T : IEvent
        {
            _eventBus.Publish(eventToPost);
        }

        private static void SafeDispose(CancellationTokenSource cts)
        {
            if (cts == null)
            {
                return;
            }

            try
            {
                if (cts.Token.CanBeCanceled)
                {
                    cts.Cancel();
                }

                cts.Dispose();
            }
            catch (ObjectDisposedException e)
            {
                Logger.Warn("CancellationTokenSource has been disposed", e);
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _stateLock.EnterWriteLock();
                try
                {
                    foreach (var systemDisable in _systemDisables)
                    {
                        SafeDispose(systemDisable.Value.cts);
                    }

                    _systemDisables.Clear();
                }
                finally
                {
                    _stateLock.ExitWriteLock();
                }

                _stateLock.Dispose();
            }

            _stateLock = null;

            _disposed = true;
        }

        private (SystemDisablePriority priority, Func<string> reason, bool affectsIdle, CancellationTokenSource cts)
            CreateDisableItem(
                Guid enableKey,
                SystemDisablePriority priority,
                Func<string> disableReason,
                bool affectsIdleState,
                TimeSpan duration)
        {
            if (duration == TimeSpan.Zero)
            {
                return (priority, disableReason, affectsIdleState, null);
            }

            var item = (priority, disableReason, affectsIdleState, cts: new CancellationTokenSource());

            Task.Delay(duration, item.cts.Token).ContinueWith(
                task =>
                {
                    if (!task.IsCanceled)
                    {
                        Enable(enableKey);
                    }
                });

            return item;
        }

        private void LogCurrentLockups()
        {
            _stateLock.EnterReadLock();
            try
            {
                Logger.Debug($"Current lockups: {string.Join(", ", _systemDisables.Keys)}");
            }
            finally
            {
                _stateLock.ExitReadLock();
            }
        }
    }
}