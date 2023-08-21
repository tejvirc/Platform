namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     Instructs VLT to compute the checksum of the
    ///     software that is currently running.
    /// </summary>
    public class ComputeChecksum : Command
    {
        /// <summary>
        ///     Gets or sets the seed value.
        /// </summary>
        public int Seed { get; set; }
    }
}
