namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    /// <summary>
    ///     Provides a mechanism to get/set disabled notes to persistence
    /// </summary>
    public interface IDisabledNotesService
    {
        /// <summary>
        ///     Gets or sets the note info.
        /// </summary>
        NoteInfo NoteInfo { get; set; }
    }
}