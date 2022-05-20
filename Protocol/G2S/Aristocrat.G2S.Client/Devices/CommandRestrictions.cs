namespace Aristocrat.G2S.Client.Devices
{
    /// <summary>
    ///     Contains a list of command restrictions
    /// </summary>
    public enum CommandRestrictions
    {
        /// <summary>
        ///     Commands are not restricted to the owner or guest.
        /// </summary>
        None,

        /// <summary>
        ///     Commands are restricted to the device owner.
        /// </summary>
        RestrictedToOwner,

        /// <summary>
        ///     Commands are restricted to the device owner and guests.
        /// </summary>
        RestrictedToOwnerAndGuests
    }
}