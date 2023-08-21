namespace Aristocrat.Monaco.Hhr.Storage.Models
{
    using Common.Storage;

    /// <summary>
    /// Class for the manual handicap entity
    /// </summary>
    public class ManualHandicapEntity : BaseEntity
    {
        /// <summary>
        /// Flag to inform a quick or manual handicap was completed
        /// </summary>
        public bool IsCompleted { get; set; }

    }
}
