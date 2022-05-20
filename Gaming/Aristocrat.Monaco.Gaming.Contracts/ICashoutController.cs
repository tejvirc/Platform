namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     A Controller class listening to Printer events to ensure that no-printing is done when a paper is in chute
    /// </summary>
    public interface ICashoutController
    {
        /// <summary>
        ///     Gets whether or not paper in chute is currently active
        /// </summary>
        bool PaperIsInChute { get; }

        /// <summary>
        ///     Gets whether or not the paper in chute notification is currently active
        /// </summary>
        bool PaperInChuteNotificationActive { get; }

        /// <summary>
        ///     Performs the request to cashout the machine
        /// </summary>
        void GameRequestedCashout();
    }
}