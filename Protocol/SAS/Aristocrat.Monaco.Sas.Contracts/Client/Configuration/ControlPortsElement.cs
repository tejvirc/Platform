namespace Aristocrat.Monaco.Sas.Contracts.Client.Configuration
{
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
            AftPort = 1;
            GeneralControlPort = 1;
            LegacyBonusPort = 1;
            ProgressivePort = 1;
            ValidationPort = 1;
            GameStartEndHosts = GameStartEndHost.None;
        }

        /// <summary>
        /// Gets or sets the port used for Aft including 
        /// long polls (72, 73, 74, 75, and 76). 
        /// </summary>
        public int AftPort { get; set; }

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
