namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Contracts.Central;

    public class PrimaryGameEscrow
    {
        /// <summary>
        ///     Constructs a new instance of the PrimaryGameEscrow class
        /// </summary>
        /// <param name="initialWager">initial main game wager</param>
        /// <param name="data">The recovery data</param>
        /// <param name="request">The outcome request</param>
        public PrimaryGameEscrow(long initialWager, byte[] data, IOutcomeRequest request)
        {
            InitialWager = initialWager;
            Data = data;
            Request = request;
            AdditionalInfo = request.AdditionalInfo ?? Enumerable.Empty<IAdditionalGamePlayInfo>();
        }

        /// <summary>
        ///     The wager for the main game
        /// </summary>
        public long InitialWager { get; }

        /// <summary>
        ///     The recovery data
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        ///     The outcome request for the main game
        /// </summary>
        public IOutcomeRequest Request { get; }

        /// <summary>
        ///     The play results
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        ///     Additional game play information
        /// </summary>
        public IEnumerable<IAdditionalGamePlayInfo> AdditionalInfo { get; set; }
    }
}