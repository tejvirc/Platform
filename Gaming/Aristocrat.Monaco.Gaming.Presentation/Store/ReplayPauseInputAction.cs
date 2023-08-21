namespace Aristocrat.Monaco.Gaming.Presentation.Store;

public record ReplayPauseInputAction
{
    public ReplayPauseInputAction(bool isPaused)
    {
        IsPaused = isPaused;
    }

    public bool IsPaused { get; }
}
