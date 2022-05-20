namespace Aristocrat.Monaco.Gaming.Contracts.Payment
{
    /// <summary>
    ///     An interface for an object that can check the outcome of a game is correct before the
    ///     game round ends. For example we might want to check the game outcome matches a central
    ///     determination server's outcome.
    /// </summary>
    public interface IOutcomeValidator
    {
        /// <summary>
        ///     Check the supplied game history log against some other stored information and take
        ///     appropriate actions if there is any discrepancy.
        /// </summary>
        /// <param name="gameHistory">The history for a game that has just ended</param>
        void Validate(IGameHistoryLog gameHistory);
    }
}
