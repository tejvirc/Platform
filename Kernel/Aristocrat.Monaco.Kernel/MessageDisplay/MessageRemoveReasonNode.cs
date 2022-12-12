namespace Aristocrat.Monaco.Kernel.MessageDisplay
{
    using System;
    using Mono.Addins;

    /// <summary>
    ///     Definition of the MessageRemoveReasonNode class.
    /// </summary>
    [CLSCompliant(false)]
    [ExtensionNode("MessageRemoveReason")]
    public class MessageRemoveReasonNode : ExtensionNode
    {
        //// fields are initialized by MonoAddins
#pragma warning disable 0649

        /// <summary>
        ///     the literal event type name used to remove the message.
        /// </summary>
        [NodeAttribute("eventType")] private string _eventTypeName;

        /// <summary>
        ///     the event type used to remove the message.
        /// </summary>
        private Type _eventType;

        /// <summary>
        ///     Gets the event type used to remove the message.
        /// </summary>
        public Type EventType => _eventType ?? (_eventType = Type.GetType(_eventTypeName));
    }
}