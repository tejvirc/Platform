namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System;
    using Common.Storage;

    /// <summary>
    ///     The Enhanced Validation item entity
    /// </summary>
    public class EnhancedValidationItem : BaseEntity, ICloneable
    {
        /// <summary>
        ///     Gets or sets the secure enhanced validation data log
        /// </summary>
        public string EnhancedValidationDataLog { get; set; }

        /// <inheritdoc />
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}