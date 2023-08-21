namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using System;

public record ReplayStartedAction
{
    public ReplayStartedAction(
        string gameName,
        string label,
        long sequence,
        DateTime startTime,
        long endCredits,
        decimal gameWinBonusAwarded,
        decimal voucherOut,
        decimal hardMeterOut,
        decimal bonusOrGameWinToCredits,
        decimal bonusOrGameWinToHandpay,
        decimal cancelledCredits)
    {
        GameName = gameName;
        Label = label;
        Sequence = sequence;
        StartTime = startTime;
        EndCredits = endCredits;
        GameWinBonusAwarded = gameWinBonusAwarded;
        VoucherOut = voucherOut;
        HardMeterOut = hardMeterOut;
        BonusOrGameWinToCredits = bonusOrGameWinToCredits;
        BonusOrGameWinToHandpay = bonusOrGameWinToHandpay;
        CancelledCredits = cancelledCredits;
    }

    public string GameName { get; }

    public string Label { get; }

    public long Sequence { get; }

    public DateTime StartTime { get; }

    public long EndCredits { get; }

    public decimal GameWinBonusAwarded { get; }

    public decimal VoucherOut { get; }

    public decimal HardMeterOut { get; }

    public decimal BonusOrGameWinToCredits { get; }

    public decimal BonusOrGameWinToHandpay { get; }

    public decimal CancelledCredits { get; }
}
