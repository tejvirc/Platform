namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    using ServerApiGateway;

    /// <summary>
    ///     Holds information about a multi-game outcome
    /// </summary>
    public class ReportMultiGameOutcomeMessage : IMessage
    {
        /// <summary>Gets or sets the Transaction Id</summary>
        public long TransactionId { get; set; }

        /// <summary>Gets or sets the multi game outcome message</summary>
        public MultiGameOutcome Message { get; set; }
    }
}