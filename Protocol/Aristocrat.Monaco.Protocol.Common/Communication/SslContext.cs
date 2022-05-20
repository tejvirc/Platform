namespace Aristocrat.Monaco.Protocol.Common.Communication
{
	using System.Net.Security;
	using System.Security.Authentication;
	using System.Security.Cryptography.X509Certificates;

	/// <summary>
	///     SSL context.
	/// </summary>
	public class SslContext
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="SslContext"/> class.
		/// </summary>
		public SslContext()
			: this(SslProtocols.Tls12)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SslContext"/> class.
		/// </summary>
		/// <param name="protocols">SSL protocols</param>
		public SslContext(SslProtocols protocols)
		{
			Protocols = protocols;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SslContext"/> class.
		/// </summary>
		/// <param name="protocols">SSL protocols</param>
		/// <param name="certificate">SSL certificate</param>
		public SslContext(SslProtocols protocols, X509Certificate certificate)
			: this(protocols, certificate, null)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SslContext"/> class.
		/// </summary>
		/// <param name="protocols">SSL protocols</param>
		/// <param name="certificate">SSL certificate</param>
		/// <param name="certificateValidationCallback">SSL certificate</param>
		public SslContext(SslProtocols protocols, X509Certificate certificate, RemoteCertificateValidationCallback certificateValidationCallback)
		{
			Protocols = protocols;
			Certificate = certificate;
			CertificateValidationCallback = certificateValidationCallback;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SslContext"/> class.
		/// </summary>
		/// <param name="protocols">SSL protocols</param>
		/// <param name="certificates">SSL certificates collection</param>
		public SslContext(SslProtocols protocols, X509Certificate2Collection certificates)
			: this(protocols, certificates, null)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SslContext"/> class.
		/// </summary>
		/// <param name="protocols">SSL protocols</param>
		/// <param name="certificates">SSL certificates collection</param>
		/// <param name="certificateValidationCallback">SSL certificate</param>
		public SslContext(SslProtocols protocols, X509Certificate2Collection certificates, RemoteCertificateValidationCallback certificateValidationCallback)
		{
			Protocols = protocols;
			Certificates = certificates;
			CertificateValidationCallback = certificateValidationCallback;
		}

		/// <summary>
		///     Gets or sets the SSL protocols.
		/// </summary>
		public SslProtocols Protocols { get; set; }

		/// <summary>
		///     Gets or sets the SSL certificate.
		/// </summary>
		public X509Certificate Certificate { get; set; }

		/// <summary>
		///     Gets or sets the SSL certificate collection.
		/// </summary>
		public X509Certificate2Collection Certificates { get; set; }

		/// <summary>
		///     Gets or sets the SSL certificate validation callback.
		/// </summary>
		public RemoteCertificateValidationCallback CertificateValidationCallback { get; set; }

		/// <summary>
		///     Gets or sets a value that indicates whether the client is asked for a certificate for authentication.
		/// </summary>
		/// <remarks>
		///     Note that this is only a request - if no certificate is provided, the server still accepts the connection request.
		/// </remarks>
		public bool ClientCertificateRequired { get; set; }
	}
}
