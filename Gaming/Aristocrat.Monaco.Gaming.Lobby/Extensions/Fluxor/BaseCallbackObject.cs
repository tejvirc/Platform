namespace Aristocrat.Extensions.Fluxor;

public class BaseCallbackObject<TPayload>
    where TPayload : BasePayload
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string type { get; set; }
    public TPayload payload { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}

public class BaseCallbackObject : BaseCallbackObject<BasePayload> { }
