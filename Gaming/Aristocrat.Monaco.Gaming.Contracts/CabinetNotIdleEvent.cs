namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     A <see cref="CabinetNotIdleEvent" /> should be posted when the EGM is no longer idle. EGM is considered idle if the
    ///     credit meters are set to 0 (zero), the EGM is playable, and there has been no activity at the EGM for the duration
    ///     specified in the idleTimePeriod attribute. No activity at the EGM typically means that there has been no activity
    ///     at
    ///     player interface devices, such as coin acceptors, note acceptors, touch screens, buttons, etc., and that no new
    ///     transactions have been initiated.If the credit meters are no longer set to 0 (zero) or there is activity at the
    ///     EGM, typically, the EGM must restart the sequence—that is, wait for the credit meters to reach 0 (zero) and then
    ///     wait for the duration specified in the idleTimePeriod attribute—before becoming idle.
    /// </summary>
    [Serializable]
    public class CabinetNotIdleEvent : BaseEvent
    {
    }
}