namespace Aristocrat.Monaco.Hardware.Contracts.ButtonDeck
{
    /// <summary>Data type that lists the different types of supported button decks</summary>
    public enum ButtonDeckType
    {
        /// <summary>No button deck</summary>
        NoButtonDeck,

        /// <summary>Virtual button deck</summary>
        Virtual,

        /// <summary>LCD button deck</summary>
        LCD,

        /// <summary>Simulated virtual button deck</summary>
        SimulatedVirtual,

        /// <summary>Simulated LCD button deck</summary>
        SimulatedLCD,

        /// <summary>Physical Button Deck</summary>
        PhysicalButtonDeck
    }
}