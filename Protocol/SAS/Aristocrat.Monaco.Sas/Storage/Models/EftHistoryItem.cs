namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System;
    using Common.Storage;

    /// <summary>
    ///     The Eft History item entity
    /// </summary>
    public class EftHistoryItem : BaseEntity, ICloneable
    {
        /// <summary>
        ///     Gets or sets the history log
        /// </summary>
        public string EftHistoryLog { get; set; } = string.Empty;

        /// <inheritdoc />
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
