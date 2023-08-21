namespace Aristocrat.G2S.Client.Communications
{
    using System;
    using System.Collections.Concurrent;
    using System.Security.Cryptography.X509Certificates;

    /// <inheritdoc />
    internal class SendEndpointProvider : ISendEndpointProvider
    {
        private readonly X509Certificate2 _certificate;

        private readonly ConcurrentDictionary<int, ISendEndpoint> _endpoints =
            new ConcurrentDictionary<int, ISendEndpoint>();

        private readonly MessageBuilder _messageBuilder;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SendEndpointProvider" /> class.
        /// </summary>
        /// <param name="messageBuilder">A message builder instance</param>
        /// <param name="certificate">The client certificate.</param>
        public SendEndpointProvider(MessageBuilder messageBuilder, X509Certificate2 certificate)
        {
            _messageBuilder = messageBuilder;
            _certificate = certificate;
        }

        /// <inheritdoc />
        public ISendEndpoint GetOrAddEndpoint(int hostId, Uri address)
        {
            return _endpoints.GetOrAdd(hostId, new SendEndpoint(address, _messageBuilder, _certificate));
        }

        /// <inheritdoc />
        public ISendEndpoint GetOrUpdateEndpoint(int hostId, Uri address)
        {
            return _endpoints.AddOrUpdate(
                hostId,
                key => new SendEndpoint(address, _messageBuilder, _certificate),
                (key, current) =>
                {
                    if (address == current.Address)
                    {
                        return current;
                    }

                    current.Close();
                    return new SendEndpoint(address, _messageBuilder, _certificate);
                });
        }

        /// <inheritdoc />
        public ISendEndpoint GetEndpoint(int hostId)
        {
            return !_endpoints.TryGetValue(hostId, out var endpoint) ? null : endpoint;
        }
    }
}