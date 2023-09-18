namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using log4net;

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

        /// <inheritdoc />
        public void Disable(Guid enableKey, SystemDisablePriority priority, Func<string> disableReason, Type type = null)
        {
            Disable(enableKey, priority, disableReason, TimeSpan.Zero, type);
        }

        /// <inheritdoc />
        public void Disable(Guid enableKey, SystemDisablePriority priority, bool resourceKeyOnly, Func<string> disableReason, Type type = null)
        {
            Disable(enableKey, priority, disableReason, TimeSpan.Zero, type, resourceKeyOnly);
        }

        /// <inheritdoc />
        public void Disable(Guid enableKey, SystemDisablePriority priority, Func<string> disableReason, bool affectsIdleState, Type type = null)
        {
            Disable(enableKey, priority, disableReason, TimeSpan.Zero, affectsIdleState, type);
        }

        /// <inheritdoc />
        public void Disable(Guid enableKey, SystemDisablePriority priority, Func<string> disableReason, TimeSpan duration, Type type = null, bool resourceKeyOnly = false)
        {
            Disable(enableKey, priority, disableReason, duration, true, type, resourceKeyOnly:resourceKeyOnly);
        }

        /// <inheritdoc />
        public void Disable(Guid enableKey, SystemDisablePriority priority, Func<string> disableReason, bool affectsIdleState, Func<string> helpText, Type type = null)
        {
            Disable(enableKey, priority, disableReason, TimeSpan.Zero, affectsIdleState, type, helpText);
        }

        /// <inheritdoc />
        public void Disable(Guid enableKey, SystemDisablePriority priority, Func<string> disableReason, TimeSpan duration, bool affectsIdleState, Type type = null, Func<string> helpText = null, bool resourceKeyOnly = false)
        {
            _stateLock.EnterWriteLock();
            try
            {
                var disabling = !IsDisabled || !DisableImmediately && priority == SystemDisablePriority.Immediate;
                var disableMessage = disableReason?.Invoke();

                if (_systemDisables.TryGetValue(enableKey, out var current))
                {
                    SafeDispose(current.cts);

                    var systemIdleStateAffected = IsIdleStateAffected || affectsIdleState;

                    _systemDisables[enableKey] = CreateDisableItem(
                        enableKey,
                        priority,
                        disableReason,
                        affectsIdleState,
                        duration);

                    PostEvent(
                        new SystemDisableUpdatedEvent(
                            priority,
                            enableKey,
                            disableMessage,
                            systemIdleStateAffected));
                }
                else
                {
                    var systemIdleStateAffected = IsIdleStateAffected || affectsIdleState;

                    _systemDisables.Add(
                        enableKey,
                        CreateDisableItem(enableKey, priority, disableReason, affectsIdleState, duration));

                    PostEvent(
                        new SystemDisableAddedEvent(
                            priority,
                            enableKey,
                            disableMessage,
                            systemIdleStateAffected));
                }

                if (disabling)
                {
                    PostEvent(new SystemDisabledEvent(priority));
                }

                Logger.Debug(
                    $"Disabled: key={enableKey}, priority={priority}, disabling={disabling}, affectsIdleState={affectsIdleState}, reason={disableMessage}, helpText={helpText?.Invoke()}");
                LogCurrentLockups();
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }

            if (!string.IsNullOrEmpty(disableReason?.Invoke()))
            {
                var messagePriority = priority == SystemDisablePriority.Immediate
                    ? DisplayableMessagePriority.Immediate
                    : DisplayableMessagePriority.Normal;

                var message = new DisplayableMessage(
                    disableReason,
                    DisplayableMessageClassification.HardError,
                    messagePriority,
                    type,
                    enableKey,
                    helpText,
                    resourceKeyOnly);

                _messageDisplay.DisplayMessage(message);
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