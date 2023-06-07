namespace Aristocrat.Monaco.G2S
{
    using System;
    using Aristocrat.G2S.Client;
    using Aristocrat.Monaco.G2S.Services.Progressive;

    /// <summary>
    ///     Initializes and starts a service.
    /// </summary>
    public interface IEngine : IDisposable
    {
        /// <summary>
        ///     Initializes and starts the engine.
        /// </summary>
        /// <param name="context">The startup context.</param>
        /// <param name="onReady">An optional callback that will be called when the engine has completed initialization</param>
        void Start(IStartupContext context, Action onReady = null);

        /// <summary>
        ///     Stops the engine.
        /// </summary>
        void Stop();

        /// <summary>
        ///     Stops, initializes, and starts the engine.
        /// </summary>
        /// <param name="context">The startup context.</param>
        void Restart(IStartupContext context);

        /// <summary>
        /// Create any needed IProgressiveDevices
        /// </summary>
        /// <param name="progressiveDeviceManager"></param>
        void AddProgressiveDevices(IProgressiveDeviceManager progressiveDeviceManager);
    }
}