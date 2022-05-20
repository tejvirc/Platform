namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    // TODO : Update Locale.cs - it is a stub.
    /// <summary>
    ///     A locale for determining messages from the ID Reader device.
    /// </summary>
    public class Locale
    {
        /// <summary>
        ///     Initialization of the <see cref="Locale"/> class.
        ///     Comparable to the PrintableRegion class.
        /// </summary>
        public Locale(
            string id,
            string name,
            string attract,
            string wait,
            string valid,
            string invalid,
            string lost,
            string offline,
            string abandoned
            )
        {
            LocaleId = id;
            LocaleName = name;
            AttractMsg = attract;
            WaitMsg = wait;
            ValidMsg = valid;
            InvalidMsg = invalid;
            LostMsg = lost;
            OfflineMsg = offline;
            AbandonedMsg = abandoned;
        }

        // TODO : Limit any set ID to 10 characters; no more.
        /// <summary>
        ///     Locale identifier.
        /// </summary>
        public string LocaleId { get; set; }

        // TODO : Set the locale name to be no more than 256 characters.
        /// <summary>
        ///     Name of the locale.
        /// </summary>
        public string LocaleName { get; set; }

        // TODO : Set all locale messages to be no more than 256 characters.

        /// <summary>
        ///     Message to display when no ID is present.
        /// </summary>
        public string AttractMsg { get; set; }

        /// <summary>
        ///     Message to display while waiting for validation.
        /// </summary>
        public string WaitMsg { get; set; }

        /// <summary>
        ///     Message to display while a valid ID is present.
        /// </summary>
        public string ValidMsg { get; set; }

        /// <summary>
        ///     Message to display while an invalid ID is present.
        /// </summary>
        public string InvalidMsg { get; set; }

        /// <summary>
        ///     Message to display while a lost ID is present.
        /// </summary>
        public string LostMsg { get; set; }

        /// <summary>
        ///     Message to display if an ID cannot be validated.
        /// </summary>
        public string OfflineMsg { get; set; }

        /// <summary>
        ///     Message to display if an ID is abandoned.
        /// </summary>
        public string AbandonedMsg { get; set; }
    }
}
