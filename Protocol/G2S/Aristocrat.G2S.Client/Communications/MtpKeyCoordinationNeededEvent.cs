namespace Aristocrat.G2S.Client.Communications
{
    using Aristocrat.Monaco.Kernel;

    /// <summary>This event is raised when MTP shared secret security has failed
    /// five consecutive times and a coordination is needed.</summary>
    public class MtpKeyCoordinationNeededEvent : BaseEvent
    {
        /// <summary>The multicast identifier.</summary>
        public string MulticastId { get; set; }

        /// <summary>The G2S host ID.</summary>
        public int HostId { get; set; }
    }
}
