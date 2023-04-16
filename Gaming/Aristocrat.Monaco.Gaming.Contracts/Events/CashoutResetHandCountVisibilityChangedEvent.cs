namespace Aristocrat.Monaco.Gaming.Contracts.Events
{
    using Aristocrat.Monaco.Kernel;

    /// <summary>
    /// Cashout dialog Visibility
    /// </summary>
    public class CashoutResetHandCountVisibilityChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="isVisible">True if overlay window is visible</param>
        public CashoutResetHandCountVisibilityChangedEvent(bool isVisible)
        {
            IsVisible = isVisible;
        }

        /// <summary>
        ///  True if visible
        /// </summary>
        public bool IsVisible { get; }
    }
}
