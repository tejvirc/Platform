namespace Aristocrat.Monaco.Sas.Contracts.Client.Configuration
{
    using SASProperties;

    /// <summary>
    /// Definition of the ControlPortsElement class.
    /// </summary>
    public class ControlPortsElement
    {
        /// <summary>
        /// Initializes a new instance of the ControlPortsElement class.  Initializes the default
        /// control port of 1;
        /// </summary>
        public ControlPortsElement()
        {
            FundTransferPort = 1;
            GeneralControlPort = 1;
            LegacyBonusPort = 1;
            ProgressivePort = 1;
            ValidationPort = 1;
            GameStartEndHosts = GameStartEndHost.None;
        }

        /// <summary>
        /// Gets or sets the port used for Aft including 
        /// long polls (72, 73, 74, 75, and 76) or Eft(1D, 27, 28, 62, 63, 63, 64, 65, 66, 69, 6A, 6B). 
        /// </summary>
        public int FundTransferPort { get; set; }

        /// <summary>
        /// Gets or sets the fund transfer type which can be Aft or Eft.
        /// </summary>
        public FundTransferType FundTransferType { get; set; }

        /// <summary>
        /// Gets or sets the port used for general system control including
        /// long polls (03, 04, 05,06, 07, 0A, 0B, 94, and A8)
        /// </summary>
        public int GeneralControlPort { get; set; }

        /// <summary>
        /// Gets or sets the port used for legacy bonus including long polls
        /// (2E, 8A, and 8B).
        /// </summary>
        public int LegacyBonusPort { get; set; }

        /// <summary>
        /// Gets or sets the port used for Sas progressives including long polls (80 and 86).
        /// </summary>
        public int ProgressivePort { get; set; }

        /// <summary>
        /// Gets or sets the port used for ticket validation including long polls 
        /// (4C, 4D, 57, 58, 70, 71, 7B, 7C, and 7D). 
        /// </summary>
        public int ValidationPort { get; set; }

        /// <summary>
        /// Gets or sets the hosts used for game start/end exception reporting 
        /// </summary>
        public GameStartEndHost GameStartEndHosts { get; set; }
    }
}
