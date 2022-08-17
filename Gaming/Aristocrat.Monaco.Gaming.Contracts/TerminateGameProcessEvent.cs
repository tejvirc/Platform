    namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     An End Game Process event is posted to terminate the current game process.
    /// </summary>
    [Serializable]
    public class TerminateGameProcessEvent : BaseEvent
    {
        /// <summary>
        ///     Indicates whether the handler should create an "expected" or "unexpected" process exit
        /// </summary>
        public bool TerminateExpected;

        /// <summary>
        ///     Construct a new TerminateGameProcessEvent object
        /// </summary>
        /// <param name="terminateExpected">
        ///     Indicates whether the handler should create an "expected" or "unexpected" process exit
        /// </param>
        public TerminateGameProcessEvent(bool terminateExpected = true)
        {
            TerminateExpected = terminateExpected;
        }
    }
}