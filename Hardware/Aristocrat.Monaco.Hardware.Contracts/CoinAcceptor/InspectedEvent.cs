namespace Aristocrat.Monaco.Hardware.Contracts.CoinAcceptor
{
    using System;
    using Properties;
    
    /// <summary>Definition of the InspectedEvent class.</summary>
    /// <remarks>
    ///     The Initialized Event is posted by the coin acceptor Service in response
    ///     to an ImplementationEventId.PrinterFirmwareVersion with a logical state of Inspecting.
    /// </remarks>
    [Serializable]
    public class InspectedEvent : CoinAcceptorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectedEvent" /> class.
        /// </summary>
        public InspectedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectedEvent" /> class.Initializes a new instance of the
        ///     InspectedEvent class with the coin acceptor's ID.
        /// </summary>
        /// <param name="coinAcceptorId">The associated coin acceptor's ID.</param>
        public InspectedEvent(int coinAcceptorId)
            : base(coinAcceptorId)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Resources.CoinAcceptorText} {Resources.InspectionFailedText} {Resources.ClearedText}";
        }
    }
}
