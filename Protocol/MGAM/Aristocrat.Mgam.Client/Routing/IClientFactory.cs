namespace Aristocrat.Mgam.Client.Routing
{
    using System.Net;
    using Monaco.Protocol.Common.Communication;

    /// <summary>
    ///     Creates communication client instances.
    /// </summary>
    internal interface IClientFactory
    {
        /// <summary>
        ///     Creates an <see cref="SslClient"/> instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        ISecureClient CreateSecureClient(SslContext context, IPEndPoint endPoint);

        /// <summary>
        ///     Creates an <see cref="UdpClient"/> instance.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        IBroadcastClient CreateBroadcastClient(IPEndPoint endPoint);
    }
}
