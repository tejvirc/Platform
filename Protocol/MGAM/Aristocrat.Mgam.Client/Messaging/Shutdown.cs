namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     Perform shutdown of applications and operating
    ///     system running on the VLT.All players and
    ///     sessions should be logged off before issuing a
    ///     shutdown command.This should be done by
    ///     issuing a LOCK and LOGOFF_PLAYER command.
    ///
    ///     If the VLT is not able to power off from software,
    ///     then it must enter a state that requires manual
    ///     intervention to be restarted and/or reconnected.
    /// </summary>
    public class Shutdown : Command
    {
    }
}
