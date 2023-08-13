namespace Aristocrat.Monaco.Gaming.Lobby.Store.Replay;

using System;

public record ReplayState
{
    public bool IsStarted { get; init; }

    public bool IsCompleted { get; init; }

    public bool IsPaused { get; init; }

    public string GameName { get; init; }

    public string Label { get; init; }

    public long Sequence { get; init; }

    public DateTime StartTime { get; init; }

    public long EndCredits { get; init; }

    public decimal GameWinBonusAwarded { get; init; }

    public long VoucherOut { get; init; }

    public long HardMeterOut { get; init; }

    public long BonusOrGameWinToCredits { get; init; }

    public long BonusOrGameWinToHandpay { get; init; }

    public long CancelledCredits { get; init; }
}
