namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System;
    using Common.Storage;

    /// <summary>
    ///     The Aft History item entity
    /// </summary>
    public class AftHistoryItem : BaseEntity, ICloneable
    {
        /// <summary>
        ///     Gets or sets the current buffer index
        /// </summary>
        public long CurrentBufferIndex { get; set; } = 1;

        /// <summary>
        ///     Gets or sets the history log
        /// </summary>
        public string AftHistoryLog { get; set; } = string.Empty;

        /// <inheritdoc />
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}