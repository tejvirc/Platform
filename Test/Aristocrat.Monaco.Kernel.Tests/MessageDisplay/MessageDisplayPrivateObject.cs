namespace Aristocrat.Monaco.Kernel.Tests.MessageDisplay
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MessageDisplay = Kernel.MessageDisplay;

    /// <summary>
    /// </summary>
    public class MessageDisplayPrivateObject : PrivateObject
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        /// <param name="messageDisplay">Message display instance.</param>
        public MessageDisplayPrivateObject(MessageDisplay messageDisplay)
            : base(messageDisplay)
        {
            // DO NOTHING
        }

        /// <summary>
        ///     The ordered list of registered message handlers, where the tail of the list
        ///     is the last handler added and the one to which messages are routed.
        /// </summary>
        public LinkedList<IMessageDisplayHandler> Handlers
        {
            get { return (LinkedList<IMessageDisplayHandler>)GetField("_handlers"); }
        }

        /// <summary>
        ///     The collection of displayable messages shared between display handlers.
        /// </summary>
        public Collection<DisplayableMessage> Messages
        {
            get { return (Collection<DisplayableMessage>)GetField("_messages"); }
        }

        /// <summary>
        ///     List of the configured display nodes
        /// </summary>
        [CLSCompliant(false)]
        public List<MessageDisplayReasonNode> ConfiguredDisplayNodes
        {
            get { return (List<MessageDisplayReasonNode>)GetField("_configuredDisplayNodes"); }
        }

        /// <summary>
        ///     List of messages which require tracing.
        /// </summary>
        internal List<MessageDisplay.ObservedMessage> ObservedMessages
        {
            get { return (List<MessageDisplay.ObservedMessage>)GetField("_observedMessages"); }
        }

        /// <summary>
        ///     Dispose(bool disposing) executes in two distinct scenarios.
        ///     If disposing equals true, the method has been called directly or
        ///     indirectly by the user's code. Managed and unmanaged resources
        ///     can be disposed. If disposing equals false, the method has been called
        ///     by the runtime from inside the finalizer and you should not reference
        ///     other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">Whether the object is being disposed</param>
        public void Dispose(bool disposing)
        {
            Invoke(
                "Dispose",
                BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
                disposing);
        }
    }
}