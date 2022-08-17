namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Cabinet.Contracts;

    /// <summary>
    ///     These event arguments are a request to inject a custom UIElement over a view window
    /// </summary>
    public class ViewInjectionEventArgs : EventArgs
    {
        /// <summary>
        ///     Ctor for ViewInjectionEventArgs
        /// </summary>
        /// <param name="element">Element to inject</param>
        /// <param name="displayRole">Window to inject into</param>
        /// <param name="action"></param>
        public ViewInjectionEventArgs(object element, DisplayRole displayRole, ViewAction action)
        {
            Element = element;
            DisplayRole = displayRole;
            Action = action;
        }

        /// <summary>
        ///     The element to inject
        /// </summary>
        public object Element { get; set; }

        /// <summary>
        ///     The windows to inject into
        /// </summary>
        public DisplayRole DisplayRole { get; set; }

        /// <summary>
        ///     Action specifying add or remove UI element
        /// </summary>
        public ViewAction Action { get; }
    }
}