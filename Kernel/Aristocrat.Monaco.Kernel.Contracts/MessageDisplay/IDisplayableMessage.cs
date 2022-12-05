namespace Aristocrat.Monaco.Kernel.Contracts.MessageDisplay
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public interface IDisplayableMessage
    {
        /// <summary>
        ///     Gets the error or warning ID for the message
        /// </summary>
        Guid Id { get; }

        /// <summary>
        ///     Gets or sets the message text to display
        /// </summary>
        string Message { get; }

        /// <summary>
        ///     Gets the message classification
        /// </summary>
        DisplayableMessageClassification Classification { get; }

        /// <summary>
        ///     Gets the message priority
        /// </summary>
        DisplayableMessagePriority Priority { get; }

        /// <summary>   
        ///     Gets the event that was the reason for this message
        /// </summary>
        Type ReasonEvent { get; }

        /// <summary>
        ///     Boolean indicating if the message has a dynamic GUID generated at the time the message was created.
        /// This is used when determining if messages are equivalent or not.  If there is
        /// a dynamic GUID, we have to use other methods to equate like string matching
        /// </summary>
        bool MessageHasDynamicGuid { get; }

        /// <summary>
        ///     Gets or sets the help text to display
        /// </summary>
        string HelpText { get; }

        /// <summary>
        ///     Gets or sets the message callback, according to the specified culture provider name.
        /// </summary>
        Func<string> MessageCallback { get; set; }

        /// <summary>
        /// IsMessageEquivalent checks to see if the IDisplayableMessage if the fields of the passed in IDisplayableMessage are
        /// equivalent to the existing message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool IsMessageEquivalent(IDisplayableMessage message);
    }
}