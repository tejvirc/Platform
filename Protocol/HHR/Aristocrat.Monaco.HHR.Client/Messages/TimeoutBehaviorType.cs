namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Defines types of behavior when a request timeout.
    /// </summary>
    public enum TimeoutBehaviorType
    {
        /// <summary>
        ///     Does nothing. This is default behavior for all request.
        /// </summary>
        Idle,

        /// <summary>
        ///     Machine Lockups up with key, string defined by request.
        /// </summary>
        Lockup
    }
}