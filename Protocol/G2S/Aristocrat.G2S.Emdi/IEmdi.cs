namespace Aristocrat.G2S.Emdi
{
    using Monaco.Kernel;

    /// <summary>
    ///     Used to setup the environment for the display media host
    /// </summary>
    public interface IEmdi : IService
    {
        /// <summary>
        ///     Starts a host to communicate with media display content
        /// </summary>
        /// <param name="port">TCP/IP port</param>
        void Start(int port);

        /// <summary>
        ///     Shutdown all hosts
        /// </summary>
        void Stop();

        /// <summary>
        ///     Unload service resources.
        /// </summary>
        void Unload();
    }
}
