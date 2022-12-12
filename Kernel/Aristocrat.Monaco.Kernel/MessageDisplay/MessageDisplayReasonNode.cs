namespace Aristocrat.Monaco.Kernel.MessageDisplay
{
    using System;
    using System.Collections.Generic;
    using Mono.Addins;

    /// <summary>
    ///     Definition of the MessageDisplayReasonNode class.
    /// </summary>
    [CLSCompliant(false)]
    [ExtensionNodeChild(typeof(MessageRemoveReasonNode))]
    public class MessageDisplayReasonNode : ExtensionNode
    {
        //// Fields are initialized by MonoAddins
#pragma warning disable 0649

        /// <summary>
        ///     A list of event types used to remove the message.
        /// </summary>
        private List<Type> _removeNodes;

        /// <summary>
        ///     The literal event type name specified in the extension node.
        /// </summary>
        [NodeAttribute("eventType")] private string _eventTypeName;

        /// <summary>
        ///     The event type reason used to display the message.
        /// </summary>
        private Type _eventType;

        /// <summary>
        ///     Gets the event type reason used to display the message.
        /// </summary>
        public Type EventType
        {
            get
            {
                if (_eventType == null)
                {
                    if (!string.IsNullOrEmpty(_eventTypeName))
                    {
                        _eventType = Type.GetType(_eventTypeName);
                    }
                }

                return _eventType;
            }
        }

        /// <summary>
        ///     Gets a list of literal event types used to remove the message.
        /// </summary>
        public ICollection<Type> RemoveNodes
        {
            get
            {
                if (_removeNodes == null)
                {
                    _removeNodes = new List<Type>();
                    foreach (var node in ChildNodes)
                    {
                        if (node is MessageRemoveReasonNode removeNode)
                        {
                            _removeNodes.Add(removeNode.EventType);
                        }
                    }
                }

                return _removeNodes;
            }
        }
    }
}