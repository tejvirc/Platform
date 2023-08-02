namespace Aristocrat.Monaco.Bingo.Commands
{
    using System.Collections.Generic;
    using Aristocrat.Bingo.Client.Messages.GamePlay;

    /// <summary>
    ///     Holds the information for a multi-play request
    /// </summary>
    public class RequestMultiPlayCommand
    {
        public RequestMultiPlayCommand(
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