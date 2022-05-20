namespace Aristocrat.Bingo.Client.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;

    public class ClientConfigurationOptions
    {
        public ClientConfigurationOptions(Uri address, TimeSpan connectionTimeout, IEnumerable<X509Certificate2> certificates)
        {
            Address = address;
            ConnectionTimeout = connectionTimeout;
            Certificates = certificates?.ToList() ?? throw new ArgumentNullException(nameof(certificates));
        }

        public Uri Address { get; }

        public TimeSpan ConnectionTimeout { get; }

        public IReadOnlyCollection<X509Certificate2> Certificates { get; }
    }
}