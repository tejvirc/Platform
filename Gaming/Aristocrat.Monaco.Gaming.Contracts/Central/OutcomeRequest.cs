namespace Aristocrat.Monaco.Gaming.Contracts.Central
{
    using System.Collections.Generic;

    /// <summary>
    ///     An implementation of <see cref="IOutcomeRequest" /> for central determinant games
    /// </summary>
    public class OutcomeRequest : IOutcomeRequest, ITemplateRequest
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OutcomeRequest" /> class.
        /// </summary>
        /// <param name="quantity">The requested outcome count</param>
        /// <param name="templateId">The template Id for the game round</param>
        /// <param name="gameId">The game id associated with the request</param>
        /// <param name="additionalInfo">additional game play information</param>
        public OutcomeRequest(int quantity, int templateId, int gameId, IEnumerable<IAdditionalGamePlayInfo> additionalInfo)
        {
            Quantity = quantity;
            TemplateId = templateId;
            GameId = gameId;
            AdditionalInfo = additionalInfo;
        }

        /// <inheritdoc />
        public int TemplateId { get; }

        /// <inheritdoc />
        public int Quantity { get; }

        /// <summary>
        ///     The id of the game requesting the outcome
        /// </summary>
        public int GameId { get; }

        /// <summary>
        ///     Additional game play requests
        /// </summary>
        public IEnumerable<IAdditionalGamePlayInfo> AdditionalInfo { get; }
    }
}