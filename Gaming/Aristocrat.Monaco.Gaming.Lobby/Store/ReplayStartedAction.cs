namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using System;

public class ReplayStartedAction
{
    public ReplayStartedAction(
        string gameName,
        string label,
        long sequence,
        DateTime startTime,
        long endCredits,
        decimal gameWinBonusAwarded,
        long voucherOut,
        long hardMeterOut,
        long bonusOrGameWinToCredits,
        long bonusOrGameWinToHandpay,
        long cancelledCredits)
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

    public long VoucherOut { get; }

    public long HardMeterOut { get; }

    public long BonusOrGameWinToCredits { get; }

    public long BonusOrGameWinToHandpay { get; }

    public long CancelledCredits { get; }
}
