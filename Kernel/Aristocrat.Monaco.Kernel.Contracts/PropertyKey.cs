namespace Aristocrat.Monaco.Kernel.Contracts
{
    /// <summary>
    ///     This class just holds keys for the various properties.
    /// </summary>
    public static class PropertyKey
    {
        // WARNING:  Don't add anything to this class.  It violates the one major architectural tenet we have in the platform by allowing any layer to access a property
        //  in a layer above.  This class will be removed when the classes that use these properties are refactored.
        //  Constants for properties should be added to the *Constants.cs class in the layer that owns the property
        // WARNING:  Read above.  Just don't do it. 

        /// <summary>The current balance. Should be in sync with querying the balance from the bank.</summary>
        public const string CurrentBalance = "System.CurrentBalance";

        /// <summary>
        ///     Indicates whether or not voucher in is enabled.
        /// </summary>
        public const string VoucherIn = "System.VoucherIn";

        /// <summary>
        ///     Indicates whether or not note in is enabled.
        /// </summary>
        public const string NoteIn = "System.NoteIn";

        /// <summary>
        ///     The maximum value that can be added to the credit meter via the note or coin acceptor
        /// </summary>
        public const string MaxCreditsIn = "System.MaxCreditsIn";

        /// <summary> Property Manager key for DefaultVolumeLevel. </summary>
        public const string DefaultVolumeLevel = "Application.DefaultVolumeLevel";

        /// <summary> The key to access the ticket text line1 property. </summary>
        public const string TicketTextLine1 = "TicketProperty.TicketTextLine1";

        /// <summary> The key to access the ticket text line2 property. </summary>
        public const string TicketTextLine2 = "TicketProperty.TicketTextLine2";

        /// <summary> The key to access the ticket text line3 property. </summary>
        public const string TicketTextLine3 = "TicketProperty.TicketTextLine3";
    }
}
