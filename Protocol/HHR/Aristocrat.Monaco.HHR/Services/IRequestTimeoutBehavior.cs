namespace Aristocrat.Monaco.Hhr.Services
{
    using Client.WorkFlow;

    /// <summary>
    ///     Interface which defines behavior of what happens when a Request Timeout.
    /// </summary>
    /// <typeparam name="TBehaviour"></typeparam>
    public interface IRequestTimeoutBehavior<in TBehaviour> where TBehaviour : IRequestTimeout
    {
        /// <summary>
        ///     Things to happen on Entry i.e. When Request Timesout.
        /// </summary>
        /// <param name="timeoutBehavior">Behavior attributes</param>
        void OnEntry(TBehaviour timeoutBehavior);

        /// <summary>
        ///     Things to happen on Exit i.e. When request is sent to server.
        /// </summary>
        /// <param name="timeoutBehavior">Timeout behavior attributes</param>
        void OnExit(TBehaviour timeoutBehavior);
    }
}