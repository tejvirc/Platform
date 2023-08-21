namespace Aristocrat.Monaco.Gaming.Presentation.Store;

public record AttractUpdateVideosAction
{
    public string? TopAttractVideoPath { get; init; }

    public string? TopperAttractVideoPath { get; init; }

    public string? BottomAttractVideoPath { get; init; }
}
