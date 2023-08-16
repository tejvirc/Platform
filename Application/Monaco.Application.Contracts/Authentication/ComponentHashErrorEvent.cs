namespace Aristocrat.Monaco.Application.Contracts.Authentication
{
    using System;
    using System.Globalization;
    using System.Threading;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     An event when a component hash results in an error
    /// </summary>

    [ProtoContract]
    [CLSCompliant(false)]
    public class ComponentHashErrorEvent : BaseEvent
    {
        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        public ComponentHashErrorEvent()
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="compVer">Component verification</param>
        /// <param name="taskToken">Use the group cancellation token as an ID.</param>
        /// <param name="error">The error message.</param>
        public ComponentHashErrorEvent(ComponentVerification compVer, CancellationToken taskToken, string error)
        {
            ComponentVerification = compVer;
            TaskToken = taskToken;
            Error = error;
        }

        /// <summary>
        ///     Component verification
        /// </summary>
        [ProtoMember(1)]
        public ComponentVerification ComponentVerification { get; }

        /// <summary>
        ///     Use the cancellation token as a unique task identifier.
        /// </summary>
        public CancellationToken TaskToken { get; }

        /// <summary>
        ///     The error message.
        /// </summary>
        [ProtoMember(2)]
        public string Error { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                $"{GetType().Name} - {ComponentVerification.ComponentId} - {Error}");
        }
    }
}