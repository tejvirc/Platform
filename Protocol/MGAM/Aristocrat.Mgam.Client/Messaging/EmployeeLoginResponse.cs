namespace Aristocrat.Mgam.Client.Messaging
{
    using System.Collections.Generic;
    using Action;

    /// <summary>
    ///     This message is sent in response to a <see cref="EmployeeLogin"/>. It returns the name and ID of the
    ///     employee.Additionally returned is a list of all Actions registered by the VLT that this employee
    ///     has been granted permission to perform.
    /// </summary>
    public class EmployeeLoginResponse : Response
    {
        /// <summary>
        ///     Gets or sets CardString.
        /// </summary>
        public string CardString { get; set; }

        /// <summary>
        ///     Gets or sets EmployeeName
        /// </summary>
        public string EmployeeName { get; set; }

        /// <summary>
        ///     Gets or sets EmployeeId
        /// </summary>
        public int EmployeeId { get; set; }

        /// <summary>
        ///     Gets or sets Actions.
        /// </summary>
        public IReadOnlyList<ActionInfo> Actions { get; internal set; }
    }
}
