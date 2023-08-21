namespace Aristocrat.Monaco.Application.Contracts.Media
{
    using Kernel;
    /// <summary>
    /// 
    /// </summary>
    public class BrowserProcessTerminatedEvent : BaseEvent
    {
        /// <summary>
        ///      Initializes a new instance of the <see cref="BrowserProcessTerminatedEvent" /> class
        /// </summary>
        /// <param name="id">Associated media player ID</param>
        public BrowserProcessTerminatedEvent(int id)
        {
            MediaPlayerId = id;
        }

        /// <summary>
        ///     Media Player ID
        /// </summary>
        public int MediaPlayerId { get; }
    }
}
