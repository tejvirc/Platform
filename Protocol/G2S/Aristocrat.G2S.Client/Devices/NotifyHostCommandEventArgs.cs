namespace Aristocrat.G2S.Client.Devices
{
    using System;
    using Diagnostics;

    /// <summary>
    ///     Provides data for an EGM command notify event.
    /// </summary>
    /// <typeparam name="TCommand">The type of the notify command</typeparam>
    public class NotifyHostCommandEventArgs<TCommand> : HostEventArgs
        where TCommand : ICommand
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NotifyHostCommandEventArgs{TCommand}" /> class with the
        ///     specified <i>hostId</i>,
        ///     <i>deviceId</i>, and <i>notifyCommand</i> set.
        /// </summary>
        /// <param name="hostId">The ID of the Host which originated the event.</param>
        /// <param name="deviceId">The ID of the device which originated the event.</param>
        /// <param name="notifyCommand">The command received from the EGM.</param>
        public NotifyHostCommandEventArgs(int hostId, int deviceId, TCommand notifyCommand)
            : base(hostId, deviceId)
        {
            if (notifyCommand == null)
            {
                SourceTrace.TraceError(
                    G2STrace.Source,
                    @"NotifyCommandEventArgs.ctor : null notifyCommand");

                throw new ArgumentNullException(nameof(notifyCommand));
            }

            NotifyCommand = notifyCommand;
            Error = new Error();
        }

        /// <summary>
        ///     Gets get the error code.
        /// </summary>
        /// <remarks><see cref="Error" /> <see cref="P:Error.Code" /> defaults to <i>G2S_none</i>.</remarks>
        public Error Error { get; }

        /// <summary>
        ///     Gets get the notify command.
        /// </summary>
        public TCommand NotifyCommand { get; }
    }
}