namespace Aristocrat.Monaco.G2S.Data.Model
{
    using Monaco.Common.Storage;

    /// <summary>
    ///     Stores data about the pending jackpot award.
    /// </summary>
    public class PendingJackpotAwards : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the pending jackpot awards.
        /// </summary>
        public string Awards { get; set; }
    }
}
