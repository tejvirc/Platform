namespace Aristocrat.Monaco.G2S.Common.Transfer.Ftp
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using Application.Contracts.Localization;
    using FluentFTP;
    using log4net;
    using Localization.Properties;
    using Monaco.Common.Exceptions;

    /// <summary>
    ///     FTP client implementation for FTP/FTPS/FTPES protocols.
    /// </summary>
    public class FtpClient : IFtpClient
    {
        /// <summary>
        ///     The buffer size
        /// </summary>
        public const int BufferSize = 8 * 1024;

        private const string IssuerNameParameterName = "issuerName";

        private const string FtpUrlRegEx =
                @"^(ftp[s]?\:\/+\/+)((?<username>[^:\/\s]+)\:(?<password>[^@\/\s]+))?\@?(?<host>[^:\/\s]+)\:*(?<port>[\d]+)+(?<path>[^\s]*)$"
            ;

        private const string FtpUrlAnonRegEx =
            @"^(ftp[s]?\:\/+\/+)((?<username>[^:\/\s]+)\:+)?\@?(?<host>[^:\/\s]+)\:*(?<port>[\d]+)+(?<path>[^\s]*)$";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool _disposed;

        private FluentFTP.FtpClient _ftpClient = new FluentFTP.FtpClient();

        private string IssuerName { get; set; }

        /// <inheritdoc />
        public string CurrentLocation { get; private set; }

        /// <inheritdoc />
        public string LocalFileLocation { get; private set; }

        /// <inheritdoc />
        public Task Connect(string location, string parameters)
        {
            var match = Regex.Match(location, FtpUrlRegEx);
            if (!match.Success)
            {
                match = Regex.Match(location, FtpUrlAnonRegEx);
                if (!match.Success)
                {
                    throw new CommandException(
                        Localizer.For(CultureFor.Operator).FormatString(ResourceKeys.IncorrectProtocolUrlErrorMessageTemplate, "FTP"));
                }
            }

            var userName = match.Groups["username"];
            var password = match.Groups["password"];

            if (string.IsNullOrEmpty(password?.Value) && userName != null &&
                userName.Value.ToLower(CultureInfo.CurrentCulture).Equals("anonymous"))
            {
                Logger.Info("FTP client: user anonymous missing password using username for password");
                password = userName;
            }

            if ((userName != null) ^ (password != null))
            {
                // in case one of pair was specified, but not another one.
                throw new CommandException(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UserNameOrPasswordNotSpecifiedErrorMessage));
            }

            if (userName != null && !string.IsNullOrEmpty(userName.Value) && !string.IsNullOrEmpty(password.Value))
            {
                _ftpClient.Credentials = new NetworkCredential(userName.Value, HttpUtility.UrlDecode(password.Value));
            }

            _ftpClient.Host = match.Groups["host"].Value;
            LocalFileLocation = match.Groups["path"].Value;
            var port = match.Groups["port"];

            if (!string.IsNullOrEmpty(port?.Value))
            {
                var portVal = int.Parse(port.Value);
                if (portVal >= 10)
                {
                    _ftpClient.Port = portVal;
                }
                else
                {
                    _ftpClient.Host += port.Value;

                    LocalFileLocation = LocalFileLocation.TrimStart(':');
                }
            }

            IssuerName = TransferService.GetParameterByName(parameters, IssuerNameParameterName);

            _ftpClient.ValidateCertificate += OnValidateCertificate;
            _ftpClient.SocketKeepAlive = true;

            if (location.StartsWith("ftps://", StringComparison.InvariantCultureIgnoreCase))
            {
                // currently only supporting explicit FTP/S protocol.
                _ftpClient.EncryptionMode = FtpEncryptionMode.Implicit;
                CurrentLocation = $"ftps://{_ftpClient.Host}:{_ftpClient.Port}{LocalFileLocation}";
            }
            else
            {
                // there is no FTPS protocol. FTPS in 'location' is only indicator for our API. But correct URL should starts with 'FTP://'.
                CurrentLocation = $"ftp://{_ftpClient.Host}:{_ftpClient.Port}{LocalFileLocation}";
            }

            Logger.Info(
                $"FTP client: host: {_ftpClient.Host} port: {_ftpClient.Port} username: {_ftpClient?.Credentials?.UserName} password: {_ftpClient?.Credentials?.Password} file path: {LocalFileLocation}");

            return _ftpClient.ConnectAsync();
        }

        /// <inheritdoc />
        public async Task Upload(Stream sourceStream, CancellationToken ct)
        {
            // Here we do not close sourceStream. It is a task for the code that created it.
            using (var ftpStream = await _ftpClient.OpenWriteAsync(LocalFileLocation, CancellationToken.None))
            {

                var buffer = new byte[BufferSize];
                int count;
                while (sourceStream.CanRead && (count = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (ct.IsCancellationRequested)
                    {
                        return;
                    }

                    ftpStream.Write(buffer, 0, count);
                }
            }
        }

        /// <inheritdoc />
        public async Task Download(Stream destinationStream, CancellationToken ct)
        {
            // Here we do not close destinationStream. It is a task for the code that created it.
            using (var ftpStream = await _ftpClient.OpenReadAsync(LocalFileLocation, FtpDataType.Binary, CancellationToken.None))
            {
                var buffer = new byte[BufferSize];
                int count;
                long totalBytes = 0;
                while (destinationStream.CanWrite &&
                       (count = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if(ct.IsCancellationRequested)
                    {
                        return;
                    }

                    destinationStream.Write(buffer, 0, count);
                    totalBytes += count;
                }

                if (!CheckFileSize(LocalFileLocation, totalBytes))
                {
                    //// Not sure this command is supported on the ftp host
                    ////throw new FtpException("Failed to download complete file from the FTP server");
                    throw new InvalidOperationException(
                        $"Failed to download complete file from the FTP server: {LocalFileLocation} only received {totalBytes} bytes");
                }
            }
        }

        /// <inheritdoc />
        public bool IsFileExists(string packageId, string location)
        {
            return _ftpClient.FileExists(location);
        }

        /// <inheritdoc />
        public void DeleteFile(string packageId, string location)
        {
            _ftpClient.DeleteFile(location);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Handles cleaning up the object instance.
        /// </summary>
        /// <param name="disposing">Indicates whether or not the class is disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing && _ftpClient != null)
            {
                _ftpClient.Disconnect();
                _ftpClient.Dispose();
            }

            _ftpClient = null;

            _disposed = true;
        }

        private void OnValidateCertificate(FluentFTP.FtpClient control, FtpSslValidationEventArgs args)
        {
            args.Accept = args.Certificate != null;
        }

        private bool CheckFileSize(string location, long bytes)
        {
            return _ftpClient.GetFileSize(location) == bytes;
        }
    }
}