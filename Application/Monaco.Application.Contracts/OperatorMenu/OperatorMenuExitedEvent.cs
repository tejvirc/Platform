namespace Aristocrat.Monaco.Application.Contracts.OperatorMenu
{
    using System;
    using Kernel;
    using ProtoBuf;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     An event to notify that the operator menu has been exited.
    /// </summary>
    /// <remarks>
    ///     This event will be posted when the <c>Hide()</c> method of <see cref="IOperatorMenuLauncher" />
    ///     is called and the operator menu is hidden from the showing status.
    /// </remarks>
    [ProtoContract]
    public class OperatorMenuExitedEvent : BaseEvent
    {
        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        public OperatorMenuExitedEvent() : this(string.Empty)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorMenuExitedEvent" /> class.
        /// </summary>
        /// <param name="operatorId">The ID of the operator who is entering the Operator Menu</param>
        public OperatorMenuExitedEvent(string operatorId = "")
        {
            OperatorId = operatorId;
        }

        /// <summary>
        ///     Gets the ID of the operator who is entering the Operator Menu
        /// </summary>
        [ProtoMember(1)]
        public string OperatorId { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(OperatorId))
            {
                return FormattableString.Invariant($"{base.ToString()} ID: {OperatorId}");

            }
            return FormattableString.Invariant($"{base.ToString()}");
        }
    }
}
