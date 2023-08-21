using Aristocrat.Monaco.Kernel;

namespace Aristocrat.Monaco.Application.Contracts.OperatorMenu
{
    /// <summary>
    ///     Event to request access to the operator menu based on user role of technician or not technician
    /// </summary>
    public class OperatorMenuAccessRequestedEvent : BaseEvent
    {
        /// <summary>
        ///     Instantiates an instance of the OperatorMenuAccessRequestedEvent class.
        /// </summary>
        /// <param name="isTechnician">Is the technician menu being requested</param>
        /// <param name="employeeId">The employee id.</param>
        /// <param name="forceTechnicianMode">Force audit menu to remain in Technician mode while a jurisdiction-specific condition applies</param>
        public OperatorMenuAccessRequestedEvent(bool isTechnician, string employeeId, bool forceTechnicianMode = false)
        {
            EmployeeId = employeeId;
            IsTechnician = isTechnician;
            ForceTechnicianMode = forceTechnicianMode;
        }

        /// <summary>
        ///     Gets EmployeeId.
        /// </summary>
        public string EmployeeId { get; }

        /// <summary>
        ///     Gets IsTechnician.
        /// </summary>
        public bool IsTechnician { get; }

        /// <summary>
        ///     Force audit menu to remain in Technician mode the entire time it's open
        ///     Currently only used by NYL when Technician card is inserted
        /// </summary>
        public bool ForceTechnicianMode { get; }
    }
}