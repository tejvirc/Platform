namespace Aristocrat.Monaco.Gaming.Presentation.Store.Replay;

using System;

public record ReplayState
{
    public bool IsStarted { get; init; }

    public bool IsCompleted { get; init; }

    public bool IsPaused { get; init; }

    public string? GameName { get; init; }

    public string? Label { get; init; }

    public long Sequence { get; init; }

    public DateTime StartTime { get; init; }

    public long EndCredits { get; init; }

    public decimal GameWinBonusAwarded { get; init; }

    public decimal VoucherOut { get; init; }

    public decimal HardMeterOut { get; init; }

    public decimal BonusOrGameWinToCredits { get; init; }

    public decimal BonusOrGameWinToHandpay { get; init; }

    public decimal CancelledCredits { get; init; }
}
