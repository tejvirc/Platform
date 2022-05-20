namespace Aristocrat.Sas.Client
{
    /// <summary>
    ///     The persistence blocks used by the platform callbacks
    /// </summary>
    public enum SasClientPersistenceBlock
    {
        /// <summary>
        ///     The exception queue persistence block
        /// </summary>
        ExceptionQueue,
        /// <summary>
        ///    The priority exception queue block
        /// </summary>
        PriorityExceptions
    }

    /// <summary>
    ///     This interface contains the calls needed by the SAS engine into the platform
    /// </summary>
    public interface IPlatformCallbacks
    {
        /// <summary>
        ///     Updates the link status for a client. This is only called at
        ///     start up and when the link status changes.
        ///     A SAS link down condition occurs if we haven't received a long poll or
        ///     general poll in the last 5 seconds. A link up condition occurs once we
        ///     receive any long poll or general poll with our address.
        /// </summary>
        /// <param name="linkUp">True if the link is up</param>
        /// <param name="client">The client (0 or 1) that had a link status change</param>
        void LinkUp(bool linkUp, int client);

        /// <summary>Disables or enables communications with Sas.</summary>
        /// <param name="enabled">True if communications should be enabled. False otherwise.</param>
        /// <param name="client">The client we're communicating with.</param>
        void ToggleCommunicationsEnabled(bool enabled, byte client);
    }

    /// <summary>
    ///     The possible sound actions
    /// </summary>
    public enum SoundActions
    {
        AllOn,
        AllOff,
        GameOff
    }

    /// <summary>
    ///     The meter types
    /// </summary>
    public enum MeterType
    {
        Lifetime,
        Period
    }
}