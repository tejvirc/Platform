namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    using System.Collections.Generic;

    /// <summary>
    ///     Holds information for multiple simultaneous play requests
    /// </summary>
    public class RequestMultipleGameOutcomeMessage
    {
        public RequestMultipleGameOutcomeMessage
        (
            string machineSerial,
            IEnumerable<RequestSingleGameOutcomeMessage> gameRequests)
        {
            MachineSerial = machineSerial;
            GameRequests = gameRequests;
        }

        /// <summary>
        ///     Gets the machine serial associated with this request
        /// </summary>
        public string MachineSerial { get; }

        /// <summary>
        ///     Gets the game requests
        /// </summary>
        public IEnumerable<RequestSingleGameOutcomeMessage> GameRequests { get; }
    }
}