namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using System;
    using System.Linq;

    /// <summary>Definition of the DisableStates enum.</summary>
    [Flags]
    public enum DisableState
    {
        /// <summary>Not disabled.</summary>
        None = 0,

        /// <summary>There is no communication with Sas host 0.</summary>
        Host0CommunicationsOffline = 1,

        /// <summary>There is no communication with Sas host 1.</summary>
        Host1CommunicationsOffline = 1 << 1,

        /// <summary>The machine was commanded by Sas host 0 to disable.</summary>
        DisabledByHost0 = 1 << 3,

        /// <summary>The machine was commanded by Sas host 1 to disable.</summary>
        DisabledByHost1 = 1 << 4,

        /// <summary>The machine is using secure enhanced validation, but has no validation id.</summary>
        ValidationIdNeeded = 1 << 6,

        /// <summary>The machine is in maintenance mode.</summary>
        MaintenanceMode = 1 << 7,

        /// <summary>The machine is not supported for progressives.</summary>
        ProgressivesNotSupported = 1 << 8,

        /// <summary>The machine validation queue is full and needs to have the backend query the data</summary>
        ValidationQueueFull = 1 << 9,

        /// <summary> Machine is disabled on PowerUp by Host 0. LP02 is required to clear this. </summary>
        PowerUpDisabledByHost0 = 1 << 10,

        /// <summary> Machine is disabled on PowerUp by Host 1. LP02 is required to clear this. </summary>
        PowerUpDisabledByHost1 = 1 << 11
    }
}