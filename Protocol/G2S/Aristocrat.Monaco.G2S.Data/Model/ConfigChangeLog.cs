namespace Aristocrat.Monaco.G2S.Data.Model
{
    using System;
    using System.Collections.Generic;
    using Common.Storage;

    /// <summary>
    ///     Represents record from ChangeLog data tables.
    /// </summary>
    public class ConfigChangeLog : BaseEntity, ILogSequence
    {
        /// <summary>
        ///     Gets or sets the date/time the log entry was last updated.
        /// </summary>
        public DateTime ChangeDateTime { get; set; }

        /// <summary>
        ///     Gets or sets the configuration identifier.
        /// </summary>
        public long ConfigurationId { get; set; }

        /// <summary>
        ///     Gets or sets device id.
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        ///     Gets or sets the transaction identifier.
        /// </summary>
        public long TransactionId { get; set; }

        /// <summary>
        ///     Gets or sets the apply condition.
        /// </summary>
        public ApplyCondition ApplyCondition { get; set; }

        /// <summary>
        ///     Gets or sets the disable condition.
        /// </summary>
        public DisableCondition DisableCondition { get; set; }

        /// <summary>
        ///     Gets or sets the start date time.
        /// </summary>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        ///     Gets or sets the end date time.
        /// </summary>
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [restart after].
        /// </summary>
        public bool RestartAfter { get; set; }

        /// <summary>
        ///     Gets or sets the change status.
        /// </summary>
        public ChangeStatus ChangeStatus { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether egm action confirmed.
        /// </summary>
        public bool EgmActionConfirmed { get; set; }

        /// <summary>
        ///     Gets or sets the change exception.
        /// </summary>
        public ChangeExceptionErrorCode ChangeException { get; set; }

        /// <summary>
        ///     Gets or sets the change data.
        /// </summary>
        public string ChangeData { get; set; }

        /// <summary>
        ///     Gets or sets the option change authorize items.
        /// </summary>
        public virtual ICollection<ConfigChangeAuthorizeItem> AuthorizeItems { get; set; }
    }
}