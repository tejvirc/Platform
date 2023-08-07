namespace Aristocrat.Monaco.Protocol.Common.DisableProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Kernel;

    public interface IProtocolDisableProvider<in TDisableStates> where TDisableStates : Enum
    {
        /// <summary>
        ///     Disables the system with an appropriate status message for the operator menu for each of the set disabled state bits
        /// </summary>
        /// <param name="states">The disable states to set.</param>
        /// <param name="priority">The priority level for the disable.</param>
        /// <returns>The task for disabling the client for requested disabled states</returns>
        Task Disable(SystemDisablePriority priority, params TDisableStates[] states);

        /// <summary>
        ///     Disables the system with an appropriate status message for the operator menu for the disabled state
        /// </summary>
        /// <param name="state">The disable state to set.</param>
        /// <param name="priority">The priority level for the disable.</param>
        /// <param name="isLockup">Whether or not to generate a lockup</param>
        /// <returns>The task for disabling the client for the requested disable state</returns>
        Task Disable(SystemDisablePriority priority, TDisableStates state, bool isLockup);

        /// <summary>
        ///     Clears the specific disable state provided.
        /// </summary>
        /// <param name="states">The states to clear.</param>
        /// <returns>The task for enabling the client for requested disabled states</returns>
        Task Enable(params TDisableStates[] states);

        /// <summary>
        ///     Gets whether or not the current system is disabled for the provided state
        /// </summary>
        /// <param name="checkState">The state to check if the system is currently disabled by</param>
        /// <returns>Whether or not the current system is disabled for the provided state</returns>
        bool IsDisableStateActive(TDisableStates checkState);

        /// <summary>
        ///     Gets whether or not the current system has the active soft error for the provided state
        /// </summary>
        /// <param name="checkState">The state to check if the system is currently in a soft error for the provided disable</param>
        /// <returns>Whether or not the current system is disabled for the provided state</returns>
        bool IsSoftErrorStateActive(TDisableStates checkState);

    }
}
