namespace Aristocrat.G2S.Emdi.Host
{
    using System;
    using Protocol.v21ext1b1;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface to management communication with the media display content
    /// </summary>
    public interface IHost : IDisposable
    {
        /// <summary>
        /// Start listening for connections from media display content
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        Task StartAsync(HostConfiguration config);

        /// <summary>
        /// Stop listening for connections from media display content
        /// </summary>
        /// <returns></returns>
        Task StopAsync();

        /// <summary>
        /// Send a command to media display content
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        Task<TResponse> SendCommandAsync<TGroup, TCommand, TResponse>(TCommand command)
            where TGroup : c_baseClass, new()
            where TCommand : c_baseCommand
            where TResponse : c_baseCommand;
    }
}
