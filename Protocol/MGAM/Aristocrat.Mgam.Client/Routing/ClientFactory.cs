namespace Aristocrat.Mgam.Client.Routing
{
    using System.Net;
    using Monaco.Protocol.Common.Communication;

    /// <summary>
    ///     Creates communication client instances.
    /// </summary>
    internal class ClientFactory : IClientFactory
    {
        /// <inheritdoc />
        public ISecureClient CreateSecureClient(SslContext context, IPEndPoint endPoint)
        {
            return new SecureClient(context, endPoint);
        }

        /// <inheritdoc />
        public IBroadcastClient CreateBroadcastClient(IPEndPoint endPoint)
        {
            return new BroadcastClient(endPoint);
        }
    }
}
