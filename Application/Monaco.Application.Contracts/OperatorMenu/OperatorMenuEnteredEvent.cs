namespace Aristocrat.Monaco.Application.Contracts.OperatorMenu
{
    using System;
    using Kernel;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     An event to notify that the operator menu has been entered.
    /// </summary>
    /// <remarks>
    ///     This event will be posted when the <c>Show()</c> method of <see cref="IOperatorMenuLauncher" />
    ///     is called and the operator menu is displayed from the hiding status.
    /// </remarks>
    [Serializable]
    public class OperatorMenuEnteredEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorMenuEnteredEvent" /> class.
        /// </summary>
        /// <param name="role">Role that was assigned when entering the Operator Menu</param>
        /// <param name="operatorId">The ID of the operator who is entering the Operator Menu</param>
        public OperatorMenuEnteredEvent(string role = "", string operatorId = "")
        {
            Role = role;
            OperatorId = operatorId;
        }

        /// <summary>
        ///     Gets the Role that was assigned when entering the Operator Menu
        /// </summary>
        public string Role { get; }

        /// <summary>
        ///     Gets the ID of the operator who is entering the Operator Menu
        /// </summary>
        public string OperatorId { get; }

        /// <summary>
        /// Gets whether this event indicates we are going into the Technician Menu
        /// </summary>
        public bool IsTechnicianRole => Role == ApplicationConstants.TechnicianRole;

        /// <inheritdoc />
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(OperatorId))
            {
                return FormattableString.Invariant($"{base.ToString()} {Role} ID: {OperatorId}");

            }
            return FormattableString.Invariant($"{base.ToString()} {Role}");
        }
    }
}
