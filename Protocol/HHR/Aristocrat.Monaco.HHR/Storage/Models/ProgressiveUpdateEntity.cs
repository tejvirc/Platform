namespace Aristocrat.Monaco.Hhr.Storage.Models
{
    using Common.Storage;

    /// <summary>
    ///     Model for the Host.
    /// </summary>
    public class ProgressiveUpdateEntity : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the current updated amount value (in cents) received from server
        /// </summary>
        public long CurrentValue { get; set; }

        /// <summary>
        ///     Gets or sets the progressive amount (in cents)  that must be paid for this hit
        /// </summary>
        public long AmountToBePaid { get; set; }

        /// <summary>
        ///     Gets or sets the progressive ID.
        /// </summary>
        public long ProgressiveId { get; set; }

        /// <summary>
        ///     Gets or sets the progressive hit count.
        /// </summary>
        public long RemainingHitCount { get; set; }

        /// <summary>
        ///  Gets or Sets the flag indicating if we need to store the updates
        ///  for this level and update it later
        /// </summary>
        public bool LockUpdates { get; set; }
    }
}
