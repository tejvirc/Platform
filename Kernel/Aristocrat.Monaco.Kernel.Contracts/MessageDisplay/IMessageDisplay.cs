namespace Aristocrat.Monaco.Kernel.Contracts.MessageDisplay
{
    using System;
    using Contracts.ErrorMessage;

    /// <summary>
    ///     Definition of the IMessageDisplay interface. Together with an associated Message Display Handler object, an object
    ///     that implements
    ///     this interface is used to display status, error, and flower box type messages in XSpin.
    /// </summary>
    /// <remarks>
    ///     In its current implementation, clients obtain an object that supports the IMessageDisplay interface from the
    ///     Service
    ///     Manager.
    ///     The IMessageDisplay service provides the methods to register Message Display Handler objects and to notify the
    ///     active
    ///     handler
    ///     that a message needs to be displayed or removed. Messages are wrapped in DisplayableMessage objects, which provide
    ///     classification
    ///     and priority information in addition to the text of the message.
    /// </remarks>
    /// <seealso cref="IMessageDisplayHandler" />
    /// <seealso cref="IDisplayableMessage" />
    public interface IMessageDisplay
    {
        /// <summary>
        ///     Sets the provided handler as the object to which all messages will be forwarded.
        /// </summary>
        /// <remarks>
        ///     Typically, a component that implements IMessageDisplayHandler will register itself using this method. When a
        ///     handler
        ///     gets registered, messages previously passed to the DisplayMessage methods are forwarded to the new handler.
        /// </remarks>
        /// <example>
        ///     This example assumes that the class containing the SetHandler method implements the IMessageDisplayHandler
        ///     interface.
        ///     <code>
        /// private void SetHandler()
        /// {
        ///     IMessageDisplay messageDisplay = ServiceManager.GetInstance().GetService{IMessageDisplay}();
        ///     messageDisplay.AddMessageDisplayHandler(this);
        /// }
        /// </code>
        /// </example>
        /// <param name="handler">The object to set as the message handler.</param>
        /// <param name="providerType">Culture provider type</param>
        /// <param name="displayPreviousMessages">
        /// Whether to display the previous soft and info messages in the MessageDisplay after the handler is added. 
        /// Note: ALL hard lockups are displayed no matter what</param>
        void AddMessageDisplayHandler(IMessageDisplayHandler handler, CultureProviderType? providerType = null, bool displayPreviousMessages = true);

        /// <summary>
        ///     Instructs the message display to no longer forward messages to the provided handler.
        /// </summary>
        /// <remarks>
        ///     Sometimes a message display handler needs to be removed when a component that provides the message display is
        ///     stopping or
        ///     going out of context. For example, a game providing the message display capability goes away if the operator enters
        ///     the
        ///     operator menu. In this case the game message display component would need to remove itself as a message handler.
        /// </remarks>
        /// <example>
        ///     This example assumes that the class containing the RemoveHandler method implements the IMessageDisplayHandler
        ///     interface.
        ///     <code>
        /// private void RemoveHandler()
        /// {
        ///     IMessageDisplay messageDisplay = ServiceManager.GetInstance().GetService{IMessageDisplay}();
        ///     messageDisplay.RemoveMessageDisplayHandler(this);
        /// }
        /// </code>
        /// </example>
        /// <param name="handler">The object to no longer use as a message handler.</param>
        void RemoveMessageDisplayHandler(IMessageDisplayHandler handler);

        /// <summary>
        ///     Forwards the provided message to the registered handler, if one exists.
        /// </summary>
        /// <example>
        ///     <code>
        ///  DisplayableMessage CashDoorMessage = new DisplayableMessage(
        ///      “Cash Door Opened”,
        ///      DisplayableMessageClassification.Informative,
        ///      DisplayableMessagePriority.Immediate);
        /// 
        ///      IMessageDisplay messageDisplay = ServiceManager.GetInstance().GetService{IMessageDisplay}();
        ///      messageDisplay.DisplayMessage(CashDoorMessage);
        ///  </code>
        /// </example>
        /// <param name="displayableMessage">The message to display.</param>
        void DisplayMessage(IDisplayableMessage displayableMessage);

        /// <summary>
        ///     Forwards the provided message to the registered handler, then sets a timer and has the message
        ///     removed after a specified number of milliseconds has elapsed.
        /// </summary>
        /// <example>
        ///     <code>
        ///  const int ThreeSeconds = 3000;
        /// 
        ///  DisplayableMessage SuccessfulTransferMessage = new DisplayableMessage(
        ///      Localization.Resource.SuccessfulTransfer,
        ///      DisplayableMessageClassification.Informative,
        ///      DisplayableMessagePriority.Normal)
        /// 
        ///      IMessageDisplay messageDisplay = ServiceManager.GetInstance().GetService{IMessageDisplay}();
        ///      messageDisplay.DisplayMessage(SuccessfulTransferMessage, ThreeSeconds);
        ///  </code>
        /// </example>
        /// <param name="displayableMessage">The message to display.</param>
        /// <param name="timeout">The amount of time the message should stay up, in milliseconds.</param>
        void DisplayMessage(IDisplayableMessage displayableMessage, int timeout);

        /// <summary>
        ///     Instructs the message handler to remove the message from the display.
        /// </summary>
        /// <remarks>
        ///     Use this method to remove a message displayed (or to be displayed) by the active handler. The object passed in
        ///     as a parameter is used to locate the message to be removed from the service and the message handler. An exception
        ///     is NOT thrown if a matching object is not found. As of this writing, object identity determines equality, so the
        ///     object reference used in the RemoveMessage call must be the same as the one sent to DisplayMessage
        /// </remarks>
        /// <example>
        ///     This example is a continuation of the one in DisplayMessage. Assume that we already have an instance
        ///     of the IMessageDisplay service and a reference to a DisplayableMessage object previously passed to DisplayMessage.
        ///     <code>
        ///     messageDisplay.RemoveMessage(CashDoorMessage);
        /// </code>
        /// </example>
        /// <param name="displayableMessage">A reference to the message to remove.</param>
        void RemoveMessage(IDisplayableMessage displayableMessage);

        /// <summary>
        /// Same as RemoveMessage((DisplayableMessage), but removes a message added with the supplied Id
        /// </summary>
        /// <param name="messageId"></param>
        void RemoveMessage(Guid messageId);

        /// <summary>
        ///     Requests that the message display handler displays the status message in the most appropriate way it deems
        /// </summary>
        /// <remarks>
        ///     This forwards a string to the currently active message display handler (if any). When there are no message
        ///     display handlers, this method does not retain the message for forwarding when one becomes available.
        /// </remarks>
        /// <param name="statusMessage">The status message to be displayed.</param>
        void DisplayStatus(string statusMessage);

        /// <summary>
        /// Use AddErrorMessageMapping to inject the ErrorMessageMapping object into the MessageDisplay Service
        /// </summary>
        /// <param name="mapping"></param>
        void AddErrorMessageMapping(IErrorMessageMapping mapping);

        /// <summary>
        /// Reload the cached messages when game language is changed
        /// </summary>
        void RefreshMessages();
    }
}