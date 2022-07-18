namespace Aristocrat.Monaco.Sas.Eft
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Aristocrat.Sas.Client;

    /// <summary>
    ///     Define common data for Eft handler/controllers
    /// </summary>
    public static class EftCommonGuids
    {
        /// <summary>
        ///     Guids to allow Eft
        /// </summary>
        public static readonly List<Guid> AllowEftGuids = new()
        {
            ApplicationConstants.LiveAuthenticationDisableKey,
            SasConstants.EftTransactionLockUpGuid
        };

        /// <summary>
        ///     Guids disabled by host
        /// </summary>
        public static readonly List<Guid> DisabledByHostGuids = new()
        {
            ApplicationConstants.DisabledByHost0Key,
            ApplicationConstants.DisabledByHost1Key,
            ApplicationConstants.Host0CommunicationsOfflineDisableKey,
            ApplicationConstants.Host1CommunicationsOfflineDisableKey
        };
    }
}
