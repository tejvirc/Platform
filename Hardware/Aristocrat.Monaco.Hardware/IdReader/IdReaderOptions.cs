namespace Aristocrat.Monaco.Hardware.IdReader
{
    using Contracts.IdReader;

    /// <summary>An ID reader options object.</summary>
    public struct IdReaderOptions
    {
        /// <summary>Gets or sets the type of the ID reader.</summary>
        /// <value>The type of the ID reader.</value>
        public IdReaderTypes IdReaderType { get; set; }

        /// <summary>Gets or sets the ID reader track.</summary>
        /// <value>The ID reader track.</value>
        public IdReaderTracks IdReaderTrack { get; set; }

        /// <summary>Gets or sets the validation method.</summary>
        /// <value>The validation method.</value>
        public IdValidationMethods ValidationMethod { get; set; }

        /// <summary>Gets or sets the wait timeout.</summary>
        /// <value>The wait timeout.</value>
        public int WaitTimeout { get; set; }

        /// <summary>Gets or sets the removal delay timeout.</summary>
        /// <value>The removal delay timeout.</value>
        public int RemovalDelay { get; set; }

        /// <summary>Gets or sets the validation timeout.</summary>
        /// <value>The validation timeout.</value>
        public int ValidationTimeout { get; set; }

        /// <summary>Gets or sets a value indicating whether offline validation is supported.</summary>
        /// <value>True if offline validation is supported, false if not.</value>
        public bool SupportsOfflineValidation { get; set; }
    }
}