namespace Aristocrat.Monaco.Hhr.Commands
{
    using System.Threading;

    public class RequestPlay
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RequestPlay" /> class.
        /// </summary>
        public RequestPlay(
            int cashBalance,
            int couponBalance,
            int paytableIndex,
            int numberOfCredits,
            int denomination,
            int gameUpcNumber,
            long transactionId,
            int outcomesRequested,
            bool isRecovering,
            CancellationToken cancellationToken)
        {
            CashBalance = cashBalance;
            CouponBalance = couponBalance;
            PaytableIndex = paytableIndex;
            NumberOfCredits = numberOfCredits;
            Denomination = denomination;
            GameUpcNumber = gameUpcNumber;
            TransactionId = transactionId;
            OutcomesRequested = outcomesRequested;
            IsRecovering = isRecovering;
            CancellationToken = cancellationToken;
        }

        /// <summary>
        ///     Indicates whether Request for game play was initiated as a result of recovery.
        /// </summary>
        public bool IsRecovering { get; }

        /// <summary>
        ///     Gets the cash balance in cents
        /// </summary>
        public int CashBalance { get; }

        /// <summary>
        ///     Gets the coupon balance in cents
        /// </summary>
        public int CouponBalance { get; }

        /// <summary>
        ///     Gets the paytable index for the game round
        /// </summary>
        public int PaytableIndex { get; }

        /// <summary>
        ///     Gets the number of credits wagered for the game round
        /// </summary>
        public int NumberOfCredits { get; }

        /// <summary>
        ///     Gets the denomination of the game being played
        /// </summary>
        public int Denomination { get; }

        /// <summary>
        ///     Gets the UPC number for the game being played
        /// </summary>
        public int GameUpcNumber { get; }

        /// <summary>
        ///     Gets the transaction Id
        /// </summary>
        public long TransactionId { get; }

        /// <summary>
        ///     A CancellationToken
        /// </summary>
        public CancellationToken CancellationToken { get; }

        public int OutcomesRequested { get; }
    }
}