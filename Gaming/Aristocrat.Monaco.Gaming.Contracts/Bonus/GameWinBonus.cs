namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    using System;

    /// <summary>
    ///     Defines a game win bonus type
    /// </summary>
    public class GameWinBonus : BonusRequest
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameWinBonus" /> class.
        /// </summary>
        /// <param name="bonusId">The bonus identifier</param>
        /// <param name="cashableAmount">The cashable amount</param>
        /// <param name="nonCashAmount">The non cashable amount</param>
        /// <param name="promoAmount">The promotional amount</param>
        /// <param name="payMethod">The pay method</param>
        /// <param name="mode">The bonus mode</param>
        /// <param name="exception">
        ///     Allows the requestor to define an exception due to an invalid request. The transaction will be
        ///     created and then terminated when the exception is not None
        /// </param>
        /// <param name="protocol">The related protocol</param>
        public GameWinBonus(
            string bonusId,
            long cashableAmount,
            long nonCashAmount,
            long promoAmount,
            PayMethod payMethod,
            BonusMode mode = BonusMode.GameWin,
            BonusException exception = BonusException.None,
            CommsProtocol protocol = CommsProtocol.None)
            : base(bonusId, cashableAmount, nonCashAmount, promoAmount, payMethod, exception, protocol)
        {
            if (mode != BonusMode.GameWin)
            {
                throw new ArgumentOutOfRangeException(nameof(mode));
            }

            Mode = mode;
            MessageDuration = TimeSpan.MaxValue;
        }
    }
}