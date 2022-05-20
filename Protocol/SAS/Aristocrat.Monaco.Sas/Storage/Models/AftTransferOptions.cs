namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common.Storage;

    /// <summary>
    ///     The aft transfer options entity
    /// </summary>
    public class AftTransferOptions : BaseEntity, ICloneable
    {
        /// <summary>
        ///     Gets or sets whether or not the last transfer has been acknowledge by the host
        /// </summary>
        public bool IsTransferAcknowledgedByHost { get; set; } = true;

        /// <summary>
        ///     Gets or sets the current transfer
        /// </summary>
        public string CurrentTransfer { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the current transfer flags
        /// </summary>
        public AftTransferFlags CurrentTransferFlags { get; set; }

        /// <inheritdoc />
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}