namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

public struct ViewItem
{
    public string ViewName { get; init; }

    public object View { get; init; }

    public bool IsActive { get; set; }
}
