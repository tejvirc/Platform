namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using Monaco.Common.Storage;

    /// <summary>
    ///     A model for the default state of the bingo card daubs
    /// </summary>
    public class BingoDaubsModel : BaseEntity
    {
        /// <summary>
        ///     Gets or sets a value indicating if the bingo card is daubed
        /// </summary>
        public bool CardIsDaubed { get; set; }
    }
}