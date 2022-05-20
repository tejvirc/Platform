namespace Aristocrat.Monaco.Hhr.Storage.Models
{
    using Common.Storage;

    /// <summary>
    /// Model for prize information
    /// </summary>
    public class PrizeInformationEntity : BaseEntity
    {
        /// <summary>
        /// The last received PrizeInformation object as Json
        /// </summary>
        public string PrizeInformationJson { get; set; }
    }
}
