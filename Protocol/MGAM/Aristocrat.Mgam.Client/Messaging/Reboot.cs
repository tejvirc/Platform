namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     Instructs the VLT to perform a reboot sequence.
    ///     The VLT is required to UnregisterInstance first.
    ///
    ///     The REBOOT command parameter may
    ///     contain the phrase “Clear Lock” in which case,
    ///     the VLT must clear any LOCK state.
    /// </summary>
    public class Reboot : Command
    {
        /// <summary>
        ///     Gets or sets a value that indicates if the locks should be cleared before reboot.
        /// </summary>
        public bool IsClearLocks { get; set; }
    }
}
