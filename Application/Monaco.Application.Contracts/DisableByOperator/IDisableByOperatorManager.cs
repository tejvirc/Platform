namespace Aristocrat.Monaco.Application.Contracts
{
    using System;

    /// <summary>
    ///     Interface through which the XSpin system can be disabled and enabled for the 'by operator' reason.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This interface allows multiple sources (components) to trigger an enable or disable 'by operator'
    ///         while allowing the implementation of the interface to encapsulate the behavior and persist the
    ///         desired state, something the ISystemDisableManager does not do.
    ///     </para>
    ///     <para>
    ///         The following example shows a simple class that disables and enables the system on behalf of the operator.
    ///         <code><![CDATA[
    ///  class MyDisabler
    ///  {
    ///      private const string DisableReason = "Operator - Menu Button";
    ///      public void DisableButtonPressed()
    ///      {
    ///          IDisableByOperatorManager manager = // get IDisableByOperatorManager reference
    ///          manager.Disable(() => DisableReason);
    ///      }
    ///      public void EnableButtonPressed()
    ///      {
    ///          IDisableByOperatorManager manager = // get IDisableByOperatorManager reference
    ///          manager.Enable();
    ///      }
    ///  }
    ///  ]]></code>
    ///     </para>
    /// </remarks>
    /// <seealso cref="SystemDisabledByOperatorEvent" />
    /// <seealso cref="SystemEnabledByOperatorEvent" />
    public interface IDisableByOperatorManager
    {
        /// <summary>
        ///     Gets a value indicating whether or not the system is disabled by the operator.
        /// </summary>
        bool DisabledByOperator { get; }

        /// <summary>
        ///     Disables the system on behalf of the operator.  If the system was not already disabled by the operator,
        ///     a <see cref="SystemDisabledByOperatorEvent" /> will be posted.
        /// </summary>
        /// <param name="disableReason">A human readable reason for the disable</param>
        void Disable(Func<string> disableReason);

        /// <summary>
        ///     Clears an the disabled-by-operator reason.  If the system was disabled by the operator, a
        ///     <see cref="SystemEnabledByOperatorEvent" /> will be posted.
        /// </summary>
        void Enable();
    }
}