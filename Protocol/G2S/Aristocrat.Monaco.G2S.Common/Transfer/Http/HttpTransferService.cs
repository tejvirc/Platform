namespace Aristocrat.Monaco.G2S.Common.Transfer.Http
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Text;

    /// <summary>
    ///     HTTP(S) implementation of transfer service that supports HTTP and HTTPS protocols.
    /// </summary>
    public class HttpTransferService : IProtocolTransferService
    {
        private const int BufferSize = 8 * 1024;
        private const string IssuerNameParameterName = "issuerName";
        private const string CredentialsParameterName = "credentials";

        private string IssuerName { get; set; }

        /// <summary>
        ///     Uploads specified source stream to Software Download Distribution Point.
        /// </summary>
        /// <param name="destinationLocation">An IP address:port/path/filename or valid network URI address.</param>
        /// <param name="transferParameters">A string containing optional parameters required for the transfer of the package.</param>
        /// <param name="sourceStream">Source stream that should be uploaded to destination</param>
        /// <param name="ct">Cancellation token used to abort upload</param>
        public void Upload(
            string destinationLocation,
            string transferParameters,
            Stream sourceStream,
            CancellationToken ct)
        {
            UploadAsync(destinationLocation, transferParameters, sourceStream).Wait(ct);
        }

        /// <summary>
        ///     Downloads specified package into stream from Software Download Distribution Point.
        /// </summary>
        /// <param name="downloadLocation">An IP address:port/path/filename or valid network URI address.</param>
        /// <param name="transferParameters">A string containing optional parameters required for the transfer of the package.</param>
        /// <param name="destinationStream">Destination stream that should contain downloaded content.</param>
        /// <param name="ct">Cancellation token used to abort upload</param>
        public void Download(
            string downloadLocation,
            string transferParameters,
            Stream destinationStream,
            CancellationToken ct)
        {
            DownloadAsync(downloadLocation, transferParameters, destinationStream).Wait(ct);
        }

        private static void SetClientCredentials(HttpClient client, string credentialsParameters)
        {
            var parametersArray = credentialsParameters.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (parametersArray.Length != 2)
            {
                return;
            }

            var userName = parametersArray[0];
            var password = parametersArray[1];

            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(
                        string.Format("{0}:{1}", userName, password))));
            }
        }

        /// <summary>
        ///     Gets full path of package.
        /// </summary>
        /// <param name="location">An IP address:port/path/filename or valid network URI address.</param>
        /// <returns>The package location</returns>
        private string GetPackageLocation(string location)
        {
            // TODO: Implement
            return location;
        }

        /// <summary>
        ///     Uploads specified source stream to Software Download Distribution Point.
        /// </summary>
        /// <param name="destinationLocation">An IP address:port/path/filename or valid network URI address.</param>
        /// <param name="transferParameters">A string containing optional parameters required for the transfer of the package.</param>
        /// <param name="sourceStream">Source stream that should be uploaded to destination</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        private async Task UploadAsync(string destinationLocation, string transferParameters, Stream sourceStream)
        {
            InitializeSecureSocketSettings(transferParameters);

            // Here we do not close sourceStream. It is a task for code that created it.
            using (var client = new HttpClient())
            {
                ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
                InitializeCredentials(client, transferParameters);
                var req = new HttpRequestMessage(HttpMethod.Post, new Uri(GetPackageLocation(destinationLocation)));
                using (var content = new StreamContent(sourceStream))
                {
                    req.Content = content;
                    using (var response = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
        }

        /// <summary>
        ///     Downloads specified package into stream from Software Download Distribution Point.
        /// </summary>
        /// <param name="downloadLocation">An IP address:port/path/filename or valid network URI address.</param>
        /// <param name="transferParameters">A string containing optional parameters required for the transfer of the package.</param>
        /// <param name="destinationStream">Destination stream that should contain downloaded content.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        private async Task DownloadAsync(string downloadLocation, string transferParameters, Stream destinationStream)
        {
            InitializeSecureSocketSettings(transferParameters);

            // Here we do not close destinationStream. It is a task for code that created it.
            using (var client = new HttpClient())
            {
                ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
                InitializeCredentials(client, transferParameters, downloadLocation);

                using (var response = await client.GetAsync(new Uri(GetPackageLocation(downloadLocation))))
                {
                    response.EnsureSuccessStatusCode();
                    var httpStream = await response.Content.ReadAsStreamAsync();
                    var buffer = new byte[BufferSize];
                    int count;
                    while ((count = httpStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        destinationStream.Write(buffer, 0, count);
                    }
                }
            }
        }

        /// <summary>
        ///     Set credentials to web client if such one were specified in transfer parameters.
        /// </summary>
        /// <param name="client">Web client instance.</param>
        /// <param name="transferParameters">A string containing optional parameters required for the transfer of the package.</param>
        /// <param name="downloadLocation">A string containing downloadLocation.</param>
        private void InitializeCredentials(HttpClient client, string transferParameters, string downloadLocation = null)
        {
            string credentialsParameters;

            if (string.IsNullOrEmpty(transferParameters))
            {
                if (!string.IsNullOrEmpty(downloadLocation))
                {
                    var index1 = downloadLocation.IndexOf("//", StringComparison.InvariantCultureIgnoreCase);
                    var index2 = downloadLocation.IndexOf("@", StringComparison.InvariantCultureIgnoreCase);

                    if (index1 != -1 && index2 != -1)
                    {
                        credentialsParameters = downloadLocation.Substring(index1 + 2, index2 - index1 - 2);
                        SetClientCredentials(client, credentialsParameters);
                    }
                }
            }
            else
            {
                credentialsParameters = TransferService.GetParameterByName(
                    transferParameters,
                    CredentialsParameterName);
                SetClientCredentials(client, credentialsParameters);
            }
        }

        /// <summary>
        ///     Configures system for security socket to be used.
        /// </summary>
        /// <param name="transferParameters">A string containing optional parameters required for the transfer of the package.</param>
        private void InitializeSecureSocketSettings(string transferParameters)
        {
            IssuerName = System.Web.HttpUtility.UrlDecode(
                TransferService.GetParameterByName(transferParameters, IssuerNameParameterName));
        }

        /// <summary>
        ///     Validates secure socket certificate.
        /// </summary>
        /// <param name="sender">Event sender instance.</param>
        /// <param name="certificate">Secure socket certificate.</param>
        /// <param name="chain">X509 chain instance.</param>
        /// <param name="sslPolicyErrors">SSL policy errors.</param>
        /// <returns>true if validated</returns>
        private bool ValidateRemoteCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return certificate != null;
        }
    }
}