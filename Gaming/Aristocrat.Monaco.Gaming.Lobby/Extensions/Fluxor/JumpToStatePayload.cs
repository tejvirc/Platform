namespace Aristocrat.Extensions.Fluxor;

public class JumpToStatePayload : BasePayload
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public int index { get; set; }
    public int actionId { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
