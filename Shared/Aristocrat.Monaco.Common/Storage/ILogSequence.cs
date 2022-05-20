namespace Aristocrat.Monaco.Common.Storage
{
    /// <summary>
    ///     Interface for entities supporting sequences.
    /// </summary>
    public interface ILogSequence
    {
        /// <summary>
        ///     Gets or sets the unique log sequence number assigned by the EGM.
        /// </summary>
        long Id { get; set; }
    }
}