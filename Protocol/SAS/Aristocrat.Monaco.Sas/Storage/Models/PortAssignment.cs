namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using Common.Storage;

    /// <summary>
    ///     The port assignment entity
    /// </summary>
    public class PortAssignment : BaseEntity
    {
        /// <summary>
        ///     Gets or sets whether or not dual host mode is used
        /// </summary>
        public bool IsDualHost { get; set; }

        /// <summary>
        /// Gets or sets the port used for Aft including 
        /// long polls (72, 73, 74, 75, and 76). 
        /// </summary>
        public HostId AftPort { get; set; }

        /// <summary>
        /// Gets or sets the port used for general system control including
        /// long polls (03, 04, 05,06, 07, 0A, 0B, 94, and A8)
        /// </summary>
        public HostId GeneralControlPort { get; set; }

        /// <summary>
        /// Gets or sets the port used for legacy bonus including long polls
        /// (2E, 8A, and 8B).
        /// </summary>
        public HostId LegacyBonusPort { get; set; }

        /// <summary>
        /// Gets or sets the port used for Sas progressives including long polls (80 and 86).
        /// </summary>
        public HostId ProgressivePort { get; set; }

        /// <summary>
        /// Gets or sets the port used for ticket validation including long polls 
        /// (4C, 4D, 57, 58, 70, 71, 7B, 7C, and 7D). 
        /// </summary>
        public HostId ValidationPort { get; set; }

        /// <summary>
        /// Gets or sets the hosts used for game start/end exception reporting 
        /// </summary>
        public GameStartEndHost GameStartEndHosts { get; set; }

        /// <summary>
        /// Gets or sets the Host 1 Non Sas Progressive Hit Reporting
        /// </summary>
        public bool Host1NonSasProgressiveHitReporting { get; set; }

        /// <summary>
        /// Gets or sets the Host 2 Non Sas Progressive Hit Reporting
        /// </summary>
        public bool Host2NonSasProgressiveHitReporting { get; set; }
    }
}