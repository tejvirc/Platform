namespace Aristocrat.G2S.Client.Communications
{    
    using System;
    using CoreWCF.Channels;
    using CoreWCF.Description;
    using System.Xml;

    /// <summary>
    ///     Custom text message binding to handle null/missing content types
    /// </summary>
    public class HostTextMessageBindingElement : MessageEncodingBindingElement, IWsdlExportExtension
    {
        private string _encoding;
        private string _mediaType;
        private MessageVersion _msgVersion;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostTextMessageBindingElement" /> class.
        /// </summary>
        /// <param name="encoding">The encoding type</param>
        /// <param name="mediaType">The media type</param>
        /// <param name="version">The message version</param>
        public HostTextMessageBindingElement(string encoding, string mediaType, MessageVersion version)
        {
            _msgVersion = version ?? throw new ArgumentNullException(nameof(version));
            _mediaType = mediaType ?? throw new ArgumentNullException(nameof(mediaType));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            ReaderQuotas = new XmlDictionaryReaderQuotas();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostTextMessageBindingElement" /> class.
        /// </summary>
        /// <param name="encoding">The encoding type</param>
        /// <param name="mediaType">The media type</param>
        public HostTextMessageBindingElement(string encoding, string mediaType)
            : this(encoding, mediaType, MessageVersion.Soap11)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostTextMessageBindingElement" /> class.
        /// </summary>
        /// <param name="encoding">The encoding type</param>
        public HostTextMessageBindingElement(string encoding)
            : this(encoding, "text/xml")
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostTextMessageBindingElement" /> class.
        /// </summary>
        public HostTextMessageBindingElement()
            : this("UTF-8")
        {
        }

        private HostTextMessageBindingElement(HostTextMessageBindingElement binding)
            : this(binding.Encoding, binding.MediaType, binding.MessageVersion)
        {
            ReaderQuotas = new XmlDictionaryReaderQuotas();
            binding.ReaderQuotas.CopyTo(ReaderQuotas);
        }

        /// <summary>
        ///     Gets or sets the message version
        /// </summary>
        public override MessageVersion MessageVersion
        {
            get => _msgVersion;

            set => _msgVersion = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        ///     Gets or sets the media type
        /// </summary>
        public string MediaType
        {
            get => _mediaType;

            set => _mediaType = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        ///     Gets or sets the encoding
        /// </summary>
        public string Encoding
        {
            get => _encoding;

            set => _encoding = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        ///     Gets or sets the reader quotas
        /// </summary>
        /// <remarks>
        ///     This encoder does not enforces any quotas for the un-secure messages. The quotas are enforced for the secure
        ///     portions of messages when this encoder is used in a binding that is configured with security.
        /// </remarks>
        public XmlDictionaryReaderQuotas ReaderQuotas { get; set; }

        /// <inheritdoc />
        public void ExportContract(WsdlExporter exporter, WsdlContractConversionContext context)
        {
        }

        /// <inheritdoc />
        public void ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context)
        {
            // The MessageEncodingBindingElement is responsible for ensuring that the WSDL has the correct
            // SOAP version. We can delegate to the WCF implementation of TextMessageEncodingBindingElement for this.
            var element = new TextMessageEncodingBindingElement { MessageVersion = _msgVersion };

            ((IWsdlExportExtension)element).ExportEndpoint(exporter, context);
        }

        /// <inheritdoc />
        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new HostTextMessageEncoderFactory(MediaType, Encoding, MessageVersion);
        }

        /// <inheritdoc />
        public override BindingElement Clone()
        {
            return new HostTextMessageBindingElement(this);
        }

        //PlanA: We disabled two below methods, because in CoreWCF.Channels.BindingElement they have been disabled.
        /*
        /// <inheritdoc />
        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.BindingParameters.Add(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        /// <inheritdoc />
        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context.CanBuildInnerChannelFactory<TChannel>();
        }
        */

        //PlanA: We disabled two below methods, because in CoreWCF.Channels.BindingElement they have been removed.
        /*
        /// <inheritdoc />
        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.BindingParameters.Add(this);
            return context.BuildInnerChannelListener<TChannel>();
        }

        /// <inheritdoc />
        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelListener<TChannel>();
        }
        */

        /// <inheritdoc />
        public override T GetProperty<T>(BindingContext context)
        {
            if (typeof(T) == typeof(XmlDictionaryReaderQuotas))
            {
                return (T)(object)ReaderQuotas;
            }

            return base.GetProperty<T>(context);
        }
    }
}