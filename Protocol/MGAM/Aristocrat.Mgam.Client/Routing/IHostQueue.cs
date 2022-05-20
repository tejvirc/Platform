namespace Aristocrat.Mgam.Client.Routing
{
    /// <summary>
    ///     Queues message to be sent to the host and dispatches messages received from the host.
    /// </summary>
    internal interface IHostQueue : IBroadcastRouter, IRequestRouter
    {
    }
}
