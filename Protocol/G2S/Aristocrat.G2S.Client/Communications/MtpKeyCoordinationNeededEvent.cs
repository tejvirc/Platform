namespace Aristocrat.G2S.Client.Communications
{
    using Aristocrat.Monaco.Kernel;

    /// <summary>This event is raised when MTP shared secret security has failed
    /// five consecutive times and a coordination is needed.</summary>
    public class MtpKeyCoordinationNeededEvent : BaseEvent
    {
    }
}
