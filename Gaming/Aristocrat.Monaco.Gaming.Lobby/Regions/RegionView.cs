namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

public struct RegionView
{
    public string Name { get; init;  }

    public bool IsActive { get; set; }

    public object View { get; init; }
}
