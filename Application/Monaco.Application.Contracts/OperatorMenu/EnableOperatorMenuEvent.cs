namespace Aristocrat.Monaco.Application.Contracts.OperatorMenu
{
    using Kernel;

    /// <summary>
    ///     Event to enable or disable all operator menu buttons and menu items
    /// </summary>
    public class EnableOperatorMenuEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="enable">Whether to enable or disable buttons and menu items</param>
        public EnableOperatorMenuEvent(bool enable)
        {
            Enable = enable;
        }

        /// <summary>
        ///     Enable or disable buttons and menu items
        /// </summary>
        public bool Enable { get; }
    }
}
