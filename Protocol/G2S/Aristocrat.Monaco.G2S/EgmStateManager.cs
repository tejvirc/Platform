namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Application.Contracts.Localization;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Kernel;
    using Kernel.Contracts.MessageDisplay;
    using Localization.Properties;

    /// <summary>
    ///     An implementation of <see cref="IEgmStateManager" />
    /// </summary>
    public class EgmStateManager : IEgmStateManager, IDisposable
    {
        private readonly ISystemDisableManager _disableManager;

        private readonly ConcurrentDictionary<Tuple<int, string, EgmState>, Guid> _disableStates =
            new ConcurrentDictionary<Tuple<int, string, EgmState>, Guid>();

        private readonly IG2SEgm _egm;
        private readonly IEventBus _eventBus;

        private readonly ConcurrentDictionary<Tuple<int, string, EgmState>, Tuple<Guid, Action>> _trackedConditions =
            new ConcurrentDictionary<Tuple<int, string, EgmState>, Tuple<Guid, Action>>();

        private readonly ConcurrentDictionary<Guid, EgmState> _trackedStates =
            new ConcurrentDictionary<Guid, EgmState>();

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EgmStateManager" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="disableManager">An <see cref="ISystemDisableManager" /> instance.</param>
        /// <param name="eventBus">An <see cref="IEventBus" /> instance.</param>
        public EgmStateManager(
            IG2SEgm egm,
            ISystemDisableManager disableManager,
            IEventBus eventBus)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _eventBus.Subscribe<SystemDisableRemovedEvent>(this, Handle);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public bool HasLock => _trackedStates.Values.Any(s => s == EgmState.HostDisabled || s == EgmState.HostLocked);

        /// <inheritdoc />
        public Guid Disable(IDevice device, EgmState state, bool immediate, Func<string> message)
        {
            return Disable(device, state, immediate, message, true);
        }

        /// <inheritdoc />
        public Guid Disable(IDevice device, EgmState state, bool immediate, Func<string> message, bool affectsIdleState)
        {
            var key = GenerateKey(device, state);

            return _disableStates.AddOrUpdate(
                key,
                k =>
                {
                    var disableKey = Guid.NewGuid();

                    Disable(disableKey, device, state, immediate, message, affectsIdleState);

                    return disableKey;
                },
                (k, existing) =>
                {
                    Disable(existing, device, state, immediate, message, affectsIdleState);

                    return existing;
                });
        }

        /// <inheritdoc />
        public void Disable(Guid disableKey, IDevice device, EgmState state, bool immediate, Func<string> message)
        {
            Disable(disableKey, device, state, immediate, message, true);
        }

        /// <inheritdoc />
        public void Disable(
            Guid disableKey,
            IDevice device,
            EgmState state,
            bool immediate,
            Func<string> message,
            bool affectsIdleState)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (!Enum.IsDefined(typeof(EgmState), state))
            {
                throw new InvalidEnumArgumentException(nameof(state), (int)state, typeof(EgmState));
            }

            if (state == EgmState.Enabled)
            {
                throw new ArgumentException(@"The Enabled state cannot be specified.", nameof(state));
            }

            Func<string> helpTextCallback = null;
            if (device is ICabinetDevice && state == EgmState.HostDisabled)
            {
                helpTextCallback = () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoG2SHostDisabled);
            }

            if (string.IsNullOrEmpty(message?.Invoke()))
            {
                var msgResourceKey = ResourceKeys.DisabledByDevice;
                var msgParams = new object[] { device.DeviceClass, device.Id };
                _disableManager.Disable(
                    disableKey,
                    immediate ? SystemDisablePriority.Immediate : SystemDisablePriority.Normal,
                    msgResourceKey,
                    CultureProviderType.Player,
                    affectsIdleState,
                    helpTextCallback,
                    null,
                    msgParams);
            }
            else
            {
                _disableManager.Disable(
                    disableKey,
                    immediate ? SystemDisablePriority.Immediate : SystemDisablePriority.Normal,
                    message,
                    affectsIdleState,
                    helpTextCallback);
            }

            _trackedStates.TryAdd(disableKey, state);

            AddCondition(device, state);
        }

        /// <inheritdoc />
        public void Lock(IDevice device, EgmState state, Func<string> message, TimeSpan duration)
        {
            Lock(device, state, message, duration, null);
        }

        /// <inheritdoc />
        public void Lock(
            IDevice device,
            EgmState state,
            Func<string> message,
            TimeSpan duration,
            Action onUnlock)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (!Enum.IsDefined(typeof(EgmState), state))
            {
                throw new InvalidEnumArgumentException(nameof(state), (int)state, typeof(EgmState));
            }

            if (state == EgmState.Enabled)
            {
                throw new ArgumentException(@"The Enabled state cannot be specified.", nameof(state));
            }

            var key = GenerateKey(device, state);

            _trackedConditions.AddOrUpdate(
                key,
                k =>
                {
                    var lockKey = Guid.NewGuid();

                    var value = Tuple.Create(lockKey, onUnlock);

                    _disableStates.TryAdd(key, lockKey);

                    _disableManager.Disable(
                        lockKey,
                        SystemDisablePriority.Normal,
                        message,
                        duration,
                        false);

                    AddCondition(device, state);

                    return value;
                },
                (k, existing) =>
                {
                    var lockKey = existing.Item1;

                    _disableManager.Disable(
                        lockKey,
                        SystemDisablePriority.Normal,
                        message,
                        duration,
                        false);

                    return existing;
                });
        }

        /// <inheritdoc />
        public Guid Enable(IDevice device, EgmState state)
        {
            var key = GenerateKey(device, state);

            if (_disableStates.TryRemove(key, out var disableKey))
            {
                Enable(disableKey, device, state);
            }

            return disableKey;
        }

        /// <inheritdoc />
        public void Enable(Guid disableKey, IDevice device, EgmState state)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (!Enum.IsDefined(typeof(EgmState), state))
            {
                throw new InvalidEnumArgumentException(nameof(state), (int)state, typeof(EgmState));
            }

            if (state == EgmState.Enabled)
            {
                throw new ArgumentException(@"The Enabled state cannot be specified.", nameof(state));
            }

            _trackedConditions.TryRemove(GenerateKey(device, state), out _);

            _disableManager.Enable(disableKey);

            _trackedStates.TryRemove(disableKey, out _);

            RemoveCondition(device, state);
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);

                foreach (var disableKey in _disableStates.Values)
                {
                    _disableManager.Enable(disableKey);
                }

                _disableStates.Clear();
            }

            _disposed = true;
        }

        private static Tuple<int, string, EgmState> GenerateKey(IDevice device, EgmState state)
        {
            return Tuple.Create(device.Id, device.DeviceClass, state);
        }

        private void Handle(SystemDisableRemovedEvent data)
        {
            var condition = _trackedConditions.FirstOrDefault(c => c.Value.Item1 == data.DisableId);

            var key = condition.Key;
            if (key != null)
            {
                _trackedConditions.TryRemove(key, out var trackedItem);

                trackedItem?.Item2?.Invoke();

                _disableStates?.TryRemove(key, out var _);

                var cabinet = _egm.GetDevice<ICabinetDevice>();

                var device = _egm.GetDevice(key.Item2, key.Item1);

                if (device != null)
                {
                    cabinet.RemoveCondition(device, key.Item3);
                }
            }
        }

        private void AddCondition(IDevice device, EgmState state)
        {
            var cabinet = _egm.GetDevice<ICabinetDevice>();

            cabinet?.AddCondition(device, state);
        }

        private void RemoveCondition(IDevice device, EgmState state)
        {
            var cabinet = _egm.GetDevice<ICabinetDevice>();

            cabinet?.RemoveCondition(device, state);
        }
    }
}
