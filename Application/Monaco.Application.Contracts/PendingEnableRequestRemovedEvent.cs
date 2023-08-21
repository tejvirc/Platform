namespace Aristocrat.Monaco.Application.Contracts
{
    using Kernel;

    /// <summary>
    ///     An event used to notify that a pending enable request has been removed.
    /// </summary>
    /// <remarks>
    ///     Posted by the NoteAcceptorCoordinator whenever the a pending enable request is removed to enable the
    ///     note acceptor.  Consumed by the NoteAcceptorMonitor to place any active note acceptor error into a
    ///     pending status if there are credits on the EGM.
    /// </remarks>
    public class PendingEnableRequestRemovedEvent : BaseEvent
    {
    }
}