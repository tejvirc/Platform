namespace Aristocrat.Monaco.Bingo.Client.Messages
{
    /// <summary>
    ///     Progressive update message when a progressive update comes in from the server.
    /// </summary>
    /// 
    public class ProgressiveUpdate
    {
        /// <summary>
        /// The progressive level
        /// </summary>
        public string ProgressiveLevel;

        /// <summary>
        /// This amount in cents
        /// </summary>
        public uint Amount;
    }
}
