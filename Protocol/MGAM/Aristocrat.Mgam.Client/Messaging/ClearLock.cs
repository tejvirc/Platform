namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     Clears any software locks on the VLT. Similar
    ///     to a REBOOT command with the “Clear Locks”
    ///     parameter, but without the rebooting.If the
    ///     cause for the lock is still present (for example,
    ///     paper still out) then this command may be
    ///     ignored for those locks.
    /// </summary>
    public class ClearLock : Command
    {
        
    }
}
