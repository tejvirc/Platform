namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using Common.Storage;

    /// <summary>
    ///     The host entity
    /// </summary>
    public class Host : BaseEntity
    {
        /// <summary>comm port used by this client</summary>
        public int ComPort { get; set; }

        /// <summary>SAS address used by this client</summary>
        public byte SasAddress { get; set; }

        /// <summary>The accounting denom for the SAS client</summary>
        public long AccountingDenom { get; set; }
    }
}