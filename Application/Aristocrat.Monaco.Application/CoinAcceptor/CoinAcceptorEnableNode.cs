namespace Aristocrat.Monaco.Application.CoinAcceptor
{
    using System;
    using Mono.Addins;

    /// <summary>
    ///     Definition of the CoinAcceptorEnableNode class.
    /// </summary>
    [CLSCompliant(false)]
    [ExtensionNode("CoinAcceptorEnable")]
    public class CoinAcceptorEnableNode : ExtensionNode
    {
        //// fields are initialized by MonoAddins
#pragma warning disable 0649

        /// <summary>
        ///     The literal event type name to enable the CoinAcceptor.
        /// </summary>
        [NodeAttribute("eventType")] private string _eventTypeName;

        /// <summary>
        ///     the event type to enable the CoinAcceptor.
        /// </summary>
        private Type _eventType;

        /// <summary>
        ///     Gets the event type to enable the CoinAcceptor.
        /// </summary>
        public Type EventType => _eventType ?? (_eventType = Type.GetType(_eventTypeName));
    }
}
