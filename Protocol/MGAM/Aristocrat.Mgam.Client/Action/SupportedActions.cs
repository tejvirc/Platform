namespace Aristocrat.Mgam.Client.Action
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Contains a collection of supported actions.
    /// </summary>
    public class SupportedActions
    {
        private static readonly ActionInfo[] Actions =
        {
            new ActionInfo(new Guid("{EBB8E262-C96E-4B28-8A9A-3C0CF3189339}"), "ATIOperator", "Can do Operator operations"),
            new ActionInfo(new Guid("{5F5E2E27-4E31-4FEE-BF0D-03719F4D04C8}"), "ATITechnician", "Can do Technician operations")
        };

        /// <summary>
        ///     Gets a list of supported actions.
        /// </summary>
        /// <returns><see cref="ActionInfo"/> enumeration.</returns>
        public static IEnumerable<ActionInfo> Get() => Actions;
    }
}
