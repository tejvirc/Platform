namespace Aristocrat.Bingo.Client.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;

    public sealed class ClientConfigurationOptions : IDisposable
    {
        private bool _disposed;
        private readonly List<X509Certificate2> _certificates;

        public ClientConfigurationOptions(Uri address, TimeSpan connectionTimeout, IEnumerable<X509Certificate2> certificates)
        {
            Address = address;
            ConnectionTimeout = connectionTimeout;
            _certificates = certificates?.ToList() ?? throw new ArgumentNullException(nameof(certificates));
        }

        public Uri Address { get; }

        public TimeSpan ConnectionTimeout { get; }

        public IEnumerable<X509Certificate2> Certificates => _certificates;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            foreach (var certificate in _certificates)
            {
                certificate.Dispose();
            }

            _certificates.Clear();
            _disposed = true;
        }
    }
}