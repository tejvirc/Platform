namespace Aristocrat.Monaco.G2S.Common.Transfer.Ftp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts.Localization;
    using Localization.Properties;
    using Monaco.Common.Exceptions;
    using Renci.SshNet;

    /// <summary>
    ///     FTP client implementation for SFTP protocol.
    /// </summary>
    public class SecureFtpClient : IFtpClient
    {
        // The URI should be encoded in a format consistent with that defined by draft-ietf-secsh-scp-sftp-ssh-uri-04.
        // This may include a user name, trusted host-key fingerprint.
        // TODO: Figure out how to use ssh key method that is specified in URL
        private const string SftpUrlRegEx =
                @"^(sftp\:\/{2})((?<username>[^;@\/\s]+)?\;?(fingerprint=((ssh-[a-z]{3})\-(?<fingerprints>([0-9a-f]{2}\-){15}[0-9a-f]{2})?))?\@)?(?<host>[^:\/\s]+)\:+(?<port>[\d]+)+(?<path>[^\s]*)$"
            ;

        private const string IssuerNameParameterName = "issuerName";
        private const string PasswordParameterName = "password";
        private const int DefaultSftpPort = 22;
        private SftpClient _ftpClient;

        /// <inheritdoc />
        public virtual string CurrentLocation { get; private set; }

        /// <inheritdoc />
        public virtual string LocalFileLocation { get; private set; }

        /// <inheritdoc />
        public async Task Connect(string location, string parameters)
        {
            await Task.Factory.StartNew(
                () =>
                {
                    var match = Regex.Match(location, SftpUrlRegEx);
                    if (match.Success == false)
                    {
                        throw new CommandException(
                            Localizer.For(CultureFor.Operator).FormatString(ResourceKeys.IncorrectProtocolUrlErrorMessageTemplate, "HTTP"));
                    }

                    var fingerprints = match.Groups["fingerprints"].Value;

                    LocalFileLocation = match.Groups["path"].Value;

                    var connectionInfo = GetConnectionInfo(location, parameters);
                    CurrentLocation = $"sftp://{connectionInfo.Host}:{connectionInfo.Port}{LocalFileLocation}";

                    _ftpClient = new SftpClient(connectionInfo) { KeepAliveInterval = TimeSpan.FromSeconds(5) };

                    if (string.IsNullOrEmpty(fingerprints) == false)
                    {
                        _ftpClient.HostKeyReceived +=
                            (sender, e) =>
                            {
                                e.CanTrust = e.FingerPrint.SequenceEqual(ConvertFingerprintToByteArray(fingerprints));
                            };
                    }

                    _ftpClient.Connect();
                });
        }

        /// <inheritdoc />
        public async Task Upload(Stream sourceStream, CancellationToken ct)
        {
            await Task.Factory.StartNew(
                () =>
                {
                    // Here we do not close sourceStream. It is a task for code that created it.
                    using (var ftpStream = _ftpClient.OpenWrite(LocalFileLocation))
                    {
                        var buffer = new byte[FtpClient.BufferSize];
                        int count;
                        while ((count = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            if(ct.IsCancellationRequested)
                            {
                                return;
                            }

                            ftpStream.Write(buffer, 0, count);
                        }
                    }
                }, CancellationToken.None);
        }

        /// <inheritdoc />
        public async Task Download(Stream destinationStream, CancellationToken ct)
        {
            await Task.Factory.StartNew(
                () =>
                {
                    // Here we do not close destinationStream. It is a task for code that created it.
                    using (var ftpStream = _ftpClient.OpenRead(LocalFileLocation))
                    {
                        var buffer = new byte[FtpClient.BufferSize];
                        int count;
                        while ((count = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            if (ct.IsCancellationRequested)
                            {
                                return;
                            }

                            destinationStream.Write(buffer, 0, count);
                        }
                    }
                }, CancellationToken.None);
        }

        /// <inheritdoc />
        public bool IsFileExists(string packageId, string location)
        {
            return _ftpClient.Exists(location);
        }

        /// <summary>
        ///     Deletes file.
        /// </summary>
        /// <param name="packageId">Uniquely identifies the package.</param>
        /// <param name="location">An IP address:port/path/filename or valid network URI address.</param>
        public void DeleteFile(string packageId, string location)
        {
            _ftpClient.DeleteFile(location);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
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
            if (disposing && _ftpClient != null)
            {
                _ftpClient.Disconnect();
                _ftpClient.Dispose();
            }
        }

        private ConnectionInfo GetConnectionInfo(string location, string parameters)
        {
            var authenticationMethods = new List<AuthenticationMethod>();
            var match = Regex.Match(location, SftpUrlRegEx);
            var ftpUser = match.Groups["username"].Value;
            var host = match.Groups["host"].Value;
            var port = DefaultSftpPort;
            if (string.IsNullOrEmpty(match.Groups["port"].Value) == false)
            {
                port = int.Parse(match.Groups["port"].Value);
            }

            var password = TransferService.GetParameterByName(parameters, PasswordParameterName);
            var issuer = TransferService.GetParameterByName(parameters, IssuerNameParameterName);

            if (string.IsNullOrEmpty(password) == false && string.IsNullOrEmpty(ftpUser) == false)
            {
                authenticationMethods.Add(new PasswordAuthenticationMethod(ftpUser, password));
            }

            if (string.IsNullOrEmpty(issuer) == false
                && string.IsNullOrEmpty(password) == false
                && string.IsNullOrEmpty(ftpUser) == false)
            {
                // TODO: API allows to specify private key file path. But what will be specified in issuer?
                authenticationMethods.Add(
                    new PrivateKeyAuthenticationMethod(
                        ftpUser,
                        new PrivateKeyFile(issuer, password)));
            }

            if (authenticationMethods.Count == 0)
            {
                return new ConnectionInfo(
                    host,
                    port,
                    ftpUser);
            }

            return new ConnectionInfo(
                host,
                port,
                ftpUser,
                authenticationMethods.ToArray());
        }

        private byte[] ConvertFingerprintToByteArray(string fingerprint)
        {
            return fingerprint.Split('-').Select(s => Convert.ToByte(s, 16)).ToArray();
        }
    }
}