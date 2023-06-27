namespace Aristocrat.Monaco.Gaming.Lobby.Store;

public record UpdateAttractVideosAction
{
    public string? TopAttractVideoPath { get; init; }

    public string? TopperAttractVideoPath { get; init; }

    public string? BottomAttractVideoPath { get; init; }
}
