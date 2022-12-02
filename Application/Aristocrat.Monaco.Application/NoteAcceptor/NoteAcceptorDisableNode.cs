namespace Aristocrat.Monaco.Application.NoteAcceptor
{
    using System;
    using System.Collections.Generic;
    using Hardware.Contracts.SharedDevice;
    using Mono.Addins;

    /// <summary>
    ///     Definition of the NoteAcceptorDisableNode class.
    /// </summary>
    [CLSCompliant(false)]
    [ExtensionNodeChild(typeof(NoteAcceptorEnableNode))]
    public class NoteAcceptorDisableNode : ExtensionNode
    {
        //// Fields are initialized by MonoAddins
#pragma warning disable 0649

        /// <summary>
        ///     A list of event types to enable the NoteAcceptor.
        /// </summary>
        private List<Type> _enableNodes;

        /// <summary>
        ///     The literal event type name specified in the extension node.
        /// </summary>
        [NodeAttribute("eventType")] private string _eventTypeName;

        /// <summary>
        ///     The disable reason specified in the extension node.
        /// </summary>
        [NodeAttribute("reason")] private string _reason;

        /// <summary>
        ///     The disabled reason code specified in the extension node.
        /// </summary>
        [NodeAttribute("disabledReason")] private string _disabledReason;

        /// <summary>
        ///     The enabled reason code specified in the extension node.
        /// </summary>
        [NodeAttribute("enabledReason")] private string _enabledReason;

        /// <summary>
        ///     The event type to disable the NoteAcceptor.
        /// </summary>
        private Type _eventType;

        /// <summary>
        ///     Gets the event type to disable the NoteAcceptor
        /// </summary>
        public Type EventType => _eventType ?? (_eventType = Type.GetType(_eventTypeName));

        /// <summary>
        ///     Gets the reason to disable the NoteAcceptor
        /// </summary>
        public string Reason => _reason;

        /// <summary>
        ///     Gets the disable reason
        /// </summary>
        public DisabledReasons DisabledReason => Enum.TryParse(_disabledReason, true, out DisabledReasons reasonCode) ? reasonCode : DisabledReasons.System;

        /// <summary>
        ///     Gets the disable reason
        /// </summary>
        public EnabledReasons EnabledReason => Enum.TryParse(_enabledReason, true, out EnabledReasons reasonCode) ? reasonCode : EnabledReasons.System;

        /// <summary>
        ///     Gets a list of literal event types to enable the NoteAcceptor.
        /// </summary>
        public ICollection<Type> EnableNodes
        {
            get
            {
                if (_enableNodes != null)
                {
                    return _enableNodes;
                }

                _enableNodes = new List<Type>();
                foreach (var node in GetChildNodes())
                {
                    if (node is NoteAcceptorEnableNode enableNode)
                    {
                        _enableNodes.Add(enableNode.EventType);
                    }
                }

                return _enableNodes;
            }
        }
    }
}