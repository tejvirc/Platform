namespace Aristocrat.Runtime.V1;

using System;
using System.Collections.Generic;

public class GameInfo
{
    public string? ThemeId { get; init; }

    public int GameId { get; init; }

    public string? Name { get; set; }

    public string? DllPath { get; init; }

    public long Denomination { get; init; }

    public IReadOnlyList<DenomInfo>? Denominations { get; init; }

    public string? BetOption { get; init; }

    public GameType GameType { get; init; }

    public string? GameSubtype { get; init; }

    public DateTime InstallDateTime { get; init; }

    public IReadOnlyList<string>? Tags { get; init; }

    public bool Enabled { get; init; }

    public GameCategory Category { get; init; }

    public GameSubCategory SubCategory { get; init; }

    public int MinimumWagerCredits { get; init; }
}
