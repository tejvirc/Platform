namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is sent by the VLT to notify the site controller about a denomination associated
    ///     with a particular game.
    /// </summary>
    public class RegisterDenomination : Request, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }

        /// <summary>
        ///     Gets or sets the identifier of the game that this denomination is associated with.
        /// </summary>
        public int GameUpcNumber { get; set; }

        /// <summary>
        ///     Gets or sets the amount of denomination, in pennies.
        /// </summary>
        public int Denomination { get; set; }
    }
}
