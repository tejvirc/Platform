namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is sent by the VLT during game initialization to notify the site controller of the
    ///     different lottery ticket pools that the game is capable of drawing from.
    /// </summary>
    public class RegisterGame : Request, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }

        /// <summary>
        ///     Gets the identifier for the index of the paytable for the game being
        ///     registered.
        /// </summary>
        public int PayTableIndex { get; set; }

        /// <summary>
        ///     Gets the number of credits played at a time in this game.
        /// </summary>
        public int NumberOfCredits { get; set; }

        /// <summary>
        ///     Gets the identifier the theme of this game.
        /// </summary>
        public int GameUpcNumber { get; set; }

        /// <summary>
        ///     Gets the theme of this game, for example "Meltdown".
        /// </summary>
        public string GameDescription { get; set; }

        /// <summary>
        ///     Gets the pay table name for the game being registered.
        /// </summary>
        public string PayTableDescription { get; set; }
    }
}
