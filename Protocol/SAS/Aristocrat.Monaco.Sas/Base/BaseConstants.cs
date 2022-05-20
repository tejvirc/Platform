namespace Aristocrat.Monaco.Sas.Base
{
    using System;
    using System.Collections.Generic;
    using Contracts.Client;

    internal static class BaseConstants
    {
        /// <summary>The key for the Aft bonus server setting</summary>
        internal const string UsingAftBonusingKey = "System.UsingAftBonusing";

        // reason = DisableStates.Host0CommunicationsOffline;
        internal static readonly IDictionary<int, DisableState> HostDisableStates = new Dictionary<int, DisableState>
        {
            { 0, DisableState.Host0CommunicationsOffline},
            { 1, DisableState.Host1CommunicationsOffline},
            {-1, DisableState.None}
        };

        /// <summary>
        ///     Gets the ISystemDisableManager key used when the SAS protocol locks the EGM during initialization
        /// </summary>
        public static Guid ProtocolDisabledKey => new("{0ED86BB9-5E5E-4D5B-803C-DA35D4C99ADB}");

    }
}
