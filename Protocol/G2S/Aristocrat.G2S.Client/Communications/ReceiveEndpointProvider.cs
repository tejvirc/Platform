namespace Aristocrat.G2S.Client.Communications
{
    using System;
    using System.Configuration;
    using System.Xml;
    using Communicator.ServiceModel;
    using Diagnostics;

    /// <summary>
    ///     Defines and instance of an IReceiveEndpointProvider.
    /// </summary>
    public class ReceiveEndpointProvider : IReceiveEndpointProvider
    {
        private readonly MessageBuilder _messageBuilder;
        private IMessageConsumer _consumer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReceiveEndpointProvider" /> class.
        ///     Constructs a new instance using a MessageBuilder instance.
        /// </summary>
        /// <param name="messageBuilder">An instance of the MessageBuilder.</param>
        public ReceiveEndpointProvider(MessageBuilder messageBuilder)
        {
            _messageBuilder = messageBuilder ?? throw new ArgumentNullException(nameof(messageBuilder));
        }

        /// <inheritdoc />
        public void ConnectConsumer(IMessageConsumer consumer)
        {
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        }

        /// <inheritdoc />
        public string ReceiveGZipMessage(string egmId, int hostId, byte[] message)
        {
            SourceTrace.TraceVerbose(
                G2STrace.Source,
                @"ReceiveEndpointProvider.ReceiveGZipMessage : Message from Host
	EgmId : {0}
	HostId : {1}
    Length : {2}",
                egmId,
                hostId,
                message.Length);

            return ReceiveMessage(egmId, hostId, GzipUtilities.Unzip(message));
        }

        /// <inheritdoc />
        public string ReceiveMessage(string egmId, int hostId, string message)
        {
            var error = new Error(); // defaults to G2S_none

            if (_messageBuilder == null)
            {
                throw new ConfigurationErrorsException(@"The MessageBuilder has not been configured.");
            }

            if (_consumer == null)
            {
                throw new ConfigurationErrorsException(@"A message consumer has not been connected.");
            }

            // A bit bloated, but it's an artifact of the logging and the G2S library implementation
            try
            {
                SourceTrace.TraceVerbose(
                    G2STrace.Source,
                    @"ReceiveEndpointProvider.ReceiveMessage : Message from Host
	EgmId : {0}
	HostId : {1}
	Message : {2}",
                    egmId,
                    hostId,
                    _messageBuilder.FormatXml(message));

                var request = _messageBuilder.DecodeMessage(message);
                if (!request.IsValid())
                {
                    SourceTrace.TraceWarning(
                        G2STrace.Source,
                        @"ClientBase.ReceiveMessage : Received invalid message 
	EgmId : {0}
	Message : {1}",
                        egmId,
                        message);

                    error.SetErrorCode(ErrorCode.G2S_MSX005); // Invalid Data Type Encountered
                }
                else if(!error.IsError)
                {
                    if (request.Item is IPoint2Point point2Point)
                    {
                        if (hostId != point2Point.hostId)
                        {
                            SourceTrace.TraceWarning(
                                G2STrace.Source,
                                @"ClientBase.ReceiveMessage : Incorrect HostId Specified
	EgmId : {0}
	Message HostId : {1}
	Request HostId : {2}",
                                egmId,
                                point2Point.hostId,
                                hostId);

                            error.SetErrorCode(ErrorCode.G2S_MSX001); // Incorrect hostId Specified
                        }
                        else if (egmId != point2Point.egmId)
                        {
                            SourceTrace.TraceWarning(
                                G2STrace.Source,
                                @"ClientBase.ReceiveMessage : Incorrect HostId Specified
	Message EgmId : {0}
	Request EgmId : {1}",
                                egmId,
                                point2Point.egmId);

                            error.SetErrorCode(ErrorCode.G2S_MSX002); // Incorrect egmId Specified
                        }
                        else
                        {
                            error = _consumer.Consumes(point2Point);
                        }
                    }
                    else if (request.Item is IMulticast multicast) // No need to check host or EGM for multicast
                    {                        
                        error = _consumer.Consumes(multicast);
                    }
                }
            }
            catch (XmlException e)
            {
                SourceTrace.TraceError(
                    G2STrace.Source,
                    @"ClientBase.ReceiveMessage : Error formatting message
	EgmId : {0}
	Message : {1}
	Exception : {2}",
                    egmId,
                    message,
                    e);

                error.SetErrorCode(ErrorCode.G2S_MSX004); // Incomplete/Malformed XML
            }
            catch (InvalidOperationException e)
            {
                SourceTrace.TraceError(
                    G2STrace.Source,
                    @"ClientBase.ReceiveMessage : Error decoding message 
	EgmId : {0}
	Message : {1}
	Exception : {2}",
                    egmId,
                    message,
                    e);

                error.SetErrorCode(ErrorCode.G2S_MSX005); // Incomplete/Malformed XML
            }
            catch (Exception e)
            {
                SourceTrace.TraceError(
                    G2STrace.Source,
                    @"ClientBase.ReceiveMessage : Unknown/unhandled error while processing message
	EgmId : {0}
	Message : {1}
	Exception : {2}",
                    egmId,
                    message,
                    e);

                error.SetErrorCode(ErrorCode.G2S_MSX999); // Incomplete/Malformed XML
                error.Text = $"Unknown error while processing message: {e.Message}";
            }

            var ack = _messageBuilder.BuildAck();
            ack.dateTimeSent = DateTime.UtcNow;
            ack.egmId = egmId;
            ack.hostId = hostId;
            ack.errorCode = error.Code;
            ack.errorText = error.Text;

            var response = _messageBuilder.BuildMessage(ack);
            var responseText = _messageBuilder.EncodeMessage(response);

            if (error.IsError)
            {
                SourceTrace.TraceWarning(
                    G2STrace.Source,
                    @"ClientBase.ReceiveMessage : Sending G2S Nack
	EgmId : {0}
	Request : {1}
	Response : {2}",
                    egmId,
                    message,
                    responseText);
            }

            return responseText;
        }

        /// <inheritdoc />
        public string[] ReceiveTransportRequest(string version)
        {
            return new[]
            {
                TransportOptionType.NO_GZIP.ToString(),
                TransportOptionType.GZIP_PAYLOAD.ToString()
            };
        }
    }
}