namespace Aristocrat.Monaco.Mgam
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    ///     Initializes and starts a service.
    /// </summary>
    public interface IEngine
    {
        /// <summary>
        ///     Initializes and starts the engine.
        /// </summary>
        /// <param name="context">The startup context.</param>
        /// <param name="onReady">An optional callback that will be called when the engine has completed initialization</param>
        Task Start(IStartupContext context, Action onReady = null);

        /// <summary>
        ///     Stops the engine.
        /// </summary>
        Task Stop();

        /// <summary>
        ///     Stops, initializes, and starts the engine.
        /// </summary>
        /// <param name="context">The startup context.</param>
        Task Restart(IStartupContext context);
    }
}
