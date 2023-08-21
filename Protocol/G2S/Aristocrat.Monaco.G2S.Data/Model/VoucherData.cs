namespace Aristocrat.Monaco.G2S.Data.Model
{
    using System;
    using Common.Storage;

    /// <summary>
    ///     Base class that represents serialized voucher data data.
    /// </summary>
    public class VoucherData : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the list id.
        /// </summary>
        public long ListId { get; set; }

        /// <summary>
        ///     Gets or sets the validation id.
        /// </summary>
        public string ValidationId { get; set; }

        /// <summary>
        ///     Gets or sets the validation seed.
        /// </summary>
        public string ValidationSeed { get; set; }

        /// <summary>
        ///     Gets or sets the expiration time.
        /// </summary>
        public DateTime ListTime { get; set; }
    }
}