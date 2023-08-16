namespace Aristocrat.Monaco.Application.Contracts.Authentication
{
    using System;
    using System.Globalization;
    using System.Threading;
    using Kernel;
    using Localization;
    using Monaco.Localization.Properties;
    using ProtoBuf;

    /// <summary>
    ///     An event when a component hash completes
    /// </summary>

    [ProtoContract]
    [CLSCompliant(false)]
    public class ComponentHashCompleteEvent : BaseEvent
    {
        private const string EventDescriptionNameDelimiter = " - ";

        /// <summary>
        /// Empty construcotr for deserialization
        /// </summary>
        public ComponentHashCompleteEvent()
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="compVer">Component verification</param>
        /// <param name="taskToken">Use the group cancellation token as an ID.</param>
        public ComponentHashCompleteEvent(ComponentVerification compVer, CancellationToken taskToken)
        {
            ComponentVerification = compVer;
            TaskToken = taskToken;
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

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join(
                EventDescriptionNameDelimiter,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ComponentHashCompleted),
                string.Format(CultureInfo.InvariantCulture, $"{ComponentVerification.ComponentId}"));
        }
    }
}