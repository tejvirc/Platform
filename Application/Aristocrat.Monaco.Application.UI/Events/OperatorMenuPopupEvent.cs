
namespace Aristocrat.Monaco.Application.UI.Events
{
    using Kernel;
    using System.Windows;

    /// <summary>
    ///     Definition of the OperatorMenuPopupEvent class.
    ///     <remarks>
    ///         This event is posted by Operator Menu pages when they
    ///         want to show a popup. The MenuSelectionViewModel 
    ///         listens to these events and will show the popup info
    ///         passed in the event.
    ///     </remarks>
    /// </summary>
    public class OperatorMenuPopupEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorMenuPopupEvent" /> class.
        /// </summary>
        /// <param name="open">True to open the popup, False to close it</param>
        /// <param name="popupText">Popup text to display</param>
        /// <param name="targetElement">The target UIElement for an info popup. Leave this null for a general center-screen popup.</param>
        /// <param name="popupTimeoutSeconds">The number of seconds to show the popup before it closes</param>
        /// <param name="closeOnLostFocus">Indicates whether or not to close the pop-up upon lost focus</param>
        public OperatorMenuPopupEvent(bool open, string popupText = null, UIElement targetElement = null, int popupTimeoutSeconds = 0, bool closeOnLostFocus = false)
        {
            PopupOpen = open;
            PopupText = popupText;
            TargetElement = targetElement;
            PopupTimeoutSeconds = popupTimeoutSeconds;
            CloseOnLostFocus = closeOnLostFocus;
        }

        /// <summary>
        ///     Gets or sets popup open state
        /// </summary>
        public bool PopupOpen { get; }

        /// <summary>
        ///     Gets or sets popup timeout in seconds
        /// </summary>
        public int PopupTimeoutSeconds { get; }

        /// <summary>
        ///     Gets or sets popup text
        /// </summary>
        public string PopupText { get; }

        /// <summary>
        ///     Gets or sets target UIElement
        /// </summary>
        public UIElement TargetElement { get; }


        /// <summary>
        ///     Gets or sets whether or not to close the pop-up upon lost focus
        /// </summary>
        public bool CloseOnLostFocus { get; }
    }
}
