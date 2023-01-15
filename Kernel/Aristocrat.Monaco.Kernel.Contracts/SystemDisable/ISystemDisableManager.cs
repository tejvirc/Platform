namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections.Generic;

    using Contracts.MessageDisplay;

    /// <summary>
    ///     Interface through which the XSpin system can be disabled and enabled for multiple,
    ///     simultaneous reasons.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The ISystemDisableManager does not take any action when the system is disabled and
    ///         enabled, other than posting notification events <see cref="SystemDisabledEvent" /> and
    ///         <see cref="SystemEnabledEvent" />.  Enabled and disabled are merely tracked as
    ///         mutually exclusive states.  It is the responsibility other components in the system to
    ///         perform necessary actions when the state changes.
    ///     </para>
    ///     <para>
    ///         Many unrelated components may want to disable the system.  When one wants to
    ///         re-enable the system, the system should only enable if nothing else wanted the
    ///         system to be disabled.  To support this need, each disabler/enabler uses a
    ///         GUID as identification.
    ///     </para>
    ///     <para>
    ///         Users of the ISystemDisableManager implementation should not expect disables
    ///         to persist across boots of the system.  Upon reboot, if the system should still
    ///         be disabled, components should use the interface to disable again.
    ///     </para>
    ///     <para>
    ///         The ISystemDisableManager does not post any messages to the system user.  Generally,
    ///         components want to message the user at the same time as disabling.  To do this, see
    ///         IMessageDisplay.
    ///     </para>
    ///     <para>
    ///         The following example shows a simple class that disables and enables the system.
    ///         <code><![CDATA[
    ///  class MyDisabler
    ///  {
    ///      private const SystemDisablePriority DisablePriority = SystemDisablePriority.Normal;
    /// 
    ///      private const string DisableReason = "An Example Disable";
    /// 
    ///      private static readonly Guid DisableGuid = new Guid("{BF156CF1-1517-4ddc-9AE2-84E12F96A2AE}");
    /// 
    ///      public void HandleError()
    ///      {
    ///          ISystemDisableManager manager = // get ISystemDisableManager reference
    ///          manager.Disable(DisableGuid, DisablePriority, () => DisableReason);
    ///      }
    /// 
    ///      public void HandleErrorClear()
    ///      {
    ///          ISystemDisableManager manager = // get ISystemDisableManager reference
    ///          manager.Enable(DisableGuid);
    ///      }
    ///  }
    ///  ]]></code>
    ///     </para>
    /// </remarks>
    /// <seealso cref="SystemDisablePriority" />
    public interface ISystemDisableManager
    {
        /// <summary>
        ///     Gets a value indicating whether the system is disabled or not
        /// </summary>
        bool IsDisabled { get; }

        /// <summary>
        ///     Gets a value indicating whether the system idle state is affected
        /// </summary>
        bool IsIdleStateAffected { get; }

        /// <summary>
        ///     Gets a list of keys that are currently disabling the system.
        /// </summary>
        IReadOnlyList<Guid> CurrentDisableKeys { get; }

        /// <summary>
        ///     Gets a list of keys that are currently disabling the system, with Immediate priority.
        /// </summary>
        IReadOnlyList<Guid> CurrentImmediateDisableKeys { get; }

        /// <summary>
        ///     Gets a value indicating whether the system should disable immediately.
        /// </summary>
        bool DisableImmediately { get; }

        /// <summary>
        ///     Adds a reason for the system to be disabled.  If the system is enabled, this will disable the system.
        /// </summary>
        /// <param name="enableKey">The unique value that will later re-enable this disable.</param>
        /// <param name="priority">The priority of this need to disable.</param>
        /// <param name="disableReasonResourceKey">Resource key of the disable reason</param>
        /// <param name="providerType">The type of the message culture provider</param>
        /// <param name="msgParams">The message parameters</param>
        public void Disable(Guid enableKey, SystemDisablePriority priority, string disableReasonResourceKey, CultureProviderType providerType, params object[] msgParams);

        /// <summary>
        ///     Adds a reason for the system to be disabled.  If the system is enabled, this will disable the system.
        /// </summary>
        /// <param name="enableKey">The unique value that will later re-enable this disable.</param>
        /// <param name="priority">The priority of this need to disable.</param>
        /// <param name="disableReasonResourceKey">Resource key of the disable reason</param>
        /// <param name="providerType">The type of the message culture provider</param>
        /// <param name="affectsIdleState">true (the default) if this disable state affects the cabinet idle state.</param>
        /// <param name="helpText">A human readable string that contains the reason of the lockup and general guidelines on how to clear it.</param>
        /// <param name="type">The type of the event that prompted this disable, if there is one</param>
        /// <param name="msgParams">The message parameters</param>
        void Disable(Guid enableKey, SystemDisablePriority priority, string disableReasonResourceKey, CultureProviderType providerType, bool affectsIdleState=true, Func<string> helpText=null, Type type = null, params object[] msgParams);

        /// <summary>
        ///     Adds a reason for the system to be disabled.  If the system is enabled, this will disable the system.
        /// </summary>
        /// <param name="enableKey">The unique value that will later re-enable this disable.</param>
        /// <param name="priority">The priority of this need to disable.</param>
        /// <param name="disableReason">A human readable reason for the disable</param>
        /// <param name="type">The type of the event that prompted this disable, if there is one</param>
        void Disable(Guid enableKey, SystemDisablePriority priority, Func<string> disableReason, Type type = null);

        /// <summary>
        ///     Adds a reason for the system to be disabled.  If the system is enabled, this will disable the system.
        /// </summary>
        /// <param name="enableKey">The unique value that will later re-enable this disable.</param>
        /// <param name="priority">The priority of this need to disable.</param>
        /// <param name="disableReason">A human readable reason for the disable</param>
        /// <param name="affectsIdleState">true (the default) if this disable state affects the cabinet idle state.</param>
        /// <param name="type">The type of the event that prompted this disable, if there is one</param>
        void Disable(Guid enableKey, SystemDisablePriority priority, Func<string> disableReason, bool affectsIdleState, Type type = null);

        /// <summary>
        ///     Adds a reason for the system to be disabled.  If the system is enabled, this will disable the system.
        /// </summary>
        /// <param name="enableKey">The unique value that will later re-enable this disable.</param>
        /// <param name="priority">The priority of this need to disable.</param>
        /// <param name="disableReason">A human readable reason for the disable</param>
        /// <param name="duration">The duration of the system disable condition</param>
        /// <param name="type">The type of the event that prompted this disable, if there is one</param>
        void Disable(Guid enableKey, SystemDisablePriority priority, Func<string> disableReason, TimeSpan duration, Type type = null);

        /// <summary>
        ///     Adds a reason for the system to be disabled.  If the system is enabled, this will disable the system.
        /// </summary>
        /// <param name="enableKey">The unique value that will later re-enable this disable.</param>
        /// <param name="priority">The priority of this need to disable.</param>
        /// <param name="disableReason">A human readable reason for the disable</param>
        /// <param name="affectsIdleState">true (the default) if this disable state affects the cabinet idle state.</param>
        /// <param name="helpText">The guidelines on how to clear the disable.</param>
        /// <param name="type">The type of the event that prompted this disable, if there is one</param>
        void Disable(Guid enableKey, SystemDisablePriority priority, Func<string> disableReason, bool affectsIdleState, Func<string> helpText, Type type = null);

        /// <summary>
        ///     Adds a reason for the system to be disabled.  If the system is enabled, this will disable the system.
        /// </summary>
        /// <param name="enableKey">The unique value that will later re-enable this disable.</param>
        /// <param name="priority">The priority of this need to disable.</param>
        /// <param name="disableReason">A human readable reason for the disable</param>
        /// <param name="duration">The duration of the system disable condition</param>
        /// <param name="affectsIdleState">true (the default) if this disable state affects the cabinet idle state.</param>
        /// <param name="type">The type of the event that prompted this disable, if there is one</param>
        /// <param name="helpText">A human readable string that contains the reason of the lockup and general guidelines on how to clear it.</param>
        /// <param name="messageResourceKey">The message resource key of the disable reason</param>
        /// <param name="providerType">The culture provider type to load localized message</param>
        /// <param name="msgParams">The message parameters</param>
        void Disable(
            Guid enableKey,
            SystemDisablePriority priority,
            Func<string> disableReason,
            TimeSpan duration,
            bool affectsIdleState,
            Type type = null,
            Func<string> helpText = null,
            string messageResourceKey = null,
            CultureProviderType? providerType = null,
            params object[] msgParams);

        /// <summary>
        ///     Clears an existing disable reason.  If this was the only reason to disable, the system will be enabled.
        /// </summary>
        /// <param name="enableKey">The unique key previously provided for a disable.</param>
        void Enable(Guid enableKey);

        /// <summary>
        ///     Adds a reason for the system to be disabled.  If the system is enabled, this will disable the system.
        /// </summary>
        /// <param name="enableKey">The unique value that will later re-enable this disable.</param>
        /// <param name="priority">The priority of this need to disable.</param>
        /// <param name="displayableMessage">A IDisplayableMessage object for the disable</param>
        void Disable(Guid enableKey, SystemDisablePriority priority, IDisplayableMessage displayableMessage);
    }
}