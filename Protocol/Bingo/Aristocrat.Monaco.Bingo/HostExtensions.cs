namespace Aristocrat.Monaco.Bingo
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using Aristocrat.Bingo.Client.Configuration;
    using Common;
    using Common.Storage.Model;

    public static class HostExtensions
    {
        public static ClientConfigurationOptions ToConfigurationOptions(this Host host, IEnumerable<X509Certificate2> certificates)
        {
            var uriBuilder = new UriBuilder { Host = host.HostName, Port = host.Port };
            return new ClientConfigurationOptions(
                uriBuilder.Uri,
                BingoConstants.DefaultConnectionTimeout,
                certificates);
        }
    }
}