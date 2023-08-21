namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     The VLT may optionally send this message to retrieve a list of all of the XLDF files assigned to
    ///     it. This can be done in response to an Employee action, for example, for informational purposes.
    ///     This message is optional, and is not needed for NYL VLT operations.
    /// </summary>
    public class GetGameAssignments : Request, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }
    }
}
