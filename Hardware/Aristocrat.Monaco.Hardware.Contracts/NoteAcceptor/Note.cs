﻿namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System;
    using static System.FormattableString;

    /// <inheritdoc />
    /// <summary>A note.</summary>
    [Serializable]
    public class Note : INote
    {
        /// <summary>Gets or sets the identifier of the note.</summary>
        /// <value>The identifier of the note.</value>
        public int NoteId { get; set; }

        /// <summary>Gets or sets the value.</summary>
        /// <value>The value.</value>
        public int Value { get; set; }

        /// <summary>Gets the ISO currency code.</summary>
        /// <value>The ISO currency code.</value>
        public ISOCurrencyCode CurrencyCode => Enum.TryParse(ISOCurrencySymbol, out ISOCurrencyCode value) ? value : ISOCurrencyCode.USD;

        /// <summary>Gets or sets the ISO currency symbol.</summary>
        /// <value>The ISO currency symbol.</value>
        public string ISOCurrencySymbol { get; set; }

        /// <inheritdoc />
        public int Version { get; set; }

        /// <summary>Assembles and returns a string representation of the event and its data.</summary>
        /// <returns>A string representation of the event and its data</returns>
        public override string ToString()
        {
            return Invariant($"{GetType()} [NoteId={NoteId}, Value={Value}, ISOCurrencySymbol={ISOCurrencySymbol}, Version={Version}.]");
        }
    }
}
