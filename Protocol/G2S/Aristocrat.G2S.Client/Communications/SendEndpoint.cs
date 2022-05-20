namespace Aristocrat.G2S.Client.Communications
{
    using System;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.Threading.Tasks;
    using Communicator.ServiceModel;
    using Diagnostics;
    using Properties;

    /// <summary>
    ///     Defines and instance of an ISendEndpoint.
    /// </summary>
    public class SendEndpoint : ISendEndpoint, IDisposable
    {
        private readonly X509Certificate2 _certificate;
        private readonly MessageBuilder _messageBuilder;

        private G2SClient _client;
        private bool _disposed;
        private bool _negotiated;
        private TransportOptionType _transportOptionType;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SendEndpoint" /> class using the provided address and an instance of
        ///     the MessageBuilder.
        /// </summary>
        /// <param name="address">The address to send to.</param>
        /// <param name="messageBuilder">An instance of the MessageBuilder.</param>
        /// <param name="certificate">The client certificate.</param>
        public SendEndpoint(Uri address, MessageBuilder messageBuilder, X509Certificate2 certificate)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            if (!EndpointUtilities.IsSchemeValid(address))
            {
                throw new ArgumentException(@"The Uri scheme is not valid it must be https or http.", nameof(address));
            }

            Address = address;
            _messageBuilder = messageBuilder ?? throw new ArgumentNullException(nameof(messageBuilder));
            _certificate = certificate;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public Uri Address { get; }

        /// <inheritdoc />
        public async Task<IPoint2PointAck> Send(ClassCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (!command.IClass.IsValid())
            {
                throw new InvalidOperationException(Resources.InvalidCommand);
            }

            await NegotiateTransportOption();

            string responseText;
            try
            {
                responseText = await SendMessage(command);

                SourceTrace.TraceVerbose(
                    G2STrace.Source,
                    @"SendEndpoint.Send : Response from Host
	EgmId : {0}
	HostId : {1}
    CommandId : {2}
	Message : {3}",
                    command.EgmId,
                    command.HostId,
                    command.CommandId,
                    _messageBuilder.FormatXml(responseText));
            }
            catch (Exception e)
            {
                SourceTrace.TraceError(
                    G2STrace.Source,
                    @"SendEndpoint.Send : Exception occurred while sending command
	EgmId : {0}
	HostId : {1}
	Exception : {2}",
                    command.EgmId,
                    command.HostId,
                    e);

                return null;
            }

            try
            {
                var messageResponse = _messageBuilder.DecodeMessage(responseText);

                return messageResponse.Item as IPoint2PointAck;
            }
            catch (Exception e)
            {
                SourceTrace.TraceError(
                    G2STrace.Source,
                    @"SendEndpoint.Send : Exception occurred while decoding response
	EgmId : {0}
	HostId : {1}
	Exception : {2}",
                    command.EgmId,
                    command.HostId,
                    e);
            }

            return null;
        }

        /// <inheritdoc />
        public void Reset()
        {
            Close();

            _client = new G2SClient(EndpointUtilities.ClientBinding(Address), new EndpointAddress(Address));

            if (_client.ClientCredentials != null)
            {
                _client.ClientCredentials.ClientCertificate.Certificate = _certificate;
            }

            _negotiated = false;
        }

        /// <inheritdoc />
        public void Close()
        {
            InternalClose();
        }

        /// <summary>
        ///     Disposes the endpoint.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _client.Close();
            }

            _client = null;

            _disposed = true;
        }

        private async Task<string> SendMessage(ClassCommand command)
        {
            var pointToPoint = _messageBuilder.BuildP2P(command.SerializableObject);
            pointToPoint.egmId = command.EgmId;
            pointToPoint.hostId = command.HostId;
            pointToPoint.dateTimeSent = DateTime.UtcNow;

            var message = _messageBuilder.BuildMessage(pointToPoint);

            if (!message.IsValid())
            {
                throw new InvalidOperationException(Resources.InvalidCommand);
            }

            var requestText = _messageBuilder.EncodeMessage(message);

            SourceTrace.TraceVerbose(
                G2STrace.Source,
                @"SendEndpoint.SendMessage : Sending from EGM
	EgmId : {0}
	HostId : {1}
    CommandId : {2}
	Message : {3}",
                command.EgmId,
                command.HostId,
                command.CommandId,
                requestText);

            string result;

            //TODO: If and when we support Gzip on the transport the negotiation will need to be overridden/skipped, since we won't want to do payload compression
            if (_transportOptionType == TransportOptionType.GZIP_PAYLOAD)
            {
                result = await _client.SendMessage(command.EgmId, command.HostId, GzipUtilities.Zip(requestText));
            }
            else
            {
                result = await _client.SendMessage(command.EgmId, command.HostId, requestText);
            }

            SourceTrace.TraceVerbose(
                G2STrace.Source,
                @"SendEndpoint.SendMessage : Sent from EGM
	EgmId : {0}
	HostId : {1}
    CommandId : {2}",
                command.EgmId,
                command.HostId,
                command.CommandId);

            return result;
        }

        private void InternalClose()
        {
            if (_client == null)
            {
                return;
            }

            if (_client.State < CommunicationState.Closing)
            {
                _client.Close();
            }
            else
            {
                _client.Abort();
            }
        }

        private async Task NegotiateTransportOption()
        {
            if (_negotiated)
            {
                return;
            }

            _negotiated = true;

            try
            {
                var response = await _client.TransportOptions(new TransportOptionsRequest(@"1.0"));

                _transportOptionType = response.TransportResponse.Options.Contains(TransportOptionType.GZIP_PAYLOAD)
                    ? TransportOptionType.GZIP_PAYLOAD
                    : TransportOptionType.NO_GZIP;
            }
            catch (Exception e)
            {
                SourceTrace.TraceError(
                    G2STrace.Source,
                    @"SendEndpoint.NegotiateTransportOption : Failed to negotiate transport options",
                    e);
            }
        }
    }
}