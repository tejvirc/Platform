namespace Aristocrat.Monaco.Gaming.UI.Events
{
    using Kernel;

    /// <summary>
    ///     Definition of the CashoutButtonStatusEvent class.  [Test Automation purposes only.]
    /// </summary>
    public class CashoutButtonStatusEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CashoutButtonStatusEvent" /> class.
        /// </summary>
        /// <param name="value">The status of CashOut enabled in PlayerMenu.</param>
        public CashoutButtonStatusEvent(bool value)
        {
            CashOutEnabledInPlayerMenu = value;
        }

        /// <summary>
        ///     Gets the CashOut enabled in PlayerMenu value
        /// </summary>
        public bool CashOutEnabledInPlayerMenu { get; }
    }
}
