namespace Aristocrat.Monaco.Hardware.Contracts.CoinAcceptor
{
    /// <summary>enum to specifying coin event types.</summary>
    public enum CoinEventType
    {
        /// <summary>Coin in event.</summary>
        CoinInEvent,
        /// <summary>Coin to hopper instead of cashboxEvent.</summary>
        CoinToHopperInsteadOfCashboxEvent,
        /// <summary>Coin to hopper in event.</summary>
        CoinToHopperInEvent,
        /// <summary>Coin to cashbox in event.</summary>
        CoinToCashboxInEvent,
        /// <summary>Coin to cashbox instead of hopper event.</summary>
        CoinToCashboxInsteadOfHopperEvent
    }
}
