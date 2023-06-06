namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Accounting.Contracts.Handpay;

    /// <summary>
    ///     Manages registered overlay presentations and strategies for the game
    /// </summary>
    public interface IOverlayMessageStrategyController : IDisposable
    {
        /// <summary>
        ///     returns whether the game has registered 
        /// </summary>
        bool GameRegistered { get; }

        /// <summary>
        ///     returns the list of presentations for which the game has registered
        /// </summary>
        IList<PresentationOverrideTypes> RegisteredPresentations { get; }

        /// <summary>
        ///     returns the primary OverlayStrategy
        /// </summary>
        IOverlayMessageStrategy OverlayStrategy { get; }

        /// <summary>
        ///     returns the fall back strategy
        /// </summary>
        IOverlayMessageStrategy FallBackStrategy { get; }

        /// <summary>
        ///     Registers presentations that the game wishes to override
        /// </summary>
        /// <param name="registered">Whether the game has registered presentations or not</param>
        /// <param name="types">Which presentations the game wishes to register for</param>
        /// <returns>If the presentations were successfully registered</returns>
        Task<bool> RegisterPresentation(bool registered, IEnumerable<PresentationOverrideTypes> types);

        /// <summary>
        ///     Sets the LastCashOutAmount on both the overlay strategy and fallback strategy
        /// </summary>
        /// <param name="cashOutAmount">Last cash out amount</param>
        void SetLastCashOutAmount(long cashOutAmount);

        /// <summary>
        ///     Sets the handpay amount, handpay type, and wager amount for both the overlay and fallback strategy
        /// </summary>
        /// <param name="handpayAmount">Handpay amount</param>
        /// <param name="handpayType">Last handpay type</param>
        /// <param name="wagerAmount">Wager amount</param>
        void SetHandpayAmountAndType(long handpayAmount, HandpayType handpayType, long wagerAmount);

        /// <summary>
        ///     Tells the game to remove any presentation
        /// </summary>
        void ClearGameDrivenPresentation();

        /// <summary>
        /// Sets the Current pay out amount
        /// </summary>
        /// <param name="cashOutAmount"></param>
        void SetCashableAmount(long cashOutAmount);
    }
}
