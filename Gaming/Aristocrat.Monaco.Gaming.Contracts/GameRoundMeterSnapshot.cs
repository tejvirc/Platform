namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;

    /// <summary>
    ///     An extended snapshot of game meters recording during gameplay.
    /// </summary>
    [Serializable]
    public class GameRoundMeterSnapshot
    {
        /// <summary>
        ///     The playstate of the game at the time of the snapshot.
        /// </summary>
        public PlayState PlayState { get; set; }

        /// <summary>
        ///     Gets or sets the credits at the time of the snapshot.
        /// </summary>
        public long CurrentCredits { get; set; }

        /// <summary>
        ///     Gets or sets the total amount bet.
        /// </summary>
        public long WageredAmount { get; set; }

        /// <summary>
        ///     Gets or sets the EGM-paid game won amount.
        /// </summary>
        public long EgmPaidGameWonAmount { get; set; }

        /// <summary>
        ///     Gets or sets the EGM-paid game bonus amount.
        /// </summary>
        public long EgmPaidGameWinBonusAmount { get; set; }

        /// <summary>
        ///     Gets or sets the EGM-paid bonus cashable in amount.
        /// </summary>
        public long EgmPaidBonusCashableInAmount { get; set; }

        /// <summary>
        ///     Gets or sets the EGM-paid bonus non-cash-in amount.
        /// </summary>
        public long EgmPaidBonusNonCashInAmount { get; set; }

        /// <summary>
        ///     Gets or sets the EGM-paid bonus promo-in amount.
        /// </summary>
        public long EgmPaidBonusPromoInAmount { get; set; }

        /// <summary>
        ///     Gets or sets the handpaid game win bonus amount.
        /// </summary>
        public long HandPaidGameWinBonusAmount { get; set; }

        /// <summary>
        ///     Gets or sets the handpaid game won amount.
        /// </summary>
        public long HandPaidGameWonAmount { get; set; }

        /// <summary>
        ///     Gets or sets the handpaid progressive won amount.
        /// </summary>
        public long HandPaidProgWonAmount { get; set; }

        /// <summary>
        ///     Gets or sets the handpaid bonus cashable in amount.
        /// </summary>
        public long HandPaidBonusCashableInAmount { get; set; }

        /// <summary>
        ///     Gets or sets the handpaid bonus non-cash-in amount.
        /// </summary>
        public long HandPaidBonusNonCashInAmount { get; set; }

        /// <summary>
        ///     Gets or sets the handpaid bonus promo-in amount.
        /// </summary>
        public long HandPaidBonusPromoInAmount { get; set; }

        /// <summary>
        ///     Gets or sets the total coin in.
        /// </summary>
        public long TrueCoinIn { get; set; }

        /// <summary>
        ///     Gets or sets the total bills in.
        /// </summary>
        public long CurrencyInAmount { get; set; }

        /// <summary>
        ///     Gets or sets the voucher-in cashable amount.
        /// </summary>
        public long VoucherInCashableAmount { get; set; }

        /// <summary>
        ///     Gets or sets the voucher-in cashable promo amount.
        /// </summary>
        public long VoucherInCashablePromoAmount { get; set; }

        /// <summary>
        ///     Gets or sets the voucher-in non-cashable  amount.
        /// </summary>
        public long VoucherInNonCashableAmount { get; set; }

        /// <summary>
        ///     Gets or sets the voucher-in non-transferable amount.
        /// </summary>
        public long VoucherInNonTransferableAmount { get; set; }

        /// <summary>
        ///     Gets or sets the total coin out.
        /// </summary>
        public long TrueCoinOut { get; set; }

        /// <summary>
        ///     Gets or sets the voucher-out cashable amount.
        /// </summary>
        public long VoucherOutCashableAmount { get; set; }

        /// <summary>
        ///     Gets or sets the voucher-out cashable promo amount.
        /// </summary>
        public long VoucherOutCashablePromoAmount { get; set; }

        /// <summary>
        ///     Gets or sets the voucher-out non-cashable promo amount.
        /// </summary>
        public long VoucherOutNonCashableAmount { get; set; }

        /// <summary>
        ///     Gets or sets the total cancelled credit.
        /// </summary>
        public long HandpaidCancelAmount { get; set; }

        /// <summary>
        ///     Gets or sets the total coin drop.
        /// </summary>
        public long CoinDrop { get; set; }

        /// <summary>
        ///     Gets or sets the total attendant-paid bonus.
        /// </summary>
        public long HandPaidBonusAmount { get; set; }

        /// <summary>
        ///     Gets or sets the total machine-paid bonus.
        /// </summary>
        public long EgmPaidBonusAmount { get; set; }

        /// <summary>
        ///     Gets or sets the total gambles played.
        /// </summary>
        public long SecondaryPlayedCount { get; set; }

        /// <summary>
        ///     Gets or sets the total gamble amount wagered.
        /// </summary>
        public long SecondaryWageredAmount { get; set; }

        /// <summary>
        ///     Gets or sets the total gamble amount won.
        /// </summary>
        public long SecondaryWonAmount { get; set; }

        /// <summary>
        ///     Gets or sets the total cashable electronic transfer in.
        /// </summary>
        public long WatOnCashableAmount { get; set; }

        /// <summary>
        ///     Gets or sets the total cashable electronic transfer out.
        /// </summary>
        public long WatOffCashableAmount { get; set; }

        /// <summary>
        ///     Gets or sets the total non-cashable electronic promo in.
        /// </summary>
        public long WatOnNonCashableAmount { get; set; }

        /// <summary>
        ///     Gets or sets the total non-cashable electronic promo out.
        /// </summary>
        public long WatOffNonCashableAmount { get; set; }

        /// <summary>
        ///     Gets or sets the total cashable electronic promo in.
        /// </summary>
        public long WatOnCashablePromoAmount { get; set; }

        /// <summary>
        ///     Gets or sets the total electronic promo out.
        /// </summary>
        public long WatOffCashablePromoAmount { get; set; }

        /// <summary>
        ///     Gets or sets the total machine-paid progressive amount.
        /// </summary>
        public long EgmPaidProgWonAmount { get; set; }

        /// <summary>
        ///     Gets or sets the total cashable promotional credits wagered.
        /// </summary>
        public long WageredPromoAmount { get; set; }

        /// <summary>
        /// Gets of sets the HardMeterOut Amount;
        /// </summary>
        public long HardMeterOutAmount { get; set; }
    }
}
