namespace Aristocrat.Monaco.G2S.Common.Transfer.Http
{
    using System;
    using System.ComponentModel;
    using System.Net;
    using System.Net.Security;

    /// <summary>
    ///     This class allows certification validation per instance
    /// </summary>
    [DesignerCategory("Code")]
    public class HttpWebClient : WebClient
    {
        private readonly RemoteCertificateValidationCallback _validator;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpWebClient" /> class.
        /// </summary>
        public HttpWebClient()
            : this(null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpWebClient" /> class.
        /// </summary>
        /// <param name="validator">Certificate validation routine</param>
        public HttpWebClient(RemoteCertificateValidationCallback validator)
        {
            _validator = validator;
        }

        /// <inheritdoc />
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);

            var httpRequest = (HttpWebRequest)base.GetWebRequest(address);

            if (httpRequest == null)
            {
                return request;
            }

            if (_validator == null)
            {
                // Use default validation (or ServicePointManager.ServerCertificateValidationCallback)
                return httpRequest;
            }

            httpRequest.ServerCertificateValidationCallback = _validator;

            return httpRequest;
        }
    }
}
