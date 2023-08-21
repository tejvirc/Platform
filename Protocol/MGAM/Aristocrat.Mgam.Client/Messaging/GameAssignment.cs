namespace Aristocrat.Mgam.Client.Messaging
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     The XLDF information assigned to a game.
    /// </summary>
    public struct GameAssignment
    {
        /// <summary>
        ///     Gets or sets the UPC number.
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public int UpcNumber { get; set; }

        /// <summary>
        ///     Gets or sets the paytable index.
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public int PayTableIndex { get; set; }

        /// <summary>
        ///     Gets or sets the denomination (in cents).
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public int Denomination { get; set; }

        /// <summary>
        ///     Gets or sets the XLDF file name.
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public string Xldf { get; set; }
    }
}
