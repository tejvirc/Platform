namespace Aristocrat.Monaco.Bingo.Common
{
    /// <summary>
    ///     The transaction types used in the
    ///     TransactionReport message to the bingo server
    /// </summary>
    public enum TransactionType
    {
        None = 0,
        CashIn,
        CashOut,
        CashPlayed,
        CashWon,
        GamesPlayed,
        GamesWon,
        Drop,
        Open,
        Close,
        Variance,
        CashOutJackpot,
        HandPayKeyOff,
        TicketIn,
        CancelledCredits,
        CashPromoTicketIn,
        TransferablePromoTicketIn,
        NonTransferablePromoTicketIn,
        CashPromoTicketOut,
        TransferablePromoTicketOut,
        NonTransferablePromoTicketOut,
        TransferIn,
        CashPromoTransferIn,
        TransferablePromoTransferIn,
        NonTransferablePromoTransferIn,
        TransferOut,
        CashPromoTransferOut,
        TransferablePromoTransferOut,
        NonTransferablePromoTransferOut,
        BonusWin,
        BonusLargeWin,
        ProgressiveWin,
        ProgressiveLargeWin,
        SleptWin,
        LargeWin,
        ExternalBonusWin,
        ExternalBonusLargeWin,
        CashoutBonus,
        CashoutProgressive,
        CashoutExternalBonus
    }
}
