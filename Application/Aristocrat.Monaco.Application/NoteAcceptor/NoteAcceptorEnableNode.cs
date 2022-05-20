namespace Aristocrat.Monaco.Application.NoteAcceptor
{
    using System;
    using Mono.Addins;

    /// <summary>
    ///     Definition of the NoteAcceptorEnableNode class.
    /// </summary>
    [CLSCompliant(false)]
    [ExtensionNode("NoteAcceptorEnable")]
    public class NoteAcceptorEnableNode : ExtensionNode
    {
        //// fields are initialized by MonoAddins
#pragma warning disable 0649

        /// <summary>
        ///     The literal event type name to enable the NoteAcceptor.
        /// </summary>
        [NodeAttribute("eventType")] private string _eventTypeName;

        /// <summary>
        ///     the event type to enable the NoteAcceptor.
        /// </summary>
        private Type _eventType;

        /// <summary>
        ///     Gets the event type to enable the NoteAcceptor.
        /// </summary>
        public Type EventType => _eventType ?? (_eventType = Type.GetType(_eventTypeName));
    }
}