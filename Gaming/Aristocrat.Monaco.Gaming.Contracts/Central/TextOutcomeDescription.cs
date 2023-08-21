namespace Aristocrat.Monaco.Gaming.Contracts.Central
{
    /// <summary>
    ///     A simple class for providing text based outcomes
    /// </summary>
    public class TextOutcomeDescription : IOutcomeDescription
    {
        /// <summary>
        ///     The game round information
        /// </summary>
        public string GameRoundInfo { get; set; }
    }
}