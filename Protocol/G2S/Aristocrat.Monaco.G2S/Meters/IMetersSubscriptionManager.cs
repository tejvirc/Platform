namespace Aristocrat.Monaco.G2S.Meters
{
    using System.Collections.Generic;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;

    /// <summary>
    ///     Defines a contract for a meters subscription manager.
    /// </summary>
    public interface IMetersSubscriptionManager
    {
        /// <summary>
        ///     Gets Wage Meter map.
        /// </summary>
        Dictionary<string, List<string>> WageMeters { get; }

        /// <summary>
        ///     Gets Currency Meter map.
        /// </summary>
        Dictionary<string, List<string>> CurrencyMeters { get; }

        /// <summary>
        ///     Gets Game Meter map.
        /// </summary>
        Dictionary<string, List<string>> GameMeters { get; }

        /// <summary>
        ///     Gets Device Meter map.
        /// </summary>
        Dictionary<string, List<string>> DeviceMeters { get; }

        /// <summary>
        ///     Schedules meter reporting.
        /// </summary>
        void Start();

        /// <summary>
        ///     Gets all subscriptions for a host by type.
        /// </summary>
        /// <param name="hostId">Host Id.</param>
        /// <param name="type">Subscription type.</param>
        /// <returns>Meter subscription</returns>
        IEnumerable<MeterSubscription> GetMeterSub(int hostId, MetersSubscriptionType type);

        /// <summary>
        ///     Clears all subscriptions for a host for type.
        /// </summary>
        /// <param name="hostId">Host Id.</param>
        /// <param name="type">Subscription type.</param>
        /// <returns>Meter subscription</returns>
        MeterSubscription ClearSubscriptions(int hostId, MetersSubscriptionType type);

        /// <summary>
        ///     Add subscriptions for a host by type.
        /// </summary>
        /// <param name="hostId">Host Id.</param>
        /// <param name="type">Subscription type.</param>
        /// <param name="subsList">Meter subscriptions.</param>
        void SetMetersSubscription(int hostId, MetersSubscriptionType type, IList<MeterSubscription> subsList);

        /// <summary>
        ///     Handles meter query for G2S GetMeterInfo
        /// </summary>
        /// <param name="queryInfo">GetMeterInfo</param>
        /// <param name="result">MeterInfo result</param>
        /// <returns>Error code</returns>
        string GetMeters(getMeterInfo queryInfo, meterInfo result);

        /// <summary>
        ///     Sends End of Day meters info.
        /// </summary>
        /// <param name="onDoorOpen">On door open flag.</param>
        /// <param name="onNoteDrop">On note drop flag.</param>
        void SendEndOfDayMeters(bool onDoorOpen = false, bool onNoteDrop = false);

        /// <summary>
        ///     Sends meter Info to host
        /// </summary>
        /// <param name="subId">Meter subscription Id.</param>
        void HandleMeterReport(long subId);
    }
}