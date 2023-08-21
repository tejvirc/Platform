namespace Aristocrat.Monaco.Gaming.Presentation.Store;

public record AttendantUpdateServiceAvailableAction
{
    public AttendantUpdateServiceAvailableAction(bool isAvaiable)
    {
        IsAvaiable = isAvaiable;
    }

    public bool IsAvaiable { get; }
}
