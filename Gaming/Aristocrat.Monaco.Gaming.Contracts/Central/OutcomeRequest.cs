namespace Aristocrat.Monaco.Gaming.Contracts.Central
{
    using System.Collections.Generic;

    /// <summary>
    ///     An implementation of <see cref="IOutcomeRequest" /> for central determinant games
    /// </summary>
    public class OutcomeRequest : IOutcomeRequest
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OutcomeRequest" /> class.
        /// </summary>
        /// <param name="quantity">The requested outcome count</param>
        /// <param name="totalWin"></param>
        /// <param name="gamePlayInfo">additional game play information</param>
        public OutcomeRequest(int quantity, long totalWin, IEnumerable<AdditionalGamePlayInfo> gamePlayInfo)
        {
            Quantity = quantity;
            TotalWin = totalWin;
            AdditionalInfo = gamePlayInfo;
        }

        /// <inheritdoc />
        public int Quantity { get; }

        /// <inheritdoc />
        public long TotalWin { get; }

        /// <summary>
        ///     Additional game play requests
        /// </summary>
        public IEnumerable<IAdditionalGamePlayInfo> AdditionalInfo { get; }
    }
}