namespace Aristocrat.G2S.Client
{
    using System.Collections.Generic;

    /// <summary>
    ///     Defines a contract for a controlling a client
    /// </summary>
    public interface IClientControl
        : IClient
    {
        /// <summary>
        ///     Start listening and tell all devices we are starting up.
        /// </summary>
        /// <param name="contexts">Startup data provided to the devices.</param>
        void Start(IEnumerable<IStartupContext> contexts);

        /// <summary>
        ///     Stop listening and tell all devices we are closing down.
        /// </summary>
        void Stop();

        /// <summary>
        ///     Restarts the hosts in the provided context list.
        /// </summary>
        /// <param name="contexts">Startup data provided to the devices.</param>
        void Restart(IEnumerable<IStartupContext> contexts);
    }
}