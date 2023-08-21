namespace Aristocrat.Mgam.Client.Messaging
{
    using System.Collections.Generic;

    /// <summary>
    ///     This message is sent in response to a <see cref="GetGameAssignments"/> message.
    /// </summary>
    public class GetGameAssignmentsResponse : Response
    {
        /// <summary>
        ///     Gets the game assignments.
        /// </summary>
        public IReadOnlyList<GameAssignment> Assignments { get; internal set; }
    }
}
