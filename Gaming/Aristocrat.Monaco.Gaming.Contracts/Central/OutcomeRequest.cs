namespace Aristocrat.Monaco.Gaming.Contracts.Central
{
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
        public OutcomeRequest(int quantity, int templateId)
        {
            Quantity = quantity;
            TemplateId = templateId;
        }

        /// <inheritdoc />
        public int TemplateId { get; }

        /// <inheritdoc />
        public int Quantity { get; }
    }
}