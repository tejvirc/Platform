namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     Exit the application running on the VLT. For
    ///     embedded applications, the VLT should
    ///     shutdown the system.All players and sessions
    ///     should be logged off before issuing an exit
    ///     command.This should be done by issuing a
    ///     LOCK and LOGOFF_PLAYER command.
    /// </summary>
    public class Exit : Command
    {
    }
}
