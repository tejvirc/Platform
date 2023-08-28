namespace Aristocrat.G2S.Client
{
    using System;
    using Aristocrat.G2S.Client.Devices;

    /// <summary>
    ///     Provides a mechanism to manage the current state of the egm by prioritizing the disable conditions, locks, cabinet
    ///     states.
    /// </summary>
    public interface IEgmStateManager
    {
        /// <summary>
        ///     Gets a value indicating whether the egm is disabled due to a tracked <see cref="EgmState" />
        /// </summary>
        bool HasLock { get; }

        /// <summary>
        ///     Applies the condition and disables the Egm.
        /// </summary>
        /// <param name="device">An <see cref="IDevice" /> instance.</param>
        /// <param name="state">The state to be applied.</param>
        /// <param name="immediate">
        ///     True to apply the condition immediately, otherwise the condition will be applied based on
        ///     priority when the gameplay state is idle.
        /// </param>
        /// <param name="message">The message to be displayed.</param>
        /// <returns>The generated key used to lock the Egm.</returns>
        Guid Disable(IDevice device, EgmState state, bool immediate, Func<string> message);

        /// <summary>
        ///     Applies the condition and disables the Egm.
        /// </summary>
        /// <param name="device">An <see cref="IDevice" /> instance.</param>
        /// <param name="state">The state to be applied.</param>
        /// <param name="immediate">
        ///     True to apply the condition immediately, otherwise the condition will be applied based on
        ///     priority when the gameplay state is idle.
        /// </param>
        /// <param name="message">The message to be displayed.</param>
        /// <returns>The generated key used to lock the Egm.</returns>
        /// <param name="affectsIdleState">true (the default) if this disable state affects the cabinet idle state.</param>
        Guid Disable(IDevice device, EgmState state, bool immediate, Func<string> message, bool affectsIdleState);

        /// <summary>
        ///     Applies the condition and disables the Egm.
        /// </summary>
        /// <param name="disableKey">The key used to disable the Egm.</param>
        /// <param name="device">An <see cref="IDevice" /> instance.</param>
        /// <param name="state">The state to be applied.</param>
        /// <param name="immediate">
        ///     True to apply the condition immediately, otherwise the condition will be applied based on
        ///     priority when the gameplay state is idle.
        /// </param>
        /// <param name="message">The message to be displayed.</param>
        void Disable(Guid disableKey, IDevice device, EgmState state, bool immediate, Func<string> message);

        /// <summary>
        ///     Applies the condition and disables the Egm.
        /// </summary>
        /// <param name="disableKey">The key used to disable the Egm.</param>
        /// <param name="device">An <see cref="IDevice" /> instance.</param>
        /// <param name="state">The state to be applied.</param>
        /// <param name="immediate">
        ///     True to apply the condition immediately, otherwise the condition will be applied based on
        ///     priority when the gameplay state is idle.
        /// </param>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="affectsIdleState">true (the default) if this disable state affects the cabinet idle state.</param>
        void Disable(
            Guid disableKey,
            IDevice device,
            EgmState state,
            bool immediate,
            Func<string> message,
            bool affectsIdleState);

        /// <summary>
        ///     Applies the condition and locks the Egm.
        /// </summary>
        /// <param name="device">An <see cref="IDevice" /> instance.</param>
        /// <param name="state">The state to be applied.</param>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="duration">The duration of the lock.</param>
        void Lock(IDevice device, EgmState state, Func<string> message, TimeSpan duration);

        /// <summary>
        ///     Applies the condition and locks the Egm.
        /// </summary>
        /// <param name="device">An <see cref="IDevice" /> instance.</param>
        /// <param name="state">The state to be applied.</param>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="duration">The duration of the lock.</param>
        /// <param name="onUnlock">Callback invoked when the lock is removed after the duration lapses.</param>
        void Lock(IDevice device, EgmState state, Func<string> message, TimeSpan duration, Action onUnlock);

        /// <summary>
        ///     Removes the condition and attempts to re-enable the Egm.
        /// </summary>
        /// <param name="device">An <see cref="IDevice" /> instance.</param>
        /// <param name="state">The state to be applied.</param>
        /// <returns>The generated key used to lock the Egm.</returns>
        Guid Enable(IDevice device, EgmState state);

        /// <summary>
        ///     Removes the condition and attempts to re-enable the Egm.
        /// </summary>
        /// <param name="disableKey">The key used to disable the Egm.</param>
        /// <param name="device">An <see cref="IDevice" /> instance.</param>
        /// <param name="state">The state to be applied.</param>
        void Enable(Guid disableKey, IDevice device, EgmState state);
    }
}