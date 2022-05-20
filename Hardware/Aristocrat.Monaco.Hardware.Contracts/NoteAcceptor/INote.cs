namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    /// <summary>Interface for note.</summary>
    public interface INote
    {
        /// <summary>Gets the value.</summary>
        /// <value>The value.</value>
        int Value { get; }

        /// <summary>Gets the ISO currency code.</summary>
        /// <value>The ISO currency code.</value>
        ISOCurrencyCode CurrencyCode { get; }

        /// <summary>Gets the version.</summary>
        /// <value>The version.</value>
        int Version { get; }
    }
}
