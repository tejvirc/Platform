namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Accounting.Contracts.Handpay;
    using Models;

    /// <summary>
    ///     Interface used to control the Overlay messaging
    /// </summary>
    public interface IOverlayMessageStrategy
    {
        /// <summary>
        ///     The most recent cash out amount
        /// </summary>
        long LastCashOutAmount { get; set; }

        /// <summary>
        ///     The current handpay amount
        /// </summary>
        long HandpayAmount { get; set; }

        /// <summary>
        ///     The current handpay amount
        /// </summary>
        long LargeWinWager { get; set; }

        /// <summary>
        ///     The most recent handpay type paid
        /// </summary>
        HandpayType? LastHandpayType { get; set; }

        /// <summary>
        ///     Whether or not the cashout button is currently pressed
        /// </summary>
        bool CashOutButtonPressed { get; set; }

        /// <summary>
        ///     Gets a value indicating if the implementation is basic or not
        /// </summary>
        bool IsBasic { get; }

        /// <summary>
        /// 
        /// </summary>
        long CashableAmount { get; set; }

        /// <summary>
        ///     Handles Cash out messaging scenarios
        /// </summary>
        /// <param name="data">The message data to be displayed</param>
        /// <param name="lastCashOutForcedByMaxBank">If Cash out was forced by max bank</param>
        /// <param name="cashOutState">The Current Cash Out State</param>
        IMessageOverlayData HandleMessageOverlayCashOut(IMessageOverlayData data, bool lastCashOutForcedByMaxBank, LobbyCashOutState cashOutState);

        /// <summary>
        ///     Handles Cash in messaging scenarios
        /// </summary>
        /// <param name="data">The message data to be displayed</param>
        /// <param name="cashInType">The Type of money added</param>
        /// <param name="stateContainsCashOut">If Cashout is allowed in the current state</param>
        /// <param name="cashOutState">The Current Cash Out State</param>
        IMessageOverlayData HandleMessageOverlayCashIn(IMessageOverlayData data, CashInType cashInType, bool stateContainsCashOut, LobbyCashOutState cashOutState);

        /// <summary>
        ///     Handles Handpay message scenarios
        /// </summary>
        /// <param name="data">The message data to be displayed</param>
        /// <param name="subText2">Additional text if needed</param>
        IMessageOverlayData HandleMessageOverlayHandPay(IMessageOverlayData data, string subText2);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        IMessageOverlayData HandleMessageOverlayPayOut(IMessageOverlayData data);
    }
}
