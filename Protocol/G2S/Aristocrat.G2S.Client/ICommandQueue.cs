namespace Aristocrat.G2S.Client
{
    using System;
    using Communications;

    /// <summary>
    ///     Provides a mechanism to push message into a command queue.
    /// </summary>
    public interface ICommandQueue : ICommBehavior, IQueueStatus
    {
        /// <summary>
        ///     Queue a notification to a host.
        /// </summary>
        /// <param name="notification">The notification to send to the host.</param>
        void SendNotification(IClass notification);

        /// <summary>
        ///     Queue a notification to a host.
        /// </summary>
        /// <param name="notification">The request to send to the host.</param>
        /// <param name="sessionTimeout">
        ///     The <see cref="TimeSpan" /> that specifies how long the a session time to
        ///     live will be set for.
        /// </param>
        void SendNotification(IClass notification, TimeSpan sessionTimeout);

        /// <summary>
        ///     Queue a request to a host.
        /// </summary>
        /// <param name="request">The request to send to the host.</param>
        /// <returns>The session created for the request.</returns>
        /// <remarks>The session time to live defaults to <see cref="P:DefaultSessionTimeout" />.</remarks>
        Session SendRequest(IClass request);

        /// <summary>
        ///     Queue a request to a host.
        /// </summary>
        /// <param name="request">The request to send to the host.</param>
        /// <param name="sessionTimeout">
        ///     The <see cref="TimeSpan" /> that specifies how long the a session time to
        ///     live will be set for.
        /// </param>
        /// <returns>The session created for the request.</returns>
        Session SendRequest(IClass request, TimeSpan sessionTimeout);

        /// <summary>
        ///     Queue a request to a host bypassing any queue related checks.
        /// </summary>
        /// <param name="request">The request to send to the host.</param>
        /// <param name="alwaysSend">
        ///     Bypasses the allow send on the host queue. This is reserved for communications class
        ///     (commsOnline and commsDisabled).
        /// </param>
        /// <returns>The session created for the request.</returns>
        /// <remarks>The session time to live defaults to <see cref="P:DefaultSessionTimeout" />.</remarks>
        Session SendRequest(IClass request, bool alwaysSend);

        /// <summary>
        ///     Queue a request to a host bypassing any queue related checks.
        /// </summary>
        /// <param name="request">The request to send to the host.</param>
        /// <param name="retryCount">The number of times to resend a command if the session expires.</param>
        /// <param name="alwaysSend">
        ///     Bypasses the allow send on the host queue. This is reserved for communications class
        ///     (commsOnline and commsDisabled).
        /// </param>
        /// <returns>The session created for the request.</returns>
        /// <remarks>The session time to live defaults to <see cref="P:DefaultSessionTimeout" />.</remarks>
        Session SendRequest(IClass request, int retryCount, bool alwaysSend);

        /// <summary>
        ///     Queue a request to a host.
        /// </summary>
        /// <param name="request">The request to send to the host.</param>
        /// <param name="callback">The <see cref="SessionCallback" /> delegate.</param>
        /// <returns>The session created for the request.</returns>
        /// <remarks>The session time to live defaults to <see cref="P:DefaultSessionTimeout" />.</remarks>
        Session SendRequest(IClass request, SessionCallback callback);

        /// <summary>
        ///     Queue a request to a host.
        /// </summary>
        /// <param name="request">The request to send to the host.</param>
        /// <param name="callback">The <see cref="SessionCallback" /> delegate.</param>
        /// <param name="retryCount">The number of times to resend a command if the session expires.</param>
        /// <returns>The session created for the request.</returns>
        /// <remarks>The session time to live defaults to <see cref="P:.DefaultSessionTimeout" />.</remarks>
        Session SendRequest(IClass request, SessionCallback callback, int retryCount);

        /// <summary>
        ///     Queue a request to a host.
        /// </summary>
        /// <param name="request">The request to send to the host.</param>
        /// <param name="callback">The <see cref="T:SessionCallback" /> delegate.</param>
        /// <param name="retryCount">The number of times to resend a command if the session expires.</param>
        /// <returns>The session created for the request.</returns>
        /// <param name="sessionTimeout">
        ///     The value that specifies how long the a session time to live will be set for in
        ///     milliseconds.
        /// </param>
        Session SendRequest(IClass request, SessionCallback callback, int retryCount, TimeSpan sessionTimeout);

        /// <summary>
        ///     Queue any responses to the <i>request</i> command.
        /// </summary>
        /// <param name="response">The <see cref="T:Aristocrat.G2S.ClassCommand" /> request to respond to.</param>
        void SendResponse(ClassCommand response);
    }
}