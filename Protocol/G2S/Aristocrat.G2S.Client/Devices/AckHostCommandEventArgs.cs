namespace Aristocrat.G2S.Client.Devices
{
    using Diagnostics;
    using System;

    /// <summary>
    ///     Provides data for an event which requests if a host command should be acknowledged.
    /// </summary>
    /// <typeparam name="TCommand">The command contract of that needs to be acknowledged.</typeparam>
    public class AckHostCommandEventArgs<TCommand> : HostEventArgs
        where TCommand : ICommand
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AckHostCommandEventArgs{TCommand}" /> class with the
        ///     specified
        ///     <i>egmId</i> and <i>deviceId</i> and the <see cref="P:Aristocrat.G2S.AckHostCommandEventArgs{T}.Ack" /> property
        ///     set
        ///     to the given value.
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="hostId">The ID of the host which originated the event.</param>
        /// <param name="deviceId">The ID of the device which originated the event.</param>
        /// <param name="request">The command that needs to be acknowledged.</param>
        /// <param name="ack"><b>true</b> to ack the command; otherwise, <b>false</b>.</param>
        public AckHostCommandEventArgs(int hostId, int deviceId, TCommand request, bool ack)
            : base(hostId, deviceId)
        {
            if (request == null)
            {
                SourceTrace.TraceError(
                    G2STrace.Source,
                    @"AckHostCommandEventArgs.ctor : null request");

                throw new ArgumentNullException(nameof(request));
            }

            Request = request;
            Ack = ack;
            Error = new Error();
        }

        /// <summary>
        ///     Gets the request command.
        /// </summary>
        public TCommand Request { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether the command should be acknowledged.
        /// </summary>
        /// <returns><b>true</b> if the command should be acknowledged; otherwise, <b>false</b>.</returns>
        public bool Ack { get; set; }

        /// <summary>
        ///     Gets the error.
        /// </summary>
        /// <remarks>
        ///     <see cref="T:Aristocrat.G2S.Error" />.<see cref="P:Aristocrat.G2S.Error.Code" /> defaults to
        ///     <i>G2S_none</i>.
        ///     <para />
        ///     If the <see cref="T:Aristocrat.G2S.Error" />.<see cref="P:Aristocrat.G2S.Error.Code" />
        ///     is set to a value other than <i>G2S_none</i> the <see cref="P:AckHostCommandEventArgs{T}.Ack" /> value will be
        ///     ignored and the <see cref="T:Aristocrat.G2S.Error" /> response will be sent instead.
        /// </remarks>
        /// <value>The error.</value>
        public Error Error { get; }
    }
}