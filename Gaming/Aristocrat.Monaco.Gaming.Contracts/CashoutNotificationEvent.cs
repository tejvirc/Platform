namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    ///   A <see cref="CashoutNotificationEvent" /> is a secondary event posted when a paper in chute(or its absence) is noted.
    ///   The event allows the subscribers to take steps (such as display warning) in cashout flow.
    /// </summary>

    public class CashoutNotificationEvent : BaseEvent
    {
        /// <summary>
        ///    True if paper is in chute. False if paper is not in chute.
        /// </summary>
        public bool PaperIsInChute { get; }

        /// <summary>
        ///    True if this is a refresh event
        /// </summary>
        public bool IsResending { get; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="inChute"></param>
        /// <param name="isResending"></param>
        public CashoutNotificationEvent(bool inChute, bool isResending = false)
        {
            PaperIsInChute = inChute;
            IsResending = isResending;
        }

    }
}
