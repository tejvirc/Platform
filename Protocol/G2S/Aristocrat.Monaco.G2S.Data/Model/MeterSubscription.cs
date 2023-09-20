namespace Aristocrat.Monaco.G2S.Data.Model
{
    using Common.Storage;
    using System;

    /// <summary>
    ///     Meters subscription type
    /// </summary>
    public enum MetersSubscriptionType
    {
        /// <summary>
        ///     End of Day meters subscription
        /// </summary>
        EndOfDay,

        /// <summary>
        ///     Periodic meters subscription
        /// </summary>
        Periodic,

        /// <summary>
        ///     Audit meters subscription
        /// </summary>
        Audit
    }

    /// <summary>
    ///     Meter type
    /// </summary>
    public enum MeterType
    {
        /// <summary>
        ///     Device meter
        /// </summary>
        Device,

        /// <summary>
        ///     Game meter
        /// </summary>
        Game,

        /// <summary>
        ///     Wage meter
        /// </summary>
        Wage,

        /// <summary>
        ///     Currency meter
        /// </summary>
        Currency
    }

    /// <summary>
    ///     Base class that represents serialized meters subscription data.
    /// </summary>
    public class MeterSubscription : BaseEntity
    {
        /// <summary>
        ///     Gets or sets host Id
        /// </summary>
        public int HostId { get; set; }

        /// <summary>
        ///     Gets or sets subscription type.
        /// </summary>
        public MetersSubscriptionType SubType { get; set; }

        /// <summary>
        ///     Gets or sets base.
        /// </summary>
        public int Base { get; set; }

        /// <summary>
        ///     Gets or sets the period interval
        /// </summary>
        public int PeriodInterval { get; set; }

        /// <summary>
        ///     Gets or sets the device id.
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        ///     Gets or sets the meter type.
        /// </summary>
        public MeterType MeterType { get; set; }

        /// <summary>
        ///     Gets or sets the classname.
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to use meter definition.
        /// </summary>
        public bool MeterDefinition { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether meter subscription is trigger on EOD.
        /// </summary>
        public bool OnEndOfDay { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether meter subscription is trigger on door open.
        /// </summary>
        public bool OnDoorOpen { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether meter subscription is trigger on coin drop.
        /// </summary>
        public bool OnCoinDrop { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether meter subscription is trigger on note drop.
        /// </summary>
        public bool OnNoteDrop { get; set; }

        /// <summary>
        ///     Gets or sets a value for the last time this subscription was acknowledged by the host if the subscription is an EOD meter.
        /// </summary>
        public DateTimeOffset LastAckedTime { get; set; }
    }
}