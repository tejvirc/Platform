namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    /// <summary>
    /// This event is posted when the RequiredForPlay setting changes and we no longer need to display the System Disabled message
    /// </summary>
    public class RemoveDisabledMessageEvent : IdReaderBaseEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveDisabledMessageEvent"/> class with the ID reader's ID
        /// </summary>
        /// <param name="id">The associated ID reader's ID</param>
        public RemoveDisabledMessageEvent(int id)
            : base(id)
        {
        }
    }
}
