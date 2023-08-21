namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Cabinet.Contracts;
    using Kernel;

    /// <summary>
    /// This event is a request to inject a custom UIElement over a view window
    /// </summary>
    public class ViewInjectionEvent : BaseEvent
    {
        /// <summary>
        /// Enum specifying the view action add or remove.
        /// </summary>
        public enum ViewAction
        {
            /// <summary>
            /// Add the UI Element
            /// </summary>
            Add,
            /// <summary>
            /// Remove the UI Element
            /// </summary>
            Remove
        };

        /// <summary>
        /// Ctor for ViewInjectionEvent
        /// </summary>
        /// <param name="element">Element to inject</param>
        /// <param name="displayRole">Window to inject into</param>
        /// <param name="action"></param>
        public ViewInjectionEvent(object element, DisplayRole displayRole, ViewAction action)
        {
            Element = element;
            DisplayRole = displayRole;
            Action = action;
        }

        /// <summary>
        /// The element to inject
        /// </summary>
        public object Element { get; set; }

        /// <summary>
        /// The windows to inject into
        /// </summary>
        public DisplayRole DisplayRole { get; set; }

        /// <summary>
        /// Action specifying add or remove UI element
        /// </summary>
        public ViewAction Action { get; }
    }
}