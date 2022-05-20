namespace Aristocrat.Monaco.Application.Contracts.OperatorMenu
{
    using Kernel;

    /// <summary>
    ///     Publish this event to set the operator menu role
    /// </summary>
    public class UpdateOperatorMenuRoleEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public UpdateOperatorMenuRoleEvent(bool isTechnicianRole)
        {
            IsTechnicianRole = isTechnicianRole;
        }

        /// <summary>
        ///     True to set the role to Technician
        /// </summary>
        public bool IsTechnicianRole { get; }
    }
}
