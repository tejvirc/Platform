namespace Aristocrat.G2S.Emdi.Host
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Manages the display media hosts
    /// </summary>
    public interface IHostQueue : IEnumerable<IHost>
    {
        /// <summary>
        /// Start a display media host
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        Task StartAsync(int port);

        /// <summary>
        /// Stop a display media host
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        Task StopAsync(int port);

        /// <summary>
        /// Stop all display media hosts
        /// </summary>
        /// <returns></returns>
        Task StopAllAsync();

        /// <summary>
        /// Gets the host for the specified port
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        IHost this[int port] { get; }
    }
}
