namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     Requires the VLT to return to the “Waiting for
    ///     Play Command” state, as if it had just received
    ///     a ReadyToPlayResponse message.  Any open
    ///     sessions on the VLT should be closed with a
    ///     EndSession message sequence.
    ///     Currently, the System will only issue this
    ///     command after site closing, or in the case the
    ///     system runs out of outcomes.
    /// </summary>
    public class Unplay : Command
    {
    }
}
